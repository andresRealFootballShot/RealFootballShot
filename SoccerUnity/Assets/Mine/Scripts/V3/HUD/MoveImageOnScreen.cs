using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveImageOnScreen : PositionOnScreen
{
    public new Camera camera;
    public float speedMouse;
    
    bool moveMouse;
    void Start()
    {
    }
    public override Vector2 getPosition()
    {
        float mouseX, mouseY;
        mouseX = Input.GetAxis("Mouse X") * speedMouse;
        mouseY = Input.GetAxis("Mouse Y") * speedMouse;
        position += new Vector2(mouseX, mouseY);
        
        return position;
    }
}
