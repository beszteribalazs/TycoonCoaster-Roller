using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridXZ{
    int width;
    int height;
    float cellSize;
    Cell[,] gridArray;
    Vector3 originPosition;

    public GridXZ(int width, int height, float cellSize, Vector3 originPosition){
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;
        gridArray = new Cell[width,height];
        
        // Fill the grid with empty Cells
        for (int x = 0; x < gridArray.GetLength(0); x++){
            for (int z = 0; z < gridArray.GetLength(1); z++){
                gridArray[x, z] = new Cell(this, x, z);
            }
        }
        
        // Debud: draw grid
        for (int x = 0; x < gridArray.GetLength(0); x++){
            for (int z = 0; z < gridArray.GetLength(1); z++){
                //debugTextArray[x, z] = UtilsClass.CreateWorldText((gridArray[x, z]?.ToString()), null,
                //GetWorldPosition(x, z) + new Vector3(cellSize / 2, cellSize / 2, 0), 10, Color.white, TextAnchor.MiddleCenter);
                Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z + 1), Color.white, 1000f);
                Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x + 1, z), Color.white, 1000f);
            }
        }

        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 1000f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 1000f);
    }

    public List<Building> GetBuildingList(){
        List<Building> buildings = new List<Building>();

        for (int x = 0; x < gridArray.GetLength(0); x++){
            for (int y = 0; y < gridArray.GetLength(1); y++){
                if (gridArray[x, y].GetBuilding() != null){
                    buildings.Add(gridArray[x,y].GetBuilding());
                }
            }
        }
        
        return buildings;
    }

    public float GetCellSize(){
        return cellSize;
    }

    public Vector3 GetWorldPosition(int x, int z){
        return new Vector3(x, 0, z) * cellSize + originPosition;
    }

    public Cell GetCell(int x, int z){
        if (x >= 0 && z >= 0 && x < width && z < height){
            return gridArray[x, z];
        }
        else{
            //throw new NotValidCellException("(" + x + "," + z + ") cell does not exist :(");
            return null;
        }
    }

    public Cell GetCell(Vector3 worldPosition){
        int x, z;
        XZFromWorldPosition(worldPosition, out x, out z);
        if (x < 0 || x >= width || z < 0 || z >= height){
            return null;
        }
        else{
            return gridArray[x, z];    
        }
    }
    
    // Converts word coordinate to grid position
    public void XZFromWorldPosition(Vector3 worldPosition, out int x, out int z){
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        z = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize);
    }
}
