using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour{
    [SerializeField] public BuildingSystem buildingSystem;
    public static GameManager instance;
    private int width;
    private int height;
    private const int REPAIRTIME = 60;
    private float money;
    private float totalHappiness;
    private float trashLevel;
    private float trashPercentage;
    private float totalCapacity;
    private float gameSpeed;
    private int dayCount;
    private int gameHour;
    private int gameSecond;
    private float countSecond;
    private float helpSecond;
    private bool gameIsActive;
    private List<Janitor> janitors;
    private List<Mechanic> mechanics;
    private List<Visitor> visitors;

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

    public List<Attraction> ReachableAttractions{
        get{
            List<Attraction> reachable = new List<Attraction>();

            GridXZ grid = buildingSystem.grid;

            // Find first cell from spawn
            int x;
            int z;
            grid.XZFromWorldPosition(buildingSystem.entryPoint.position + Vector3.forward * buildingSystem.CellSize, out x, out z);
            //Debug.Log("x: " + x + " z: " + z);

            if (grid.GetCell(x, z).GetBuilding() == null){
                return reachable;
            }
                
                
            if (grid.GetCell(x, z).GetBuilding().Type.type == BuildingTypeSO.Type.Attraction){
                // only first building is reachable
                reachable.Add((Attraction) grid.GetCell(x, z).GetBuilding());
            }
            else if (grid.GetCell(x, z).GetBuilding().Type.type == BuildingTypeSO.Type.Decoration){
                // nothing reachable
            }
            else if (grid.GetCell(x, z).GetBuilding().Type.type == BuildingTypeSO.Type.Road){
                List<Cell> visited = new List<Cell>();
                Stack<Cell> path = new Stack<Cell>();
                Cell currentCell = grid.GetCell(x, z);

                bool finished = false;
                while (!finished){
                    //Debug.Log("CurrentCell: " + currentCell.PositionString + " " + currentCell.GetBuilding());
                    
                    // add current to visited
                    if (!visited.Contains(currentCell)){
                        visited.Add(currentCell);
                    }
                    //if current cell is attraction, add to reachable and go back
                    if (currentCell.GetBuilding().Type.type == BuildingTypeSO.Type.Attraction){
                        reachable.Add((Attraction)currentCell.GetBuilding());
                        currentCell = path.Pop();
                    } // else search
                    else if (currentCell.GetBuilding().Type.type == BuildingTypeSO.Type.Road){
                        // choose random direction
                        Cell direction = null;
                        foreach (Cell neighbour in currentCell.Neighbours){
                            // check if neighbour cell has building
                            if (neighbour.GetBuilding() != null){
                                // check if already visited
                                if (!visited.Contains(neighbour)){
                                    // not visited neighbour exists
                                    direction = neighbour;
                                    break;
                                }
                            }
                        }
                        // if exists
                        if (direction != null){
                            // add current to path
                            // go there
                            path.Push(currentCell);
                            currentCell = direction;
                        } //else
                        else{
                            // if path.count > 0
                            if (path.Count > 0){
                                // go back
                                currentCell = path.Pop();
                            }
                            else{
                                finished = true;
                            }
                        }
                    }else if (currentCell.GetBuilding().Type.type == BuildingTypeSO.Type.Decoration){
                        currentCell = path.Pop();
                    }
                }
            }
            return reachable;
        }
    }

    private void Awake(){
        instance = this;
    }

    void Start(){
        this.width = MapSizeController.mapSize;
        this.height = MapSizeController.mapSize;
        this.money = 100000f;
        this.totalHappiness = 1f;
        this.trashLevel = 0f;
        this.trashPercentage = 0f;
        this.gameSpeed = 10f;
        this.dayCount = 0;
        this.gameIsActive = true;
        this.janitors = new List<Janitor>();
        this.mechanics = new List<Mechanic>();
        this.visitors = new List<Visitor>();
    }

    void Update(){
        if (gameIsActive){
            helpSecond = helpSecond + Time.deltaTime;
            if (helpSecond >= (1 / gameSpeed)){
                GameLoop();
                helpSecond = 0;
            }
        }
    }

    private void GameLoop(){
        this.countSecond++;
        this.gameSecond++;

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
    }

    public bool RepairBuilding(Attraction building){
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
    }

    public bool BuyJanitor(){
        if (this.money >= 150f){
            this.money = this.money - 150f;
            this.janitors.Add(new Janitor());
            return true;
        }

        return false;
    }

    public bool BuyMechanic(){
        if (this.money >= 300f){
            this.money = this.money - 300f;
            this.mechanics.Add(new Mechanic());
            return true;
        }

        return false;
    }

    public bool RemoveJanitor(){
        if (this.janitors.Count > 0){
            this.janitors.RemoveAt(this.janitors.Count - 1);
            return true;
        }

        return false;
    }

    public bool RemoveMechanic(){
        if (this.mechanics.Count > 0){
            Mechanic helperMechanic = null;
            foreach (Mechanic mechanic in this.mechanics){
                if (mechanic.Occupied == false){
                    helperMechanic = mechanic;
                    break;
                }
            }

            if (helperMechanic != null){
                this.mechanics.Remove(helperMechanic);
                return true;
            }
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
            this.money -= building.Upkeep;
            this.money += building.Income;

            float rand_float = Random.Range(0f, 1f);
            if (rand_float < building.BreakChance){
                building.Broke = true;
            }
        }

        foreach (Janitor janitor in this.janitors){
            this.money -= janitor.Salary;
        }

        foreach (Mechanic mechanic in this.mechanics){
            this.money -= mechanic.Salary;
        }

        foreach (Visitor visitor in this.visitors){
            this.trashLevel += 0.2f / 24f / 60f;
        }

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
        this.gameSpeed = 10f;
        this.gameIsActive = true;
    }

    public void Pause(){
        this.gameIsActive = false;
    }

    public void ChangeSpeed(float number){
        gameIsActive = true;
        this.gameSpeed = number * 10f;
    }

    public bool ChangeSelectedType(BuildingTypeSO buildingTypeSO){
        if (this.money >= buildingTypeSO.price){
            buildingSystem.SetSelectedBuildingType(buildingTypeSO);
            return true;
        }

        return false;
    }

    public int Width{
        get => width;
    }

    public int Height{
        get => height;
    }

    public float TotalHappiness{
        get => totalHappiness;
    }

    public float TrashPercentage{
        get => trashPercentage;
    }

    public int DayCount{
        get => dayCount;
    }

    public int GameHour{
        get => gameHour;
    }

    public int GameSecond{
        get => gameSecond;
    }

    public float Money{
        get => money;
    }

    public List<Janitor> Janitors{
        get => janitors;
    }

    public List<Mechanic> Mechanics{
        get => mechanics;
    }

    public float CountSecond{
        get => countSecond;
    }
}