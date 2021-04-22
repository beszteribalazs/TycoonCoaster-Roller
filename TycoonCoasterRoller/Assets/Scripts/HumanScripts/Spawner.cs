using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour{
    public GameObject visitorPrefab; //prefab to spawn
    public GameObject mechanicPrefab;
    public GameObject janitorPrefab;
    public GameObject prefabParent; //parent gameobject of the prefab


    int lastSpawnedTick = 0;
    int spawnFrequency = 1;

    void Update(){
        if (TimeManager.instance.Tick - lastSpawnedTick >= spawnFrequency){
            int available = (int)GameManager.instance.TotalCapacity - (int)GameManager.instance.CurrentVisitors; 
            if (available > 0){
                float chance = ((available * 2f) / GameManager.instance.TotalCapacity);
                //Debug.Log(chance);
                if (Random.Range(0f, 1f) - 0.3f <= chance){
                    SpawnVisitor(BuildingSystem.instance.entryPoint.position + new Vector3(1, 0, 1) * (BuildingSystem.instance.CellSize / 2));    
                }
                lastSpawnedTick = TimeManager.instance.Tick;
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