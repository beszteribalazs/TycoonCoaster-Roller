using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour{
    public BuildingSystem buildingSystem;
    [SerializeField] Spawner spawner;
    
    public static GameManager instance;
    private int width;
    private int height;
    private const int REPAIRTIME = 60;
    private float money;
    private float totalHappiness;
    private float trashLevel;
    private float trashPercentage;
    private float totalCapacity;
    
    //private float gameSpeed;
    private int dayCount;
    private int gameHour;
    private int gameSecond;
    //private float countSecond;
    //private float helpSecond;
    private bool gameIsActive;
    private List<Janitor> janitors;

    public int totalMechanics = 0;
    public int availableMechanics = 0;
    float mechanicSalary;
        
    int janitorCount = 0;
    
    //public int GameSpeed => (int)gameSpeed / 10;

    public List<Attraction> Attractions{
        get{
            List<Attraction> list = new List<Attraction>();
            foreach (Building building in buildingSystem.Buildings){
                if (building.Type.type == BuildingTypeSO.Type.Attraction){
                    list.Add((Attraction) building);
                }
            }

            return list;
        }
    }

    private void Awake(){
        instance = this;
        mechanicSalary = 300 * 0.1f / 24 / 60;
    }

    void Start(){
        this.width = MapSizeController.mapSize;
        this.height = MapSizeController.mapSize;
        this.money = 100000f;
        this.totalHappiness = 1f;
        this.trashLevel = 0f;
        this.trashPercentage = 0f;
        //this.gameSpeed = 10f;
        this.dayCount = 0;
        this.gameIsActive = true;
        this.janitors = new List<Janitor>();
        EventManager.instance.SpeedChanged(1);
    }

    /*void Update(){
        if (gameIsActive){
            helpSecond = helpSecond + Time.deltaTime;
            if (helpSecond >= (1 / gameSpeed)){
                GameLoop();
                helpSecond = 0;
            }
        }
    }*/


    public void RepairAttraction(Attraction target){
        if (availableMechanics > 0 && NavigationManager.instance.IsTargetReachable(target)){
            GameObject obj = spawner.SpawnMechanic(buildingSystem.entryPoint.position + new Vector3(1, 0, 1) * (buildingSystem.CellSize / 2));
            Mechanic mechanic = obj.GetComponent<Mechanic>();
            mechanic.Repair(target);
            target.beingRepaired = true;
            availableMechanics--;
        }
    }

    public void GameLoop(){
        int countSecond = TimeManager.instance.Tick;
        gameSecond++;

        //evening-daytime
        if (countSecond >= 0 && countSecond < 720){ }
        else if (countSecond >= 720 && countSecond < 1440){ }
        else if (Math.Abs(countSecond - 1440f) < 0.0001f){
            dayCount++;
            countSecond = 0;
            gameHour = 0;
            gameSecond = 0;
        }

        //hour-second
        if (Math.Abs(gameSecond - 60f) < 0.0001f){
            gameHour++;
            gameSecond = 0;
        }

        UpdateProperties();
    }

    public void BuyBuilding(BuildingTypeSO type){
        this.money = this.money - type.price;
        this.totalCapacity = this.totalCapacity + type.capacity;
    }

    public bool UpgradeBuilding(Attraction building){
        if (this.money >= building.UpgradePrice){
            this.money = this.money - building.UpgradePrice;
            building.Upgrade();
            return true;
        }

        return false;
    }

    public void SellBuilding(Building building){
        this.money = this.money + building.SellPrice;
        EventManager.instance.SoldBuilding(building.SellPrice);
        this.totalCapacity = this.totalCapacity - building.Type.capacity;
    }

    /*public bool RepairBuilding(Attraction building){
        Mechanic helperMechanic = null;
        foreach (Mechanic mechanic in this.mechanics){
            if (mechanic.Occupied == false){
                helperMechanic = mechanic;
                break;
            }
        }

        if (helperMechanic != null){
            building.Repair(helperMechanic);
            return true;
        }

        return false;
    }*/

    public bool BuyJanitor(){
        if (this.money >= 150f){
            this.money = this.money - 150f;
            GameObject obj = spawner.SpawnJanitor(buildingSystem.entryPoint.position + new Vector3(1, 0, 1) * (buildingSystem.CellSize / 2));
            Janitor janitor = obj.GetComponent<Janitor>();
            janitors.Add(janitor);
            janitorCount++;
            return true;
        }

        return false;
    }

    public bool BuyMechanic(){
        if (this.money >= 300f){
            this.money = this.money - 300f;
            totalMechanics++;
            availableMechanics++;
            return true;
        }

        return false;
    }

    public bool RemoveJanitor(){
        if (this.janitors.Count > 0){
            janitors[0].Sell();
            janitors.Remove(janitors[0]);
            return true;
        }
        

        return false;
    }

    public bool RemoveMechanic(){
        if (availableMechanics > 0){
            totalMechanics--;
            availableMechanics--;
            return true;
        }
        return false;
    }

    public void NormalMode(){
        buildingSystem.SwitchMode(BuildingSystem.ClickMode.Normal);
    }

    public void SwitchMode(){
        if (buildingSystem.currentMode == BuildingSystem.ClickMode.Destroy){
            buildingSystem.SwitchMode(BuildingSystem.ClickMode.Normal);
        }
        else{
            buildingSystem.SwitchMode(BuildingSystem.ClickMode.Destroy);
        }
    }

    public void ResetSelectedBuilding(){
        buildingSystem.SetSelectedBuildingType(null);
    }

    private void UpdateProperties(){
        foreach (Building building in this.buildingSystem.Buildings){
            if (building.Type.type == BuildingTypeSO.Type.Attraction){
                Attraction current = (Attraction)building;
                this.money -= current.Upkeep;
                this.money += current.Income;

                float rand_float = Random.Range(0f, 1f);
                if (rand_float < current.BreakChance){
                    //building.Broke = true;
                    current.BreakBuilding();
                }    
            }
        }

        foreach (Janitor janitor in this.janitors){
            this.money -= janitor.Salary;
        }

        this.money -= (mechanicSalary * totalMechanics);
        /*foreach (Mechanic mechanic in this.mechanics){
            this.money -= mechanic.Salary;
        }*/

        //TODO visitorok számának számolása
        /*foreach (Visitor visitor in this.visitors){
            this.trashLevel += 0.2f / 24f / 60f;
        }*/

        if (this.trashLevel > this.totalCapacity){
            this.trashLevel = this.totalCapacity;
        }

        if (this.totalCapacity == 0){
            this.trashPercentage = 0;
        }
        else{
            this.trashPercentage = this.trashLevel / this.totalCapacity;
        }

        this.totalHappiness = 1f - this.trashPercentage;
    }

    private void UpdateWeather(){ }

    public void Resume(){
        //this.gameSpeed = 10f;
        TimeManager.instance.GameSpeed = 10;
        this.gameIsActive = true;
        TimeManager.instance.Paused = false;
        EventManager.instance.SpeedChanged(1);
    }

    public void Pause(){
        this.gameIsActive = false;
        TimeManager.instance.Paused = true;
        EventManager.instance.SpeedChanged(0);
    }

    public void ChangeSpeed(float number){
        gameIsActive = true;
        //this.gameSpeed = number * 10f;
        if (number > 0){
            TimeManager.instance.Paused = false;    
        }
        TimeManager.instance.GameSpeed = (int)(number * 10);
        EventManager.instance.SpeedChanged((int)number);
    }

    public bool ChangeSelectedType(BuildingTypeSO buildingTypeSO){
        if (this.money >= buildingTypeSO.price){
            buildingSystem.SetSelectedBuildingType(buildingTypeSO);
            return true;
        }

        return false;
    }

    public int Width => width;

    public int Height => height;

    public float TotalHappiness => totalHappiness;

    public float TrashPercentage => trashPercentage;

    public int DayCount => dayCount;

    public int GameHour => gameHour;

    public int GameSecond => gameSecond;

    public float Money => money;

    public List<Janitor> Janitors => janitors;

    //public List<Mechanic> Mechanics => mechanics;

    //public float CountSecond => countSecond;
}