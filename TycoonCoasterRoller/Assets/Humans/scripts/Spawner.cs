using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour{
    [SerializeField] GameObject visitorPrefab; //prefab to spawn
    [SerializeField] GameObject mechanicPrefab;
    [SerializeField] GameObject janitorPrefab;
    public GameObject prefabParent; //parent gameobject of the prefab


    int lastSpawnedTick = 0;
    int spawnFrequency = 10;

    void Update(){
        if (TimeManager.instance.Tick - lastSpawnedTick >= spawnFrequency){
            if (GameManager.instance.TotalCapacity - GameManager.instance.CurrentVisitors > 0){
                lastSpawnedTick = TimeManager.instance.Tick;
                SpawnVisitor(BuildingSystem.instance.entryPoint.position + new Vector3(1, 0, 1) * (BuildingSystem.instance.CellSize / 2));
            }
        }
    }


    public GameObject SpawnVisitor(Vector3 pos){
        GameObject newNPC = Instantiate(visitorPrefab, pos, Quaternion.identity);
        GameManager.instance.CurrentVisitors++;
        return newNPC;
    }

    public GameObject SpawnMechanic(Vector3 pos){
        GameObject newNPC = Instantiate(mechanicPrefab, pos, Quaternion.identity);
        return newNPC;
    }

    public GameObject SpawnJanitor(Vector3 pos){
        GameObject newNPC = Instantiate(janitorPrefab, pos, Quaternion.identity);
        return newNPC;
    }
}