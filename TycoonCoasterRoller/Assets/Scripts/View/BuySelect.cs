using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuySelect : MonoBehaviour
{
    public static BuySelect instance;
    [SerializeField] GameObject buyMenu;
    [SerializeField] GameObject inspectorMenu;
    [SerializeField] GameObject pauseMenu;
    public bool check;


    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        check = true;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && !pauseMenu.activeSelf)
        {
            inspectorMenu.SetActive(false);
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
