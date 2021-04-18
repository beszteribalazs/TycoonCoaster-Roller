﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingSystem : MonoBehaviour
{
    public enum ClickMode
    {
        Normal,
        Destroy
    }

    public static BuildingSystem instance;

    public GridXZ grid;
    Transform ground;

    [Header("Setup")] [SerializeField] int gridWidth;
    [SerializeField] int gridHeight;

    [SerializeField] float cellSize = 3f;

    public float CellSize => cellSize;

    //[SerializeField] List<BuildingTypeSO> placableBuildings;
    [SerializeField] Transform groundVisualPrefab;
    [SerializeField] Transform entryPointPrefab;

    public Transform entryPoint;

    List<Building> placedBuildings = new List<Building>();
    BuildingTypeSO selectedBuildingSO;
    BuildingTypeSO.Direction currentBuildingRotation;

    public ClickMode currentMode;

    //public List<Building> Buildings => grid.GetBuildingList();
    public List<Building> Buildings => placedBuildings;
    [SerializeField] BuildingTypeSO roadStraight;
    [SerializeField] BuildingTypeSO roadTurn;
    [SerializeField] BuildingTypeSO roadT;
    [SerializeField] BuildingTypeSO roadX;

    private int lastX, lastZ;

    void Awake()
    {
        instance = this;
        gridWidth = MapSizeController.mapSize;
        gridHeight = MapSizeController.mapSize;
        grid = new GridXZ(gridWidth, gridHeight, cellSize, Vector3.zero);

        // Create ground visual
        float groundSizeX = gridWidth * cellSize;
        float groundSizeY = gridHeight * cellSize;
        ground = Instantiate(groundVisualPrefab, new Vector3(groundSizeX / 2, -0.5f, groundSizeY / 2),
            Quaternion.identity);
        ground.localScale = new Vector3(groundSizeX, 1, groundSizeY);

        selectedBuildingSO = null;
        currentBuildingRotation = BuildingTypeSO.Direction.Down;
    }

    void Start()
    {
        entryPoint = Instantiate(entryPointPrefab, new Vector3((float) ((int) (gridWidth / 2) * cellSize), 0, -3),
            Quaternion.identity);
        lastX = -1;
        lastZ = -1;
    }

    int buildingIndex = 0;


    bool startedDragging = false;
    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (currentMode == ClickMode.Normal)
            {
                // Select placed building
                if (Input.GetMouseButtonDown(0) && selectedBuildingSO == null)
                {
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo,
                        1000f,
                        (1 << 9)))
                    {
                        //Debug.Log(hitInfo.collider.transform.parent.name);
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            InspectorMenu.instance.DisplayDetails(hitInfo.collider.transform.parent
                                .GetComponent<Attraction>());
                        }
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    SetSelectedBuildingType(null);
                }
                
                // Place building
                if (selectedBuildingSO != null && Input.GetMouseButtonDown(0)){
                    PlaceBuilding();
                }

                // Drag road
                if (selectedBuildingSO != null && Input.GetMouseButton(0) && selectedBuildingSO.type == BuildingTypeSO.Type.Road){
                    int x, z;
                    grid.XZFromWorldPosition(GetMouseWorldPosition(), out x, out z);
                    if (grid.GetCell(x, z) == null) return;
                    if ( (lastX != x) || (lastZ != z) )
                    {
                        GameManager.instance.BuyBuilding(roadStraight);
                        lastX = x;
                        lastZ = z;
                        UpdateRoad(x, z);
                        foreach (Cell cell in grid.GetCell(x,z).Neighbours)
                        {
                            if (cell.GetBuilding() != null && cell.GetBuilding().Type.type == BuildingTypeSO.Type.Road)
                            {
                                UpdateRoad(cell.GetX(),cell.GetY());
                            }
                        }
                    }
                }

                if (Input.GetMouseButtonUp(0)){
                    EventManager.instance.MapChanged();
                }

                // Hide preview if not enough money
                if (Input.GetMouseButtonUp(0) && selectedBuildingSO != null &&
                    GameManager.instance.Money < selectedBuildingSO.price)
                {
                    SetSelectedBuildingType(null);
                }

                // Rotate building
                if (Input.GetKeyDown(KeyCode.R))
                {
                    currentBuildingRotation = BuildingTypeSO.GetNextDirectionLeft(currentBuildingRotation);
                }
                else if (Input.GetKeyDown(KeyCode.F))
                {
                    currentBuildingRotation = BuildingTypeSO.GetNextDirectionRight(currentBuildingRotation);
                }
            }
            else if (currentMode == ClickMode.Destroy)
            {
                if (Input.GetMouseButton(0))
                {
                    if (selectedBuildingSO == null)
                    {
                        SellBuilding();
                    }
                    else
                    {
                        SetSelectedBuildingType(null);
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    currentMode = ClickMode.Normal;
                    EventManager.instance.ModeChanged(currentMode);
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            SetSelectedBuildingType(null);
            currentMode = ClickMode.Normal;
            EventManager.instance.ModeChanged(currentMode);
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            SetSelectedBuildingType(null);
            currentMode = ClickMode.Normal;
            EventManager.instance.ModeChanged(currentMode);
        }

        //Cycle buildings
        /*if (Input.GetKeyDown(KeyCode.Tab)){
            selectedBuildingSO = placableBuildings[buildingIndex];
            EventManager.instance.SelectedBuildingChanged();
            buildingIndex++;
            if (buildingIndex >= placableBuildings.Count){
                buildingIndex = 0;
            }
        }*/
    }

    public void SwitchMode(ClickMode m)
    {
        currentMode = m;
        EventManager.instance.ModeChanged(m);
    }

    public void SetSelectedBuildingType(BuildingTypeSO type)
    {
        selectedBuildingSO = type;
        EventManager.instance.SelectedBuildingChanged();
    }

    void UpdateRoad(int x, int z)
    {
        if (grid.GetCell(x, z) == null) return;
        switch (grid.GetCell(x, z).AdjacentRoads)
        {
            case 0:
                ChangeRoadDirection(roadX, BuildingTypeSO.Direction.Down, x, z);
                break;
            case 1:
                ChangeRoadDirection(roadStraight, BuildingTypeSO.Direction.Down, x, z);
                break;
            case 2:
                ChangeRoadDirection(roadStraight, BuildingTypeSO.Direction.Right, x, z);
                break;
            case 3:
                ChangeRoadDirection(roadStraight, BuildingTypeSO.Direction.Down, x, z);
                break;
            case 4:
                ChangeRoadDirection(roadStraight, BuildingTypeSO.Direction.Right, x, z);
                break;
            case 5:
                ChangeRoadDirection(roadTurn, BuildingTypeSO.Direction.Right, x, z);
                break;
            case 6:
                ChangeRoadDirection(roadTurn, BuildingTypeSO.Direction.Down, x, z);
                break;
            case 7:
                ChangeRoadDirection(roadTurn, BuildingTypeSO.Direction.Left, x, z);
                break;
            case 8:
                ChangeRoadDirection(roadTurn, BuildingTypeSO.Direction.Up, x, z);
                break;
            case 9:
                ChangeRoadDirection(roadStraight, BuildingTypeSO.Direction.Right, x, z);
                break;
            case 10:
                ChangeRoadDirection(roadStraight, BuildingTypeSO.Direction.Down, x, z);
                break;
            case 11:
                ChangeRoadDirection(roadT, BuildingTypeSO.Direction.Right, x, z);
                break;
            case 12:
                ChangeRoadDirection(roadT, BuildingTypeSO.Direction.Down, x, z);
                break;
            case 13:
                ChangeRoadDirection(roadT, BuildingTypeSO.Direction.Left, x, z);
                break;
            case 14:
                ChangeRoadDirection(roadT, BuildingTypeSO.Direction.Up, x, z);
                break;
            case 15:
                ChangeRoadDirection(roadX, BuildingTypeSO.Direction.Down, x, z);
                break;
            case 16:
                ChangeRoadDirection(roadX, BuildingTypeSO.Direction.Down, x, z);
                break;
            case 17:
                ChangeRoadDirection(roadX, BuildingTypeSO.Direction.Down, x, z);
                break;
            case 18:
                ChangeRoadDirection(roadTurn, BuildingTypeSO.Direction.Down, x, z);
                break;
            case 19:
                ChangeRoadDirection(roadTurn, BuildingTypeSO.Direction.Left, x, z);
                break;
            case 20:
                ChangeRoadDirection(roadT, BuildingTypeSO.Direction.Down, x, z);
                break;
            case 21:
                ChangeRoadDirection(roadT, BuildingTypeSO.Direction.Up, x, z);
                break;
            case 22:
                ChangeRoadDirection(roadT, BuildingTypeSO.Direction.Left, x, z);
                break;
            case 23:
                ChangeRoadDirection(roadStraight, BuildingTypeSO.Direction.Down, x, z);
                break;
            default:
                Debug.LogError("MEGBASZOMANYADATHASZAR");
                break;
        }
    }


    void ChangeRoadDirection(BuildingTypeSO road, BuildingTypeSO.Direction rotation, int x, int z)
    {
        Cell currentCell = grid.GetCell(x, z);
        List<Vector2Int> positionList = road.GetPositionList(new Vector2Int(x, z), rotation);

        if (currentCell.GetBuilding()!=null && currentCell.GetBuilding().Type.type == BuildingTypeSO.Type.Road)
        {
            Building building = currentCell.GetBuilding();
            currentCell.ClearBuilding();
            placedBuildings.Remove(building);
            building.Destroy();
        }

        // Check if all coordinates are empty
        bool canBuild = true;
        foreach (Vector2Int gridPosition in positionList)
        {
            // If any of the cells is occupied, don't build:c
            try
            {
                if (!grid.GetCell(gridPosition.x, gridPosition.y).IsEmpty())
                {
                    canBuild = false;
                    //Debug.Log("Placing on (" + x + "," + z + ") would collide with another building");
                    break;
                }
            }
            catch (NotValidCellException e)
            {
                canBuild = false;
            }
        }

        // Place building if area is clear
        if (canBuild)
        {
            Vector2Int rotationOffset = road.GetRotationOffset(rotation);
            Vector3 worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
            Building placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), rotation, road, positionList);
            
            foreach (Vector2Int gridPositions in positionList)
            {
                grid.GetCell(gridPositions.x, gridPositions.y).SetBuilding(placedBuilding);
            }

            if (!Input.GetKey(KeyCode.LeftShift))
            {
                SetSelectedBuildingType(null);
            }

            //EventManager.instance.MapChanged();
            
            placedBuildings.Add(placedBuilding);
        }

    }


    void PlaceBuilding()
    {
        try
        {
            int x, z;
            grid.XZFromWorldPosition(GetMouseWorldPosition(), out x, out z);

            // Get list of the buildings coordinates
            List<Vector2Int> positionList =
                selectedBuildingSO.GetPositionList(new Vector2Int(x, z), currentBuildingRotation);

            // Check if all coordinates are empty
            bool canBuild = true;
            foreach (Vector2Int gridPosition in positionList)
            {
                // If any of the cells is occupied, don't build
                try
                {
                    if (!grid.GetCell(gridPosition.x, gridPosition.y).IsEmpty())
                    {
                        canBuild = false;
                        //Debug.Log("Placing on (" + x + "," + z + ") would collide with another building");
                        break;
                    }
                }
                catch (NotValidCellException e)
                {
                    canBuild = false;
                }
            }

            if (GameManager.instance.Money < selectedBuildingSO.price)
            {
                canBuild = false;
                SetSelectedBuildingType(null);
            }

            // Place building if area is clear
            if (canBuild)
            {
                Vector2Int rotationOffset = selectedBuildingSO.GetRotationOffset(currentBuildingRotation);
                Vector3 worldPosition = grid.GetWorldPosition(x, z) +
                                        new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                Building placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                    currentBuildingRotation, selectedBuildingSO, positionList);

                //placedBuilding.RemovePreviewBox();
                foreach (Vector2Int gridPositions in positionList)
                {
                    grid.GetCell(gridPositions.x, gridPositions.y).SetBuilding(placedBuilding);
                }

                GameManager.instance.BuyBuilding(selectedBuildingSO);


                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    SetSelectedBuildingType(null);
                }


                EventManager.instance.MapChanged();
                placedBuildings.Add(placedBuilding);
            }
        }
        catch (Exception e)
        {
            //Debug.Log("Mouse out of the map :(");
        }
    }

    private void SellBuilding()
    {
        Cell clickedCell = grid.GetCell(GetMouseWorldPosition());
        if (clickedCell == null) return;
        Building clickedBuilding = clickedCell.GetBuilding();
        if (clickedBuilding != null)
        {
            if (clickedBuilding.Type.type == BuildingTypeSO.Type.Road){
                int posX, posZ;
                posX = clickedCell.GetX();
                posZ = clickedCell.GetY();
                
                List<Vector2Int> destroyedCoordinates = clickedBuilding.GetGridPositionList();
                foreach (Vector2Int gridPos in destroyedCoordinates)
                {
                    grid.GetCell(gridPos.x, gridPos.y).ClearBuilding();
                }

                placedBuildings.Remove(clickedBuilding);
                GameManager.instance.SellBuilding(clickedBuilding);
                clickedBuilding.Destroy();
                foreach (Cell cell in grid.GetCell(posX,posZ).Neighbours)
                {
                    if (cell.GetBuilding() != null && cell.GetBuilding().Type.type == BuildingTypeSO.Type.Road)
                    {
                        UpdateRoad(cell.GetX(),cell.GetY());
                    }
                }
                
                Invoke(nameof(Aaaaa), 0.1f);    
            }
            else{
                List<Vector2Int> destroyedCoordinates = clickedBuilding.GetGridPositionList();
                foreach (Vector2Int gridPos in destroyedCoordinates)
                {
                    grid.GetCell(gridPos.x, gridPos.y).ClearBuilding();
                }

                placedBuildings.Remove(clickedBuilding);
                GameManager.instance.SellBuilding(clickedBuilding);
                clickedBuilding.Destroy();
                Invoke(nameof(Aaaaa), 0.1f);    
            }
        }
    }

    private void Aaaaa()
    {
        EventManager.instance.MapChanged();
    }

    public Quaternion GetCurrentBuildingRotation()
    {
        return Quaternion.Euler(0, selectedBuildingSO.GetRotationAngle(currentBuildingRotation), 0);
    }

    public BuildingTypeSO GetSelectedBuildingType()
    {
        return selectedBuildingSO;
    }

    // Converts pointer position to world position
    // layer 8: ground
    public Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, 1000f, (1 << 8)))
        {
            return hitInfo.point;
        }
        else
        {
            //DO NOT BUILD
            throw new MouseOutOfMapException("Invalid position!");
        }
    }

    // Snaps worlds position to the grid for building preview ghost
    // layer 8 :ground
    public Vector3 GetMouseWorldSnappedPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, 1000f, (1 << 8)))
        {
            Vector3 mousePosition = hitInfo.point;

            grid.XZFromWorldPosition(mousePosition, out int x, out int z);

            if (selectedBuildingSO != null)
            {
                Vector2Int rotationOffset = selectedBuildingSO.GetRotationOffset(currentBuildingRotation);
                Vector3 previewPosition = grid.GetWorldPosition(x, z) +
                                          new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                return previewPosition;
            }
            else
            {
                return mousePosition;
            }
        }
        else
        {
            //DO NOT BUILD
            throw new MouseOutOfMapException("Invalid position!");
        }
    }
}