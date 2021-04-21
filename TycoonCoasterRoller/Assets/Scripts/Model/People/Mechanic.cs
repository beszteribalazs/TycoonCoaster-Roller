using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechanic : Employee
{
    Attraction targeted;
    bool targetRepaired = false;

    public void Repair(Attraction repairTarget)
    {
        targeted = repairTarget;
        GoToBuilding(targeted);
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        transform.parent = GameObject.Find("Mechanics").transform;
    }

    int repairStartTick;
    bool repairing = false;


    protected override void Update()
    {
        base.Update();

        if (repairing)
        {
            if (TimeManager.instance.Tick - repairStartTick >= targeted.RepairTickDuration)
            {
                LeaveBuilding();
            }
        }

        if (!IsOnNavMesh())
        {
            EventManager.instance.onSpeedChanged -= ChangeSpeed;
            GameManager.instance.availableMechanics++;
            targeted.beingRepaired = false;
            Destroy(gameObject);
        }

        if (leaving)
        {
            if ((transform.position - targetPosition).magnitude <= 1f)
            {
                EventManager.instance.onSpeedChanged -= ChangeSpeed;
                GameManager.instance.availableMechanics++;
                Destroy(gameObject);
            }
        }
        else if (goingToRoad && wantsToLeave)
        {
            if ((transform.position - targetPosition).magnitude <= visitDistance)
            {
                goingToRoad = false;
                TryToLeavePark();
            }
        }
        else
        {
            if (wantsToLeave)
            {
                // try leaving
                if (!leaving)
                {
                    TryToLeavePark();
                }
            }
            else
            {
                // Try to enter building if close to it
                if (goingToAttraction)
                {
                    if ((transform.position - targetPosition).magnitude <= visitDistance)
                    {
                        EnterBuilding();
                    }
                }

                if (goingToRoad)
                {
                    if ((transform.position - targetPosition).magnitude <= visitDistance)
                    {
                        goingToRoad = false;
                        TryToLeavePark();
                    }
                }
            }
        }
    }


    void EnterBuilding()
    {
        goingToAttraction = false;
        mesh.SetActive(false);
        targeted.transform.Find("Broke").GetComponent<MeshRenderer>().material =
            Resources.Load<Material>("Materials/buildingRepairing");
        repairing = true;
        repairStartTick = TimeManager.instance.Tick;
        GameManager.instance.Money -= targeted.Value * 0.1f;
    }

    public void LeaveBuilding()
    {
        targeted.RepairBuilding();
        targetRepaired = true;
        targeted.transform.Find("Broke").GetComponent<MeshRenderer>().material =
            Resources.Load<Material>("Materials/buildingBroke");
        mesh.SetActive(true);
        repairing = false;
        TryToLeavePark();
    }

    private bool occupied;

    public Mechanic()
    {
        this.price = 300;
        this.salary = ((this.price * SALARYMULTIPLIER) / 24) / 60;
        this.occupied = false;
    }
}