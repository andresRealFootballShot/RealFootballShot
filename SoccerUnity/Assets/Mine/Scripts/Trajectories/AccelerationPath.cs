using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationPath
{
    public static float getV(float t, float v0,float a)
    {
        return v0 - Mathf.Abs(a) * t;
    }
    public static float getT(float v, float v0, float a)
    {
        //print("acceleration=" + acceleration);
        return Mathf.Abs((v - v0) / (a));
    }

    public static float getDistanceWhereStartDecelerate(float v0, float v2, float a1, float a2, float d3)
    {
        float d1 = ((v0 * v0) - (v2 * v2) + (2 * a2 * d3)) / (2 * (a2 - a1));
        return d1;
    }
    public static Vector3 getX(Vector3 x0, Vector3 x, Vector3 v0, float t, float a)
    {
        Vector3 dir = x - x0;
        return x0 + v0 * t + dir.normalized * (a * t * t) / 2;
    }
    public static float getX3(float v0, float t, float a)
    {
        return v0 * t + (a * t * t) / 2;
    }
    public static float getX2(float v0, float v, float a)
    {
        return ((v * v) - (v0 * v0)) / (2 * a);
    }
    public static bool getT(float d, float v0, float ac, out float result)
    {
        //No funciona con desaceleración
        float a = ac / 2;
        float b = v0;
        float c = -d;
        float t1, t2;
        //print("a=" + a + " | b=" + b + " | c=" + c);
        if (MyFunctions.SolveQuadratic(a, b, c, out t1, out t2))
        {
            //print(t1 + " " + t2);
            result = t1 < t2 ? t2 : t1;
            return true;
        }
        else
        {
            result = Mathf.Infinity;
            return false;
        }
    }
    public static bool getT(Vector3 x, Vector3 x0, Vector3 v0, float ac, out float result)
    {
        //No funciona con desaceleración
        float a = ac / 2;
        float b = v0.magnitude;
        float c = -Vector3.Distance(x0, x);
        float t1, t2;
        //print("a=" + a + " | b=" + b + " | c=" + c);
        if (MyFunctions.SolveQuadratic(a, b, c, out t1, out t2))
        {
            //print(t1 + " " + t2);
            result = t1 < t2 ? t2 : t1;
            return true;
        }
        else
        {
            result = Mathf.Infinity;
            return false;
        }
    }
}
