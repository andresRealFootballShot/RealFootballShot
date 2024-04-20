using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class pruebas : MonoBehaviour
{
    public Transform transDir1, transDir2;
    public Transform ball,goalKeeper;
    public Transform player;
    public float distance = 5;
    public Rigidbody ballRigidbody;
    public Vector3 dir;
    public float force = 10;
    public Transform impactPointTrans;
    public float height;
    public Vector3 dir2;
    public float force2;
    public float time = 5;
    public float k;
    public float v2Magnitude, v1Magnitude;
    public float zPrueba;
    public float t;
    void Start()
    {
        //k=0.34792
        /*OptimalPosition optimalPosition = new OptimalPosition(transDir1.forward, transDir2.forward, transDir1.position, transDir2.position,t, v2Magnitude, v1Magnitude, 0, 0, 10, 10);
        optimalPosition.calculate();
        StartCoroutine(prueba(optimalPosition.z, optimalPosition.k, t));*/
        //print("k = " + optimalPosition.calculateK(5));
        //optimalPosition.calculateZ(out z1, out z2);
        //print(z1 + " " + z2);
        //float vf= calculateVelocity2(time, dir2 * force2, k, ballRigidbody.mass);
        //calculateVerticalVelocityWithAirResistance();
        //print("velocity=" + vf);
        //optimalPosition.calculateK();
        //print("c "+optimalPosition.calculateK(0));
    }
    bool floatIsNotInfinityOrNan(float value)
    {
        return !(float.IsNaN(value) || float.IsInfinity(value));
    }
    float p;
    private void Update()
    {
        /*
        OptimalPosition optimalPosition = new OptimalPosition(transDir1.forward, transDir2.forward, transDir1.position, transDir2.position, t, v2Magnitude, v1Magnitude, 0, 0, 10, 10);
        float z1, z2;
        optimalPosition.calculateZ(out z1, out z2);
        float k1, k2;
        optimalPosition.calculateK(zPrueba, out k1, out k2);
        if (floatIsNotInfinityOrNan(k1) && p < Mathf.Abs(k1))
        {
            p = Mathf.Abs(k1);
        }
        if (floatIsNotInfinityOrNan(k2) && p < Mathf.Abs(k2))
        {
            p = Mathf.Abs(k2);
        }*/
        //print("p=" + p);
        //print("k1="+k1 + " | k2="+k2);
        //calculateZ();
    }

    float maxValue(float v1, float v2)
    {
        if (v1 > v2)
        {
            return v1;
        }
        else
        {
            return v2;
        }
    }
    IEnumerator prueba(float z,float k, float t)
    {
        float time = 0,time2=0;
        Vector3 v3 = transDir1.forward;
        Vector3 v1 = transDir2.forward * v1Magnitude;
        Vector3 q0 = transDir1.position;
        Vector3 p = transDir2.position;
        Vector3 pq0 = q0 - p;
        Vector3 ballInitPosition = player.position;
        Vector3 PlayerP = p - player.position;
        Vector3 Q = q0 + k * v3;
        Vector3 FP = p + v1 * z;
        Vector3 QFP = FP - Q;
        goalKeeper.position = Q;
        //print("werq "+(Vector3.Distance(Q,FP)/v2Magnitude) + " | "+((Vector3.Distance(p,FP)/v1Magnitude) + t));
        while (time < t)
        {
            ball.position = Vector3.Lerp(ballInitPosition,transDir2.position,time/t);
            goalKeeper.position = Q + QFP.normalized * v2Magnitude * time;
            yield return null;
            time += Time.deltaTime;
        }
        time2 = time;
        time = 0;
        
        while (time<z)
        {
            ball.position = p + v1*time;
            goalKeeper.position = Q + QFP.normalized * v2Magnitude * (time + t);
            yield return null;
            time += Time.deltaTime;
            time2 += Time.deltaTime;
        }
        //print("time=" + time2 + " | distance=" + Vector3.Distance(goalKeeper.position, FP));
    }
    Vector3 calculateVelocity(float time, Vector3 v0, float k, float mass)
    {
        return v0 * Mathf.Exp(-(k / mass) * time);
    }
    float calculateVelocity2(float time, Vector3 v0, float k, float mass)
    {
        return v0.magnitude - 9.8f*k*time;
    }
    float calculateVelocity3(float time, Vector3 v0)
    {
        SphereCollider sphereCollider = ballRigidbody.GetComponent<SphereCollider>();
        float mass = ballRigidbody.mass;
        float radius = sphereCollider.radius;
        float I = (2 / 3) * mass * radius * radius;
        return (0.1f * mass * 9.8f * radius * time) / I;
    }
    float calculateVelocity4(float time, Vector3 v0)
    {
        SphereCollider sphereCollider = ballRigidbody.GetComponent<SphereCollider>();
        float mass = ballRigidbody.mass;
        float radius = sphereCollider.radius;
        float I = (2 / 3) * mass * radius * radius;
        return v0.magnitude/(1+I/(mass*radius*radius));
    }
    void calculateHorintalVelocityWithAirResistance()
    {
        float drag = ballRigidbody.drag;
        float initVelocity = force2;
        float endVelocity = initVelocity * Mathf.Exp(-drag*time);
        print("calculateVelocityDrag="+endVelocity);
    }
    void calculateVerticalVelocityWithAirResistance()
    {
        //float drag = ballRigidbody.drag;
        float drag = 0.3f;
        float initVelocity = force2;
        float angle = Vector3.Angle(dir2, dir2 - Vector3.up * dir2.y)*Mathf.Deg2Rad;
        float g = Mathf.Abs( Physics.gravity.y);
        //float m = ballRigidbody.mass;
        float m = 143;
        float t = Mathf.Sqrt(m / (drag * g))*acosh(Mathf.Exp((ballRigidbody.position.y*drag)/m));
        //float vt = Mathf.Sqrt(m*-g/drag)*(float)Math.Tanh(t/Mathf.Sqrt(m/(-g*drag)));
        float vt = Mathf.Sqrt((m*g)/drag) * (float)Math.Tanh(time * Mathf.Sqrt(g*drag /m));
        //float vt = g * time;
        float endVelocity = initVelocity* Mathf.Sin(angle) * Mathf.Exp(g * time/ vt)-vt*(1-Mathf.Exp(-g*time/vt));
        //float endVelocity = vt* (float)Math.Tanh(t *Mathf.Sqrt(m* / (* drag)));
        print("calculateVelocityDrag endvelocity=" + endVelocity + " | vt= "+vt+" | t="+t);
    }
    /*void calculateVerticalVelocityWithAirResistance()
    {
        float drag = ballRigidbody.drag;
        float initVelocity = force2;
        float angle = Vector3.Angle(dir2, dir2 - Vector3.up * dir2.y) * Mathf.Deg2Rad;
        float g = Physics.gravity.y;
        float m = ballRigidbody.mass;
        float t = Mathf.Sqrt(m / (drag * -g)) * acosh(Mathf.Exp((ballRigidbody.position.y * drag) / m));
        //float vt = Mathf.Sqrt(m*-g/drag)*(float)Math.Tanh(t/Mathf.Sqrt(m/(-g*drag)));
        float vt = Mathf.Sqrt((m * -g) / drag);
        //float vt = g * time;
        //float endVelocity = initVelocity* Mathf.Sin(angle) * Mathf.Exp(g * time/ vt)-vt*(1-Mathf.Exp(-g*time/vt));
        float endVelocity = m*g/k - Mathf.Exp(-(drag*time/m))*(m*g/k-force2);
        print("calculateVelocityDrag=" + endVelocity + " | vt= " + vt + " | t=" + t);
    }*/
    float acosh(float x)
    {
        return (float)Math.Log(x + Math.Sqrt(x * x - 1));
    }
    Vector3 getPointOfLine(Vector3 point,Vector3 dir,float lenght)
    {
        float x = point.x + lenght * dir.x;
        float y = point.y + lenght * dir.y;
        float z = point.z + lenght * dir.z;
        return new Vector3(x,y,z);
    }
    IEnumerator coroutine(float duration)
    {
        float time = 0;
        while (time < duration)
        {
            if ( Mathf.Abs( height - ballRigidbody.position.y) < 0.1f)
            {
                print("ball reach height | time =" + time);
            }
            yield return null;
            time += Time.deltaTime;
        }
    }
    void calculatePoints()
    {
        Vector3 point1 = transDir1.position;
        Vector3 point2 = transDir2.position;
        Vector3 dir1 = transDir1.forward;
        Vector3 dir2 = transDir2.forward;
        float x2menosx1 = point2.x - point1.x;
        float vx2menosvx1 = dir2.x - dir1.x;
        float y2menosy1 = point2.y - point1.y;
        float vy2menosvy1 = dir2.y - dir1.y;
        float z2menosz1 = point2.z - point1.z;
        float vz2menosvz1 = dir2.z - dir1.z;
        float a = Mathf.Pow(vx2menosvx1, 2) + Mathf.Pow(vy2menosvy1, 2) + Mathf.Pow(vz2menosvz1, 2);
        float b = 2 * x2menosx1 * vx2menosvx1 + 2 * y2menosy1 * vy2menosvy1 + 2 * z2menosz1 * vz2menosvz1;
        float c = Mathf.Pow(x2menosx1, 2) + Mathf.Pow(y2menosy1, 2) + Mathf.Pow(z2menosz1, 2) - Mathf.Pow(distance, 2);
        float result1, result2;
        MyFunctions.SolveQuadratic(a, b, c, out result1, out result2);
        //print("Results = " + result1 + " " + result2);
        Vector3 point1Result1 = getPointOfLine(point1, dir1, result1);
        Vector3 point2Result1 = getPointOfLine(point2, dir2, result1);
        Vector3 point1Result2 = getPointOfLine(point1, dir1, result2);
        Vector3 point2Result2 = getPointOfLine(point2, dir2, result2);
        //print(Vector3.Distance(point1Result1, point2Result1) + " " + Vector3.Distance(point1Result2, point2Result2));
        Debug.DrawLine(point1-100*dir1,point1+100*dir1,Color.red);
        Debug.DrawLine(point2 - 100 * dir2, point2 + 100 * dir2, Color.red);
        Debug.DrawLine(point1Result1, point2Result1, Color.green);
        Debug.DrawLine(point1Result2, point2Result2, Color.blue);
    }
    
}

