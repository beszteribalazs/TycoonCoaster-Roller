using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InspectorMenu : MonoBehaviour
{
    public static InspectorMenu instance;

    Attraction selectedBuilding;

    [SerializeField] GameObject display;

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI level;
    [SerializeField] TextMeshProUGUI capacity;
    [SerializeField] TextMeshProUGUI upkeep;
    [SerializeField] TextMeshProUGUI income;
    [SerializeField] TextMeshProUGUI upgradePrice;
    [SerializeField] TextMeshProUGUI repairPrice;
    [SerializeField] TextMeshProUGUI netIncome;

    void Awake()
    {
        instance = this;
    }

    public void DisplayDetails(Attraction building)
    {
        selectedBuilding = building;
        display.gameObject.SetActive(true);
        nameText.text = building.Name;
        level.text = "Level: " + building.Level;
        capacity.text = "- Capacity: " + building.CurrentVisitors + "/" + building.TotalCapacity;
        upkeep.text = "- Daily Upkeep: " + Math.Round(building.DailyUpkeep, 0) + "$";
        income.text = "- Daily Income: " + Math.Round(building.DailyIncome, 0) + "$";
        netIncome.text = "- Net Income: " + Math.Round(building.DailyIncome - building.DailyUpkeep, 0) + "$";
        upgradePrice.text = Math.Round(building.UpgradePrice, 0) + "$";
        repairPrice.text = "nincs:c $";
    }

    public void UpgradeBuilding()
    {
        if (GameManager.instance.UpgradeBuilding(selectedBuilding))
        {
            //Refresh UI
            DisplayDetails(selectedBuilding);
        }
        else
        {
            ErrorHandler.instance.NoMoneyError();
        }
    }

    public void ReapirBuilding()
    {
        /*if (GameManager.instance.RepairBuilding()){
   
        }
        else
        {
            ErrorHandler.instance.NoMoneyError();
        }*/
    }


    public void CloseDisplay()
    {
        selectedBuilding = null;
        display.gameObject.SetActive(false);
    }
}