using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HappinessMoneyText : MonoBehaviour{
    public TextMeshProUGUI text;

    void Awake(){
        EventManager.instance.onHappinessMoney += DisplayText;
    }

    public void DisplayText(float money){
        text.text = "+" + Math.Round(money,2) + "$";
        text.enabled = true;
        Invoke(nameof(DestroySelf), 2f);
    }

    void DestroySelf(){
        text.enabled = false;
    }
}