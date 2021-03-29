using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [SerializeField] TextMeshProUGUI gotMoneyText;
    Coroutine co;
    
    private void Awake()
    {
        instance = this;
    }
    
    private void Start()
    {
        EventManager.instance.onBuildingSold += GotMoney;
    }
    
    public void BuyMechanic()
    {
        if (!GameManager.instance.BuyMechanic())
        {
            
            if (co != null)
            {
                StopCoroutine(co);
            }
            co=StartCoroutine(NoMoneyWait());
        }
    }
    
    public void BuyJanitor()
    {
        if (!GameManager.instance.BuyJanitor())
        {
            if (co != null)
            {
                StopCoroutine(co);
            }
            co=StartCoroutine(NoMoneyWait());
        }
    }
    
    public void SellMechanic()
    {
        if (!GameManager.instance.RemoveMechanic())
        {
            if (co != null)
            {
                StopCoroutine(co);
            }
            co=StartCoroutine(NoMechanicWait());
        }
    }
    
    public void SellJanitor()
    {
        if (!GameManager.instance.RemoveJanitor())
        {
            if (co != null)
            {
                StopCoroutine(co);
            }
            co=StartCoroutine(NoJanitorWait());
        }
    }
    
    public void BuyBuilding(BuildingTypeSO type)
    {
        if (!GameManager.instance.ChangeSelectedType(type))
        {
            if (co != null)
            {
                StopCoroutine(co);
            }
            co=StartCoroutine(NoMoneyWait());
        }
        else
        {
            buyMenu.SetActive(false);
        }
    }
    
    public void NoMoneyError()
    {
        if (co != null)
        {
            StopCoroutine(co);
        }
        co=StartCoroutine(NoMoneyWait());
    }

    public void GotMoney(float number)
    {
        gotMoneyText.text = "You got " + (int) Math.Floor(number) + "$.";
        if (co != null)
        {
            StopCoroutine(co);
        }
        co=StartCoroutine(GotMoneyWait());
    }

    IEnumerator NoMoneyWait()
    {
        gotMoney.SetActive(false);
        noJanitor.SetActive(false);
        noMechanic.SetActive(false);
        noMoney.SetActive(true);
        yield return new WaitForSeconds(1);
        noMoney.SetActive(false);
    }
    
    IEnumerator NoMechanicWait()
    {
        gotMoney.SetActive(false);
        noJanitor.SetActive(false);
        noMoney.SetActive(false);
        noMechanic.SetActive(true);
        yield return new WaitForSeconds(1);
        noMechanic.SetActive(false);
    }
    
    IEnumerator NoJanitorWait()
    {
        gotMoney.SetActive(false);
        noMechanic.SetActive(false);
        noMoney.SetActive(false);
        noJanitor.SetActive(true);
        yield return new WaitForSeconds(1);
        noJanitor.SetActive(false);
    }

    IEnumerator GotMoneyWait()
    {
        noMechanic.SetActive(false);
        noMoney.SetActive(false);
        noJanitor.SetActive(false);
        gotMoney.SetActive(true);
        yield return new WaitForSeconds(1);
        gotMoney.SetActive(false);
    }
}
