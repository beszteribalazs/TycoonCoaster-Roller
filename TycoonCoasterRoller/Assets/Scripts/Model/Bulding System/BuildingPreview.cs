using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPreview : MonoBehaviour{
    [Header("Setup"), SerializeField] BuildingSystem buildingSystem;
    Transform visual;
    BuildingTypeSO buildingType;

    Vector3 targetPosition;

    void Start(){
        EventManager.instance.onSelectedBuildingChanged += RefreshVisual;

        RefreshVisual();
        targetPosition = Vector3.zero;
    }


    void LateUpdate(){
        try{
            targetPosition = buildingSystem.GetMouseWorldSnappedPosition();
            targetPosition.y = 0f;
        }
        catch (Exception e){
            // ignored
        }

        if (visual != null){
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 20f);
            transform.rotation = Quaternion.Lerp(transform.rotation, buildingSystem.GetCurrentBuildingRotation(), Time.deltaTime * 20f);    
        }
        
    }

    void RefreshVisual(){
        if (visual != null){
            Destroy(visual.gameObject);
            visual = null;
        }

        buildingType = buildingSystem.GetSelectedBuildingType();

        if (buildingType != null){
            visual = Instantiate(buildingType.preview, Vector3.zero, Quaternion.identity);
            visual.parent = transform;
            visual.localPosition = Vector3.zero;
            visual.localEulerAngles = Vector3.zero;
        }
        else{
            if (visual != null){
                Destroy(visual.gameObject);
            }
        }
    }
}