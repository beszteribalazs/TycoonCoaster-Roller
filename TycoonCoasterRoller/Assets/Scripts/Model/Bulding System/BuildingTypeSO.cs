using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BuildingTypeSO : ScriptableObject{
    public enum Direction{
        Up,
        Right,
        Down,
        Left
    }

    public static Direction GetNextDirectionRight(Direction dir){
        switch (dir){
            default:
            case Direction.Up: return Direction.Right;
            case Direction.Right: return Direction.Down;
            case Direction.Down: return Direction.Left;
            case Direction.Left: return Direction.Up;
        }
    }

    public static Direction GetNextDirectionLeft(Direction dir){
        switch (dir){
            default:
            case Direction.Up: return Direction.Left;
            case Direction.Right: return Direction.Up;
            case Direction.Down: return Direction.Right;
            case Direction.Left: return Direction.Down;
        }
    }


    public string buildingName;
    [Header("Game setup")] public float price;
    public float sellMultiplier = 0.5f;
    [Tooltip("Income at level 1")] public float baseIncome;
    [Range(0f,1f)]
    public float breakChance;

    [Header("Map setup")] public int width;
    public int height;
    public Transform prefab;
    public Transform preview;


    // Calculates new offset on the grid based on rotation
    public Vector2Int GetRotationOffset(Direction dir){
        switch (dir){
            default:
            case Direction.Up: return new Vector2Int(0, 0);
            case Direction.Right: return new Vector2Int(0, width);
            case Direction.Down: return new Vector2Int(width, height);
            case Direction.Left: return new Vector2Int(height, 0);
        }
    }

    public int GetRotationAngle(Direction dir){
        switch (dir){
            default:
            case Direction.Up: return 0;
            case Direction.Right: return 90;
            case Direction.Down: return 180;
            case Direction.Left: return 270;
        }
    }

    // Calculates position on the grid based on offset and rotation direction
    public List<Vector2Int> GetPositionList(Vector2Int offset, Direction dir){
        List<Vector2Int> gridPositionList = new List<Vector2Int>();
        switch (dir){
            default:
            case Direction.Down:
            case Direction.Up:
                for (int x = 0; x < width; x++){
                    for (int y = 0; y < height; y++){
                        gridPositionList.Add(offset + new Vector2Int(x, y));
                    }
                }

                break;
            case Direction.Left:
            case Direction.Right:
                for (int x = 0; x < height; x++){
                    for (int y = 0; y < width; y++){
                        gridPositionList.Add(offset + new Vector2Int(x, y));
                    }
                }

                break;
        }

        return gridPositionList;
    }
}