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

    public void DisableButton()
    {
        stop.interactable = false;
        resume.interactable = false;
        twoX.interactable = false;
        threeX.interactable = false;
    }

    public void EnableButton()
    {
        stop.interactable = true;
        resume.interactable = true;
        twoX.interactable = true;
        threeX.interactable = true;
    }
    
}
