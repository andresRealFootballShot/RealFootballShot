using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerSpeedMouse : MonoBehaviour
{
    public string info;
    public float speedMouseX, speedMouseY,speedJoystickX, speedJoystickY;
    public float speedMinX, speedMaxX, speedMinY, speedMaxY,joyMinX,joyMaxX,joyMinY,joyMaxY;
    void Start()
    {
        
    }
    public void ChangeSpeedX_Value(float value)
    {
        speedMouseX = Mathf.Lerp(speedMinX, speedMaxX, value);
    }
    public void ChangeSpeedY_Value(float value)
    {
        speedMouseY = Mathf.Lerp(speedMinY, speedMaxY, value);
    }
    public void ChangeJoyX_Value(float value)
    {
        speedJoystickX = Mathf.Lerp(joyMinX, joyMaxX, value);
    }
    public void ChangeJoyY_Value(float value)
    {
        speedJoystickY = Mathf.Lerp(joyMinY, joyMaxY, value);
    }

}
