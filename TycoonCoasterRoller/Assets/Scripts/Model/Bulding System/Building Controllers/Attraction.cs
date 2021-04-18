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

    /*public bool HasRoad{
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
    }*/

    public List<Visitor> peopleInside = new List<Visitor>();
    
    
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
    public override float Income => _broke? 0 : DailyIncome / 24f / 60f * ((float)peopleInside.Count / (float)TotalCapacity);
    public float DailyIncome => level * buildingType.baseIncome;

    public float CurrentDailyIncome => Income * 24f * 60f;
    public override float BreakChance => buildingType.breakChance / 24f / 60f;

    public int TotalCapacity => _broke ? 0 : buildingType.capacity;
    public int CurrentVisitorCount => peopleInside.Count;

    public bool Broke => _broke;

    public int Level => level;


    public void BreakBuilding(){
        _broke = true;
        brokeVisual.gameObject.SetActive(true);
        //visitorok kiküldése
        while (peopleInside.Count > 0){
            peopleInside[0].LeaveBuilding();
        }
        
    }

    public void RepairBuilding(){
        _broke = false;
        brokeVisual.gameObject.SetActive(false);
        beingRepaired = false;
    }
}