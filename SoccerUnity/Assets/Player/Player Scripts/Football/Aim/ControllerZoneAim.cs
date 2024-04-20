using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerZoneAim : MonoBehaviour
{
    public ControllerSpeedMouse controllerSpeedMouse,ctrlSpeedWithEffect;
    public ControllerDistance controllerDistance;
    public ShotWithEffect shotWithEffect;
    public float adjuctResolution;
    float resolution;
    void Start()
    {
        resolution = Vector2.SqrMagnitude(new Vector2(Screen.width, Screen.height))*adjuctResolution;
        
    }

    // Update is called once per frame
    public Vector2 ControlZoneAim(Vector2 position,float radioLimit,float deltaTime)
    {

        float mouseX, mouseY;
        //mouseX = (Input.GetAxis("Mouse X") * controllerSpeedMouse.speedMouseX + Input.GetAxis("RightStickX") * controllerSpeedMouse.speedJoystickX) * resolution * deltaTime;
        //mouseY = (Input.GetAxis("Mouse Y") * controllerSpeedMouse.speedMouseY - Input.GetAxis("RightStickY") * controllerSpeedMouse.speedJoystickY) * resolution * deltaTime;
        mouseX = Input.GetAxis("Mouse X") * controllerSpeedMouse.speedMouseX*deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * controllerSpeedMouse.speedMouseY*deltaTime;
        
        if (isInCircle(position.x + mouseX, position.y + mouseY,radioLimit))
        {
            position += new Vector2(mouseX, mouseY) ;
        }
        else
        {
            position = position.normalized*radioLimit;
        }
        return position;
    }
    bool isInCircle(float x, float y,float radioLimit)
    {
        float radioPos = Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
        return radioPos <= radioLimit;
    }
}
