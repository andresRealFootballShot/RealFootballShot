using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyLine
{
    public Vector3 pos0,posf;
    public MyLine()
    {
        pos0 = Vector3.positiveInfinity;
        posf = Vector3.positiveInfinity;
    }
    public MyLine(Vector3 pos0,Vector3 posf)
    {
        this.pos0 = pos0;
        this.posf = posf;
    }
}
