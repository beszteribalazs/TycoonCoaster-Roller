using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Visitor : Person
{
    int ticksToStayInPark;
    int enteredPark;
    int minStayTick = 240;
    int maxStayTick = 960;

    protected override void Awake()
    {
        base.Awake();
        EventManager.instance.onMapChanged += DelayedRecheck;

        walkSpeedMultiplier = Random.Range(0.9f, 1.1f);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventManager.instance.onMapChanged -= DelayedRecheck;

        GameManager.instance.CurrentVisitors--;
        if (inBuilding)
        {
            inBuilding = false;
            target.peopleInside.Remove(this);
            mesh.SetActive(true);
        }
    }

    void DelayedRecheck()
    {
        Invoke(nameof(RecheckNavigationTarget), 0.1f);
        Invoke(nameof(CheckIfReachable), 0.1f);
    }

    void RecheckNavigationTarget()
    {
        // if going to attraction
        if (goingToAttraction)
        {
            // if cant reach target, choose a new one
            if (!NavigationManager.instance.reachableAttractions.Contains(target))
            {
                GoToRandomBuilding();
            }
            else
            {
                GoToBuilding(target);
            }
        }
        // if going to road
        else if (goingToRoad)
        {
            // recalculate available roads
            if (!NavigationManager.instance.reachableRoads.Contains(roadTarget))
            {
                GoToRandomRoad();
            }
        }
        // if going to exit
        else if (leaving)
        {
            // recalculate available roads
            if (!NavigationManager.instance.reachableRoads.Contains(roadTarget))
            {
                GoToRandomRoad();
            }
        }
    }


    protected override void Start()
    {
        base.Start();
        enteredPark = TimeManager.instance.Tick;
        ticksToStayInPark = Random.Range(minStayTick, maxStayTick);
        transform.parent = GameObject.Find("Visitors").transform;
        GoToRandomBuilding();
    }

    bool inBuilding = false;
    int tickToStay;
    int enterTime;


    int tickToRetarget = 10;
    int lastRetarget = -1000;

    protected override void Update()
    {
        base.Update();

        if (!IsOnNavMesh())
        {
            Destroy(gameObject);
        }

        if (!wantsToLeave)
        {
            if (TimeManager.instance.Tick - enteredPark >= ticksToStayInPark)
            {
                if (inBuilding)
                {
                    LeaveBuilding();
                }

                TryToLeavePark();
            }
        }


        if (inBuilding)
        {
            if (TimeManager.instance.Tick - enterTime >= tickToStay)
            {
                LeaveBuilding();
            }
        }

        if (leaving)
        {
            if ((transform.position - targetPosition).magnitude <= 1f)
            {
                EventManager.instance.onSpeedChanged -= ChangeSpeed;
                EventManager.instance.onMapChanged -= DelayedRecheck;
                Destroy(gameObject);
            }
        }
        else if (goingToRoad && wantsToLeave)
        {
            if (TimeManager.instance.Tick - lastRetarget >= tickToRetarget)
            {
                if ((transform.position - targetPosition).magnitude <= visitDistance)
                {
                    goingToRoad = false;
                    TryToLeavePark();
                }

                lastRetarget = TimeManager.instance.Tick;
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
                        TryToEnterBuilding();
                    }
                }

                if (goingToRoad)
                {
                    if (TimeManager.instance.Tick - lastRetarget >= tickToRetarget)
                    {
                        if ((transform.position - targetPosition).magnitude <= visitDistance)
                        {
                            goingToRoad = false;
                            GoToRandomRoad();
                        }

                        lastRetarget = TimeManager.instance.Tick;
                    }
                }
            }
        }
    }

    void TryToEnterBuilding()
    {
        goingToAttraction = false;
        // if target is full or broke
        if (target.peopleInside.Count >= target.TotalCapacity || target.Broke)
        {
            // go to a random road then go to a random building
            GoToRandomBuilding();
        }
        // enter building
        else
        {
            EnterBuilding();
        }
    }


    void EnterBuilding()
    {
        tickToStay = Random.Range(120, 360);
        goingToAttraction = false;
        target.peopleInside.Add(this);
        previousBuilding = target;
        mesh.SetActive(false);
        inBuilding = true;
        enterTime = TimeManager.instance.Tick;
    }

    public void LeaveBuilding()
    {
        inBuilding = false;
        target.peopleInside.Remove(this);
        mesh.SetActive(true);
        GoToRandomBuilding();
    }

    public void EjectBuilding()
    {
        LeaveBuilding();
        TryToLeavePark();
        transform.position = BuildingSystem.instance.entryPoint.position;
    }

    void CheckIfReachable()
    {
        // find cell person is standing on
        GridXZ grid = BuildingSystem.instance.grid;
        int x;
        int z;
        if (transform.position.x <= grid.Width * grid.GetCellSize() && transform.position.x >= 0 &&
            transform.position.z <= grid.Height * grid.GetCellSize() && transform.position.z >= 0)
        {
            grid.XZFromWorldPosition(transform.position, out x, out z);
            Road road = (Road) grid.GetCell(x, z).GetBuilding();
            if (road != null && !NavigationManager.instance.reachableRoads.Contains(road))
            {
                if (inBuilding)
                {
                    LeaveBuilding();
                }

                Destroy(gameObject);
            }
        }
    }

    void GoToRandomBuilding()
    {
        if (NavigationManager.instance.reachableAttractionCount == 0)
        {
            wantsToLeave = true;
        }
        else
        {
            // choose a random building as target
            // choose an enterable building as target
            List<Attraction> enterable = new List<Attraction>();
            foreach (Attraction attraction in NavigationManager.instance.reachableAttractions)
            {
                if (attraction.CurrentVisitorCount < attraction.TotalCapacity && attraction != previousBuilding)
                {
                    enterable.Add(attraction);
                }
            }

            if (enterable.Count == 0)
            {
                TryToLeavePark();
                return;
            }

            target = enterable[Random.Range(0, enterable.Count)];


            // Find first cell from spawn
            int x;
            int z;
            BuildingSystem.instance.grid.XZFromWorldPosition(
                BuildingSystem.instance.entryPoint.position + Vector3.forward * BuildingSystem.instance.CellSize, out x,
                out z);

            //find the closest tile (that is reachable)
            float sqrDistance = Single.MaxValue;
            Vector3 closestPosition = Vector3.zero;
            foreach (Vector2Int coords in target.gridPositionlist)
            {
                //if building has a tile on the root tile, only this tile is reachable so go there
                if (coords.x == x && coords.y == z)
                {
                    closestPosition = BuildingSystem.instance.grid.GetCell(coords.x, coords.y).WorldPosition;
                    break;
                }

                if (NavigationManager.instance.reachableCells.Contains(
                    BuildingSystem.instance.grid.GetCell(coords.x, coords.y)))
                {
                    float tmpdist = (BuildingSystem.instance.grid.GetCell(coords.x, coords.y).WorldPosition -
                                     transform.position).sqrMagnitude;
                    if (tmpdist < sqrDistance)
                    {
                        sqrDistance = tmpdist;
                        closestPosition = BuildingSystem.instance.grid.GetCell(coords.x, coords.y).WorldPosition;
                    }
                }
            }

            targetPosition = closestPosition;
            agent.SetDestination(closestPosition);
            goingToAttraction = true;
        }
    }
}