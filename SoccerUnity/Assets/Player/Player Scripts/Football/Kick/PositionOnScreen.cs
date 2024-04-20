using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionOnScreen : MonoBehaviour
{
    public Vector2 position;
    void Start()
    {
        
    }

    // Update is called once per frame
    public virtual Vector2 getPosition()
    {
       return position;
    }
}
