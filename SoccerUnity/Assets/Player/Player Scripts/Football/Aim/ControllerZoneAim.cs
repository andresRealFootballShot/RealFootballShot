using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerZoneAim : MonoBehaviour
{
    public ControllerSpeedMouse controllerSpeedMouse,ctrlSpeedWithEffect;
    public ControllerDistance controllerDistance;
    public ShotWithEffect shotWithEffect;
    public PlayerInput playerInput;
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
        Vector2 mouseDelta = playerInput.actions["AimKick"].ReadValue<Vector2>();
        mouseX = mouseDelta.x * controllerSpeedMouse.speedMouseX * Screen.width * deltaTime;
        mouseY = mouseDelta.y * controllerSpeedMouse.speedMouseY * Screen.height * deltaTime;
        
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
