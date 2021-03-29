﻿public class Road : Building{
    public override float SellPrice => buildingType.price * 0.5f;
    public override float Upkeep => 0;
    public override float Income => 0;
    public override float BreakChance => 0;
    public override bool Broke{
        get => false;
        set{ }
    }
}