using System.Collections;
using System.Collections.Generic;

public class Janitor : Employee{

    protected override void Start(){
        base.Start();
        GoToRandomRoad();
    }

    public void Sell(){
        wantsToLeave = true;
        TryToLeavePark();
    }
    
    protected override void Update(){
        base.Update();

        if (leaving){
            if ((transform.position - targetPosition).magnitude <= 0.1f){
                EventManager.instance.onSpeedChanged -= ChangeSpeed;
                //EventManager.instance.onMapChanged -= RecheckNavigationTarget;
                Destroy(gameObject);
            }
        }
        else if (goingToRoad && wantsToLeave){
            if ((transform.position - targetPosition).magnitude <= visitDistance){
                goingToRoad = false;
                TryToLeavePark();
            }
        }
        else{
            if (wantsToLeave){
                // try leaving
                if (!leaving){
                    TryToLeavePark();
                }
            }
            else{
                if (goingToRoad){
                    if ((transform.position - targetPosition).magnitude <= visitDistance){
                        goingToRoad = false;
                        GoToRandomRoad();
                    }
                }
            }
        }
    }

    public Janitor(){
        this.price = 150;
        this.salary = ((this.price * SALARYMULTIPLIER) / 24) / 60;
    }
}