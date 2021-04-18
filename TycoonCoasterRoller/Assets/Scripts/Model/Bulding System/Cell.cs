using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    GridXZ grid;
    int x;
    int y;
    Building building;

    public string PositionString => "x: " + x + " y: " + y;
    public Vector3 WorldPosition => new Vector3(x * grid.GetCellSize() + grid.GetCellSize() / 2, 0, y * grid.GetCellSize() + grid.GetCellSize() / 2);

    public int AdjacentRoads
    {
        get
        {
            Dictionary<string, bool> roads = new Dictionary<string, bool>();
            roads.Add("up", false);
            roads.Add("right", false);
            roads.Add("down", false);
            roads.Add("left", false);

            if (grid.GetCell(x + 1, y) != null && grid.GetCell(x + 1, y).GetBuilding() != null)
            {
                if (grid.GetCell(x + 1, y).GetBuilding().Type.type == BuildingTypeSO.Type.Road)
                {
                    roads["right"] = true;
                }
            }

            if (grid.GetCell(x - 1, y) != null && grid.GetCell(x - 1, y).GetBuilding() != null)
            {
                if (grid.GetCell(x - 1, y).GetBuilding().Type.type == BuildingTypeSO.Type.Road)
                {
                    roads["left"] = true;
                }
            }

            if (grid.GetCell(x, y + 1) != null && grid.GetCell(x, y + 1).GetBuilding() != null)
            {
                if (grid.GetCell(x, y + 1).GetBuilding().Type.type == BuildingTypeSO.Type.Road)
                {
                    roads["up"] = true;
                }
            }

            if (grid.GetCell(x, y - 1) != null && grid.GetCell(x, y - 1).GetBuilding() != null)
            {
                if (grid.GetCell(x, y - 1).GetBuilding().Type.type == BuildingTypeSO.Type.Road)
                {
                    roads["down"] = true;
                }
            }

            int startX;
            int startZ;
            grid.XZFromWorldPosition(
                BuildingSystem.instance.entryPoint.position + Vector3.forward * BuildingSystem.instance.CellSize,
                out startX, out startZ);

            if (this.x==startX && this.y==startZ)
            {
                roads["down"] = true;
                if (roads["up"] == false && roads["down"] && roads["left"] == false && roads["right"] == false)//le
                {
                    return 16;
                }
                else if (roads["up"] && roads["down"] && roads["left"] && roads["right"]) //fel,le,jobbra,balra
                {
                    return 17;
                }
                else if (roads["up"] == false && roads["down"] && roads["left"] == false && roads["right"]) //le,jobbra
                {
                    return 18;
                }
                else if (roads["up"] == false && roads["down"] && roads["left"] && roads["right"] == false) //le,balra
                {
                    return 19;
                }
                else if (roads["up"] && roads["down"] && roads["left"] == false && roads["right"]) //fel,le,jobbra
                {
                    return 20;
                }
                else if (roads["up"] && roads["down"] && roads["left"] && roads["right"] == false) //fel,le,balra
                {
                    return 21;
                }
                else if (roads["up"] == false && roads["down"] && roads["left"] && roads["right"]) //balra,jobbra
                {
                    return 22;
                }
                if (roads["up"] && roads["down"] && roads["left"] == false && roads["right"] == false)//fel,le
                {
                    return 23;
                }
            }
            else
            {
                if (roads["up"] && roads["down"] == false && roads["left"] == false && roads["right"] == false) //fel
                {
                    return 1;
                }
                else if (roads["up"] == false && roads["down"] && roads["left"] == false && roads["right"] == false) //le
                {
                    return 3;
                }
                else if (roads["up"] == false && roads["down"] == false && roads["left"] && roads["right"] == false) //balra
                { 
                    return 4;
                }
                else if (roads["up"] == false && roads["down"] == false && roads["left"] == false && roads["right"]) //jobbra
                {
                    return 2;
                }
                else if (roads["up"] && roads["down"] && roads["left"] == false && roads["right"] == false) //fel,le
                {
                    return 10;
                }
                else if (roads["up"] && roads["down"] == false && roads["left"] && roads["right"] == false) //fel,balra
                {
                    return 8;
                }
                else if (roads["up"] && roads["down"] == false && roads["left"] == false && roads["right"]) //fel,jobbra
                {
                    return 5;
                }
                else if (roads["up"] == false && roads["down"] && roads["left"] && roads["right"] == false) //le,balra
                {
                    return 7;
                }
                else if (roads["up"] == false && roads["down"] && roads["left"] == false && roads["right"]) //le,jobbra
                {
                    return 6;
                }
                else if (roads["up"] == false && roads["down"] == false && roads["left"] && roads["right"]) //balra,jobbra
                {
                    return 9;
                }
                else if (roads["up"] && roads["down"] && roads["left"] && roads["right"] == false) // fel,le,balra
                {
                    return 14;
                }
                else if (roads["up"] && roads["down"] && roads["left"] == false && roads["right"]) //fel,le,jobbra
                {
                    return 12;
                }
                else if (roads["up"] && roads["down"] == false && roads["left"] && roads["right"]) //fel,balra,jobbra
                {
                    return 11;
                }
                else if (roads["up"] == false && roads["down"] && roads["left"] && roads["right"]) //le,balra,jobbra
                {
                    return 13;
                }
                else if (roads["up"] && roads["down"] && roads["left"] && roads["right"]) //fel,le,jobbra,balra
                {
                    return 15;
                }
                else if (roads["up"] == false && roads["down"] == false && roads["left"] == false && roads["right"] == false)
                {
                    return 0;
                }
            }
            return -666;
        }
    }

    public List<Cell> Neighbours
    {
        get
        {
            List<Cell> list = new List<Cell>();

            if (grid.GetCell(x + 1, y) != null)
            {
                list.Add(grid.GetCell(x + 1, y));
            }

            if (grid.GetCell(x - 1, y) != null)
            {
                list.Add(grid.GetCell(x - 1, y));
            }

            if (grid.GetCell(x, y + 1) != null)
            {
                list.Add(grid.GetCell(x, y + 1));
            }

            if (grid.GetCell(x, y - 1) != null)
            {
                list.Add(grid.GetCell(x, y - 1));
            }

            return list;
        }
    }

    public Cell(GridXZ grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public void SetBuilding(Building newBuilding)
    {
        this.building = newBuilding;
    }

    public void ClearBuilding()
    {
        building = null;
    }

    public bool IsEmpty()
    {
        return building == null;
    }

    public Building GetBuilding()
    {
        return building;
    }

    public int GetX()
    {
        return this.x;
    }

    public int GetY()
    {
        return this.y;
    }
}