using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public BuildingSystem buildingSystem;
    [SerializeField] Spawner spawner;
    public static GameManager instance;
    private int width;
    private int height;
    private float money;
    private float totalHappiness;
    private float trashLevel;
    private float trashPercentage;
    private float currentVisitors;
    private int dayCount;
    private int gameHour;
    private int gameSecond;
    private bool gameIsActive;
    private float beforeSpeed;
    private List<Janitor> janitors;
    private float mechanicSalary;
    public int totalMechanics = 0;
    public int availableMechanics = 0;

    public int storedJanitors = 0;

    void Update()
    {
        if (storedJanitors > 0)
        {
            spawner.SpawnJanitor(buildingSystem.entryPoint.position +
                                 new Vector3(1, 0, 1) * (buildingSystem.CellSize / 2));
            storedJanitors--;
        }
    }

    private void Awake()
    {
        instance = this;
        mechanicSalary = 300 * 0.1f / 24 / 60;
    }

    void Start()
    {
        this.width = MapSizeController.mapSize;
        this.height = MapSizeController.mapSize;
        this.money = 100000f;
        this.totalHappiness = 1f;
        this.trashLevel = 0f;
        this.trashPercentage = 0f;
        this.dayCount = 0;
        this.gameIsActive = true;
        this.janitors = new List<Janitor>();
        this.beforeSpeed = 10;
        TimeManager.instance.GameSpeed = 10;
        EventManager.instance.SpeedChanged(1);
    }

    public void RepairAttraction(Attraction target)
    {
        if ((target.Value * 0.1f) <= this.money)
        {
            if (availableMechanics > 0)
            {
                if (NavigationManager.instance.reachableAttractions.Contains(target))
                {
                    GameObject obj = spawner.SpawnMechanic(buildingSystem.entryPoint.position +
                                                           new Vector3(1, 0, 1) * (buildingSystem.CellSize / 2));
                    Mechanic mechanic = obj.GetComponent<Mechanic>();
                    mechanic.Repair(target);
                    target.beingRepaired = true;
                    availableMechanics--;
                }
                else
                {
                    EventManager.instance.NoPathToBuilding();
                }
            }
        }
        else
        {
            EventManager.instance.NoMoney();
        }
    }

    public void GameLoop()
    {
        int countSecond = TimeManager.instance.Tick;
        gameSecond++;

        //evening-daytime
        if (countSecond >= 0 && countSecond < 720)
        {
        }
        else if (countSecond >= 720 && countSecond < 1440)
        {
        }
        else if (Math.Abs(countSecond - 1440f) < 0.0001f)
        {
            dayCount++;
            countSecond = 0;
            gameHour = 0;
            gameSecond = 0;
        }

        //hour-second
        if (Math.Abs(gameSecond - 60f) < 0.0001f)
        {
            gameHour++;
            gameSecond = 0;
        }

        UpdateProperties();
    }

    public void BuyBuilding(BuildingTypeSO type)
    {
        this.money = this.money - type.price;
    }

    public bool UpgradeBuilding(Attraction building)
    {
        if (this.money >= building.UpgradePrice)
        {
            this.money = this.money - building.UpgradePrice;
            building.Upgrade();
            return true;
        }

        return false;
    }

    public void SellBuilding(Building building)
    {
        this.money = this.money + building.SellPrice;
        EventManager.instance.SoldBuilding(building.SellPrice);
    }

    public bool BuyJanitor()
    {
        if (this.money >= 150f)
        {
            this.money = this.money - 150f;
            GameObject obj = spawner.SpawnJanitor(buildingSystem.entryPoint.position +
                                                  new Vector3(1, 0, 1) * (buildingSystem.CellSize / 2));
            Janitor janitor = obj.GetComponent<Janitor>();
            janitors.Add(janitor);
            return true;
        }

        return false;
    }

    public bool BuyMechanic()
    {
        if (this.money >= 300f)
        {
            this.money = this.money - 300f;
            totalMechanics++;
            availableMechanics++;
            return true;
        }

        return false;
    }

    public bool RemoveJanitor()
    {
        if (this.janitors.Count > 0)
        {
            janitors[0].Sell();
            janitors.Remove(janitors[0]);
            return true;
        }

        return false;
    }

    public bool RemoveMechanic()
    {
        if (availableMechanics > 0)
        {
            totalMechanics--;
            availableMechanics--;
            return true;
        }

        return false;
    }

    public void NormalMode()
    {
        buildingSystem.SwitchMode(BuildingSystem.ClickMode.Normal);
        ChangeSpeed(beforeSpeed / 10);
    }

    public void SwitchMode()
    {
        if (buildingSystem.currentMode == BuildingSystem.ClickMode.Destroy)
        {
            NormalMode();
        }
        else if (buildingSystem.currentMode == BuildingSystem.ClickMode.Normal)
        {
            buildingSystem.SwitchMode(BuildingSystem.ClickMode.Destroy);
            beforeSpeed = TimeManager.instance.GameSpeed;
            this.gameIsActive = false;
            TimeManager.instance.Paused = true;
            EventManager.instance.SpeedChanged(0);
        }
        else if (buildingSystem.currentMode == BuildingSystem.ClickMode.Road)
        {
            buildingSystem.SwitchMode(BuildingSystem.ClickMode.Destroy);
            beforeSpeed = TimeManager.instance.GameSpeed;
            this.gameIsActive = false;
            TimeManager.instance.Paused = true;
            EventManager.instance.SpeedChanged(0);
        }
    }

    public void SwitchRoadMode()
    {
        if (buildingSystem.currentMode == BuildingSystem.ClickMode.Road)
        {
            NormalMode();
            ResetSelectedBuilding();
        }
        else
        {
            buildingSystem.SwitchMode(BuildingSystem.ClickMode.Road);
            ChangeSpeed(beforeSpeed / 10);
        }
    }

    public void ResetSelectedBuilding()
    {
        buildingSystem.SetSelectedBuildingType(null);
    }

    private void UpdateProperties()
    {
        foreach (Building building in this.buildingSystem.Buildings)
        {
            if (building.Type.type == BuildingTypeSO.Type.Attraction)
            {
                Attraction current = (Attraction) building;
                this.money -= current.Upkeep;
                this.money += current.Income;

                float rand_float = Random.Range(0f, 1f);
                if (rand_float < current.BreakChance)
                {
                    current.BreakBuilding();
                }
            }
        }


        this.money -= (mechanicSalary * totalMechanics);

        //Calculate trash level

        trashLevel += currentVisitors * 0.2f / 24f / 60f;

        foreach (Janitor janitor in this.janitors)
        {
            this.money -= janitor.Salary;
            trashLevel -= 0.2f / 24f / 60f * 15;
        }


        if (this.trashLevel > this.TotalCapacity)
        {
            this.trashLevel = this.TotalCapacity;
        }

        if (trashLevel < 0)
        {
            trashLevel = 0;
        }

        if (this.TotalCapacity == 0)
        {
            this.trashPercentage = 0;
        }
        else
        {
            this.trashPercentage = this.trashLevel / this.TotalCapacity;
        }

        this.totalHappiness = 1f - this.trashPercentage;
    }
    
    public void Resume()
    {
        TimeManager.instance.GameSpeed = 10;
        this.gameIsActive = true;
        TimeManager.instance.Paused = false;
        EventManager.instance.SpeedChanged(1);
        beforeSpeed = TimeManager.instance.GameSpeed;
    }

    public void Pause()
    {
        this.gameIsActive = false;
        TimeManager.instance.Paused = true;
        EventManager.instance.SpeedChanged(0);
        beforeSpeed = TimeManager.instance.GameSpeed;
    }

    public void ChangeSpeed(float number)
    {
        gameIsActive = true;
        if (number > 0)
        {
            TimeManager.instance.Paused = false;
        }

        TimeManager.instance.GameSpeed = (int) (number * 10);
        EventManager.instance.SpeedChanged((int) number);
        beforeSpeed = TimeManager.instance.GameSpeed;
    }

    public bool ChangeSelectedType(BuildingTypeSO buildingTypeSO)
    {
        if (this.money >= buildingTypeSO.price)
        {
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

    public float Money
    {
        get => money;
        set => money = value;
    }

    public float TotalCapacity => NavigationManager.instance.reachableCapacity;

    public float CurrentVisitors
    {
        get => currentVisitors;
        set => currentVisitors = value;
    }

    public List<Janitor> Janitors => janitors;

    public float BeforeSpeed => beforeSpeed;
}