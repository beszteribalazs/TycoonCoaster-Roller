using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public class TimeManager : MonoBehaviour{
    const float TICK_TIMER_MAX = 1f;
    int tick;
    int gameSpeed;
    public static TimeManager instance;
    bool paused = false;

    float tickTimer;
    public int Tick => tick;
    
    public bool Paused{
        get => paused;
        set{
            paused = value;
            if (paused){
                gameSpeed = 0;    
            }
        }
    }

    public int GameSpeed{
        get => gameSpeed;
        set => gameSpeed = value;
    }

    void Awake(){
        instance = this;
        tick = 0;
        gameSpeed = 10;
    }

    void Update(){
        if (paused) return;
        tickTimer += Time.deltaTime;
        if (tickTimer >= TICK_TIMER_MAX / gameSpeed){
            tickTimer -= TICK_TIMER_MAX / gameSpeed;
            tick++;
            GameManager.instance.GameLoop();
            //Debug.Log(tick);
        }
    }
}
