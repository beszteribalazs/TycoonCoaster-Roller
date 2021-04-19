using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSwitch : MonoBehaviour
{
    [SerializeField] Button stop;
    [SerializeField] Button resume;
    [SerializeField] Button twoX;
    [SerializeField] Button threeX;

    private void Start()
    {
        EventManager.instance.onModeChanged += ButtonChange;
    }

    public void ButtonChange(BuildingSystem.ClickMode clickMode)
    {
        if (clickMode == BuildingSystem.ClickMode.Destroy)
        {
            stop.interactable = false;
            resume.interactable = false;
            twoX.interactable = false;
            threeX.interactable = false;
        }
        else if (clickMode == BuildingSystem.ClickMode.Normal)
        {
            stop.interactable = true;
            resume.interactable = true;
            twoX.interactable = true;
            threeX.interactable = true;
        }
    }

    public void RoadModeFromDestroyMode()
    {
        if (GameManager.instance.buildingSystem.currentMode == BuildingSystem.ClickMode.Destroy)
        {
            GameManager.instance.NormalMode();
        }
    }
    
    public void BuyMenuFromDestroyMode()
    {
        if (GameManager.instance.buildingSystem.currentMode == BuildingSystem.ClickMode.Destroy)
        {
            GameManager.instance.NormalMode();
        }
    }
}
