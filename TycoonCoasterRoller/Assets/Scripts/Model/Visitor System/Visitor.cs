using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Visitor : MonoBehaviour{
    [SerializeField] GameObject mesh;
    NavMeshAgent agent;
    List<Cell> reachableCells;
    List<Road> reachableRoads;

    Attraction previousBuilding = null;


    [SerializeField] float visitDistance = 3f;
    bool goingToAttraction = false;
    bool goingToRoad = false;
    bool wantsToLeave = false;
    bool leaving = false;
    Vector3 targetPosition;
    Attraction target;
    Road roadTarget;

    void Awake(){
        EventManager.instance.onSpeedChanged += ChangeSpeed;
        EventManager.instance.onMapChanged += RecheckNavigationTarget;
    }

    void RecheckNavigationTarget(){
        // if going to attraction
        if (goingToAttraction){
            // recalculate available buildings
            List<Attraction> reachable = CalculateReachablePositions();
            //Debug.Log(reachable.Count);
            // if cant reach target, choose a new one
            if (!reachable.Contains(target)){
                GoToRandomBuilding();
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

    void TryToLeavePark(){
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

    void ChangeSpeed(int multiplier){
        switch (multiplier){
            case 0:
                agent.speed = 0;
                break;
            case 1:
                agent.speed = 10;
                break;
            case 2:
                agent.speed = 20;
                break;
            case 3:
                agent.speed = 30;
                break;
            default:
                Debug.LogError("Wrong game speed multiplier! -> " + multiplier);
                break;
        }
    }

    void Start(){
        agent = GetComponent<NavMeshAgent>();
        lastFramePosition = transform.position;
        transform.parent = GameObject.Find("Visitors").transform;
        //ChangeSpeed(TimeManager.instance.GameSpeed);
        agent.speed = TimeManager.instance.GameSpeed;

        /*foreach (Attraction attraction in GameManager.instance.ReachableAttractions){
            Debug.Log(attraction);
        }*/

        GoToRandomBuilding();
    }

    Vector3 velocity;
    Vector3 lastFramePosition;

    void FixedUpdate(){
        velocity = transform.position - lastFramePosition;
        lastFramePosition = transform.position;
    }

    void Update(){
        /*if (Input.GetKeyDown(KeyCode.C)){
            agent.SetDestination(GetMouseWorldPosition());
        }*/

        /*if (Input.GetKeyDown(KeyCode.V)){
            GoToRandomBuilding();
        }*/

        // Rotate visitor in direction of movement
        if (velocity != Vector3.zero){
            Quaternion newRotation = Quaternion.Euler(0, 90, 0) * Quaternion.LookRotation(velocity, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, Time.deltaTime * 720);
        }


        if (leaving){
            if ((transform.position - targetPosition).magnitude <= 0.1f){
                EventManager.instance.onSpeedChanged -= ChangeSpeed;
                EventManager.instance.onMapChanged -= RecheckNavigationTarget;
                Destroy(gameObject);
            }
        }else if (goingToRoad && wantsToLeave){
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
                        TryToEnterBuilding();
                    }
                }

                if (goingToRoad){
                    if ((transform.position - targetPosition).magnitude <= visitDistance){
                        goingToRoad = false;
                        GoToRandomBuilding();
                    }
                }
            }
        }
    }

    void TryToEnterBuilding(){
        goingToAttraction = false;
        // if target is full
        if (target.peopleInside.Count >= target.TotalCapacity){
            // go to a random road then go to a random building
            GoToRandomRoad();
        }
        // enter building
        else{
            EnterBuilding();
        }
    }

    void GoToRandomRoad(){
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

    void EnterBuilding(){
        goingToAttraction = false;
        target.peopleInside.Add(this);
        previousBuilding = target;
        mesh.SetActive(false);
        Invoke(nameof(LeaveBuilding), 10000f);
    }

    public void LeaveBuilding(){
        target.peopleInside.Remove(this);
        mesh.SetActive(true);
        GoToRandomBuilding();
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
            target = reachable[Random.Range(0, reachable.Count)];

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


    /* Vector3 GetMouseWorldPosition(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, 1000f, (1 << 8))){
            return hitInfo.point;
        }
        else{
            //DO NOT BUILD
            throw new MouseOutOfMapException("Invalid position!");
        }
    }*/


    List<Attraction> CalculateReachablePositions(){
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
                // except if it was the previous building
                if (currentCell.GetBuilding().Type.type == BuildingTypeSO.Type.Attraction){
                    if (previousBuilding != currentCell.GetBuilding()){
                        reachable.Add((Attraction) currentCell.GetBuilding());
                    }

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