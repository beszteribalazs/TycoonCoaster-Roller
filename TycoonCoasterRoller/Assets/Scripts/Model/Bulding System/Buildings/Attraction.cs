using System;
using UnityEngine;

public class Attraction : Building{
    int level;
    [SerializeField] bool _broke;

    public Attraction(){
        this.level = 1;
    }

    public bool HasRoad{
        get{
            //For every building cell
            bool roadFound = false;
            foreach (Vector2Int cellPos in gridPositionlist){
                //Check if neighbour cell is road
                GridXZ g = BuildingSystem.instance.grid;

                if (g.GetCell(cellPos.x + 1, cellPos.y).GetBuilding() != null && g.GetCell(cellPos.x + 1, cellPos.y).GetBuilding().Type.type == BuildingTypeSO.Type.Road){
                    roadFound = true;
                    break;
                }

                if (g.GetCell(cellPos.x - 1, cellPos.y).GetBuilding() != null && g.GetCell(cellPos.x - 1, cellPos.y).GetBuilding().Type.type == BuildingTypeSO.Type.Road){
                    roadFound = true;
                    break;
                }

                if (g.GetCell(cellPos.x, cellPos.y + 1).GetBuilding() != null && g.GetCell(cellPos.x, cellPos.y + 1).GetBuilding().Type.type == BuildingTypeSO.Type.Road){
                    roadFound = true;
                    break;
                }

                if (g.GetCell(cellPos.x, cellPos.y - 1).GetBuilding() != null && g.GetCell(cellPos.x, cellPos.y - 1).GetBuilding().Type.type == BuildingTypeSO.Type.Road){
                    roadFound = true;
                    break;
                }

                //Debug.Log(cellPos + "neighbour: " + BuildingSystem.instance.grid.GetCell(cellPos.x + 1, cellPos.y).GetBuilding());
            }

            return roadFound;
        }
    }

    public void Repair(Mechanic mechanic){
        Debug.Log("REPAIR NOT IMPLEMENTED :(");
    }

    public void Upgrade(){
        level++;
    }

    public float UpgradePrice => (float) Math.Round((buildingType.price / 2) * Mathf.Pow(level, 1.2f), 2);

    public override float SellPrice{
        get{
            float moneyAmount = buildingType.price;
            for (int i = 0; i < level; i++){
                moneyAmount += (float) Math.Round((buildingType.price / 2) * Mathf.Pow(i, 1.2f), 2);
            }

            moneyAmount *= 0.5f;
            return moneyAmount;
        }
    }

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