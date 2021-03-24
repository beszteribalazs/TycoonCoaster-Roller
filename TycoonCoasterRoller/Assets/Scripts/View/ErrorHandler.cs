using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ErrorHandler : MonoBehaviour
{
    public static ErrorHandler instance;
    [SerializeField] GameObject buyMenu;
    [SerializeField] GameObject noMoney;
    [SerializeField] GameObject noJanitor;
    [SerializeField] GameObject noMechanic;

    private void Awake()
    {
        instance = this;
    }

    public void BuyMechanic()
    {
        if (!GameManager.instance.BuyMechanic())
        {
            StartCoroutine(NoMoneyWait());
        }
    }
    
    public void BuyJanitor()
    {
        if (!GameManager.instance.BuyJanitor())
        {
            StartCoroutine(NoMoneyWait());
        }
    }
    
    public void SellMechanic()
    {
        if (!GameManager.instance.RemoveMechanic())
        {
            StartCoroutine(NoMechanicWait());
        }
    }
    
    public void SellJanitor()
    {
        if (!GameManager.instance.RemoveJanitor())
        {
            StartCoroutine(NoJanitorWait());
        }
    }
    
    public void BuyBuilding(BuildingTypeSO type)
    {
        if (!GameManager.instance.ChangeSelectedType(type))
        {
            StartCoroutine(NoMoneyWait());
        }
        else
        {
            buyMenu.SetActive(false);
        }
    }
    

    public void NoMoneyError()
    {
        StartCoroutine(NoMoneyWait());
    }

    IEnumerator NoMoneyWait()
    {
        noJanitor.SetActive(false);
        noMechanic.SetActive(false);
        noMoney.SetActive(true);
        yield return new WaitForSeconds(3);
        noMoney.SetActive(false);
    }
    
    IEnumerator NoMechanicWait()
    {
        noJanitor.SetActive(false);
        noMoney.SetActive(false);
        noMechanic.SetActive(true);
        yield return new WaitForSeconds(3);
        noMechanic.SetActive(false);
    }
    
    IEnumerator NoJanitorWait()
    {
        noMechanic.SetActive(false);
        noMoney.SetActive(false);
        noJanitor.SetActive(true);
        yield return new WaitForSeconds(3);
        noJanitor.SetActive(false);
    }
}
