using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;
    public BallData(Vector3 position,Quaternion rotation,Vector3 velocity,Vector3 angularVelocity)
    {
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
        this.angularVelocity = angularVelocity;
    }
    public BallData(object[] data)
    {
        int index = 0;
        position = (Vector3)data[index++];
        rotation = (Quaternion)data[index++];
        velocity = (Vector3)data[index++];
        angularVelocity = (Vector3)data[index++];
    }
    public object[] getData()
    {
        List<object> data = new List<object>();
        data.Add(position);
        data.Add(rotation);
        data.Add(velocity);
        data.Add(angularVelocity);
        return data.ToArray();
    }
}
