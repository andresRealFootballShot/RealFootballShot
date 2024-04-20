using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateNormal : MonoBehaviour
{
    public ControllerSpeedMouse controllerSpeed;
    public ComponentsPlayer components;
    [SerializeField] float  angleMax = 290, angleMin = 70;
    public float speedCamera=50;
    void Start()
    {
        
    }
    public void Update()
    {
        float mouseX, mouseY;
        mouseX = Input.GetAxis("Mouse X") * controllerSpeed.speedMouseX * Time.deltaTime + Input.GetAxis("RightStickX") * controllerSpeed.speedJoystickX * Time.deltaTime;
        mouseY = -Input.GetAxis("Mouse Y") * controllerSpeed.speedMouseY * Time.deltaTime + Input.GetAxis("RightStickY") * controllerSpeed.speedJoystickY * Time.deltaTime;
        float aux = mouseY + components.transCamera.eulerAngles.x;
        if (aux > angleMin && aux < 180)
        {
            mouseY = 0;
        }
        else if (aux > 200 && aux < angleMax)
        {
            mouseY = 0;
        }
        Vector3 trace = new Vector3(mouseY + components.transCamera.eulerAngles.x, components.transModelo.eulerAngles.y, 0);
        components.transCamera.eulerAngles = new Vector3( mouseY + components.transCamera.eulerAngles.x, components.transBody.eulerAngles.y, 0);
    }
    public void EnableScript()
    {
        enabled = true;
    }
    public void DisableScript()
    {
        enabled = false;
    }
}
