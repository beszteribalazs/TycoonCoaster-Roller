using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public BuildingSystem buildingSystem;
    public static GameManager instance;
    private int width = 50;
    private int height = 50;
    private const float REFUNDMULTIPLIER = 0.25f;
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

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        this.money = 1000f;
        this.totalHappiness = 1f;
        this.trashLevel = 0f;
        this.trashPercentage = 0f;
        this.gameSpeed = 100f;
        this.dayCount = 0;
        this.gameIsActive = true;
    }

    void Update()
    {
        if (gameIsActive)
        {
            helpSecond = helpSecond + Time.deltaTime;
            if (helpSecond >= (1 / gameSpeed))
            {
                GameLoop();
                helpSecond = 0;
            }
        }
    }

    private void GameLoop()
    {
        Debug.Log("Days: " + dayCount + "           Hours: " + gameHour + "           Minute: " + gameSecond);
        this.countSecond++;
        this.gameSecond++;

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



    private void UpdateProperties()
    {
        
    }

    public void Resume()
    {
        this.gameSpeed = 1f;
        this.gameIsActive = true;
    }

    public void Pause()
    {
        this.gameIsActive = false;
    }

    public void ChangeSpeed(float number)
    {
        this.gameSpeed = number;
    }

    public void ChangeSelectedType(BuildingTypeSO buildingTypeSO)
    {
        buildingSystem.SetSelectedBuildingType(buildingTypeSO);
    }

    public int Width
    {
        get => width;
    }

    public int Height
    {
        get => height;
    }

    public float TotalHappiness
    {
        get => totalHappiness;
    }

    public float TrashPercentage
    {
        get => trashPercentage;
    }

    public int DayCount
    {
        get => dayCount;
    }

    public int GameHour
    {
        get => gameHour;
    }

    public int GameSecond
    {
        get => gameSecond;
    }

    public float Money
    {
        get => money;
    }
}
