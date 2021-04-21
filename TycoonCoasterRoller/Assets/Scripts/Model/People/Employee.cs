using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Employee : Person
{
    protected float price;
    protected float salary;
    protected const float SALARYMULTIPLIER = 0.1f;

    protected Employee()
    {
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    public float Salary
    {
        get => salary;
    }
}