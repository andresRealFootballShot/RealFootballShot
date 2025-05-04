using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationPath
{
    public static float getV(float t, float v0,float a)
    {
        return v0 - Mathf.Abs(a) * t;
    }
    public static float getV2(float v0, float t, float a)
    {
        return v0 + a * t;
    }
    public static float getV3(float v0,float vf, float t, float a)
    {
        float sign = Mathf.Sign(vf-v0);
        return Mathf.Clamp(v0 + sign*a * t,vf,Mathf.Infinity);
    }
    public static float getT_StartToDesacelerate(float v0,float vf, float a,float da,float x)
    {
        float _a =(a / 2) - ((a * a) / (2 * da));
        float _b = v0 - ((a*(v0+vf))/da);
        float _c = (-((v0 * v0) - (vf * vf)) / (2 * da) )- x;
        MyFunctions.SolveQuadratic(_a,_b, _c,out float t1,out float t2);
        if(t1 > 0)
        {
            return t1;
        }else if (t2 > 0)
        {
            return t2;
        }
        else
        {
            return t1;
        }
    }
    public static float getX_StartToDesacelerate(float v0, float vf, float a, float da, float x,float vmax)
    {
        float td = (vf - vmax) / da;
        float xd = (vmax * td) + (0.5f * da * td * td);
        return xd;
    }
    public static float getT_StartToDesacelerate2(float v0, float vf, float a, float da, float x)
    {
        float t1 = a*(1-(a/da));
        float t = Mathf.Sqrt((2 * x )/ t1);
        return t;
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
    public static float getDistanceWhereStartDecelerate2(float v0, float v2, float a)
    {
        float t = (v2 - v0) / a;
        float d1 = (v0*t)+0.5f*a*t*t;
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
