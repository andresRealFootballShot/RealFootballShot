using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot : Kick
{
    bool cancelShot=true;
    public ComponentsKeys keys;
    public AnimationCurve curveValue;
    public float timeInit;
    MyRaycastHit myRaycastHit;
    // Update is called once per frame
    new void Start()
    {
        base.Start();
        myRaycastHit = componentsPlayer.scriptsPlayer.raycastAim;
    }
    void Update()
    {
        Kick();
    }
    void Kick()
    {
        if(Input.GetKeyDown(ComponentsKeys.joyShot) || Input.GetKeyDown(ComponentsKeys.keyShot)){
            cancelShot = false;
        }
        if (!cancelShot)
        {
            getCurrentShot();
            if (Input.GetKeyUp(ComponentsKeys.joyShot) || Input.GetKeyUp(ComponentsKeys.keyShot))
            {
                if (controllerDistance.isClose())
                {
                    if (myRaycastHit.isHitting)
                    {
                        float valueEvaluate = curveValue.Evaluate((currentValue -min)/ (max-min));
                        addForceAtPosition(-myRaycastHit.hit.normal * (valueEvaluate * (max - min) + min), myRaycastHit.hit.point);
                    }
                    
                }
                cancelShot = true;
                InitCurrentForce();
            }
            if (currentValue >= max)
            {
                Invoke("initForce", timeInit);

            }
        }
        
    }
    void initForce()
    {
        cancelShot = true;
        InitCurrentForce();
    }

}
