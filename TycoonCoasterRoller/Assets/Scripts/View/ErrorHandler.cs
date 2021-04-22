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
    [SerializeField] GameObject brokenSell;
    [SerializeField] GameObject noPath;
    [SerializeField] TextMeshProUGUI gotMoneyText;

    private Coroutine co;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        EventManager.instance.onBuildingSold += GotMoney;
        EventManager.instance.onNoMoney += NoMoneyError;
        EventManager.instance.onBrokeBuildingSold += BrokenAttractionError;
        EventManager.instance.onNoPathToBuilding += NoPathError;
    }

    public void BuyMechanic()
    {
        if (!GameManager.instance.BuyMechanic())
        {
            if (co != null)
            {
                StopCoroutine(co);
            }

            co = StartCoroutine(NoMoneyWait());
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

            co = StartCoroutine(NoMoneyWait());
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

            co = StartCoroutine(NoMechanicWait());
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

            co = StartCoroutine(NoJanitorWait());
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

            co = StartCoroutine(NoMoneyWait());
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

        co = StartCoroutine(NoMoneyWait());
    }

    public void GotMoney(float number)
    {
        gotMoneyText.text = "You got " + (int) Math.Floor(number) + "$.";
        if (co != null)
        {
            StopCoroutine(co);
        }

        co = StartCoroutine(GotMoneyWait());
    }

    public void BrokenAttractionError()
    {
        if (co != null)
        {
            StopCoroutine(co);
        }

        co = StartCoroutine(BrokenSellWait());
    }

    public void NoPathError()
    {
        if (co != null)
        {
            StopCoroutine(co);
        }

        co = StartCoroutine(NoPathWait());
    }

    IEnumerator NoMoneyWait()
    {
        noPath.SetActive(false);
        brokenSell.SetActive(false);
        gotMoney.SetActive(false);
        noJanitor.SetActive(false);
        noMechanic.SetActive(false);
        noMoney.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        noMoney.SetActive(false);
    }

    IEnumerator NoMechanicWait()
    {
        noPath.SetActive(false);
        brokenSell.SetActive(false);
        gotMoney.SetActive(false);
        noJanitor.SetActive(false);
        noMoney.SetActive(false);
        noMechanic.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        noMechanic.SetActive(false);
    }

    IEnumerator NoJanitorWait()
    {
        noPath.SetActive(false);
        brokenSell.SetActive(false);
        gotMoney.SetActive(false);
        noMechanic.SetActive(false);
        noMoney.SetActive(false);
        noJanitor.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        noJanitor.SetActive(false);
    }

    IEnumerator GotMoneyWait()
    {
        noPath.SetActive(false);
        brokenSell.SetActive(false);
        noMechanic.SetActive(false);
        noMoney.SetActive(false);
        noJanitor.SetActive(false);
        gotMoney.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        gotMoney.SetActive(false);
    }

    IEnumerator BrokenSellWait()
    {
        noPath.SetActive(false);
        noMechanic.SetActive(false);
        noMoney.SetActive(false);
        noJanitor.SetActive(false);
        gotMoney.SetActive(false);
        brokenSell.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        brokenSell.SetActive(false);
    }

    IEnumerator NoPathWait()
    {
        noMechanic.SetActive(false);
        noMoney.SetActive(false);
        noJanitor.SetActive(false);
        gotMoney.SetActive(false);
        brokenSell.SetActive(false);
        noPath.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        noPath.SetActive(false);
    }
}