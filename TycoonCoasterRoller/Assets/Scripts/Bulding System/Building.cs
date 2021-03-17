using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building: MonoBehaviour{
    BuildingTypeSO buildingType;
    Vector2Int gridOrigin;
    BuildingTypeSO.Direction buildingDirection;
    
    public List<Vector2Int> GetGridPositionList(){
        return buildingType.GetPositionList(gridOrigin, buildingDirection);
    }
    
    public void Destroy(){
        Destroy(this.gameObject);
    }

    public static Building SpawnBuilding(Vector3 worldPosition, Vector2Int gridOrigin, BuildingTypeSO.Direction buildingDirection, BuildingTypeSO buildingType){
        Quaternion worldRotation = Quaternion.Euler(0, buildingType.GetRotationAngle(buildingDirection), 0);
        Transform spawnedBuildingTransform = Instantiate(buildingType.prefab, worldPosition, worldRotation);
        Building spawnedBuilding = spawnedBuildingTransform.GetComponent<Building>();

        spawnedBuilding.buildingType = buildingType;
        spawnedBuilding.gridOrigin = gridOrigin;
        spawnedBuilding.buildingDirection = buildingDirection;
        return spawnedBuilding;
    }

    public override string ToString(){
        return buildingType.buildingName;
    }
}
