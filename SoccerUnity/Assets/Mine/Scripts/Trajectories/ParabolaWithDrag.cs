using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ParabolaWithDrag:Path
{
    float g;
    public float impactTime;
    public Vector3 impactPoint;
    public float k;
    SortedList pointsAtTime = new SortedList();
    SortedList velocityAtTime = new SortedList();
    public ParabolaWithDrag(Vector3 _pos0, Vector3 _V0, float _t0, float _g , float _k) : base(_pos0,_V0,_t0)
    {
        g = _g;
        Vf = Vector3.down *( g/_k);
        k = _k;
        //minV = getMinVelocity();
        //Debug.Log("vf=" + vfMagnitude);
    }
    public ParabolaWithDrag(Vector3 _pos0, Vector3 _V0, Vector3 _Vf, float _t0, float _g, float _k) : base(_pos0, _V0,_Vf, _t0)
    {
        g = _g;
        Vf = Vector3.down * (g / _k);
        k = _k;
        //minV = getMinVelocity();
        //Debug.Log("vf=" + vfMagnitude);
    }
    public Vector3 getV0(float t, Vector3 pos0, Vector3 posf)
    {
        Vector2 XZ0 = new Vector2(pos0.x,pos0.z);
        Vector2 XZf = new Vector2(posf.x, posf.z);
        Vector2 dir = XZf - XZ0;
        float distanceXZ = Vector3.Distance(XZ0, XZf);
        float e = (1 - Mathf.Exp(-k * t));
        Vector2 vxz0 = dir.normalized*(distanceXZ * k / e);

        float distanceY = posf.y - pos0.y;
        float vf = g / k;
        float vy0 = ((distanceY + vf * t) * k / e) - vf;
        return new Vector3(vxz0.x,vy0,vxz0.y);
    }
    
    public List<float> timeToReachY2(float y,float precision)
    {
        float maxY = getMaximumY();
        List<float> result = new List<float>();
        if (maxY > y)
        {

            float maxYtime = getMaximumYTime();
            float p = getPositionYAtTime(maxYtime);
            //Debug.Log("maxYtime=" + maxYtime+" | p="+p+ " | maxY="+maxY);
            float t = busquedaBinaria1(y, 0, maxYtime, precision);
            if (t != -1)
            {
                result.Add(t);
            }
            float y2;
            float t2 = maxYtime;
            int count = 0;
            do
            {
                t2 += 0.5f;
                y2 = getPositionYAtTime(t2);
                count++;
                if (count > 100)
                {
                    Debug.Log("n");
                    break;
                }
            } while (y2>y);
            t = busquedaBinaria2(y, maxYtime, t2 , precision);
            if (t != -1)
            {
                result.Add(t);
                result.Sort();
            }
        }
        else if (maxY == y)
        {
            float maxYtime = getMaximumYTime();
            
            result.Add(maxYtime);
        }
        return result;
    }
    float busquedaBinaria1(float y, float t0, float tf,float precision)
    {
        int count = 0;
        while (t0 <= tf)
        {
            count++;
            if (count > 100)
            {
                Debug.Log("m");
                break;
            }
            float t = (t0 + tf) / 2;
            float result = getPositionYAtTime(t);
            if (Mathf.Abs(result - y) <= precision)
            {
                return t;
            }
            // Si no, debido a que esto ya está ordenado, analizamos dónde podría estar
            if (y < result)
            {
                // Si lo que buscamos es menor, debe estar a la izquierda
                tf = t;
            }
            else
            {
                // Si no, está a la derecha
                t0 = t;
            }
        }
        return -1;
    }
    float busquedaBinaria2(float y, float t0, float tf, float precision)
    {
        //Es igual que busqueda binaria 1 pero teniendo en cuenta que el objeto está cayendo
        int count = 0;
        while (t0 <= tf)
        {
            count++;
            if (count > 100)
            {
                Debug.Log("l");
                break;
            }
            float t = (t0 + tf) / 2;
            float result = getPositionYAtTime(t);
            if (Mathf.Abs(result - y) <= precision)
            {
                return t;
            }
            if (y > result)
            {
                //La comprobación es alreves que en busqueda binaria 1
                tf = t;
            }
            else
            {
                // Si no, está a la derecha
                t0 = t;
            }
        }
        return -1;
    }
    public override List<float> timeToReachY(float y,int timesCount)
    {
        List<float> results = new List<float>();
        int count = 0;
        for (int i = pointsAtTime.Count-1; i>=0;i--)
        {
            Vector3 pos1 = (Vector3)pointsAtTime.GetByIndex(i);
            if (Mathf.Abs(pos1.y - y) < 0.005f)
            {
                results.Add((float)pointsAtTime.GetKey(i));
                count++;
                if (count == timesCount)
                {
                    return results;
                }
            }
            else if (pos1.y < y)
            {
                if (i-1>= 0 )
                {
                    Vector3 pos2 = (Vector3)pointsAtTime.GetByIndex(i-1);
                    if (pos2.y > y)
                    {
                        float t = getPlaneRayIntersectionTime((float)pointsAtTime.GetKey(i - 1), y, pos2, pos1);
                        results.Add(t);
                        count++;
                        if (count==timesCount)
                        {
                            return results;
                        }
                    }
                }
            }
            else
            {
                if (i - 1 >= 0)
                {
                    Vector3 pos2 = (Vector3)pointsAtTime.GetByIndex(i - 1);
                    if (pos2.y < y)
                    {
                        float t = getPlaneRayIntersectionTime((float)pointsAtTime.GetKey(i - 1), y, pos2, pos1);
                        results.Add(t);
                        count++;
                        if (count == timesCount)
                        {
                            return results;
                        }
                    }
                }
            }
        }
        return results;
    }
    public override Vector3 getPositionAtTime(float t)
    {
        Vector2 vx0 = new Vector2(V0.x, V0.z);
        float ekt = Mathf.Exp(-k * t);
        Vector2 x = (vx0 / k) * (1 - ekt);
        float y = getPosYAtTime(t);
        //Debug.Log("t=" + t + " | y=" + y);
        Vector3 endPosition = new Vector3(x.x, y, x.y) + new Vector3(Pos0.x,0,Pos0.z);
        if (!pointsAtTime.ContainsKey(t))
        {
            pointsAtTime.Add(t, endPosition);
        }
        return endPosition;
    }
    public float getPosYAtTime(float t)
    {
        float ekt = Mathf.Exp(-k * t);
        float y = -vfMagnitude * t + ((vy0 + vfMagnitude) / k) * (1 - ekt);
        return y + Pos0.y;
    }
    public float getPositionYAtTime(float t)
    {
        float ekt = Mathf.Exp(-k * t);
        float y = -vfMagnitude * t + ((vy0 + vfMagnitude) / k) * (1 - ekt);
        //y += Pos0.y;
        return y;
    }
    Vector3 calculateVelocityAtTime(float t)
    {
        float ekt = Mathf.Exp(-k * t);
        float vx = vx0.x * ekt;
        float vz = vx0.y * ekt;
        float vy = -vfMagnitude + (vy0 + vfMagnitude) * ekt;
        Vector3 velocity = new Vector3(vx, vy, vz);
        velocityAtTime.Add(t, velocity);
        return velocity;
    }
    public override Vector3 getVelocityAtTime(float t)
    {
        if (!velocityAtTime.ContainsKey(t))
        {
            return calculateVelocityAtTime(t);
        }
        else
        {
            return (Vector3)velocityAtTime[t];
        }
    }
    public override List<float> getTimeOfVelocity(float v)
    {
        /*
        float xd = (vx0.magnitude * (yd+vf)) / (vy0+vf);
        float t = (Mathf.Log(xd ) - Mathf.Log(vx0.magnitude))/ -k;*/
        float u = vy0 + vfMagnitude;
        float a = (vx0.magnitude* vx0.magnitude) + (u * u);
        float b = -2 * vfMagnitude * u;
        float c = (vfMagnitude * vfMagnitude) - (v * v);
        float q1, q2;
        MyFunctions.SolveQuadratic(a, b, c, out q1, out q2);
        
        float t1 = Mathf.Log(q1) / -k;
        float t2 = Mathf.Log(q2) / -k;
        List<float> list = new List<float>();
        if (!MyFunctions.floatIsNanOrInfinity(t1))
        {
            list.Add(t1);
        }
        if (!MyFunctions.floatIsNanOrInfinity(t2))
        {
            list.Add(t2);
        }
        Debug.Log("getTimeOfVelocity v="+v+" | t1="+t1 + " | t2=" + t2+" | minV="+minV);
        return list;
    }
    float getMinVelocity()
    {
        float u = vy0 + vfMagnitude;
        float a = (vx0.magnitude * vx0.magnitude) + (u * u);
        float b = -2 * vfMagnitude * u;

        float minV1 = Mathf.Sqrt((-(b * b) + (4 * a * vfMagnitude * vfMagnitude)) / (4 * a));
        //float minV2 = Mathf.Sqrt((vfMagnitude*vfMagnitude)-((2*b*b)/(4*a)));
        float minV2 = vfMagnitude;
        float minV;
        if (MyFunctions.floatIsNan(minV1))
        {
            minV = 0;
        }
        else
        {
            float aux = minV1 + 0.001f;
            float c = (vfMagnitude * vfMagnitude) - (aux * aux);
            float q1, q2;
            MyFunctions.SolveQuadratic(a, b, c, out q1, out q2);
            if (q1 > 0)
            {
                minV = minV1;
            }
            else
            {
                minV = minV2;
            }
            Debug.Log("q1=" + q1 + " | q2=" + q2);
        }
        Debug.Log("minV1=" + minV1 + " | minV3=" + minV2 + " | minV=" + minV);
        return minV;
    }
    public float getMaximumYTime()
    {
        float t = (1/k)*Mathf.Log(1+(k*vy0/g));
        return t;
    }
    public float getMaximumY()
    {
        float y = vy0 / k - (g / (k * k)) * Mathf.Log(1+(k*vy0/g));
        return y;
    }
    public static float getMaximumY(float vy0,float k,float g)
    {
        float y = vy0 / k - (g / (k * k)) * Mathf.Log(1 + (k * vy0 / g));
        return y;
    }
    public override Path newPath(Vector3 _pos0, Vector3 _V0, float _t0)
    {
        return new ParabolaWithDrag(_pos0, _V0, _t0,g,k);
    }
    public void DrawPath2(string info, float maxVelocity)
    {
        Debug.Log("A "+pointsAtTime.Count + " | " + velocityAtTime.Count);
        for (int i = 0; i < pointsAtTime.Count; i++)
        {
            Vector3 Pos0 =(Vector3) pointsAtTime.GetByIndex(i);
            Vector3 V0 = (Vector3) velocityAtTime.GetByIndex(i);
            //Vector3 Posf = Pos0 + V0;
            
            GUIStyle style = new GUIStyle();
            style.fontSize = 10;
            style.normal.textColor = Color.yellow;
#if UNITY_EDITOR
            Handles.color = Color.green;
            Handles.Label(Pos0 + Vector3.up * 0.5f, info + " | Position=" + Pos0.ToString() + " | Velocity=" + V0.magnitude.ToString(), style);
#endif
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(Pos0, 0.05f);
            Debug.DrawLine(Pos0, Posf);
            Debug.DrawRay(Pos0, V0 * 2 / maxVelocity, Color.green);
        }

    }
}
