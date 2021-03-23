using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationManager : MonoBehaviour{
    NavMeshSurface surface;
    Spawner spawner;
    [SerializeField] BuildingSystem buildingSystem;

    void Start(){
        surface = GetComponent<NavMeshSurface>();
        EventManager.instance.onMapChanged += RebakeMap;
        spawner = GetComponent<Spawner>();
        RebakeMap();
    }

    private void RebakeMap(){
        surface.BuildNavMesh();
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.T)){
            spawner.Spawn(buildingSystem.entryPoint.position + new Vector3(1.5f, 0, 1.5f));
        }
    }
}