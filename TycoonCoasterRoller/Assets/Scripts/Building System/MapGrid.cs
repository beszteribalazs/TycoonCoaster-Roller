using System;
using UnityEngine;

public class MapGrid<TGridObject>{
    int width;
    int height;
    float cellSize;
    TGridObject[,] gridArray;
    TextMesh[,] debugTextArray;
    Vector3 originPosition;

    public MapGrid(int width, int height, float cellSize, Vector3 originPosition, Func<MapGrid<TGridObject>, int, int, TGridObject> createGridObject){
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];
        debugTextArray = new TextMesh[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++){
            for (int y = 0; y < gridArray.GetLength(1); y++){
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }

        for (int x = 0; x < gridArray.GetLength(0); x++){
            for (int y = 0; y < gridArray.GetLength(1); y++){
                debugTextArray[x, y] = UtilsClass.CreateWorldText((gridArray[x, y]?.ToString()), null,
                    GetWorldPosition(x, y) + new Vector3(cellSize / 2, cellSize / 2, 0), 10, Color.white, TextAnchor.MiddleCenter);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 1000f);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 1000f);
            }
        }

        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 1000f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 1000f);
    }

    Vector3 GetWorldPosition(int x, int y){
        return new Vector3(x, y) * cellSize + originPosition;
    }

    Vector2Int GetXY(Vector3 worldPosition){
        int x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        int y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
        return new Vector2Int(x, y);
    }

    public void SetGridObject(int x, int y, TGridObject value){
        if (x >= 0 && y >= 0 && x < width && y < height){
            gridArray[x, y] = value;
            debugTextArray[x, y].text = gridArray[x, y].ToString();
        }
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value){
        Vector2Int pos = GetXY(worldPosition);
        SetGridObject(pos.x, pos.y, value);
    }

    public TGridObject GetGridObject(int x, int y){
        if (x >= 0 && y >= 0 && x < width && y < height){
            return gridArray[x, y];
        }
        else{
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObject(Vector3 worldPosition){
        Vector2Int pos = GetXY(worldPosition);
        return gridArray[pos.x, pos.y];
    }
}