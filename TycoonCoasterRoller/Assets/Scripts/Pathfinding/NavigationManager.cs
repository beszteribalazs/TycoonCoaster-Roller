using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationManager : MonoBehaviour
{
    NavMeshSurface surface;
    Spawner spawner;
    [SerializeField] BuildingSystem buildingSystem;
    public List<Road> reachableRoads;
    public List<Cell> reachableCells;
    public List<Attraction> reachableAttractions;
    public int reachableAttractionCount;
    public int reachableCapacity = 0;
    public static NavigationManager instance;

    void Awake()
    {
        instance = this;
        EventManager.instance.onMapChanged += CalculateReachableAttractions;
    }

    void Start()
    {
        surface = GetComponent<NavMeshSurface>();
        EventManager.instance.onMapChanged += RebakeMap;
        spawner = GetComponent<Spawner>();
        RebakeMap();

        surface.overrideVoxelSize = true;
        surface.voxelSize = 0.25f;
        surface.overrideTileSize = true;
        surface.tileSize = 64;
    }

    private void RebakeMap()
    {
        surface.RemoveData();
        surface.BuildNavMesh();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            spawner.SpawnVisitor(buildingSystem.entryPoint.position + new Vector3(1.5f, 0, 1.5f));
        }
    }

    void CalculateReachableAttractions()
    {
        List<Attraction> reachable = new List<Attraction>();
        reachableCells = new List<Cell>();
        reachableRoads = new List<Road>();
        reachableCapacity = 0;

        GridXZ grid = BuildingSystem.instance.grid;

        int x;
        int z;
        // Find first cell from spawn
        grid.XZFromWorldPosition(
            BuildingSystem.instance.entryPoint.position + Vector3.forward * BuildingSystem.instance.CellSize, out x,
            out z);

        // if first cell is empty, nothing is reachable
        if (grid.GetCell(x, z).GetBuilding() == null)
        {
            return;
        }

        // if first cell is building, only this building is reachable
        if (grid.GetCell(x, z).GetBuilding().Type.type == BuildingTypeSO.Type.Attraction)
        {
            reachable.Add((Attraction) grid.GetCell(x, z).GetBuilding());
        }
        // if first cell is decoration, nothing is reachable
        else if (grid.GetCell(x, z).GetBuilding().Type.type == BuildingTypeSO.Type.Decoration)
        {
        }
        // if first cell is road, start search
        else if (grid.GetCell(x, z).GetBuilding().Type.type == BuildingTypeSO.Type.Road)
        {
            List<Cell> visited = new List<Cell>();
            Stack<Cell> path = new Stack<Cell>();
            Cell currentCell = grid.GetCell(x, z);

            bool finished = false;
            while (!finished)
            {
                // add current to visited
                if (!visited.Contains(currentCell))
                {
                    visited.Add(currentCell);
                }

                //if current cell is attraction, add to reachable and go back
                if (currentCell.GetBuilding().Type.type == BuildingTypeSO.Type.Attraction)
                {
                    Attraction current = (Attraction) currentCell.GetBuilding();
                    if (!reachable.Contains(current))
                    {
                        reachable.Add(current);
                    }

                    if (!reachableCells.Contains(currentCell))
                    {
                        reachableCells.Add(currentCell);
                    }

                    currentCell = path.Pop();
                } // else search
                else if (currentCell.GetBuilding().Type.type == BuildingTypeSO.Type.Road)
                {
                    Road road = (Road) currentCell.GetBuilding();
                    if (!reachableRoads.Contains(road))
                    {
                        reachableRoads.Add(road);
                    }

                    // choose random direction
                    Cell direction = null;
                    foreach (Cell neighbour in currentCell.Neighbours)
                    {
                        // check if neighbour cell has building
                        if (neighbour.GetBuilding() != null)
                        {
                            // check if already visited
                            if (!visited.Contains(neighbour))
                            {
                                // not visited neighbour exists
                                direction = neighbour;
                                break;
                            }
                        }
                    }

                    // if exists
                    if (direction != null)
                    {
                        // add current to path
                        // go there
                        path.Push(currentCell);
                        currentCell = direction;
                    } //else
                    else
                    {
                        // if path.count > 0
                        if (path.Count > 0)
                        {
                            // go back
                            currentCell = path.Pop();
                        }
                        else
                        {
                            finished = true;
                        }
                    }
                }
                else if (currentCell.GetBuilding().Type.type == BuildingTypeSO.Type.Decoration)
                {
                    currentCell = path.Pop();
                }
            }
        }

        foreach (Attraction attraction in reachable)
        {
            reachableCapacity += attraction.TotalCapacity;
        }

        reachableAttractions = reachable;
        reachableAttractionCount = reachable.Count;
    }
}