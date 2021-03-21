using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Employee
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
