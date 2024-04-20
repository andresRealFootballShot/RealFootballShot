using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TouchWithDirect : Kick
{
    Vector2 finalDir;
    bool moveBall;
    Vector3 dirCameraForward,dirCameraRight;
    public ControllerSpeedMouse controllerSpeed;
    public string strKeyCode;
    public ComponentsKeys keys;
    public float sensibilityMouse, sensibilityJoy;
    private void Awake()
    {

    }
    void Update()
    {
        Kick();
    }
    void Kick()
    {
        if (Input.GetKeyDown(ComponentsKeys.joyTouchedManaged)&&false || Input.GetKeyDown(ComponentsKeys.keyTouchManaged))
        {
            moveBall = true;
            dirCameraForward = componentsPlayer.transCamera.forward;
            dirCameraRight = componentsPlayer.transCamera.right;
            finalDir = Vector2.zero;
            /*if (controllerRayHit.RayCastHit(controllerAim,controllerDistance))
                {
                    moveBall = true;
                    dirCameraForward = componentsPlayer.transCamera.forward;
                    dirCameraRight = componentsPlayer.transCamera.right;
                    finalDir = Vector2.zero;
                }*/
        }
        if (moveBall)
        {
            if (controllerDistance.isClose())
            {
                //Vector2 mouse = new Vector2(Input.GetAxis("Mouse X")*sensibilityMouse + Input.GetAxis("RightStickX")* sensibilityJoy, Input.GetAxis("Mouse Y") * sensibilityMouse - Input.GetAxis("RightStickY") * sensibilityJoy) ;
                Vector2 mouse = new Vector2(Input.GetAxis("Mouse X") * sensibilityMouse, Input.GetAxis("Mouse Y") * sensibilityMouse);
                if (mouse.magnitude> 0f)
                {
                    finalDir += mouse * Time.deltaTime;
                }
                
            }
        }
        if (Input.GetKeyUp(ComponentsKeys.joyTouchedManaged) && false || Input.GetKeyUp(ComponentsKeys.keyTouchManaged))
        {
            if (moveBall)
            {
                
                if (controllerDistance.isClose())
                {
                    //Vector2 mouse = new Vector2(Input.GetAxis("Mouse X") * sensibilityMouse + Input.GetAxis("RightStickX") * sensibilityJoy, Input.GetAxis("Mouse Y") * sensibilityMouse - Input.GetAxis("RightStickY")*sensibilityJoy) * Time.deltaTime ;
                    Vector2 mouse = new Vector2(Input.GetAxis("Mouse X") * sensibilityMouse, Input.GetAxis("Mouse Y") * sensibilityMouse) * Time.deltaTime;
                    finalDir += mouse;
                    if (finalDir.magnitude > 0.01f)
                    {
                        Vector3 dir = dirCameraRight * finalDir.x + new Vector3(dirCameraForward.x, 0, dirCameraForward.z) * finalDir.y;
                        float magnitude = finalDir.magnitude;
                        float angle = Vector2.Angle(new Vector2(componentsPlayer.componentsBall.rigBall.velocity.x, componentsPlayer.componentsBall.rigBall.velocity.z), finalDir);
                        
                        currentValue = Mathf.Clamp(magnitude + min, min, max);
                        if (angle > 100)
                        {
                            Vector3 brake = -componentsPlayer.componentsBall.rigBall.velocity * Mathf.Clamp01(Mathf.Pow(90 / angle, 3));
                            addForce(dir.normalized * currentValue);
                        }
                        else
                        {
                            addForce(dir.normalized * currentValue);
                        }
                    }
                    else
                    {
                        ballControll();
                    }
                }
            }
            moveBall = false;
        }
    }
}
