using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectorMenu : MonoBehaviour{
    public static InspectorMenu instance;

    Attraction selectedBuilding;

    [SerializeField] GameObject display;
    [SerializeField] GameObject buyMenu;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI level;
    [SerializeField] TextMeshProUGUI capacity;
    [SerializeField] TextMeshProUGUI upkeep;
    [SerializeField] TextMeshProUGUI income;
    [SerializeField] TextMeshProUGUI upgradePrice;
    [SerializeField] TextMeshProUGUI repairPrice;
    [SerializeField] TextMeshProUGUI netIncome;
    [SerializeField] Transform previewModel;
    [SerializeField] Button repairButton;
    Transform previewModelObject;

    bool inspectorOpen = false;

    void Awake(){
        instance = this;
    }

    void Update(){
        if (Input.GetKeyUp(KeyCode.Escape)){
            CloseDisplay();
        }

        if (inspectorOpen){
            DisplayDetails(selectedBuilding);
        }
    }

    public void DisplayDetails(Attraction building){
        buyMenu.SetActive(false);
        BuySelect.instance.SetCheck();
        selectedBuilding = building;
        display.gameObject.SetActive(true);
        nameText.text = building.Name;
        level.text = "Level: " + building.Level;
        capacity.text = "Capacity: " + building.CurrentVisitorCount + "/" + building.TotalCapacity;
        upkeep.text = "Upkeep: -" + Math.Round(building.DailyUpkeep, 0) + "$";
        income.text = "Income: " + Math.Round(building.CurrentDailyIncome, 0) + "$";
        netIncome.text = "Net Income: " + Math.Round(building.CurrentDailyIncome - building.DailyUpkeep, 0) + "$";
        upgradePrice.text = Math.Round(building.UpgradePrice, 0) + "$";
        repairPrice.text = "9999$";
        if (previewModelObject != null){
            Destroy(previewModelObject.gameObject);
        }

        previewModelObject = Instantiate(building.Type.uiPrefab, previewModel);
        //GameManager.instance.Pause();


        if (selectedBuilding.Broke){
            repairButton.interactable = true;
        }
        else{
            repairButton.interactable = false;
        }

        if (selectedBuilding.beingRepaired){
            repairButton.interactable = false;
        }

        inspectorOpen = true;
    }

    public void UpgradeBuilding(){
        if (GameManager.instance.UpgradeBuilding(selectedBuilding)){
            //Refresh UI
            DisplayDetails(selectedBuilding);
        }
        else{
            ErrorHandler.instance.NoMoneyError();
        }
    }

    public void ReapirBuilding(){
        selectedBuilding.beingRepaired = true;
        GameManager.instance.RepairAttraction(selectedBuilding);
        CloseDisplay();
    }

    public void CloseDisplay(){
        selectedBuilding = null;
        display.gameObject.SetActive(false);
        //GameManager.instance.Resume();
        inspectorOpen = false;
    }
}