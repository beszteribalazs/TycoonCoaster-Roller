﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Janitor : Employee{
    protected override void Start(){
        base.Start();
        transform.parent = GameObject.Find("Janitors").transform;
        GoToRandomRoad();
    }

    public void Sell(){
        wantsToLeave = true;
        TryToLeavePark();
    }

    protected override void Awake(){
        base.Awake();
        //EventManager.instance.onMapChanged += RecheckNavigationTarget;
        EventManager.instance.onMapChanged += DelayedCheck;
        walkSpeedMultiplier = Random.Range(0.4f, 0.6f);
    }

    void DelayedCheck(){
        Invoke(nameof(CheckIfReachable), 0.05f);
    }
    
    void CheckIfReachable(){
        // find cell person is standing on
        GridXZ grid = BuildingSystem.instance.grid;
        int x;
        int z;
        //if inside map
        if (transform.position.x <= grid.Width * grid.GetCellSize() && transform.position.x >= 0 && transform.position.z <= grid.Height * grid.GetCellSize() && transform.position.z >= 0){
            grid.XZFromWorldPosition(transform.position, out x, out z);
            if (grid.GetCell(x, z) != null && grid.GetCell(x,z).GetBuilding() != null){
                if (grid.GetCell(x, z).GetBuilding().Type.type == BuildingTypeSO.Type.Road){
                    Road road = (Road) grid.GetCell(x, z).GetBuilding();
                    if (road != null && !NavigationManager.instance.reachableRoads.Contains(road)){
                        //GameManager.instance.storedJanitors++;
                        //Destroy(gameObject);

                        agent.Warp(BuildingSystem.instance.entryPoint.position + new Vector3(1, 0, 1) * BuildingSystem.instance.CellSize / 2);
                        //RecheckNavigationTarget();
                        //GoToRandomRoad();
                    }
                    else{
                        //GoToRandomRoad();
                        //RecheckNavigationTarget();
                        
                    }
                }
                else{
                    agent.Warp(BuildingSystem.instance.entryPoint.position + new Vector3(1, 0, 1) * BuildingSystem.instance.CellSize / 2);
                    GoToRandomRoad();
                    //GameManager.instance.storedJanitors++;
                    //Destroy(gameObject);
                }
            }
            else{
                agent.Warp(BuildingSystem.instance.entryPoint.position + new Vector3(1, 0, 1) * BuildingSystem.instance.CellSize / 2);
                GoToRandomRoad();
                //GameManager.instance.storedJanitors++;
                //Destroy(gameObject);
            }
        }
        else{
            GoToRandomRoad();
            //RecheckNavigationTarget();
        }
        RecheckNavigationTarget();
    }

    void RecheckNavigationTarget(){
        int x, z;
        BuildingSystem.instance.grid.XZFromWorldPosition(transform.position, out x, out z);
        /*if (BuildingSystem.instance.grid.GetCell(x, z) == null){
            transform.position = BuildingSystem.instance.entryPoint.position + Vector3.one * BuildingSystem.instance.grid.GetCellSize();
            RecheckNavigationTarget();
            return;
        }*/

        //if on spawn go to random road
        int firstX, firstZ;
        BuildingSystem.instance.grid.XZFromWorldPosition(BuildingSystem.instance.entryPoint.position + Vector3.forward * BuildingSystem.instance.CellSize, out firstX, out firstZ);
        if (!wantsToLeave){
            if (x == firstX && z == firstZ){
                GoToRandomRoad();
            }
        }

        // if going to road
        if (goingToRoad){
            // recalculate available roads
            if (!NavigationManager.instance.reachableRoads.Contains(roadTarget)){
                GoToRandomRoad();
            }
            else{
                agent.SetDestination(targetPosition);
            }
        }
        // if going to exit
        else if (leaving){
            // recalculate available roads
            if (!NavigationManager.instance.reachableRoads.Contains(roadTarget)){
                GoToRandomRoad();
            }
        }
    }

    int tickToRetarget = 10;
    int lastRetarget = -1000;

    protected override void Update(){
        base.Update();

        /*if (roadTarget == null){
            agent.Warp(BuildingSystem.instance.entryPoint.position + new Vector3(1, 0, 1) * BuildingSystem.instance.CellSize / 2);
            GoToRandomRoad();
            //GameManager.instance.storedJanitors++;
            //Destroy(gameObject);
            
        }*/

        if (!IsOnNavMesh()){
            agent.Warp(BuildingSystem.instance.entryPoint.position + new Vector3(1, 0, 1) * BuildingSystem.instance.CellSize / 2);
            GoToRandomRoad();
            //GameManager.instance.storedJanitors++;
            //Destroy(gameObject);
        }

        if (leaving){
            if ((transform.position - targetPosition).magnitude <= 1f){
                //EventManager.instance.onSpeedChanged -= ChangeSpeed;
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

    protected override void OnDestroy(){
        base.OnDestroy();
        //EventManager.instance.onMapChanged -= RecheckNavigationTarget;
        EventManager.instance.onMapChanged -= DelayedCheck;
        EventManager.instance.onSpeedChanged -= ChangeSpeed;
    }

    public Janitor(){
        this.price = 150;
        this.salary = ((this.price * SALARYMULTIPLIER) / 24) / 60;
    }
}