using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSystem : MonoBehaviour{
    //public static BuildingSystem instance;
    GridXZ grid;
    Transform ground;

    [Header("Setup")] [SerializeField] int gridWidth;
    [SerializeField] int gridHeight;
    [SerializeField] float cellSize = 3f;
    [SerializeField] List<BuildingTypeSO> placableBuildings;
    [SerializeField] Transform groundVisualPrefab;

    BuildingTypeSO selectedBuildingSO;
    BuildingTypeSO.Direction currentBuildingRotation;

    public List<Building> Buildings => grid.GetBuildingList();
    
    void Awake(){
        //instance = this;
        gridWidth = MapSizeController.mapSize;
        gridHeight = MapSizeController.mapSize;
        grid = new GridXZ(gridWidth, gridHeight, cellSize, Vector3.zero);

        // Create ground visual
        float groundSizeX = gridWidth * cellSize;
        float groundSizeY = gridHeight * cellSize;
        ground = Instantiate(groundVisualPrefab, new Vector3(groundSizeX / 2, -0.5f, groundSizeY / 2), Quaternion.identity);
        ground.localScale = new Vector3(groundSizeX, 1, groundSizeY);

        selectedBuildingSO = null;
        currentBuildingRotation = BuildingTypeSO.Direction.Down;
    }

    int buildingIndex = 0;
    
    void Update(){
        // Place building
        if (Input.GetMouseButtonDown(0)){
            //left click
            PlaceBuilding();
        }
        // Destroy building
        else if (Input.GetMouseButtonDown(1)){
            //right click
            DestroyBuilding();
        }

        // Rotate building
        if (Input.GetKeyDown(KeyCode.R)){
            currentBuildingRotation = BuildingTypeSO.GetNextDirectionLeft(currentBuildingRotation);
        }
        else if (Input.GetKeyDown(KeyCode.F)){
            currentBuildingRotation = BuildingTypeSO.GetNextDirectionRight(currentBuildingRotation);
        }
        
        //Cycle buildings
        if (Input.GetKeyDown(KeyCode.Tab)){
            selectedBuildingSO = placableBuildings[buildingIndex];
            EventManager.instance.SelectedBuildingChanged();
            buildingIndex++;
            if (buildingIndex >= placableBuildings.Count){
                buildingIndex = 0;
            }
        }
    }

    public void SetSelectedBuildingType(BuildingTypeSO type){
        selectedBuildingSO = type;
        EventManager.instance.SelectedBuildingChanged();
    }

    private void PlaceBuilding(){
        try{
            int x, z;
            grid.XZFromWorldPosition(GetMouseWorldPosition(), out x, out z);

            // Get list of the buildings coordinates
            List<Vector2Int> positionList = selectedBuildingSO.GetPositionList(new Vector2Int(x, z), currentBuildingRotation);

            // Check if all coordinates are empty
            bool canBuild = true;
            foreach (Vector2Int gridPosition in positionList){
                // If any of the cells is occupied, don't build
                try{
                    if (!grid.GetCell(gridPosition.x, gridPosition.y).IsEmpty()){
                        canBuild = false;
                        break;
                    }
                }
                catch (NotValidCellException e){
                    canBuild = false;
                }
            }

            // Place building if area is clear
            if (canBuild){
                Vector2Int rotationOffset = selectedBuildingSO.GetRotationOffset(currentBuildingRotation);
                Vector3 worldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                Building placedBuilding = Building.SpawnBuilding(worldPosition, new Vector2Int(x, z), currentBuildingRotation, selectedBuildingSO);

                //placedBuilding.RemovePreviewBox();
                foreach (Vector2Int gridPositions in positionList){
                    grid.GetCell(gridPositions.x, gridPositions.y).SetBuilding(placedBuilding);
                }
                SetSelectedBuildingType(null);
            }
            else{
                if (grid.GetCell(x, z).GetBuilding() != null){
                    Debug.Log("(" + x + "," + z + ") is already occupied by " + grid.GetCell(x, z).GetBuilding());
                }
                else{
                    Debug.Log("Placing on (" + x + "," + z + ") would collide with another building");
                }
            }
        }
        catch (Exception e){
            Console.WriteLine("Mouse out of the map :(");
        }
    }

    private void DestroyBuilding(){
        Cell clickedCell = grid.GetCell(GetMouseWorldPosition());
        Building clickedBuilding = clickedCell.GetBuilding();
        if (clickedBuilding != null){
            List<Vector2Int> destroyedCoordinates = clickedBuilding.GetGridPositionList();

            foreach (Vector2Int gridPos in destroyedCoordinates){
                grid.GetCell(gridPos.x, gridPos.y).ClearBuilding();
            }

            clickedBuilding.Destroy();
        }
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
                Vector3 previewPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
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