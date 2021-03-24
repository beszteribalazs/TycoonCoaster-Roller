using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuySelect : MonoBehaviour
{
    public GameObject buyMenu;
    public GameObject inspectorMenu;
    public Button buyButton;
    public bool check;
    
    // Start is called before the first frame update
    void Start()
    {
        check = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            inspectorMenu.SetActive(false);
            buyMenu.SetActive(check);
            check = !check;
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
