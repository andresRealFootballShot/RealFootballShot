using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using DOTS_ChaserDataCalculation;

[BurstCompile]

public class StraightXZDragPathDOTS
{
    public static float getT(Vector3 x0, Vector3 xf, float v0, float k)
    {
        float d = Vector3.Distance(MyFunctions.setY0ToVector3(x0), MyFunctions.setY0ToVector3(xf));
        float a = (d * k) / v0;
        float ln = Mathf.Log(1 - a);
        float t = ln / -k;
        //print("A=" + A + " | A0=" + A0);
        return Mathf.Clamp(t, 0, Mathf.Infinity);
    }
    public static float getT(float d, float v0, float k)
    {
        float a = (d * k) / v0;
        float ln = Mathf.Log(1 - a);
        float t = ln / -k;
        //print("A=" + A + " | A0=" + A0);
        return Mathf.Clamp(t, 0, Mathf.Infinity);
    }
    public static void getXZV0(ref GetV0DOTSResult result,float t, Vector3 Pos0, Vector3 Posf, float maxKickForce, ref GetStraightV0Params getV0Params,float k)
    {
        Vector3 pos0 = MyFunctions.setY0ToVector3(Pos0);
        Vector3 posf = MyFunctions.setY0ToVector3(Posf);
        float d = Vector3.Distance(pos0, posf);
        float maxControlSpeed = Mathf.Lerp(2, getV0Params.maxControlSpeed, d / getV0Params.maxControlSpeedLerpDistance);
        Vector3 v0 = Vector3.zero, vt = Vector3.zero;
        float vtSpeed = 0;
        if (t != 0)
        {
            v0 = getV0(t, pos0, posf, k);
            vt = getVelocityAtTime(t, v0, k);
            vtSpeed = vt.magnitude;
        }


        bool maximumControlSpeedReached = false;
        if (vt.magnitude > maxControlSpeed || t == 0)
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
        result.foundedResult = true;
        result.vt = vtSpeed;
        result.v0 = v0;
        result.v0Magnitude = v0.magnitude;
        result.maximumControlSpeedReached = maximumControlSpeedReached;
        result.maxKickForceReached = maxKickForceReach;

        if (result.maximumControlSpeedReached || result.maxKickForceReached)
        {
            float t2 = StraightXZDragPathDOTS.getT(pos0, posf, result.v0Magnitude, k);
            result.ballReachTargetPositionTime = t2;
        }
    }
    public static void getXZV0(ref GetPassV0Element data,ref PathDataDOTS pathComponent,float t,Vector3 Pos0,Vector3 Posf,float maxKickForce)
    {
        Vector3 pos0 = MyFunctions.setY0ToVector3(Pos0);
        Vector3 posf = MyFunctions.setY0ToVector3(Posf);
        float d = Vector3.Distance(pos0, posf);
        float maxControlSpeed = Mathf.Lerp(2, data.maxControlSpeed, d / data.maxControlSpeedLerpDistance);
        Vector3 v0 = Vector3.zero, vt = Vector3.zero;
        float vtSpeed = 0;
        if (t != 0)
        {
            v0 = getV0(t, pos0, posf, data.k);
            vt = getVelocityAtTime(t, v0, data.k);
            vtSpeed = vt.magnitude;
        }
       
        
        bool maximumControlSpeedReached = false;
        if (vt.magnitude > maxControlSpeed || t == 0)
        {
            v0 = getV0ByVt(maxControlSpeed, pos0, posf, data.k);
            vtSpeed = maxControlSpeed;
            maximumControlSpeedReached = true;
        }
        bool maxKickForceReach = false;
        if (v0.magnitude > maxKickForce)
        {
            v0 = v0.normalized * maxKickForce;
            maxKickForceReach = true;
        }
        data.result.foundedResult = true;
        data.result.vt = vtSpeed;
        data.result.v0 = v0;
        data.result.v0Magnitude = v0.magnitude;
        data.result.maximumControlSpeedReached = maximumControlSpeedReached;
        data.result.maxKickForceReached = maxKickForceReach;

        if (data.result.maximumControlSpeedReached || data.result.maxKickForceReached)
        {
            
            pathComponent.V0 = data.result.v0;
            pathComponent.v0Magnitude = data.result.v0Magnitude;
            pathComponent.normalizedV0 = data.result.v0.normalized;
            float t2 = StraightXZDragPathDOTS.getT(pos0,posf, data.result.v0Magnitude, data.k);
            data.t = t2;
            data.result.ballReachTargetPositionTime = t2;
        }
    }
    static Vector3 getVelocityAtTime(float t, Vector3 v0,float k)
    {
        float ekt = Mathf.Exp(-k * t);
        float vx = v0.x * ekt;
        float vz = v0.z * ekt;
        Vector3 velocity = new Vector3(vx, v0.y, vz);
        //Debug.Log("k");
        return velocity;
    }
    public static Vector3 getV0(float t, Vector3 pos0, Vector3 posf, float k)
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
        Vector3 XZ0 = new Vector3(pos0.x, 0, pos0.z);
        Vector3 XZf = new Vector3(posf.x, 0, posf.z);
        Vector3 dir = XZf - XZ0;
        float distanceXZ = Vector3.Distance(XZ0, XZf);
        return dir.normalized * (distanceXZ * k + vt);
    }
}
