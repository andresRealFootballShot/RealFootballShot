using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentsKeys : MonoBehaviour
{
    public string strkeyTouchManaged="V",strKeyTouch="C",strShot="Mouse0",strShotEffect="Mouse1", strJoyManaged="JoystickButton4",strJoyTouch= "JoystickButton5",
                   strJoyShot= "JoystickButton7",strJoyShotEffect= "JoystickButton6",strKeyFreeRotateBody="X",strKeyNormalMove="R", strJoyFreeRotateBody = "JoystickButton0",strJoyNormalMove = "JoystickButton2",strFreeCamera="X"
                    , strLookingBall= "Z" , joyRotateAroundStr = "JoystickButton11",strSprint= "LeftShift", strJoySprint= "JoystickButton6",strRotRight90,strRotLeft90,useStr="F", defensivePositionStr = "CapsLock";
    [HideInInspector] public static KeyCode keyTouchManaged,keyTouch,keyShot,keyShotEffect, joyTouchedManaged,joyTouch,joyShot,joyShotEffect,joyFreeRotateBody,
                        keyFreeRotateBody,keyNormalMove,joyNormalMove,keyFreeCamera, keyLookingBall, joyRotateAround,keySprint,joySprint, keyRotRight, keyRotRight90, keyRotLeft90,use,defensivePosition;
    //[HideInInspector] public static KeyCode forward = KeyCode.W, back = KeyCode.S, right=KeyCode.D, left = KeyCode.A;
    void Start()
    {
        
        keyTouchManaged = (KeyCode)System.Enum.Parse(typeof(KeyCode), strkeyTouchManaged);
        keyTouch = (KeyCode)System.Enum.Parse(typeof(KeyCode), strKeyTouch);
        keyShot = (KeyCode)System.Enum.Parse(typeof(KeyCode), strShot);
        keyShotEffect = (KeyCode)System.Enum.Parse(typeof(KeyCode), strShotEffect);
        keyFreeRotateBody= (KeyCode)System.Enum.Parse(typeof(KeyCode), strKeyFreeRotateBody);
        keyNormalMove = (KeyCode)System.Enum.Parse(typeof(KeyCode), strKeyNormalMove);
        keyFreeCamera= (KeyCode)System.Enum.Parse(typeof(KeyCode), strFreeCamera);
        keyLookingBall = (KeyCode)System.Enum.Parse(typeof(KeyCode), strLookingBall);
        joyTouchedManaged = (KeyCode)System.Enum.Parse(typeof(KeyCode), strJoyManaged);
        joyTouch = (KeyCode)System.Enum.Parse(typeof(KeyCode), strJoyTouch);
        joyShot = (KeyCode)System.Enum.Parse(typeof(KeyCode), strJoyShot);
        joyShotEffect = (KeyCode)System.Enum.Parse(typeof(KeyCode), strJoyShotEffect);
        joyFreeRotateBody = (KeyCode)System.Enum.Parse(typeof(KeyCode), strJoyFreeRotateBody);
        joyNormalMove = (KeyCode)System.Enum.Parse(typeof(KeyCode), strJoyNormalMove);
        keySprint = (KeyCode)System.Enum.Parse(typeof(KeyCode), strSprint);
        joySprint = (KeyCode)System.Enum.Parse(typeof(KeyCode), strJoySprint);
        joyRotateAround = (KeyCode)System.Enum.Parse(typeof(KeyCode), joyRotateAroundStr);
        keyRotRight90 = (KeyCode)System.Enum.Parse(typeof(KeyCode), strRotRight90);
        keyRotLeft90 = (KeyCode)System.Enum.Parse(typeof(KeyCode), strRotLeft90);
        use = (KeyCode)System.Enum.Parse(typeof(KeyCode), useStr);
        defensivePosition = (KeyCode)System.Enum.Parse(typeof(KeyCode), defensivePositionStr);
    }
}
