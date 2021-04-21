using System;
using System.Collections.Generic;
using UnityEngine;

public class Attraction : Building{
    int level;
    [SerializeField] bool _broke = false;
    [SerializeField] Transform brokeVisual;
    public bool beingRepaired;

    public Attraction(){
        this.level = 1;
    }

    public List<Visitor> peopleInside = new List<Visitor>();


    public void Repair(Mechanic mechanic){
        Debug.Log("REPAIR NOT IMPLEMENTED :(");
    }

    public void Upgrade(){
        level++;
    }

    public float UpgradePrice => (float) Math.Round((buildingType.price / 2) * Mathf.Pow(level, 1.2f), 2);

    public float Value{
        get{
            float totalPrice = buildingType.price;
            for (int i = 0; i < level; i++){
                totalPrice += (float) Math.Round((buildingType.price / 2) * Mathf.Pow(i, 1.2f), 2);
            }

            return totalPrice;
        }
    }

    public override float SellPrice => Value / 2;

    public int RepairTickDuration => gridPositionlist.Count * 15;

    public override float Upkeep => DailyUpkeep / 24f / 60f;
    public float DailyUpkeep => Mathf.Pow(DailyIncome, 0.75f);
    public override float Income => _broke ? 0 : DailyIncome / 24f / 60f * ((float) peopleInside.Count / (float) TotalCapacity);
    public float DailyIncome => level * buildingType.baseIncome;

    public float CurrentDailyIncome => Income * 24f * 60f;
    public override float BreakChance => buildingType.breakChance / 24f / 60f;

    public int TotalCapacity => _broke ? 0 : buildingType.capacity;
    public int CurrentVisitorCount => peopleInside.Count;

    public bool Broke => _broke;

    public int Level => level;


    public void BreakBuilding(){
        //EventManager.instance.MapChanged();
        _broke = true;
        brokeVisual.gameObject.SetActive(true);

        //visitorok kiküldése
        SendOutVisitors();
        EventManager.instance.MapChanged();
    }

    public void SendOutVisitors(){
        //Debug.Log(peopleInside.Count);
        while (peopleInside.Count > 0){
            peopleInside[0].LeaveBuilding();
        }
        //Debug.Log(peopleInside.Count);
    }

    public void EjectVisitors(){
        while (peopleInside.Count > 0){
            peopleInside[0].EjectBuilding();
        }
    }

    public void RepairBuilding(){
        _broke = false;
        brokeVisual.gameObject.SetActive(false);
        beingRepaired = false;
        EventManager.instance.MapChanged();
    }
}