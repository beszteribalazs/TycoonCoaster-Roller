using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ErrorHandler : MonoBehaviour
{
    public GameObject noMoney;
    public GameObject noJanitor;
    public GameObject noMechanic;
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
