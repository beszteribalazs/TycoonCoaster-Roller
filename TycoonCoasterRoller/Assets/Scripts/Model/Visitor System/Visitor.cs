using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Visitor : Person{
    Attraction previousBuilding = null;

    protected override void Awake(){
        base.Awake();
        EventManager.instance.onMapChanged += RecheckNavigationTarget;
    }

    void RecheckNavigationTarget(){
        // if going to attraction
        if (goingToAttraction){
            // recalculate available buildings
            List<Attraction> reachable = CalculateReachablePositions();
            //Debug.Log(reachable.Count);
            // if cant reach target, choose a new one
            if (!reachable.Contains(target)){
                GoToRandomBuilding();
            }
        }
        // if going to road
        else if (goingToRoad){
            // recalculate available roads
            CalculateReachablePositions();
            if (!reachableRoads.Contains(roadTarget)){
                GoToRandomRoad();
            }
        }
        // if going to exit
        else if (leaving){
            // recalculate available roads
            CalculateReachablePositions();
            if (!reachableRoads.Contains(roadTarget)){
                GoToRandomRoad();
            }
        }
    }


    protected override void Start(){
        base.Start();
        transform.parent = GameObject.Find("Visitors").transform;
        GoToRandomBuilding();
    }


    protected override void Update(){
        base.Update();
        /*if (Input.GetKeyDown(KeyCode.C)){
            agent.SetDestination(GetMouseWorldPosition());
        }*/

        /*if (Input.GetKeyDown(KeyCode.V)){
            GoToRandomBuilding();
        }*/
        

        if (leaving){
            if ((transform.position - targetPosition).magnitude <= 0.1f){
                EventManager.instance.onSpeedChanged -= ChangeSpeed;
                EventManager.instance.onMapChanged -= RecheckNavigationTarget;
                Destroy(gameObject);
            }
        }
        else if (goingToRoad && wantsToLeave){
            if ((transform.position - targetPosition).magnitude <= visitDistance){
                goingToRoad = false;
                TryToLeavePark();
            }
        }
        else{
            if (wantsToLeave){
                // try leaving
                if (!leaving){
                    TryToLeavePark();
                }
            }
            else{
                // Try to enter building if close to it
                if (goingToAttraction){
                    if ((transform.position - targetPosition).magnitude <= visitDistance){
                        TryToEnterBuilding();
                    }
                }

                if (goingToRoad){
                    if ((transform.position - targetPosition).magnitude <= visitDistance){
                        goingToRoad = false;
                        GoToRandomBuilding();
                    }
                }
            }
        }
    }

    void TryToEnterBuilding(){
        goingToAttraction = false;
        // if target is full
        if (target.peopleInside.Count >= target.TotalCapacity){
            // go to a random road then go to a random building
            GoToRandomRoad();
        }
        // enter building
        else{
            EnterBuilding();
        }
    }


    void EnterBuilding(){
        goingToAttraction = false;
        target.peopleInside.Add(this);
        previousBuilding = target;
        mesh.SetActive(false);
        Invoke(nameof(LeaveBuilding), 10000f);
    }

    public void LeaveBuilding(){
        target.peopleInside.Remove(this);
        mesh.SetActive(true);
        GoToRandomBuilding();
    }

    void GoToRandomBuilding(){
        /*int target = Random.Range(0, GameManager.instance.ReachableAttractions.Count);
        Vector3 targetPosition = GameManager.instance.ReachableAttractions[target].Position;
        agent.SetDestination(targetPosition);*/

        List<Attraction> reachable = CalculateReachablePositions();

        if (reachable.Count == 0){
            wantsToLeave = true;
        }
        else{
            // choose a random building as target
            target = reachable[Random.Range(0, reachable.Count)];

            // Find first cell from spawn
            int x;
            int z;
            BuildingSystem.instance.grid.XZFromWorldPosition(BuildingSystem.instance.entryPoint.position + Vector3.forward * BuildingSystem.instance.CellSize, out x, out z);

            //find the closest tile (that is reachable)
            float sqrDistance = Single.MaxValue;
            Vector3 closestPosition = Vector3.zero;
            foreach (Vector2Int coords in target.gridPositionlist){
                //if building has a tile on the root tile, only this tile is reachable so go there
                if (coords.x == x && coords.y == z){
                    closestPosition = BuildingSystem.instance.grid.GetCell(coords.x, coords.y).WorldPosition;
                    break;
                }

                if (reachableCells.Contains(BuildingSystem.instance.grid.GetCell(coords.x, coords.y))){
                    float tmpdist = (BuildingSystem.instance.grid.GetCell(coords.x, coords.y).WorldPosition - transform.position).sqrMagnitude;
                    if (tmpdist < sqrDistance){
                        sqrDistance = tmpdist;
                        closestPosition = BuildingSystem.instance.grid.GetCell(coords.x, coords.y).WorldPosition;
                    }
                }
            }

            //Vector3 targetPosition = reachable[target].Position;
            targetPosition = closestPosition;
            agent.SetDestination(closestPosition);
            goingToAttraction = true;
        }
    }
}