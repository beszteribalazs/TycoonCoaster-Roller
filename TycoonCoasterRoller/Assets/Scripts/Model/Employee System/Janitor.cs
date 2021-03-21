using System.Collections;
using System.Collections.Generic;

public class Janitor : Employee
{
    public Janitor()
    {
        this.price=150;
        this.salary=((this.price*SALARYMULTIPLIER)/24)/60;
    }
}
