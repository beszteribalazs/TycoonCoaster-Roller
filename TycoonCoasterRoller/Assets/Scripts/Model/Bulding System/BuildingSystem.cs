using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class BuildingSystem : MonoBehaviour
{
    public enum ClickMode
    {
        Normal,
        Destroy,
        Road
    }

    public static BuildingSystem instance;
    public GridXZ grid;
    Transform ground;
    [Header("Setup")] [SerializeField] int gridWidth;
    [SerializeField] int gridHeight;
    [SerializeField] float cellSize = 3f;
    public float CellSize => cellSize;
    public Transform groundVisualPrefab;
    [SerializeField] Transform entryPointPrefab;
    public Transform entryPoint;
    List<Building> placedBuildings = new List<Building>();
    List<Attraction> placedAttractions = new List<Attraction>();
    public List<Attraction> Attractions => placedAttractions;
    BuildingTypeSO selectedBuildingSO;
    BuildingTypeSO.Direction currentBuildingRotation;
    public ClickMode currentMode;
    public List<Building> Buildings => placedBuildings;
    [SerializeField] BuildingTypeSO roadStraight;
    [SerializeField] BuildingTypeSO roadTurn;
    [SerializeField] BuildingTypeSO roadT;
    [SerializeField] BuildingTypeSO roadX;
    [SerializeField] BuildingTypeSO treeOne;
    [SerializeField] BuildingTypeSO treeTwo;
    [SerializeField] BuildingTypeSO treeThree;
    private int lastX, lastZ;

    void Awake()
    {
        EventManager.instance.onModeChanged += ResetLastClickedTile;

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

        float spawnChance = 0.05f; // The spawning chance of the trees when the game starts
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                float randFloat = Random.Range(0f, 1f);
                if (randFloat < spawnChance)
                {
                    spawnTree(i, j);
                }
            }
        }
    }

    void ResetLastClickedTile(ClickMode cm)
    {
        lastX = -1;
        lastZ = -1;
    }

    void Start()
    {
        entryPoint = Instantiate(entryPointPrefab, new Vector3((float) ((int) (gridWidth / 2) * cellSize), 0, -3),
            Quaternion.identity);
        lastX = -1;
        lastZ = -1;
    }

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
                if (selectedBuildingSO != null && Input.GetMouseButtonDown(0))
                {
                    PlaceBuilding();
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

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    SetSelectedBuildingType(null);
                }

                if (Input.GetKeyDown(KeyCode.B))
                {
                    SetSelectedBuildingType(null);
                }
            }
            else if (currentMode == ClickMode.Destroy)
            {
                int x, z;
                grid.XZFromWorldPosition(GetMouseWorldPosition(), out x, out z);
                if (Input.GetMouseButton(0))
                {
                    if (selectedBuildingSO == null)
                    {
                        if ((lastX != x) || (lastZ != z))
                        {
                            lastX = x;
                            lastZ = z;
                            SellBuilding();
                        }
                    }
                    else
                    {
                        SetSelectedBuildingType(null);
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    ResetLastClickedTile(ClickMode.Destroy);
                }

                if (Input.GetMouseButtonDown(1))
                {
                    SetSelectedBuildingType(null);
                    GameManager.instance.SwitchMode();
                    EventManager.instance.ModeChanged(currentMode);
                }

                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    SetSelectedBuildingType(null);
                    GameManager.instance.SwitchMode();
                    EventManager.instance.ModeChanged(currentMode);
                }

                if (Input.GetKeyDown(KeyCode.B))
                {
                    SetSelectedBuildingType(null);
                    GameManager.instance.SwitchMode();
                    EventManager.instance.ModeChanged(currentMode);
                }
            }
            else if (currentMode == ClickMode.Road)
            {
                if (selectedBuildingSO != null && Input.GetMouseButton(0) &&
                    selectedBuildingSO.type == BuildingTypeSO.Type.Road)
                {
                    int x, z;
                    grid.XZFromWorldPosition(GetMouseWorldPosition(), out x, out z);
                    if (grid.GetCell(x, z) == null) return;
                    if (grid.GetCell(x, z).GetBuilding() != null) return;
                    if ((lastX != x) || (lastZ != z))
                    {
                        if (grid.GetCell(x, z).GetBuilding() == null)
                        {
                            GameManager.instance.BuyBuilding(roadStraight);
                        }

                        lastX = x;
                        lastZ = z;
                        UpdateRoad(x, z);
                        foreach (Cell cell in grid.GetCell(x, z).Neighbours)
                        {
                            if (cell.GetBuilding() != null && cell.GetBuilding().Type.type == BuildingTypeSO.Type.Road)
                            {
                                UpdateRoad(cell.GetX(), cell.GetY());
                            }
                        }
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    EventManager.instance.MapChanged();
                }

                if (Input.GetMouseButton(0) && selectedBuildingSO != null &&
                    GameManager.instance.Money < selectedBuildingSO.price)
                {
                    SetSelectedBuildingType(null);
                    GameManager.instance.SwitchRoadMode();
                    EventManager.instance.ModeChanged(currentMode);
                }

                if (Input.GetMouseButtonDown(1))
                {
                    SetSelectedBuildingType(null);
                    GameManager.instance.SwitchRoadMode();
                    EventManager.instance.ModeChanged(currentMode);
                }

                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    SetSelectedBuildingType(null);
                    GameManager.instance.SwitchRoadMode();
                    EventManager.instance.ModeChanged(currentMode);
                }

                if (Input.GetKeyDown(KeyCode.B))
                {
                    SetSelectedBuildingType(null);
                    GameManager.instance.SwitchRoadMode();
                    EventManager.instance.ModeChanged(currentMode);
                }
            }
        }
    }

    void EvictUnreachableBuildings()
    {
        foreach (Building building in placedBuildings)
        {
            if (building.Type.type == BuildingTypeSO.Type.Attraction)
            {
                Attraction attraction = (Attraction) building;
                if (!NavigationManager.instance.reachableAttractions.Contains(attraction))
                {
                    attraction.EjectVisitors();
                }
            }
        }
    }

    public void spawnTree(int x, int z)
    {
        int randTree = Random.Range(1, 4);
        BuildingTypeSO.Direction rotation = BuildingTypeSO.Direction.Down;
        List<Vector2Int> positionList = new List<Vector2Int>();
        positionList.Add(new Vector2Int(0, 0));
        Vector2Int rotationOffset;
        Vector3 worldPosition;
        Building placedBuilding;

        switch (randTree)
        {
            case 1:
                rotationOffset = treeOne.GetRotationOffset(BuildingTypeSO.Direction.Down);
                worldPosition = grid.GetWorldPosition(x, z) +
                                new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                placedBuilding =
                    Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), rotation, treeOne, positionList);
                grid.GetCell(x, z).SetBuilding(placedBuilding);
                placedBuildings.Add(placedBuilding);
                break;
            case 2:
                rotationOffset = treeTwo.GetRotationOffset(BuildingTypeSO.Direction.Down);
                worldPosition = grid.GetWorldPosition(x, z) +
                                new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                placedBuilding =
                    Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), rotation, treeTwo, positionList);
                grid.GetCell(x, z).SetBuilding(placedBuilding);
                placedBuildings.Add(placedBuilding);
                break;
            case 3:
                rotationOffset = treeThree.GetRotationOffset(BuildingTypeSO.Direction.Down);
                worldPosition = grid.GetWorldPosition(x, z) +
                                new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), rotation, treeThree,
                    positionList);
                grid.GetCell(x, z).SetBuilding(placedBuilding);
                placedBuildings.Add(placedBuilding);
                break;
        }
    }

    ClickMode previousMode = ClickMode.Normal;

    public void SwitchMode(ClickMode m)
    {
        previousMode = currentMode;
        if (previousMode == ClickMode.Destroy && m != ClickMode.Destroy)
        {
            EventManager.instance.MapChanged();
        }

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
        Road r = (Road) grid.GetCell(x, z).GetBuilding();
        if (r == null)
        {
            r = ChangeRoadDirection(roadX, BuildingTypeSO.Direction.Down, x, z);
        }

        GameObject visual = r.visual.Find("visual").gameObject;
        r.roadS.SetActive(false);
        r.roadL.SetActive(false);
        r.roadT.SetActive(false);
        r.roadX.SetActive(false);
        visual.transform.rotation = Quaternion.Euler(0, 0, 0);

        switch (grid.GetCell(x, z).AdjacentRoads)
        {
            case 0:
                r.roadX.SetActive(true);
                break;
            case 1:
                r.roadS.SetActive(true);
                break;
            case 2:
                r.roadS.SetActive(true);
                visual.transform.rotation = Quaternion.Euler(0, 90, 0);
                break;
            case 3:
                r.roadS.SetActive(true);
                break;
            case 4:
                r.roadS.SetActive(true);
                visual.transform.rotation = Quaternion.Euler(0, 90, 0);
                break;
            case 5:
                r.roadL.SetActive(true);
                visual.transform.rotation = Quaternion.Euler(0, 90, 0);
                break;
            case 6:
                r.roadL.SetActive(true);
                visual.transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
            case 7:
                r.roadL.SetActive(true);
                visual.transform.rotation = Quaternion.Euler(0, -90, 0);
                break;
            case 8:
                r.roadL.SetActive(true);
                break;
            case 9:
                r.roadS.SetActive(true);
                visual.transform.rotation = Quaternion.Euler(0, 90, 0);
                break;
            case 10:
                r.roadS.SetActive(true);
                break;
            case 11:
                r.roadT.SetActive(true);
                visual.transform.rotation = Quaternion.Euler(0, 90, 0);
                break;
            case 12:
                r.roadT.SetActive(true);
                visual.transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
            case 13:
                r.roadT.SetActive(true);
                visual.transform.rotation = Quaternion.Euler(0, -90, 0);
                break;
            case 14:
                r.roadT.SetActive(true);
                break;
            case 15:
                r.roadX.SetActive(true);
                break;
            case 16:
                r.roadS.SetActive(true);
                break;
            case 17:
                r.roadX.SetActive(true);
                break;
            case 18:
                r.roadL.SetActive(true);
                visual.transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
            case 19:
                r.roadL.SetActive(true);
                visual.transform.rotation = Quaternion.Euler(0, -90, 0);
                break;
            case 20:
                r.roadT.SetActive(true);
                visual.transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
            case 21:
                r.roadT.SetActive(true);
                break;
            case 22:
                r.roadT.SetActive(true);
                visual.transform.rotation = Quaternion.Euler(0, -90, 0);
                break;
            case 23:
                r.roadS.SetActive(true);
                break;
            default:
                break;
        }

        ResetLastClickedTile(ClickMode.Road);
    }


    Road ChangeRoadDirection(BuildingTypeSO road, BuildingTypeSO.Direction rotation, int x, int z)
    {
        Cell currentCell = grid.GetCell(x, z);
        List<Vector2Int> positionList = road.GetPositionList(new Vector2Int(x, z), rotation);

        if (currentCell.GetBuilding() != null && currentCell.GetBuilding().Type.type == BuildingTypeSO.Type.Road)
        {
            Road tmp = (Road) currentCell.GetBuilding();
            tmp.roadS.SetActive(false);
            tmp.roadL.SetActive(false);
            tmp.roadT.SetActive(false);
            tmp.roadX.SetActive(false);

            switch (rotation)
            {
                case BuildingTypeSO.Direction.Down:
                    break;
                case BuildingTypeSO.Direction.Left:
                    break;
                case BuildingTypeSO.Direction.Up:
                    break;
                case BuildingTypeSO.Direction.Right:
                    break;
            }
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
            Vector3 worldPosition = grid.GetWorldPosition(x, z) +
                                    new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
            Building placedBuilding =
                Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), rotation, road, positionList);

            foreach (Vector2Int gridPositions in positionList)
            {
                grid.GetCell(gridPositions.x, gridPositions.y).SetBuilding(placedBuilding);
            }

            if (!Input.GetMouseButton(0))
            {
                SetSelectedBuildingType(null);
            }

            placedBuildings.Add(placedBuilding);
            return (Road) placedBuilding;
        }

        return null;
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

                foreach (Vector2Int gridPositions in positionList)
                {
                    grid.GetCell(gridPositions.x, gridPositions.y).SetBuilding(placedBuilding);
                }

                GameManager.instance.BuyBuilding(selectedBuildingSO);

                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    SetSelectedBuildingType(null);
                }

                placedBuildings.Add(placedBuilding);
                if (placedBuilding.Type.type == BuildingTypeSO.Type.Attraction)
                {
                    placedAttractions.Add((Attraction) placedBuilding);
                }

                EventManager.instance.MapChanged();
            }
        }
        catch (Exception e)
        {
        }
    }

    private void SellBuilding()
    {
        Cell clickedCell = grid.GetCell(GetMouseWorldPosition());
        if (clickedCell == null) return;
        Building clickedBuilding = clickedCell.GetBuilding();
        if (clickedBuilding != null)
        {
            //Selling a road
            if (clickedBuilding.Type.type == BuildingTypeSO.Type.Road)
            {
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
                foreach (Cell cell in grid.GetCell(posX, posZ).Neighbours)
                {
                    if (cell.GetBuilding() != null && cell.GetBuilding().Type.type == BuildingTypeSO.Type.Road)
                    {
                        UpdateRoad(cell.GetX(), cell.GetY());
                    }
                }
            }
            // Selling an attraction
            else if (clickedBuilding.Type.type == BuildingTypeSO.Type.Attraction)
            {
                Attraction clicked = (Attraction) clickedBuilding;
                if (clicked.Broke)
                {
                    EventManager.instance.BrokeBuildingSold();
                    lastX = -1;
                    lastZ = -1;
                }
                else
                {
                    clicked.SendOutVisitors();
                    List<Vector2Int> destroyedCoordinates = clickedBuilding.GetGridPositionList();
                    foreach (Vector2Int gridPos in destroyedCoordinates)
                    {
                        grid.GetCell(gridPos.x, gridPos.y).ClearBuilding();
                    }

                    placedBuildings.Remove(clickedBuilding);
                    placedAttractions.Remove((Attraction) clickedBuilding);
                    GameManager.instance.SellBuilding(clickedBuilding);
                    clickedBuilding.Destroy();
                }
            }
            else
            {
                List<Vector2Int> destroyedCoordinates = clickedBuilding.GetGridPositionList();
                foreach (Vector2Int gridPos in destroyedCoordinates)
                {
                    grid.GetCell(gridPos.x, gridPos.y).ClearBuilding();
                }

                placedBuildings.Remove(clickedBuilding);
                GameManager.instance.SellBuilding(clickedBuilding);
                clickedBuilding.Destroy();
            }
        }
    }

    private void MapChanged()
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
            throw new MouseOutOfMapException("Invalid position!");
        }
    }
}