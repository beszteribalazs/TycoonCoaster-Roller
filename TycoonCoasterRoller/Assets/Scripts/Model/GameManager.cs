using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public BuildingSystem buildingSystem;
    public static GameManager instance;
    private int width=50;
    private int height=50;
    
    
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

    public int Width
    {
        get => width;
    }

    public int Height
    {
        get => height;
    }
}
