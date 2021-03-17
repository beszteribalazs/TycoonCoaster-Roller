using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public BuildingSystem buildingSystem;
    static public GameManager instance;


    private void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        
    }
    
    void Update()
    {
        
    }

    public void ChangeSelectedType(BuildingTypeSO buildingTypeSO)
    {
        buildingSystem.SetSelectedBuildingType(buildingTypeSO);
    }
}
