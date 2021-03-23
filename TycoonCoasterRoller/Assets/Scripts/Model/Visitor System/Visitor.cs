using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Visitor : MonoBehaviour{
    NavMeshAgent agent;

    void Start(){
        agent = GetComponent<NavMeshAgent>();
    }
    

    void Update(){
        if (Input.GetKeyDown(KeyCode.C)){
            agent.SetDestination(GetMouseWorldPosition());
        }
        //InvokeRepeating(nameof(RandomTarget), 0f, 1f);
    }
    
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

    private void RandomTarget(){
        Vector3 newPos = RandomNavSphere(transform.position, 0.5f, ~(1 << 10));
        agent.SetDestination(newPos);
    }
    
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask) {
        Vector3 randDirection = Random.insideUnitSphere * dist;
 
        randDirection += origin;
 
        NavMeshHit navHit;
 
        NavMesh.SamplePosition (randDirection, out navHit, dist, layermask);
 
        return navHit.position;
    }
}
