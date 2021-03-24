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
    [SerializeField] GameObject gotMoney;
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

    public void GotMoney()
    {
        StartCoroutine(GotMoneyWait());
    }

    IEnumerator NoMoneyWait()
    {
        gotMoney.SetActive(false);
        noJanitor.SetActive(false);
        noMechanic.SetActive(false);
        noMoney.SetActive(true);
        yield return new WaitForSeconds(3);
        noMoney.SetActive(false);
    }
    
    IEnumerator NoMechanicWait()
    {
        gotMoney.SetActive(false);
        noJanitor.SetActive(false);
        noMoney.SetActive(false);
        noMechanic.SetActive(true);
        yield return new WaitForSeconds(3);
        noMechanic.SetActive(false);
    }
    
    IEnumerator NoJanitorWait()
    {
        gotMoney.SetActive(false);
        noMechanic.SetActive(false);
        noMoney.SetActive(false);
        noJanitor.SetActive(true);
        yield return new WaitForSeconds(3);
        noJanitor.SetActive(false);
    }

    IEnumerator GotMoneyWait()
    {
        noMechanic.SetActive(false);
        noMoney.SetActive(false);
        noJanitor.SetActive(false);
        gotMoney.SetActive(true);
        yield return new WaitForSeconds(3);
        gotMoney.SetActive(false);
    }
}
