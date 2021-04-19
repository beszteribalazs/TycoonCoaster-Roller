using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Janitor : Employee{

    protected override void Start(){
        base.Start();
        GoToRandomRoad();
    }

    public void Sell(){
        wantsToLeave = true;
        TryToLeavePark();
    }

    protected override void Awake(){
        base.Awake();
        EventManager.instance.onMapChanged += RecheckNavigationTarget;
        walkSpeedMultiplier = Random.Range(0.4f, 0.6f);
    }

    void RecheckNavigationTarget(){


        int x, z;
        BuildingSystem.instance.grid.XZFromWorldPosition(transform.position, out x, out z);
        if (BuildingSystem.instance.grid.GetCell(x, z) == null){
            transform.position = BuildingSystem.instance.entryPoint.position + Vector3.one * BuildingSystem.instance.grid.GetCellSize();
            RecheckNavigationTarget();
            return;
        }
        
        // if going to road
        if (goingToRoad){
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
    
    protected override void Update(){
        base.Update();

        if (leaving){
            if ((transform.position - targetPosition).magnitude <= 0.1f){
                EventManager.instance.onSpeedChanged -= ChangeSpeed;
                //EventManager.instance.onMapChanged -= RecheckNavigationTarget;
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
                if (goingToRoad){
                    if ((transform.position - targetPosition).magnitude <= visitDistance){
                        goingToRoad = false;
                        GoToRandomRoad();
                    }
                }
            }
        }
    }

    void OnDestroy(){
        EventManager.instance.onMapChanged -= RecheckNavigationTarget;
    }

    public Janitor(){
        this.price = 150;
        this.salary = ((this.price * SALARYMULTIPLIER) / 24) / 60;
    }
}