using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject visitorPrefab; //prefab to spawn
    [SerializeField] GameObject mechanicPrefab;
    [SerializeField] GameObject janitorPrefab;
    public GameObject prefabParent; //parent gameobject of the prefab


    public GameObject SpawnVisitor(Vector3 pos)
    {
        GameObject newNPC = Instantiate(visitorPrefab, pos, Quaternion.identity);
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