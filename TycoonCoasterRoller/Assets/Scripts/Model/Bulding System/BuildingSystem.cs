using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingSystem : MonoBehaviour{
    public enum ClickMode{
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
    public BuildingTypeSO roadStraight;
    public BuildingTypeSO roadTurn;
    public BuildingTypeSO roadT;
    public BuildingTypeSO roadX;

    private int lastX, lastZ;

    void Awake(){
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

    void Start(){
        entryPoint = Instantiate(entryPointPrefab, new Vector3((float) ((int) (gridWidth / 2) * cellSize), 0, -3),
            Quaternion.identity);
        lastX = -1; lastZ = -1;
    }

    int buildingIndex = 0;

    void Update(){
        if (!EventSystem.current.IsPointerOverGameObject()){
            if (currentMode == ClickMode.Normal){
                // Select placed building
                if (Input.GetMouseButtonDown(0) && selectedBuildingSO == null){
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, 1000f,
                        (1 << 9))){
                        //Debug.Log(hitInfo.collider.transform.parent.name);
                        if (!EventSystem.current.IsPointerOverGameObject()){
                            InspectorMenu.instance.DisplayDetails(hitInfo.collider.transform.parent.GetComponent<Attraction>());
                        }
                    }
                }

                if (Input.GetMouseButtonDown(1)){
                    SetSelectedBuildingType(null);
                }

                // Place building
                if (Input.GetMouseButtonDown(0)){
                    //left click
                    PlaceBuilding();
                }
                
                // Drag roads only
                if (selectedBuildingSO != null && GetSelectedBuildingType().type == BuildingTypeSO.Type.Road && Input.GetMouseButton(0))
                {
                    //placeRoad();
                    int x, z;
                    grid.XZFromWorldPosition(GetMouseWorldPosition(), out x, out z);
                    if ( !(x == lastX && z == lastZ) )
                    {
                        lastX = x;
                        lastZ = z;
                        //PlaceBuilding();
                        placeRoad();
                    }
                }

                // Hide preview if not enough money
                if (Input.GetMouseButtonUp(0) && selectedBuildingSO != null && GameManager.instance.Money < selectedBuildingSO.price){
                    SetSelectedBuildingType(null);
                }

                // Rotate building
                if (Input.GetKeyDown(KeyCode.R)){
                    currentBuildingRotation = BuildingTypeSO.GetNextDirectionLeft(currentBuildingRotation);
                }
                else if (Input.GetKeyDown(KeyCode.F)){
                    currentBuildingRotation = BuildingTypeSO.GetNextDirectionRight(currentBuildingRotation);
                }
            }
            else if (currentMode == ClickMode.Destroy){
                if (Input.GetMouseButtonDown(0)){
                    if (selectedBuildingSO == null){
                        // Destroy building
                        SellBuilding();
                    }
                    else{
                        SetSelectedBuildingType(null);
                    }
                }

                if (Input.GetMouseButtonDown(1)){
                    currentMode = ClickMode.Normal;
                    EventManager.instance.ModeChanged(currentMode);
                }
            }
        }



        if (Input.GetKeyUp(KeyCode.Escape)){
            SetSelectedBuildingType(null);
            currentMode = ClickMode.Normal;
            EventManager.instance.ModeChanged(currentMode);
        }

        if (Input.GetKeyUp(KeyCode.B)){
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

    public void SwitchMode(ClickMode m){
        currentMode = m;
        EventManager.instance.ModeChanged(m);
    }

    public void SetSelectedBuildingType(BuildingTypeSO type){
        selectedBuildingSO = type;
        EventManager.instance.SelectedBuildingChanged();
    }

    void updateNeighbours(int x, int z)
    {
        Dictionary<string, bool> neighbours = grid.GetCell(x,z).AdjacentRoads;
        List<Vector2Int> positionList = roadStraight.GetPositionList(new Vector2Int(x, z), BuildingTypeSO.Direction.Up);
        
        Vector2Int rotationOffset;
        Vector3 worldPosition;
        Building placedBuilding;

        if ( neighbours["up"] && neighbours["down"] == false && neighbours["left"] == false && neighbours["right"] == false)
                {
                    //csak fel
                    rotationOffset = roadStraight.GetRotationOffset(BuildingTypeSO.Direction.Up);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    //Cell updateableCell = grid.GetCell(x,z+1);
                    //if (updateableCell == null) return;
                    
                    Building updateableRoad = grid.GetCell(x,z).GetBuilding();
                    deleteNeighbourRoad(updateableRoad,x,z);
                    
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Up, roadStraight, positionList);
                    grid.GetCell(x, z).SetBuilding(placedBuilding);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                }
                else if (neighbours["up"] == false && neighbours["down"] && neighbours["left"] == false && neighbours["right"] == false)
                {
                    //csak le
                    rotationOffset = roadStraight.GetRotationOffset(BuildingTypeSO.Direction.Up);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    //Cell updateableCell = grid.GetCell(x,z+1);
                    //if (updateableCell == null) return;
                    
                    Building updateableRoad = grid.GetCell(x,z).GetBuilding();
                    deleteNeighbourRoad(updateableRoad,x,z);
                    
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Up, roadStraight, positionList);
                    grid.GetCell(x, z).SetBuilding(placedBuilding);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                    
                }
                else if (neighbours["up"] == false && neighbours["down"] == false && neighbours["left"] && neighbours["right"] == false)
                {
                    //csak bal
                    rotationOffset = roadStraight.GetRotationOffset(BuildingTypeSO.Direction.Right);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    //Cell updateableCell = grid.GetCell(x,z+1);
                    //if (updateableCell == null) return;
                    
                    Building updateableRoad = grid.GetCell(x,z).GetBuilding();
                    deleteNeighbourRoad(updateableRoad,x,z);
                    
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Right, roadStraight, positionList);
                    grid.GetCell(x, z).SetBuilding(placedBuilding);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                }
                else if (neighbours["up"] == false && neighbours["down"] == false && neighbours["left"] == false && neighbours["right"])
                {
                    //csak jobb
                    rotationOffset = roadStraight.GetRotationOffset(BuildingTypeSO.Direction.Left);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    //Cell updateableCell = grid.GetCell(x,z+1);
                    //if (updateableCell == null) return;
                    
                    Building updateableRoad = grid.GetCell(x,z).GetBuilding();
                    deleteNeighbourRoad(updateableRoad,x,z);

                    
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Left, roadStraight, positionList);
                    grid.GetCell(x, z).SetBuilding(placedBuilding);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                }
                else if (neighbours["up"]  && neighbours["down"] && neighbours["left"] == false && neighbours["right"] == false)
                {
                    //fent és lent
                    rotationOffset = roadStraight.GetRotationOffset(BuildingTypeSO.Direction.Up);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();

                    /*Cell updateableCell = grid.GetCell(x,z+1);
                    if (updateableCell == null) return;*/
                    
                    Building updateableRoad = grid.GetCell(x,z).GetBuilding();
                    deleteNeighbourRoad(updateableRoad,x,z);

                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), BuildingTypeSO.Direction.Up, roadStraight, positionList);
                    grid.GetCell(x, z).SetBuilding(placedBuilding);

                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                }
                else if (neighbours["up"] && neighbours["down"] == false && neighbours["left"] && neighbours["right"] == false)
                {
                    //fel és bal
                    rotationOffset = roadTurn.GetRotationOffset(BuildingTypeSO.Direction.Up);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();

                    /*Cell updateableCell = grid.GetCell(x,z+1);
                    if (updateableCell == null) return;*/
                    
                    Building updateableRoad = grid.GetCell(x,z).GetBuilding();
                    deleteNeighbourRoad(updateableRoad,x,z);

                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), BuildingTypeSO.Direction.Up, roadTurn, positionList);
                    grid.GetCell(x, z).SetBuilding(placedBuilding);

                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                }
                else if (neighbours["up"]  && neighbours["down"] == false && neighbours["left"] == false && neighbours["right"])
                {
                    //fel és jobb
                    rotationOffset = roadTurn.GetRotationOffset(BuildingTypeSO.Direction.Right);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();

                    /*Cell updateableCell = grid.GetCell(x,z+1);
                    if (updateableCell == null) return;*/
                    
                    Building updateableRoad = grid.GetCell(x,z).GetBuilding();
                    deleteNeighbourRoad(updateableRoad,x,z);

                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), BuildingTypeSO.Direction.Right, roadTurn, positionList);
                    grid.GetCell(x, z).SetBuilding(placedBuilding);

                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                }
                else if (neighbours["up"] == false && neighbours["down"] && neighbours["left"]  && neighbours["right"] == false )
                {
                    //le és bal
                    rotationOffset = roadTurn.GetRotationOffset(BuildingTypeSO.Direction.Left);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();

                    /*Cell updateableCell = grid.GetCell(x,z+1);
                    if (updateableCell == null) return;*/
                    
                    Building updateableRoad = grid.GetCell(x,z).GetBuilding();
                    deleteNeighbourRoad(updateableRoad,x,z);

                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), BuildingTypeSO.Direction.Left, roadTurn, positionList);
                    grid.GetCell(x, z).SetBuilding(placedBuilding);

                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                }
                else if (neighbours["up"] == false && neighbours["down"] && neighbours["left"] == false && neighbours["right"])
                {
                    //le és jobb
                    rotationOffset = roadTurn.GetRotationOffset(BuildingTypeSO.Direction.Down);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();

                    /*Cell updateableCell = grid.GetCell(x,z+1);
                    if (updateableCell == null) return;*/
                    
                    Building updateableRoad = grid.GetCell(x,z).GetBuilding();
                    deleteNeighbourRoad(updateableRoad,x,z);

                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), BuildingTypeSO.Direction.Down, roadTurn, positionList);
                    grid.GetCell(x, z).SetBuilding(placedBuilding);

                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                }
                else if (neighbours["up"] == false && neighbours["down"] == false && neighbours["left"] && neighbours["right"])
                {
                    //bal és jobb
                    rotationOffset = roadStraight.GetRotationOffset(BuildingTypeSO.Direction.Right);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();

                    /*Cell updateableCell = grid.GetCell(x,z+1);
                    if (updateableCell == null) return;*/
                    
                    Building updateableRoad = grid.GetCell(x,z).GetBuilding();
                    deleteNeighbourRoad(updateableRoad,x,z);

                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), BuildingTypeSO.Direction.Right, roadStraight, positionList);
                    grid.GetCell(x, z).SetBuilding(placedBuilding);

                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                }
                else if (neighbours["up"]  && neighbours["down"] && neighbours["left"]  && neighbours["right"] == false)
                {
                    //fent le bal
                    rotationOffset = roadT.GetRotationOffset(BuildingTypeSO.Direction.Up);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();

                    /*Cell updateableCell = grid.GetCell(x,z+1);
                    if (updateableCell == null) return;*/
                    
                    Building updateableRoad = grid.GetCell(x,z).GetBuilding();
                    deleteNeighbourRoad(updateableRoad,x,z);

                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), BuildingTypeSO.Direction.Up, roadT, positionList);
                    grid.GetCell(x, z).SetBuilding(placedBuilding);

                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                }
                else if (neighbours["up"]  && neighbours["down"] && neighbours["left"] == false && neighbours["right"])
                {
                    //fent le jobb
                    rotationOffset = roadT.GetRotationOffset(BuildingTypeSO.Direction.Down);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();

                    /*Cell updateableCell = grid.GetCell(x,z+1);
                    if (updateableCell == null) return;*/
                    
                    Building updateableRoad = grid.GetCell(x,z).GetBuilding();
                    deleteNeighbourRoad(updateableRoad,x,z);

                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), BuildingTypeSO.Direction.Down, roadT, positionList);
                    grid.GetCell(x, z).SetBuilding(placedBuilding);

                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                }
                else if (neighbours["up"]  && neighbours["down"] == false && neighbours["left"]  && neighbours["right"])
                {
                    //fent bal joob
                    rotationOffset = roadT.GetRotationOffset(BuildingTypeSO.Direction.Right);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();

                    /*Cell updateableCell = grid.GetCell(x,z+1);
                    if (updateableCell == null) return;*/
                    
                    Building updateableRoad = grid.GetCell(x,z).GetBuilding();
                    deleteNeighbourRoad(updateableRoad,x,z);

                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), BuildingTypeSO.Direction.Right, roadT, positionList);
                    grid.GetCell(x, z).SetBuilding(placedBuilding);

                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                }
                else if (neighbours["up"] == false  && neighbours["down"] && neighbours["left"]  && neighbours["right"])
                {
                    //le bal jobb
                    rotationOffset = roadT.GetRotationOffset(BuildingTypeSO.Direction.Left);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();

                    /*Cell updateableCell = grid.GetCell(x,z+1);
                    if (updateableCell == null) return;*/
                    
                    Building updateableRoad = grid.GetCell(x,z).GetBuilding();
                    deleteNeighbourRoad(updateableRoad,x,z);

                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), BuildingTypeSO.Direction.Left, roadT, positionList);
                    grid.GetCell(x, z).SetBuilding(placedBuilding);

                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                }
                else if (neighbours["up"]  && neighbours["down"]  && neighbours["left"]  && neighbours["right"])
                {
                    //minden oldal
                    rotationOffset = roadX.GetRotationOffset(BuildingTypeSO.Direction.Up);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();

                    /*Cell updateableCell = grid.GetCell(x,z+1);
                    if (updateableCell == null) return;*/
                    
                    Building updateableRoad = grid.GetCell(x,z).GetBuilding();
                    deleteNeighbourRoad(updateableRoad,x,z);

                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), BuildingTypeSO.Direction.Up, roadX, positionList);
                    grid.GetCell(x, z).SetBuilding(placedBuilding);


                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                }
                else
                {
                    
                }
    }

    void deleteNeighbourRoad(Building updateableRoad, int x,int z)
    {
        //if (updateableRoad != null)
        //{
            grid.GetCell(x,z).ClearBuilding();
            placedBuildings.Remove(updateableRoad);
            updateableRoad.Destroy();
        //}
    }

    void placeRoad()
    {
        try{
            int x, z;
            grid.XZFromWorldPosition(GetMouseWorldPosition(), out x, out z);

            // Get list of the buildings coordinates
            List<Vector2Int> positionList =
                roadStraight.GetPositionList(new Vector2Int(x, z), BuildingTypeSO.Direction.Up);

            // Check if all coordinates are empty
            bool canBuild = true;
            foreach (Vector2Int gridPosition in positionList){
                // If any of the cells is occupied, don't build
                try{
                    if (!grid.GetCell(gridPosition.x, gridPosition.y).IsEmpty()){
                        canBuild = false;
                        //Debug.Log("Placing on (" + x + "," + z + ") would collide with another building");
                        break;
                    }
                }
                catch (NotValidCellException e){
                    canBuild = false;
                }
            }

            if (GameManager.instance.Money < roadStraight.price){
                canBuild = false;
                SetSelectedBuildingType(null);
            }

            // Place building if area is clear
            if (canBuild)
            {
                Dictionary<string, bool> neighbours = grid.GetCell(x,z).AdjacentRoads;
                //Debug.Log(neighbours["up"]+" "+neighbours["down"]+" "+neighbours["left"]+" "+neighbours["right"]);
                Vector2Int rotationOffset;
                Vector3 worldPosition;
                Building placedBuilding;
                //Debug.Log("Name: "+cellNeighbours[0].GetBuilding().Name);

                //Lehetséges esetek
                if ( neighbours["up"] && neighbours["down"] == false && neighbours["left"] == false && neighbours["right"] == false)
                {
                    //csak fel
                    rotationOffset = roadStraight.GetRotationOffset(BuildingTypeSO.Direction.Up);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Up, roadStraight, positionList);
                    grid.GetCell(x,z).SetBuilding(placedBuilding);
                    GameManager.instance.BuyBuilding(roadStraight);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                    updateNeighbours(x,z+1);
                }
                else if (neighbours["up"] == false && neighbours["down"] && neighbours["left"] == false && neighbours["right"] == false)
                {
                    //csak le
                    rotationOffset = roadStraight.GetRotationOffset(BuildingTypeSO.Direction.Down);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Down, roadStraight, positionList);
                    grid.GetCell(x,z).SetBuilding(placedBuilding);
                    GameManager.instance.BuyBuilding(roadStraight);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                    updateNeighbours(x,z-1);
                }
                else if (neighbours["up"] == false && neighbours["down"] == false && neighbours["left"] == true && neighbours["right"] == false)
                {
                    //csak bal
                    rotationOffset = roadStraight.GetRotationOffset(BuildingTypeSO.Direction.Right);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Right, roadStraight, positionList);
                    grid.GetCell(x,z).SetBuilding(placedBuilding);
                    GameManager.instance.BuyBuilding(roadStraight);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                    updateNeighbours(x-1,z);
                }
                else if (neighbours["up"] == false && neighbours["down"] == false && neighbours["left"] == false && neighbours["right"])
                {
                    //csak jobb
                    rotationOffset = roadStraight.GetRotationOffset(BuildingTypeSO.Direction.Left);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Left, roadStraight, positionList);
                    grid.GetCell(x,z).SetBuilding(placedBuilding);
                    GameManager.instance.BuyBuilding(roadStraight);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                    updateNeighbours(x+1,z);
                }
                else if (neighbours["up"]  && neighbours["down"] && neighbours["left"] == false && neighbours["right"] == false)
                {
                    //fent és lent
                    rotationOffset = roadStraight.GetRotationOffset(BuildingTypeSO.Direction.Up);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Up, roadStraight, positionList);
                    grid.GetCell(x,z).SetBuilding(placedBuilding);
                    GameManager.instance.BuyBuilding(roadStraight);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                    updateNeighbours(x,z+1);
                    updateNeighbours(x,z-1);
                }
                else if (neighbours["up"] && neighbours["down"] == false && neighbours["left"] && neighbours["right"] == false)
                {
                    //fel és bal
                    rotationOffset = roadTurn.GetRotationOffset(BuildingTypeSO.Direction.Up);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Up, roadTurn, positionList);
                    grid.GetCell(x,z).SetBuilding(placedBuilding);
                    GameManager.instance.BuyBuilding(roadStraight);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                    updateNeighbours(x,z+1);
                    updateNeighbours(x-1,z);
                }
                else if (neighbours["up"]  && neighbours["down"] == false && neighbours["left"] == false && neighbours["right"])
                {
                    //fel és jobb
                    rotationOffset = roadTurn.GetRotationOffset(BuildingTypeSO.Direction.Right);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Right, roadTurn, positionList);
                    grid.GetCell(x,z).SetBuilding(placedBuilding);
                    GameManager.instance.BuyBuilding(roadStraight);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                    updateNeighbours(x,z+1);
                    updateNeighbours(x+1,z);
                }
                else if (neighbours["up"] == false && neighbours["down"] && neighbours["left"]  && neighbours["right"] == false )
                {
                    //le és bal
                    rotationOffset = roadTurn.GetRotationOffset(BuildingTypeSO.Direction.Left);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Left, roadTurn, positionList);
                    grid.GetCell(x,z).SetBuilding(placedBuilding);
                    GameManager.instance.BuyBuilding(roadStraight);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                    updateNeighbours(x,z-1);
                    updateNeighbours(x-1,z);
                }
                else if (neighbours["up"] == false && neighbours["down"] && neighbours["left"] == false && neighbours["right"])
                {
                    //le és jobb
                    rotationOffset = roadTurn.GetRotationOffset(BuildingTypeSO.Direction.Down);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Down, roadTurn, positionList);
                    grid.GetCell(x,z).SetBuilding(placedBuilding);
                    GameManager.instance.BuyBuilding(roadStraight);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                    updateNeighbours(x,z-1);
                    updateNeighbours(x+1,z);
                }
                else if (neighbours["up"] == false && neighbours["down"] == false && neighbours["left"] && neighbours["right"])
                {
                    //bal és jobb
                    rotationOffset = roadTurn.GetRotationOffset(BuildingTypeSO.Direction.Right);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Right, roadTurn, positionList);
                    grid.GetCell(x,z).SetBuilding(placedBuilding);
                    GameManager.instance.BuyBuilding(roadStraight);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                    updateNeighbours(x-1,z);
                    updateNeighbours(x+1,z);
                }
                else if (neighbours["up"]  && neighbours["down"] && neighbours["left"]  && neighbours["right"] == false)
                {
                    //fent le bal
                    rotationOffset = roadT.GetRotationOffset(BuildingTypeSO.Direction.Up);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Up, roadT, positionList);
                    grid.GetCell(x,z).SetBuilding(placedBuilding);
                    GameManager.instance.BuyBuilding(roadStraight);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                    updateNeighbours(x,z+1);
                    updateNeighbours(x,z-1);
                    updateNeighbours(x-1,z);
                }
                else if (neighbours["up"]  && neighbours["down"] && neighbours["left"] == false && neighbours["right"])
                {
                    //fent le jobb
                    rotationOffset = roadT.GetRotationOffset(BuildingTypeSO.Direction.Down);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Down, roadT, positionList);
                    grid.GetCell(x,z).SetBuilding(placedBuilding);
                    GameManager.instance.BuyBuilding(roadStraight);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                    updateNeighbours(x,z+1);
                    updateNeighbours(x,z-1);
                    updateNeighbours(x+1,z);
                }
                else if (neighbours["up"]  && neighbours["down"] == false && neighbours["left"]  && neighbours["right"])
                {
                    //fent bal joob
                    rotationOffset = roadT.GetRotationOffset(BuildingTypeSO.Direction.Right);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Right, roadT, positionList);
                    grid.GetCell(x,z).SetBuilding(placedBuilding);
                    GameManager.instance.BuyBuilding(roadStraight);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                    updateNeighbours(x,z+1);
                    updateNeighbours(x-1,z);
                    updateNeighbours(x+1,z);
                }
                else if (neighbours["up"] == false  && neighbours["down"] && neighbours["left"]  && neighbours["right"])
                {
                    //le bal jobb
                    rotationOffset = roadT.GetRotationOffset(BuildingTypeSO.Direction.Left);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Left, roadT, positionList);
                    grid.GetCell(x,z).SetBuilding(placedBuilding);
                    GameManager.instance.BuyBuilding(roadStraight);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                    updateNeighbours(x,z-1);
                    updateNeighbours(x-1,z);
                    updateNeighbours(x+1,z);
                }
                else if (neighbours["up"]  && neighbours["down"]  && neighbours["left"]  && neighbours["right"])
                {
                    //minden oldal
                    rotationOffset = roadX.GetRotationOffset(BuildingTypeSO.Direction.Up);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Up, roadX, positionList);
                    grid.GetCell(x,z).SetBuilding(placedBuilding);
                    GameManager.instance.BuyBuilding(roadStraight);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                    updateNeighbours(x,z+1);
                    updateNeighbours(x,z-1);
                    updateNeighbours(x+1,z);
                    updateNeighbours(x-1,z);
                }
                else
                {
                    rotationOffset = roadX.GetRotationOffset(BuildingTypeSO.Direction.Up);
                    worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                        BuildingTypeSO.Direction.Up, roadX, positionList);
                    grid.GetCell(x,z).SetBuilding(placedBuilding);
                    GameManager.instance.BuyBuilding(roadStraight);
                    //EventManager.instance.MapChanged();
                    placedBuildings.Add(placedBuilding);
                }
                EventManager.instance.MapChanged();
                
                if (!Input.GetKey(KeyCode.LeftShift)){
                    SetSelectedBuildingType(null);
                }
            }
        }
        catch (Exception e){
            //Debug.Log("Mouse out of the map :(");
        }
    }

    void PlaceBuilding(){
        try{
            int x, z;
            grid.XZFromWorldPosition(GetMouseWorldPosition(), out x, out z);

            // Get list of the buildings coordinates
            List<Vector2Int> positionList =
                selectedBuildingSO.GetPositionList(new Vector2Int(x, z), currentBuildingRotation);

            // Check if all coordinates are empty
            bool canBuild = true;
            foreach (Vector2Int gridPosition in positionList){
                // If any of the cells is occupied, don't build
                try{
                    if (!grid.GetCell(gridPosition.x, gridPosition.y).IsEmpty()){
                        canBuild = false;
                        //Debug.Log("Placing on (" + x + "," + z + ") would collide with another building");
                        break;
                    }
                }
                catch (NotValidCellException e){
                    canBuild = false;
                }
            }

            if (GameManager.instance.Money < selectedBuildingSO.price){
                canBuild = false;
                SetSelectedBuildingType(null);
            }

            // Place building if area is clear
            if (canBuild){
                Vector2Int rotationOffset = selectedBuildingSO.GetRotationOffset(currentBuildingRotation);
                Vector3 worldPosition = grid.GetWorldPosition(x, z) +
                                        new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                Building placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z),
                    currentBuildingRotation, selectedBuildingSO, positionList);

                //placedBuilding.RemovePreviewBox();
                foreach (Vector2Int gridPositions in positionList){
                    grid.GetCell(gridPositions.x, gridPositions.y).SetBuilding(placedBuilding);
                }

                GameManager.instance.BuyBuilding(selectedBuildingSO);


                if (!Input.GetKey(KeyCode.LeftShift)){
                    SetSelectedBuildingType(null);
                }


                EventManager.instance.MapChanged();
                placedBuildings.Add(placedBuilding);
            }
        }
        catch (Exception e){
            //Debug.Log("Mouse out of the map :(");
        }
    }

    private void SellBuilding(){
        Cell clickedCell = grid.GetCell(GetMouseWorldPosition());
        if (clickedCell == null) return;
        Building clickedBuilding = clickedCell.GetBuilding();
        if (clickedBuilding != null){
            if (clickedBuilding.Type.type == BuildingTypeSO.Type.Road)
            {
                int x, z;
                grid.XZFromWorldPosition(clickedBuilding.Position, out x, out z);
                List<Cell> neighbours = grid.GetCell(x, z).Neighbours;
                grid.GetCell(x, z).ClearBuilding();
                placedBuildings.Remove(clickedBuilding);
                GameManager.instance.SellBuilding(clickedBuilding);
                clickedBuilding.Destroy();
                Debug.Log("COUNT: "+neighbours.Count);
                foreach (Cell neighbour in neighbours)
                {
                    Debug.Log(neighbour.PositionString+"   BUILDING TYPE: "+neighbour.GetBuilding());
                    if (neighbour.GetBuilding() != null && neighbour.GetBuilding().Type.type == BuildingTypeSO.Type.Road)
                    {
                        updateNeighbours(neighbour.GetX(),neighbour.GetY());
                        Debug.Log("BELEMENT MERT ROAD VOLT");
                    }
                }
            }
            else
            {
                List<Vector2Int> destroyedCoordinates = clickedBuilding.GetGridPositionList();
                foreach (Vector2Int gridPos in destroyedCoordinates){
                    grid.GetCell(gridPos.x, gridPos.y).ClearBuilding();
                }
                placedBuildings.Remove(clickedBuilding);
                GameManager.instance.SellBuilding(clickedBuilding);
                clickedBuilding.Destroy();
            }
            Invoke(nameof(Aaaaa), 0.1f);
        }
    }

    private void Aaaaa(){
        EventManager.instance.MapChanged();
    }

    public Quaternion GetCurrentBuildingRotation(){
        return Quaternion.Euler(0, selectedBuildingSO.GetRotationAngle(currentBuildingRotation), 0);
    }

    public BuildingTypeSO GetSelectedBuildingType(){
        return selectedBuildingSO;
    }

    // Converts pointer position to world position
    // layer 8: ground
    public Vector3 GetMouseWorldPosition(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, 1000f, (1 << 8))){
            return hitInfo.point;
        }
        else{
            //DO NOT BUILD
            throw new MouseOutOfMapException("Invalid position!");
        }
    }

    // Snaps worlds position to the grid for building preview ghost
    // layer 8 :ground
    public Vector3 GetMouseWorldSnappedPosition(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, 1000f, (1 << 8))){
            Vector3 mousePosition = hitInfo.point;

            grid.XZFromWorldPosition(mousePosition, out int x, out int z);

            if (selectedBuildingSO != null){
                Vector2Int rotationOffset = selectedBuildingSO.GetRotationOffset(currentBuildingRotation);
                Vector3 previewPosition = grid.GetWorldPosition(x, z) +
                                          new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                return previewPosition;
            }
            else{
                return mousePosition;
            }
        }
        else{
            //DO NOT BUILD
            throw new MouseOutOfMapException("Invalid position!");
        }
    }
}