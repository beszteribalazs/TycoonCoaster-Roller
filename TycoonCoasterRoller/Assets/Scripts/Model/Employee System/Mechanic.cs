using System.Collections;
using System.Collections.Generic;

public class Mechanic : Employee
{
    private bool occupied;

    public Mechanic()
    {
        this.price=300;
        this.salary=((this.price*SALARYMULTIPLIER)/24)/60;
        this.occupied = false;
    }

    public bool Occupied
    {
        get => occupied;
        set => occupied = value;
    }
}
