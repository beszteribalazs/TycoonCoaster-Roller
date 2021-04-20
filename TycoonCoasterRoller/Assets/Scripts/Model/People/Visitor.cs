using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Visitor : Person{
    int ticksToStayInPark;
    int enteredPark;
    int minStayTick = 120;
    int maxStayTick = 720;

    protected override void Awake(){
        base.Awake();
        EventManager.instance.onMapChanged += DelayedRecheck;
        walkSpeedMultiplier = Random.Range(0.9f, 1.1f);
    }

    protected override void OnDestroy(){
        base.OnDestroy();
        EventManager.instance.onMapChanged -= DelayedRecheck;
    }

    void DelayedRecheck(){
        Invoke(nameof(RecheckNavigationTarget), 0.1f);
    }

    void RecheckNavigationTarget(){
        int x, z;
        BuildingSystem.instance.grid.XZFromWorldPosition(transform.position, out x, out z);
        if (BuildingSystem.instance.grid.GetCell(x, z) == null){
            transform.position = BuildingSystem.instance.entryPoint.position + Vector3.one * BuildingSystem.instance.grid.GetCellSize();
            RecheckNavigationTarget();
            return;
        }

        // if going to attraction
        if (goingToAttraction){
            // recalculate available buildings
            List<Attraction> reachable = CalculateReachablePositions();
            //Debug.Log(reachable.Count);
            // if cant reach target, choose a new one
            if (!reachable.Contains(target)){
                GoToRandomBuilding();
            }
            else{
                GoToBuilding(target);
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
        enteredPark = TimeManager.instance.Tick;
        ticksToStayInPark = Random.Range(minStayTick, maxStayTick);
        transform.parent = GameObject.Find("Visitors").transform;
        GoToRandomBuilding();
    }

    bool inBuilding = false;
    int tickToStay = 30;
    int enterTime;


    int tickToRetarget = 10;
    int lastRetarget = -1000;

    protected override void Update(){
        base.Update();

        if (!wantsToLeave){
            if (TimeManager.instance.Tick - enteredPark >= ticksToStayInPark){
                if (inBuilding){
                    LeaveBuilding();
                }
                TryToLeavePark();
            }    
        }
        

        if (inBuilding){
            if (TimeManager.instance.Tick - enterTime >= tickToStay){
                LeaveBuilding();
            }
        }

        if (leaving){
            if ((transform.position - targetPosition).magnitude <= 0.1f){
                EventManager.instance.onSpeedChanged -= ChangeSpeed;
                EventManager.instance.onMapChanged -= DelayedRecheck;
                GameManager.instance.CurrentVisitors--;
                Destroy(gameObject);
            }
        }
        else if (goingToRoad && wantsToLeave){
            if (TimeManager.instance.Tick - lastRetarget >= tickToRetarget){
                if ((transform.position - targetPosition).magnitude <= visitDistance){
                    goingToRoad = false;
                    TryToLeavePark();
                }

                lastRetarget = TimeManager.instance.Tick;
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
                    if (TimeManager.instance.Tick - lastRetarget >= tickToRetarget){
                        if ((transform.position - targetPosition).magnitude <= visitDistance){
                            goingToRoad = false;
                            GoToRandomRoad();
                        }

                        lastRetarget = TimeManager.instance.Tick;
                    }
                }
            }
        }
    }

    void TryToEnterBuilding(){
        goingToAttraction = false;
        // if target is full or broke
        if (target.peopleInside.Count >= target.TotalCapacity || target.Broke){
            // go to a random road then go to a random building
            //GoToRandomRoad();
            GoToRandomBuilding();
        }
        // enter building
        else{
            EnterBuilding();
        }
    }


    void EnterBuilding(){
        tickToStay = Random.Range(30, 181);
        goingToAttraction = false;
        target.peopleInside.Add(this);
        previousBuilding = target;
        mesh.SetActive(false);
        inBuilding = true;
        enterTime = TimeManager.instance.Tick;
        //Invoke(nameof(LeaveBuilding), 10000f);
    }

    public void LeaveBuilding(){
        inBuilding = false;
        target.peopleInside.Remove(this);
        mesh.SetActive(true);
        GoToRandomBuilding();
    }

    public void EjectBuilding(){
        LeaveBuilding();
        TryToLeavePark();
        transform.position = BuildingSystem.instance.entryPoint.position;
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
            //target = reachable[Random.Range(0, reachable.Count)];

            // choose an enterable building as target
            List<Attraction> enterable = new List<Attraction>();
            foreach (Attraction attraction in reachable){
                if (attraction.CurrentVisitorCount < attraction.TotalCapacity){
                    enterable.Add(attraction);
                }
            }

            if (enterable.Count == 0){
                TryToLeavePark();
                return;
            }

            target = enterable[Random.Range(0, enterable.Count)];


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