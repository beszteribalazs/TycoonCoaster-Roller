using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell{
    GridXZ grid;
    int x;
    int y;
    Building building;

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