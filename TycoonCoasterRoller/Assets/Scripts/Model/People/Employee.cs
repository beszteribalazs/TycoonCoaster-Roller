using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Employee : Person
{
    protected float price;
    protected float salary;
    protected const float SALARYMULTIPLIER=0.1f;

    protected Employee() { }

    public float Salary
    {
        get => salary;
    }
}
