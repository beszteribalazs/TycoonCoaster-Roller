using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechanic : Employee{
    Attraction targeted;
    bool targetRepaired = false;
    
    public void Repair(Attraction repairTarget){
        targeted = repairTarget;
        //List<Attraction> reachable = CalculateReachablePositions();
        //if (reachable.Contains(repairTarget)){
        GoToBuilding(targeted);
        //}
    }

    protected override void Awake(){
        base.Awake();
    }

    protected override void Start(){
        base.Start();
        transform.parent = GameObject.Find("Mechanics").transform;
    }

    protected override void Update(){
        base.Update();

        if (leaving){
            if ((transform.position - targetPosition).magnitude <= 0.1f){
                EventManager.instance.onSpeedChanged -= ChangeSpeed;
                //EventManager.instance.onMapChanged -= RecheckNavigationTarget;
                GameManager.instance.availableMechanics++;
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
                        EnterBuilding();
                    }
                }

                if (goingToRoad){
                    if ((transform.position - targetPosition).magnitude <= visitDistance){
                        goingToRoad = false;
                        TryToLeavePark();
                    }
                }
            }
        }
    }
    
    /*void GoToBuilding(Attraction targetBuilding){
        List<Attraction> reachable = CalculateReachablePositions();

        if (reachable.Count == 0){
            wantsToLeave = true;
        }
        else{
            // choose a random building as target
            target = targetBuilding;

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
    }*/

    void EnterBuilding(){
        goingToAttraction = false;
        //target.peopleInside.Add(this);
        //previousBuilding = target;
        mesh.SetActive(false);
        Invoke(nameof(LeaveBuilding), 3f);
    }

    public void LeaveBuilding(){
        //target.peopleInside.Remove(this);
        targeted.RepairBuilding();
        targetRepaired = true;
        mesh.SetActive(true);
        //GoToRandomBuilding();
        TryToLeavePark();
    }

    private bool occupied;

    public Mechanic(){
        this.price = 300;
        this.salary = ((this.price * SALARYMULTIPLIER) / 24) / 60;
        this.occupied = false;
    }

    public bool Occupied{
        get => occupied;
        set => occupied = value;
    }
}