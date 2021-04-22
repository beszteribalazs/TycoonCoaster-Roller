using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Person : MonoBehaviour
{
    protected GameObject mesh;
    protected NavMeshAgent agent;
    protected float visitDistance = 3f;
    protected bool goingToAttraction = false;
    protected bool goingToRoad = false;
    protected bool wantsToLeave = false;
    protected bool leaving = false;
    protected Vector3 targetPosition;
    protected Attraction target;
    protected Road roadTarget;
    protected Animator animator;
    protected float walkSpeedMultiplier;
    protected Attraction previousBuilding = null;

    protected virtual void Awake()
    {
        mesh = transform.Find("human_mesh").gameObject;
        agent = GetComponent<NavMeshAgent>();
        EventManager.instance.onSpeedChanged += ChangeSpeed;
        animator = GetComponent<Animator>();
        walkSpeedMultiplier = 1f;
    }

    protected virtual void OnDestroy()
    {
        EventManager.instance.onSpeedChanged -= ChangeSpeed;
    }

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        lastFramePosition = transform.position;
        ChangeSpeed(TimeManager.instance.GameSpeed / 10);
    }

    protected virtual void Update()
    {
        // Rotate visitor in direction of movement
        if (velocity != Vector3.zero)
        {
            Quaternion newRotation = Quaternion.Euler(0, 90, 0) * Quaternion.LookRotation(velocity, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, Time.deltaTime * 720);
        }

        animator.speed = velocity.magnitude * 10;
        Debug.DrawLine(transform.position + Vector3.up, targetPosition + Vector3.up, Color.red);
        Debug.DrawLine(transform.position + Vector3.up, agent.destination + Vector3.up, Color.blue);
    }

    protected bool IsOnNavMesh()
    {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(transform.position, out hit, 2, NavMesh.AllAreas))
        {
            return true;
        }

        return false;
    }

    protected Vector3 velocity;
    protected Vector3 lastFramePosition;

    void FixedUpdate()
    {
        velocity = transform.position - lastFramePosition;
        lastFramePosition = transform.position;
    }

    protected void GoToBuilding(Attraction targetBuilding)
    {
        if (NavigationManager.instance.reachableAttractionCount == 0)
        {
            wantsToLeave = true;
        }
        else
        {
            // choose a random building as target
            target = targetBuilding;

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


    protected void TryToLeavePark()
    {
        wantsToLeave = true;
        // Find first cell from spawn
        int x;
        int z;
        BuildingSystem.instance.grid.XZFromWorldPosition(
            BuildingSystem.instance.entryPoint.position + Vector3.forward * BuildingSystem.instance.CellSize, out x,
            out z);

        // check if first cell has something
        if (BuildingSystem.instance.grid.GetCell(x, z).GetBuilding() != null)
        {
            // if first cell is road, check if reachable
            if (BuildingSystem.instance.grid.GetCell(x, z).GetBuilding().Type.type == BuildingTypeSO.Type.Road)
            {
                Road exitRoad = (Road) BuildingSystem.instance.grid.GetCell(x, z).GetBuilding();
                roadTarget = exitRoad;
                if (NavigationManager.instance.reachableRoads.Contains(exitRoad))
                {
                    agent.SetDestination(exitRoad.Position + new Vector3(0, 0, -BuildingSystem.instance.CellSize));
                    targetPosition = exitRoad.Position + new Vector3(0, 0, -BuildingSystem.instance.CellSize);
                    leaving = true;
                    goingToAttraction = false;
                    goingToRoad = false;
                }
                // can't exit, wander randomly
                else
                {
                    Destroy(gameObject);
                }
            }
            // can't exit, wander randomly
            else
            {
                Destroy(gameObject);
            }
        }
        // can't exit, wander randomly
        else
        {
            Destroy(gameObject);
        }
    }


    protected void GoToRandomRoad()
    {
        if (NavigationManager.instance.reachableRoads.Count > 0)
        {
            roadTarget =
                NavigationManager.instance.reachableRoads[
                    Random.Range(0, NavigationManager.instance.reachableRoads.Count)];
            targetPosition = roadTarget.Position;
            agent.SetDestination(targetPosition);
            goingToRoad = true;
            goingToAttraction = false;
            leaving = false;
        }
        else
        {
            targetPosition = BuildingSystem.instance.entryPoint.position +
                             new Vector3(1, 0, 1) * BuildingSystem.instance.CellSize;
            goingToRoad = true;
            goingToAttraction = false;
            leaving = false;
        }
    }

    protected void ChangeSpeed(int multiplier)
    {
        switch (multiplier)
        {
            case 0:
                agent.speed = 0;
                break;
            case 1:
                agent.speed = 10 * walkSpeedMultiplier;
                break;
            case 2:
                agent.speed = 20 * walkSpeedMultiplier;
                break;
            case 3:
                agent.speed = 30 * walkSpeedMultiplier;
                break;
            default:
                break;
        }
    }
}