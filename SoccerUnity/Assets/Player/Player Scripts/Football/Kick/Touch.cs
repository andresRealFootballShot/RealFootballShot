using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Touch : Kick
{
    public ComponentsKeys keys;
    MyRaycastHit myRaycast;
    
    new void Start()
    {
        base.Start();
        myRaycast = componentsPlayer.scriptsPlayer.raycastAim;
    }
    void Update()
    {
        Kick();
    }
    void Kick()
    {
        if (Input.GetKey(ComponentsKeys.joyTouch) || Input.GetKey(ComponentsKeys.keyTouch))
        {
            getCurrentShot();
        }
        if (Input.GetKeyUp(ComponentsKeys.joyTouch) || Input.GetKeyUp(ComponentsKeys.keyTouch))
        {
            if (controllerDistance.isClose())
            {
                if (myRaycast.isHitting)
                {
                    addForceAtPosition(-myRaycast.hit.normal * currentValue, myRaycast.hit.point);
                }
            }
            InitCurrentForce();
        }
    }
    
}
