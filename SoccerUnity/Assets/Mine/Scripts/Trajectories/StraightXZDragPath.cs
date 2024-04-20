using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightXZDragPath : Path
{
    float k;
    float timeToReachYIncrement;
    float timeToReachYMaxFind;
    public StraightXZDragPath(Vector3 _pos0, Vector3 _posf, Vector3 _V0, Vector3 _Vf, float _t0, float _tf,float _drag) : base(_pos0,_posf, _V0,_Vf, _t0,_tf)
    {
        Pos0 = _pos0;
        V0 = _V0;
        vx0 = new Vector2(V0.x, V0.z);
        x0 = new Vector2(Pos0.x, Pos0.z);
        y0 = Pos0.y;
        vy0 = V0.y;
        t0 = _t0;
        Posf = _posf;
        Vf = _Vf;
        tf = _tf;
        minV = 0;
        k = _drag;
        timeToReachYIncrement = 0.1f;
    }
    public StraightXZDragPath(Vector3 _pos0, Vector3 _V0, float _t0, float _drag) : base(_pos0, _V0, _t0)
    {
        Pos0 = _pos0;
        V0 = _V0;
        vx0 = new Vector2(V0.x, V0.z);
        x0 = new Vector2(Pos0.x, Pos0.z);
        y0 = Pos0.y;
        vy0 = V0.y;
        t0 = _t0;
        minV = 0;
        k = _drag;
        timeToReachYIncrement = 0.1f;
    }
    public StraightXZDragPath(float _drag) : base()
    {
        k = _drag;
        timeToReachYIncrement = 0.1f;
    }
    public static float getT(Vector3 x0, Vector3 xf, float v0,float k)
    {
        float d = Vector3.Distance(MyFunctions.setY0ToVector3(x0), MyFunctions.setY0ToVector3(xf));
        float a = (d * k) / v0;
        float ln = Mathf.Log(1 - a);
        float t = ln/-k;
        //print("A=" + A + " | A0=" + A0);
        return Mathf.Clamp(t,0,Mathf.Infinity);
    }
    public override Path newPath(Vector3 _pos0, Vector3 _V0, float _t0)
    {
        return new StraightXZDragPath(_pos0,Posf, _V0,Vf, _t0,tf,k);
    }
    public override Vector3 getVelocityAtTime(float t,out Path path)
    {
        float ekt = Mathf.Exp(-k * t);
        float vx = vx0.x * ekt;
        float vz = vx0.y * ekt;
        Vector3 velocity = new Vector3(vx, vy0, vz);
        path = this;
        //Debug.Log("k");
        return velocity;
    }
    public override Vector3 getVelocityAtTime(float t)
    {
        float ekt = Mathf.Exp(-k * t);
        float vx = vx0.x * ekt;
        float vz = vx0.y * ekt;
        Vector3 velocity = new Vector3(vx, vy0, vz);
        //Debug.Log("k");
        return velocity;
    }
    Vector3 getVelocityAtTime(float t,Vector3 v0)
    {
        float ekt = Mathf.Exp(-k * t);
        float vx = v0.x * ekt;
        float vz = v0.z * ekt;
        Vector3 velocity = new Vector3(vx, v0.y, vz);
        //Debug.Log("k");
        return velocity;
    }
    public override Vector3 getPositionAtTime(float t)
    {
        
        Vector2 vx0 = new Vector2(V0.x, V0.z);
        float ekt = Mathf.Exp(-k * t);
        Vector2 x = (vx0 / k) * (1 - ekt);
        //Debug.Log("t=" + t + " | y=" + y);
        Vector3 endPosition = new Vector3(x.x, 0, x.y) + Pos0;
        //Debug.Log("h "+ endPosition);
        return endPosition;
    }
    public GetV0Result getXZV0(float t, float maxControlSpeed, float maxControlSpeedLerpDistance,float maxKickForce)
    {
        Vector3 pos0 = MyFunctions.setY0ToVector3(Pos0);
        Vector3 posf = MyFunctions.setY0ToVector3(Posf);
        float d = Vector3.Distance(pos0, posf);
        maxControlSpeed = Mathf.Lerp(2, maxControlSpeed, d / maxControlSpeedLerpDistance);
        Vector3 v0 = getV0(t, pos0, posf, k);
        Vector3 vt = getVelocityAtTime(t,v0);
        float vtSpeed = vt.magnitude;
        GetV0Result result;
        bool maximumControlSpeedReached = false;
        if (vt.magnitude > maxControlSpeed)
        {
            v0 = getV0ByVt(maxControlSpeed, pos0, posf, k);
            vtSpeed = maxControlSpeed;
            maximumControlSpeedReached = true;
        }
        bool maxKickForceReach = false;
        if (v0.magnitude > maxKickForce)
        {
            v0 = v0.normalized * maxKickForce;
            maxKickForceReach = true;
        }
        result = new GetV0Result(true, vtSpeed, v0, maximumControlSpeedReached, maxKickForceReach);
        return result;
    }
    public static Vector3 getV0(float t, Vector3 pos0, Vector3 posf,float k)
    {
        Vector2 XZ0 = new Vector2(pos0.x, pos0.z);
        Vector2 XZf = new Vector2(posf.x, posf.z);
        Vector2 dir = XZf - XZ0;
        float distanceXZ = Vector2.Distance(XZ0, XZf);
        float e = (1 - Mathf.Exp(-k * t));
        Vector2 vxz0 = dir.normalized * (distanceXZ * k / e);
        return new Vector3(vxz0.x, 0, vxz0.y);
    }
    public static Vector3 getV0ByVt(float vt, Vector3 pos0, Vector3 posf, float k)
    {
        Vector3 XZ0 = new Vector3(pos0.x,0, pos0.z);
        Vector3 XZf = new Vector3(posf.x,0, posf.z);
        Vector3 dir = XZf - XZ0;
        float distanceXZ = Vector3.Distance(XZ0, XZf);
        return dir.normalized*( distanceXZ * k + vt);
    }
    public static Vector3 getV0ByVt(float vt, Vector3 dir, float k,float t)
    {
        float ekt = Mathf.Exp(-k * t);
        float vx0 = vt / ekt;
        return dir*vx0;
    }
    public override List<float> timeToReachY(float y, int timesCount)
    {
        List<float> results = new List<float>();
        
        if (checkThereIsTimeToReachY(y))
        {
            Vector3 pos0 = Pos0;
            Vector3 v0 = V0;
            Vector3 vf = Vf;
            float tf = 0;
            float t0 = 0;
            while (tf <= timeToReachYMaxFind)
            {
                tf += timeToReachYIncrement;
                Vector3 posf = getPositionAtTime(tf);
                vf = getVelocityAtTime(tf);
                if (pos0.y == y)
                {
                    results.Add(t0);
                    return results;
                }else if(posf.y == y){
                    results.Add(tf);
                    return results;
                }
                else if((pos0.y<y && posf.y > y) || (pos0.y > y && posf.y < y))
                {
                    Plane plane = new Plane(Vector3.up, Vector3.up * y);
                    Ray ray = new Ray(pos0,v0);
                    float lenght;
                    if(plane.Raycast(ray,out lenght))
                    {
                        float time = lenght / v0.magnitude;
                        results.Add(time);
                        return results;
                    }
                }
                v0 = vf;
                pos0 = posf;
            }
        }
        return results;
    }
    bool checkThereIsTimeToReachY(float y)
    {
        float angle = Vector3.Angle(Vector3.up * (y - Pos0.y), V0);
        return angle < 90;
    }
}
