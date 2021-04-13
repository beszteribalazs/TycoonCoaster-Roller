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
        lastFramePosition = transform.position;
        transform.parent = GameObject.Find("Visitors").transform;

        /*foreach (Attraction attraction in GameManager.instance.ReachableAttractions){
            Debug.Log(attraction);
        }*/
    }

    Vector3 velocity;
    Vector3 lastFramePosition;
    void FixedUpdate(){
        velocity = transform.position - lastFramePosition;
        lastFramePosition = transform.position;
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.C)){
            agent.SetDestination(GetMouseWorldPosition());
        }

        if (Input.GetKeyDown(KeyCode.V)){
            GoToRandomBuilding();
        }

        if (velocity != Vector3.zero){
            Quaternion newRotation = Quaternion.Euler(0, 90, 0) * Quaternion.LookRotation(velocity, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, Time.deltaTime * 720);
        }
        
        //InvokeRepeating(nameof(RandomTarget), 0f, 1f);
    }

    void GoToRandomBuilding(){
        int target = Random.Range(0, GameManager.instance.ReachableAttractions.Count);
        Vector3 targetPosition = GameManager.instance.ReachableAttractions[target].Position;
        agent.SetDestination(targetPosition);
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

    /*
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
    }*/
}
