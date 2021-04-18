using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;

public class UpperElementsUpdate : MonoBehaviour
{
    [SerializeField] TMP_Text day;
    [SerializeField] TMP_Text time;
    [SerializeField] TMP_Text money;
    [SerializeField] TMP_Text happiness;
    [SerializeField] TMP_Text trash;
    [SerializeField] TMP_Text janitor;
    [SerializeField] TMP_Text mechanic;
    [SerializeField] TMP_Text visitors;
    void Update()
    {
        day.text = "Day: "+GameManager.instance.DayCount;

        TimeSpan result = TimeSpan.FromMinutes((GameManager.instance.GameHour*60)+GameManager.instance.GameSecond);
        string timeString = result.ToString("hh':'mm");
        time.text = timeString;
        
        money.text = (int)Math.Floor(GameManager.instance.Money)+"$";
        happiness.text = GameManager.instance.TotalHappiness*100+"%";
        trash.text = GameManager.instance.TrashPercentage*100+"%";
        janitor.text = GameManager.instance.Janitors.Count.ToString();

        mechanic.text = GameManager.instance.availableMechanics + "/"+ GameManager.instance.totalMechanics;
    }

}
