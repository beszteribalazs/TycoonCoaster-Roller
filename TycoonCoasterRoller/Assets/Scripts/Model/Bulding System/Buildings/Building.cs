﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Building : MonoBehaviour{
    protected BuildingTypeSO buildingType;
    Vector2Int gridOrigin;
    BuildingTypeSO.Direction buildingDirection;

    public abstract float SellPrice{ get; }
    public abstract float Upkeep{ get; }
    public abstract float Income{ get; }
    public abstract float BreakChance{ get; }
    public abstract bool Broke{ get; set; }

    public BuildingTypeSO Type => buildingType;
    
    public string Name => buildingType.buildingName;

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