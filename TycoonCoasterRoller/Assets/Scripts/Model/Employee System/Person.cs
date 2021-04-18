﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Person : MonoBehaviour{
    protected GameObject mesh;
    protected NavMeshAgent agent;
    protected List<Cell> reachableCells;
    protected List<Road> reachableRoads;

    protected float visitDistance = 3f;

    protected bool goingToAttraction = false;
    protected bool goingToRoad = false;
    protected bool wantsToLeave = false;
    protected bool leaving = false;
    protected Vector3 targetPosition;
    protected Attraction target;
    protected Road roadTarget;

    protected Animator animator;

    protected virtual void Awake(){
        mesh = transform.Find("human_mesh").gameObject;
        agent = GetComponent<NavMeshAgent>();
        EventManager.instance.onSpeedChanged += ChangeSpeed;
        animator = GetComponent<Animator>();
    }

    protected virtual void Start(){
        agent = GetComponent<NavMeshAgent>();
        lastFramePosition = transform.position;
        agent.speed = TimeManager.instance.GameSpeed;
    }

    protected virtual void Update(){
        // Rotate visitor in direction of movement
        if (velocity != Vector3.zero){
            Quaternion newRotation = Quaternion.Euler(0, 90, 0) * Quaternion.LookRotation(velocity, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, Time.deltaTime * 720);
        }
    }

    protected Vector3 velocity;
    protected Vector3 lastFramePosition;

    void FixedUpdate(){
        velocity = transform.position - lastFramePosition;
        lastFramePosition = transform.position;
    }


    protected void TryToLeavePark(){
        // Find first cell from spawn
        int x;
        int z;
        BuildingSystem.instance.grid.XZFromWorldPosition(BuildingSystem.instance.entryPoint.position + Vector3.forward * BuildingSystem.instance.CellSize, out x, out z);

        // check if first cell has something
        if (BuildingSystem.instance.grid.GetCell(x, z).GetBuilding() != null){
            // if first cell is road, check if reachable
            if (BuildingSystem.instance.grid.GetCell(x, z).GetBuilding().Type.type == BuildingTypeSO.Type.Road){
                Road exitRoad = (Road) BuildingSystem.instance.grid.GetCell(x, z).GetBuilding();
                roadTarget = exitRoad;
                CalculateReachablePositions();
                if (reachableRoads.Contains(exitRoad)){
                    agent.SetDestination(exitRoad.Position + new Vector3(0, 0, -BuildingSystem.instance.CellSize));
                    targetPosition = exitRoad.Position + new Vector3(0, 0, -BuildingSystem.instance.CellSize);
                    leaving = true;
                }
                // can't exit, wander randomly
                else{
                    GoToRandomRoad();
                }
            }
            // can't exit, wander randomly
            else{
                GoToRandomRoad();
            }
        }
        // can't exit, wander randomly
        else{
            GoToRandomRoad();
        }
    }


    protected void GoToRandomRoad(){
        CalculateReachablePositions();
        if (reachableRoads.Count > 0){
            roadTarget = reachableRoads[Random.Range(0, reachableRoads.Count)];
            targetPosition = roadTarget.Position;
            agent.SetDestination(roadTarget.Position);
            //Debug.Log(targetRoad.Position);
            goingToRoad = true;
            leaving = false;
        }
    }

    protected void ChangeSpeed(int multiplier){
        switch (multiplier){
            case 0:
                agent.speed = 0;
                animator.speed = 0;
                break;
            case 1:
                agent.speed = 10;
                animator.speed = 1;
                break;
            case 2:
                agent.speed = 20;
                animator.speed = 2;
                break;
            case 3:
                agent.speed = 30;
                animator.speed = 3;
                break;
            default:
                Debug.LogError("Wrong game speed multiplier! -> " + multiplier);
                break;
        }
    }


    protected List<Attraction> CalculateReachablePositions(){
        List<Attraction> reachable = new List<Attraction>();
        reachableCells = new List<Cell>();
        reachableRoads = new List<Road>();

        GridXZ grid = BuildingSystem.instance.grid;

        // find cell person is standing on
        int x;
        int z;
        if (transform.position.x <= grid.Width * grid.GetCellSize() && transform.position.x >= 0 && transform.position.z <= grid.Height * grid.GetCellSize() && transform.position.z >= 0){
            grid.XZFromWorldPosition(transform.position, out x, out z);
        }
        else{
            // Find first cell from spawn
            grid.XZFromWorldPosition(BuildingSystem.instance.entryPoint.position + Vector3.forward * BuildingSystem.instance.CellSize, out x, out z);
        }

        if (grid.GetCell(x, z).GetBuilding() == null){
            return reachable;
        }


        // if first cell is building, only this building is reachable
        if (grid.GetCell(x, z).GetBuilding().Type.type == BuildingTypeSO.Type.Attraction){
            reachable.Add((Attraction) grid.GetCell(x, z).GetBuilding());
        }
        // if first cell is decoration, nothing is reachable
        else if (grid.GetCell(x, z).GetBuilding().Type.type == BuildingTypeSO.Type.Decoration){ }
        // if first cell is road, start search
        else if (grid.GetCell(x, z).GetBuilding().Type.type == BuildingTypeSO.Type.Road){
            List<Cell> visited = new List<Cell>();
            Stack<Cell> path = new Stack<Cell>();
            Cell currentCell = grid.GetCell(x, z);

            bool finished = false;
            while (!finished){
                /*Debug.Log("CurrentCell: " + currentCell.PositionString + " " + currentCell.GetBuilding());
                Debug.Log("path: " + path.Count);
                Debug.Log("visited: " + visited.Count);*/

                // add current to visited
                if (!visited.Contains(currentCell)){
                    visited.Add(currentCell);
                }

                //if current cell is attraction, add to reachable and go back
                // [REMOVED]except if it was the previous building
                if (currentCell.GetBuilding().Type.type == BuildingTypeSO.Type.Attraction){
                    //if (previousBuilding != currentCell.GetBuilding()){
                    reachable.Add((Attraction) currentCell.GetBuilding());
                    //}

                    reachableCells.Add(currentCell);
                    currentCell = path.Pop();
                } // else search
                else if (currentCell.GetBuilding().Type.type == BuildingTypeSO.Type.Road){
                    if (!reachableRoads.Contains((Road) currentCell.GetBuilding())){
                        reachableRoads.Add((Road) currentCell.GetBuilding());
                    }

                    // choose random direction
                    Cell direction = null;
                    foreach (Cell neighbour in currentCell.Neighbours){
                        // check if neighbour cell has building
                        if (neighbour.GetBuilding() != null){
                            // check if already visited
                            if (!visited.Contains(neighbour)){
                                // not visited neighbour exists
                                direction = neighbour;
                                break;
                            }
                        }
                    }

                    // if exists
                    if (direction != null){
                        // add current to path
                        // go there
                        path.Push(currentCell);
                        currentCell = direction;
                    } //else
                    else{
                        // if path.count > 0
                        if (path.Count > 0){
                            // go back
                            currentCell = path.Pop();
                        }
                        else{
                            finished = true;
                        }
                    }
                }
                else if (currentCell.GetBuilding().Type.type == BuildingTypeSO.Type.Decoration){
                    currentCell = path.Pop();
                }
            }
        }

        return reachable;
    }
}