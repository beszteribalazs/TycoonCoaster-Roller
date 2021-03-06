using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuySelect : MonoBehaviour
{
    public static BuySelect instance;
    [SerializeField] GameObject buyMenu;
    [SerializeField] GameObject pauseMenu;
    public bool check;
    
    void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        check = true;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && !pauseMenu.activeSelf)
        {
            if (GameManager.instance.buildingSystem.currentMode == BuildingSystem.ClickMode.Destroy)
            {
                GameManager.instance.Resume();
            }
            InspectorMenu.instance.CloseDisplay();
            buyMenu.SetActive(check);
            check = !check;
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            buyMenu.SetActive(false);
            check = true;
        }
    }

    public void SetCheck()
    {
        check = true;
    }

    public void BuyButtonSet()
    {
        buyMenu.SetActive(check);
        check = !check;
    }
}
