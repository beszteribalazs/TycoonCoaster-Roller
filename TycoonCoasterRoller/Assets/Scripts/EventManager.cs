using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour{
    public static EventManager instance;

    void Awake(){
        instance = this;
    }

    public event Action<int> onSpeedChanged;

    public void SpeedChanged(int speedMultiplier){
        onSpeedChanged?.Invoke(speedMultiplier);
    }
    
    public event Action onSelectedBuildingChanged;
    public void SelectedBuildingChanged(){
        onSelectedBuildingChanged?.Invoke();
    }

    public event Action onMapChanged;
    public void MapChanged(){
        onMapChanged?.Invoke();
    }

    public event Action<BuildingSystem.ClickMode> onModeChanged;
    public void ModeChanged(BuildingSystem.ClickMode m){
        onModeChanged?.Invoke(m);
    }

    public event Action<float> onBuildingSold;
    public void SoldBuilding(float sellPrice){
        onBuildingSold?.Invoke(sellPrice);
    }

    public event Action onBrokeBuildingSold;

    public void BrokeBuildingSold(){
        onBrokeBuildingSold?.Invoke();
    }
}
