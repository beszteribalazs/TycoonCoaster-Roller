using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour{
    MapGrid<int> grid;
    
    void Start(){
        grid = new MapGrid<int>(10, 10, 3f, new Vector3(0f,0f,0f), (MapGrid<int> g, int x, int y) => new int());
    }

    void Update(){
        if (Input.GetMouseButtonDown(0)){
            grid.SetGridObject(UtilsClass.GetMouseWorldPosition(), 1);
        }
        if (Input.GetMouseButtonDown(1)){
            Debug.Log(grid.GetGridObject(UtilsClass.GetMouseWorldPosition()));
        }
    }
}