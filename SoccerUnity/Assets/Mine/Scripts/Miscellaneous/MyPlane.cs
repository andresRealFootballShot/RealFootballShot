using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
[BurstCompile]
public class MyPlane
{
    [BurstCompile]
    public static bool GetSide(Plane plane, in Vector3 point)
    {
        Vector3 dir1 = point + plane.distance * plane.normal;
        float dot = Vector3.Dot(plane.normal, dir1.normalized);
        return dot >= 0;
    }
    [BurstCompile]
    public static bool Raycast(in Ray ray,in Plane plane,out float lenght)
    {
        Vector3 P0 = plane.normal * -plane.distance;
        Vector3 p0 = ray.origin;
        Vector3 v = ray.direction;
        float X0 = P0.x;
        float Y0 = P0.y;
        float Z0 = P0.z;
        float A = plane.normal.x;
        float B = plane.normal.y;
        float C = plane.normal.z;
        float x0 = p0.x;
        float y0 = p0.y;
        float z0 = p0.z;
        float a = v.x;
        float b = v.y;
        float c = v.z;
        float t1 = (A * (X0 - x0)) + (B * (Y0 - y0)) + (C * (Z0 - z0));
        float t2 = (A*a)+(B*b)+(C*c);
        if (t2 != 0)
        {
            lenght = t1 / t2;
            if (lenght >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            lenght = 0;
            return false;
        }
    }
}
