using UnityEngine;
using UnityEngine.Serialization;

public class Road : Building
{
    [FormerlySerializedAs("RoadS")] public GameObject roadS;
    public GameObject roadL;
    public GameObject roadT;
    public GameObject roadX;

    public override float SellPrice => buildingType.price * 0.5f;
    public new Vector3 Position => visual.GetChild(0).position;
    public override float Upkeep => 0;
    public override float Income => 0;
    public override float BreakChance => 0;
}