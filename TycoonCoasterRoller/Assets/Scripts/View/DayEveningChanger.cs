using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayEveningChanger : MonoBehaviour
{
    [SerializeField] GameObject directionalLightObject;
    private Light directionalLight;
    [SerializeField] AnimationCurve curve;
    void Start()
    {
        directionalLight = directionalLightObject.GetComponent<Light>();
    }

    void Update()
    {
        directionalLight.intensity=curve.Evaluate(Mathf.Lerp(0, 1, (float)GameManager.instance.CountSecond/1440));
    }
}
