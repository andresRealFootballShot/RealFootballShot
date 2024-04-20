using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptimalPosition
{
    Vector3 v3, v1, q0, p;
    Vector3 pq0;
    float t,v2Magnitude,v1Magnitude,minK,maxK;
    float v2Pow2,v3Pow2, v1Pow2,pqPow2,tPow2,v3xv1,pqxv1,pqxv3;
    public float k, z;

    public OptimalPosition()
    {
    }

    public OptimalPosition(Vector3 v3,Vector3 v1,Vector3 q0,Vector3 p,float t,float v2Magnitude,float v1Magnitude,float minK,float maxK)
    {
        this.v3 = v3* v2Magnitude;
        this.v1 = v1*v1Magnitude;
        this.q0 = q0;
        this.p = p;
        this.t = t;
        this.v2Magnitude = v2Magnitude;
        this.v1Magnitude = v1Magnitude;
        this.minK = minK;
        this.maxK = maxK;
        pq0 = q0 - p;
        v2Pow2 = v2Magnitude * v2Magnitude;
        v3Pow2 = Vector3XVector3(this.v3, this.v3);
        v1Pow2 = Vector3XVector3(this.v1, this.v1);
        pqPow2 = Vector3XVector3(pq0, pq0);
        tPow2 = t * t;
        v3xv1 = Vector3XVector3(this.v3, this.v1);
        pqxv1 = Vector3XVector3(pq0, this.v1);
        pqxv3 = Vector3XVector3(pq0, this.v3);
    }
    public float calculate()
    {
        float z1, z2;
        k = minK;
        calculateZ(out z1, out z2);
        //float r1 = secondDerivative(z1);
        //float r2 = secondDerivative(z2);
        //Debug.Log("z1=" + z1 + " | z2=" + z2 + " | r1=" + r1 + " | r2=" + r2);
        List<float> listZ = new List<float>();
        listZ.Add(z1);
        listZ.Add(z2);
        List<float> maximums = new List<float>();
        //Debug.Log("z1="+z1+" z2="+z2);
        foreach (var item in listZ)
        {
            if (isMaximum(item))
            {
                maximums.Add(item);
            }
        }
        //print("z=" + z);
        //print("z=" + z+" | k=" + k);
        foreach (var maximum in maximums)
        {
            float k1, k2,k3;
            if(calculateK(maximum,out k1,out k2))
            {
                //Debug.Log("k1=" + k1 + " | " + "k2=" + k2);
                if (k1 > k2)
                {
                    k3 = k1;
                }
                else
                {
                    k3 = k2;
                }
                k3 = Mathf.Clamp(k3, minK, maxK);
                //Debug.Log("k3=" + k3);
                if (k3 > k)
                {
                    k = k3;
                    z = maximum;
                }
            }
            //Debug.Log("k1=" + k1+ " k2=" + k2);
        }
        //k = selectMaximum(listK);
        //Debug.Log("k=" + k);
        return k;
    }
    bool isMaximum(float v)
    {
        return secondDerivative(v) < 0;
    }
    
    public bool calculateZ(out float z1,out float z2)
    {
        //a=2*v3^2*v1^2 + 4*v3^2*v2^2
        float m = 4*(v3xv1*v3xv1 - (v3Pow2*(v1Pow2- v2Pow2)));
        float n = 8 * (v3Pow2*(pqxv1 + v2Pow2*t) - (pqxv3*v3xv1));
        float e = 4*(pqxv3*pqxv3 - v3Pow2*(pqPow2 - v2Pow2* tPow2));
        float hPow2 = (-2 * v3xv1) * (-2 * v3xv1);
        float f = 4*m*(m-hPow2);
        float r = 4*n*(m-hPow2);
        float u = n*n - 4*hPow2*e;
        //Debug.Log("rr - 4ac =" + (r * r - 4 * f * u));
        //Debug.Log("m=" + m + " | "+"n="+n + " | " + "e=" + e + " | " + "f=" + f + " | " + "r=" + r + " | " + "u=" + u);
        return MyFunctions.SolveQuadratic(f, r, u, out z1, out z2);
    }
    
    float secondDerivative(float z)
    {
        float m = 4 * (v3xv1 * v3xv1 - (v3Pow2 * (v1Pow2 - v2Pow2)));
        float n = 8 * (v3Pow2 * (pqxv1 + v2Pow2 * t) - (pqxv3 * v3xv1));
        float e = 4 * (pqxv3 * pqxv3 - v3Pow2 * (pqPow2 - v2Pow2 * tPow2));
        float a = v3Pow2;

        return (2*m*4*a*Mathf.Sqrt(m*z*z + n*z + e)-(2*m*z + n)*2*a*(2*m*z + n))/(16*a*a*(m*z*z + n*z + e)*Mathf.Sqrt(m*z*z + n*z + e));
    }
    public bool calculateK(float z,out float k1,out float k2)
    {
        float a = v3Pow2;
        float b = 2 * (pqxv3 - (v3xv1 * z));
        float c = ((v1Pow2 - v2Pow2)* (z * z)) - (2 * (pqxv1 + (v2Pow2 * t)) * z) + (pqPow2 - (v2Pow2 * tPow2));
        //Debug.Log("sqrt(bb-4ac)=" + prueba1 + " | -b="+b+ " | -b+sqrt(bb-4ac)="+(prueba2+prueba1));
        //Debug.Log("a=" + a +" | " + "b=" + b + " | c=" + c);
        return MyFunctions.SolveQuadratic(a, b, c, out k1, out k2);
    }

    bool floatIsNotInfinityOrNan(float value)
    {
        return !(float.IsNaN(value) || float.IsInfinity(value));
    }
    float Vector3XVector3(Vector3 v1, Vector3 v2)
    {
        Vector3 scale = Vector3.Scale(v1, v2);
        return scale.x + scale.y + scale.z;
    }

    /*
    float calculateZ()
    {
        Vector3 pq0 = q0 - p;
        //a=2*v3^2*v1^2 + 4*v3^2*v2^2
        float a = 4 * Vector3XVector3(v3, v1) * Vector3XVector3(v3, v1) - 4 * Vector3XVector3(v3, v3) * (v1.magnitude * v1.magnitude - v2Magnitude * v2Magnitude);
        float b = (8 * Vector3XVector3(v3, v3) * Vector3XVector3(pq0, v1) + v2Magnitude * v2Magnitude * t) - 8 * Vector3XVector3(pq0, v3) * Vector3XVector3(v3, v1);
        float c = 4 * Vector3XVector3(pq0, v3) * Vector3XVector3(pq0, v3) - 4 * v3.magnitude * v3.magnitude * (pq0.magnitude * pq0.magnitude - v2Magnitude * v2Magnitude * t * t);
        //print("a=" + a + " | b=" + b);
        //Ahora vamos a buscar el máximo de la ecuación
        if (a < 0)
        {
            //Hay un máximo en la ecuacion(parábola)
            //return c - b * b / (4 * a);
            return -b / (2 * a);
        }
        else if (a == 0)
        {
            //La ecuación es una recta
            if (b == 0)
            {
                //La recta no tiene pendiente
                return c;
            }
            else
            {
                //La recta tiene pendiente
                return Mathf.Infinity;
            }
        }
        else
        {
            //parabola hacia abajo
            return Mathf.Infinity;
        }
    }
    void checkMaximum()
    {
        float m = 4 * (v3xv1 * v3xv1 - (v3Pow2 * (v1Pow2 - v2Pow2)));
        float n = 8 * (v3Pow2 * (pqxv1 + v2Pow2 * t) - (pqxv3 * v3xv1));
        float e = 4 * (pqxv3 * pqxv3 - v3Pow2 * (pqPow2 - v2Pow2 * tPow2));
        float hPow2 = (-2 * v3xv1) * (-2 * v3xv1);
        float f = 4 * m * (m - hPow2);
        float r = 4 * n * (m - hPow2);
        float u = n * n - 4 * hPow2 * e;
        float sqrtpart = (r * r) - (4 * f * u);
        float z1 = (-r + Mathf.Sqrt(sqrtpart)) / (2 * f);
        derivateG2(z1);
        derivateG2(z1-0.01f);
        derivateG2(z1 + 0.01f);
    }
    void derivateG1(float z1)
    {
        float m = 4 * (v3xv1 * v3xv1 - (v3Pow2 * (v1Pow2 - v2Pow2)));
        float n = 8 * (v3Pow2 * (pqxv1 + v2Pow2 * t) - (pqxv3 * v3xv1));
        float e = 4 * (pqxv3 * pqxv3 - v3Pow2 * (pqPow2 - v2Pow2 * tPow2));
        float h = -2 * v3xv1;
        float a = v3Pow2;
        float p1 = ((-h) / (2 * a)) + (((2 * m * z1) + n) / (4 * a * Mathf.Sqrt((m * z1 * z1) + (n * z1) + e)));
        //float p2 = ((-h) / (2 * a)) - (((2 * m * z1) + n) / (4 * a * Mathf.Sqrt((m * z1 * z1) + (n * z1) + e)));
        Debug.Log("derivate 1 | "+ p1);
        //Debug.Log("derivate 2 | " + p2);
    }
    void derivateG2(float z1)
    {
        float m = 4 * (v3xv1 * v3xv1 - (v3Pow2 * (v1Pow2 - v2Pow2)));
        float n = 8 * (v3Pow2 * (pqxv1 + v2Pow2 * t) - (pqxv3 * v3xv1));
        float e = 4 * (pqxv3 * pqxv3 - v3Pow2 * (pqPow2 - v2Pow2 * tPow2));
        float h = -2 * v3xv1;
        float a = v3Pow2;
        //float p1 = ((-h) / (2 * a)) + (((2 * m * z1) + n) / (4 * a * Mathf.Sqrt((m * z1 * z1) + (n * z1) + e)));
        float p2 = ((-h) / (2 * a)) - (((2 * m * z1) + n) / (4 * a * Mathf.Sqrt((m * z1 * z1) + (n * z1) + e)));
        //Debug.Log("derivate 1 | " + p1);
        Debug.Log("derivate 2 | " + p2);
    }*/
}
