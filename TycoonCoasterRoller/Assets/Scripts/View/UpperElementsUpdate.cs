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

    // Update is called once per frame
    void Update()
    {
        //day.text = "Day: "+;
        //time.text = hour + ":" + minute;
        //money.text = +"$";
        //happiness.text = +"%";
        //trash.text = +"%";
    }
    
    public void Resume()
    {
        Time.timeScale = 1f;
    }

    public void Pause()
    {
        Time.timeScale = 0f;
    }
}
