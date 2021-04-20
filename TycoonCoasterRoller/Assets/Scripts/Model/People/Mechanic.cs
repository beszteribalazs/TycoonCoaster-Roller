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

    int repairStartTick;
    bool repairing = false;
    

    protected override void Update(){
        base.Update();

        if (repairing){
            if (TimeManager.instance.Tick - repairStartTick >= targeted.RepairTickDuration){
                LeaveBuilding();
            }
        }
        
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
    

    void EnterBuilding(){
        goingToAttraction = false;
        //target.peopleInside.Add(this);
        //previousBuilding = target;
        mesh.SetActive(false);
        targeted.transform.Find("Broke").GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/buildingRepairing");
        repairing = true;
        repairStartTick = TimeManager.instance.Tick;
        //Invoke(nameof(LeaveBuilding), 3f);
    }

    public void LeaveBuilding(){
        //target.peopleInside.Remove(this);
        targeted.RepairBuilding();
        targetRepaired = true;
        targeted.transform.Find("Broke").GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/buildingBroke");
        mesh.SetActive(true);
        //GoToRandomBuilding();
        repairing = false;
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