using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateControlBall : MonoBehaviour
{
    public Transform transPivotCam,transCamera,transHolded,transCameraNormal;
    public float speedMouse = 10, angleMax = 70, angleMin = -70;
    void Start()
    {
        
    }
    void Update()
    {
        float mouseX, mouseY;
        mouseX = Input.GetAxis("Mouse X") * speedMouse*Time.deltaTime;
        mouseY = -Input.GetAxis("Mouse Y") * speedMouse * Time.deltaTime;
        float aux = mouseY + transPivotCam.eulerAngles.x;
        if (aux > 0 && aux < 90)
        {
            mouseY = 0;
        }
        else if (aux < 290 && aux > 270)
        {
            mouseY = 0;
        }
        transPivotCam.eulerAngles += new Vector3(mouseY, -mouseX, 0);
        transCamera.LookAt(transPivotCam.position);
    }
    private void OnEnable()
    {
        transPivotCam.LookAt(transCameraNormal);
    }
}
