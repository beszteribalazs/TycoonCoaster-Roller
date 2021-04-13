using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Visitor : MonoBehaviour{
    NavMeshAgent agent;

    void Awake(){
        EventManager.instance.onSpeedChanged += ChangeSpeed;
    }

    void ChangeSpeed(int multiplier){
        switch (multiplier){
            case 1:
                agent.speed = 10;
                break;
            case 2:
                agent.speed = 20;
                break;
            case 3:
                agent.speed = 30;
                break;
        }
    }

    void Start(){
        agent = GetComponent<NavMeshAgent>();
        lastFramePosition = transform.position;
        transform.parent = GameObject.Find("Visitors").transform;

        /*foreach (Attraction attraction in GameManager.instance.ReachableAttractions){
            Debug.Log(attraction);
        }*/
    }

    Vector3 velocity;
    Vector3 lastFramePosition;

    void FixedUpdate(){
        velocity = transform.position - lastFramePosition;
        lastFramePosition = transform.position;
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.C)){
            agent.SetDestination(GetMouseWorldPosition());
        }

        if (Input.GetKeyDown(KeyCode.V)){
            GoToRandomBuilding();
        }

        if (velocity != Vector3.zero){
            Quaternion newRotation = Quaternion.Euler(0, 90, 0) * Quaternion.LookRotation(velocity, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, Time.deltaTime * 720);
        }

        //InvokeRepeating(nameof(RandomTarget), 0f, 1f);
    }

    void GoToRandomBuilding(){
        /*int target = Random.Range(0, GameManager.instance.ReachableAttractions.Count);
        Vector3 targetPosition = GameManager.instance.ReachableAttractions[target].Position;
        agent.SetDestination(targetPosition);*/

        List<Attraction> reachable = CalculateReachableAttractions();
        // choose a random building as target
        Attraction target = reachable[Random.Range(0, reachable.Count)];
        
        //find the closest tile
        float sqrDistance = Single.MaxValue;
        Vector3 closestPosition = Vector3.zero;
        foreach (Vector2Int coords in target.gridPositionlist){
            float tmpdist = (BuildingSystem.instance.grid.GetCell(coords.x, coords.y).WorldPosition - transform.position).sqrMagnitude;
            if (tmpdist < sqrDistance){
                sqrDistance = tmpdist;
                closestPosition = BuildingSystem.instance.grid.GetCell(coords.x, coords.y).WorldPosition;
            }
        }
        
        //Vector3 targetPosition = reachable[target].Position;
        agent.SetDestination(closestPosition);
    }

    public Vector3 GetMouseWorldPosition(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, 1000f, (1 << 8))){
            return hitInfo.point;
        }
        else{
            //DO NOT BUILD
            throw new MouseOutOfMapException("Invalid position!");
        }
    }

    List<Attraction> CalculateReachableAttractions(){
        List<Attraction> reachable = new List<Attraction>();

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


        if (grid.GetCell(x, z).GetBuilding().Type.type == BuildingTypeSO.Type.Attraction){
            // only first building is reachable
            reachable.Add((Attraction) grid.GetCell(x, z).GetBuilding());
        }
        else if (grid.GetCell(x, z).GetBuilding().Type.type == BuildingTypeSO.Type.Decoration){
            // nothing reachable
        }
        else if (grid.GetCell(x, z).GetBuilding().Type.type == BuildingTypeSO.Type.Road){
            List<Cell> visited = new List<Cell>();
            Stack<Cell> path = new Stack<Cell>();
            Cell currentCell = grid.GetCell(x, z);

            bool finished = false;
            while (!finished){
                Debug.Log("CurrentCell: " + currentCell.PositionString + " " + currentCell.GetBuilding());
                Debug.Log("path: " + path.Count);
                Debug.Log("visited: " + visited.Count);

                // add current to visited
                if (!visited.Contains(currentCell)){
                    visited.Add(currentCell);
                }

                //if current cell is attraction, add to reachable and go back
                if (currentCell.GetBuilding().Type.type == BuildingTypeSO.Type.Attraction){
                    reachable.Add((Attraction) currentCell.GetBuilding());
                    currentCell = path.Pop();
                } // else search
                else if (currentCell.GetBuilding().Type.type == BuildingTypeSO.Type.Road){
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