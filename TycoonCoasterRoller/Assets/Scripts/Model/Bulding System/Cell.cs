using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell{
    GridXZ grid;
    int x;
    int y;
    Building building;

    public string PositionString => "x: " + x + " y: " + y;

    public List<Cell> Neighbours{
        get{
            List<Cell> list = new List<Cell>();

            if (grid.GetCell(x + 1, y) != null){
                list.Add(grid.GetCell(x + 1, y));
            }

            if (grid.GetCell(x - 1, y) != null){
                list.Add(grid.GetCell(x - 1, y));
            }

            if (grid.GetCell(x, y + 1) != null){
                list.Add(grid.GetCell(x, y + 1));
            }

            if (grid.GetCell(x, y - 1) != null){
                list.Add(grid.GetCell(x, y - 1));
            }

            return list;
        }
    }

    public Cell(GridXZ grid, int x, int y){
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public void SetBuilding(Building newBuilding){
        this.building = newBuilding;
    }

    public void ClearBuilding(){
        building = null;
    }

    public bool IsEmpty(){
        return building == null;
    }

    public Building GetBuilding(){
        return building;
    }
}