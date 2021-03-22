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
    
    public float UpgradePrice => buildingType.price * (float)level;

    public override float SellPrice => (buildingType.price * (float) level) * buildingType.sellMultiplier;
    public override float Upkeep => ((buildingType.price * (float) level)) * buildingType.upgradeMultiplier;
    public override float Income => buildingType.baseIncome * level;
    public override float BreakChance => buildingType.breakChance;

    public override bool Broke{
        get => _broke;
        set => _broke = value;
    }
}