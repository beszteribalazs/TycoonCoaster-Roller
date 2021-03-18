using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;

public class UpperElementsUpdate : MonoBehaviour
{
    public TMP_Text day;
    public TMP_Text time;
    public TMP_Text money;
    public TMP_Text happiness;
    public TMP_Text trash;
    
    void Start()
    {
        
    }
    
    void Update()
    {
        day.text = "Day: "+GameManager.instance.DayCount;

        TimeSpan result = TimeSpan.FromMinutes((GameManager.instance.GameHour*60)+GameManager.instance.GameSecond);
        string timeString = result.ToString("hh':'mm");
        time.text = timeString;
        
        money.text = GameManager.instance.Money+"$";
        happiness.text = GameManager.instance.TotalHappiness*100+"%";
        trash.text = GameManager.instance.TrashPercentage*100+"%";
    }

}
