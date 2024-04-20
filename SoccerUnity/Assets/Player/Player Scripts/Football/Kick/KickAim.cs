using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickAim : Kick
{
    public MoveImageOnScreen moveImage;
    public ComponentsKeys keys;
    public MyRaycastHit myRaycast;
    void Update()
    {
        if (Input.GetKey(KeyCode.JoystickButton7) || Input.GetKey(KeyCode.Mouse0))
        {
            getCurrentShot();
        }
        if (Input.GetKeyUp(KeyCode.JoystickButton7) || Input.GetKeyUp(KeyCode.Mouse0))
        {
            Vector2 posScreen = new Vector2(moveImage.position.x + (Screen.width / 2), moveImage.position.y + (Screen.height / 2));
            Ray ray = GetComponent<Camera>().ScreenPointToRay(posScreen);
            if(myRaycast.isHitting){
                    addForceAtPosition(-myRaycast.hit.normal * currentValue, myRaycast.hit.point);
            }
            InitCurrentForce();
        }

    }
}
