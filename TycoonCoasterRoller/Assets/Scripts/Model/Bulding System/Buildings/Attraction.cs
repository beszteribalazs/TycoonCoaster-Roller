using System;
using UnityEngine;

public class Attraction : Building{
    int level;
    [SerializeField] bool _broke;

    public Attraction(){
        this.level = 1;
    }

    public void Repair(Mechanic mechanic){
        Debug.Log("REPAIR NOT IMPLEMENTED :(");
    }

    public void Upgrade(){
        level++;
    }
    
    public float UpgradePrice => (float)Math.Round((buildingType.price/2) * Mathf.Pow(level, 1.2f), 2);

    public override float SellPrice => (buildingType.price * (float) level) * buildingType.sellMultiplier;
    public override float Upkeep => DailyUpkeep / 24f / 60f;
    public float DailyUpkeep => Mathf.Pow(DailyIncome, 0.75f);
    public override float Income => DailyIncome / 24f / 60f;
    public float DailyIncome => level * buildingType.baseIncome;
    public override float BreakChance => buildingType.breakChance;

    public int TotalCapacity => buildingType.capacity;
    public int CurrentVisitors => TotalCapacity;
    
    public int Level => level;


    public override bool Broke{
        get => _broke;
        set => _broke = value;
    }
}