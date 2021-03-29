using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject newPrefab; //prefab to spawn
    public GameObject prefabParent; //parent gameobject of the prefab

    public void Spawn(Vector3 pos)
    {
        GameObject newNPC = Instantiate(newPrefab, pos, Quaternion.identity);
        if (prefabParent != null) //Check if parent is set
        {
            //newNPC.transform.parent = prefabParent.transform; //Move into to parent gameobject
        }
    }
}