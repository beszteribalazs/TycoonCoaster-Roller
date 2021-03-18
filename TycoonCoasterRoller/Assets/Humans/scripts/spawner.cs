using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawner : MonoBehaviour
{
    public GameObject newPrefab; //prefab to spawn
    public GameObject prefabParent; //parent gameobject of the prefab

    public void Spawn()
    {
        GameObject newNPC = Instantiate(newPrefab, new Vector3(0, 0, 0), transform.rotation);
        if (prefabParent != null) //Check if parent is set
        {
            newNPC.transform.parent = prefabParent.transform; //Move into to parent gameobject
        }
    }
}