using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Path
{
    public virtual Vector3 Pos0 { get; set; }
    public virtual Vector3 Posf { get; set; }
    public virtual Vector3 V0 { get; set; }
    private Vector3 _Vf { get; set; }
    public virtual Vector3 Vf { get=>_Vf; set { _Vf = value; vfMagnitude = value.magnitude; } }
    public virtual Vector3 AverageV { get => (V0 + Vf) / 2; }
    public float vfMagnitude;
    public virtual float minV { get; set; }
    protected Vector2 vx0, x0;
    protected float y0, vy0;
    public virtual float t0 { get; set; }
    public virtual float tf { get; set; }
    public Path(Vector3 _pos0, Vector3 _V0,float _t0)
    {
        Pos0 = _pos0;
        V0 = _V0;
        vx0 = new Vector2(V0.x, V0.z);
        x0 = new Vector2(Pos0.x, Pos0.z);
        y0 = Pos0.y;
        vy0 = V0.y;
        t0 = _t0;
        Posf = Vector3.positiveInfinity;
        Vf = V0;
        tf = Vector3.Distance(Posf, Pos0) / V0.magnitude;
        minV = 0;
    }
    public Path()
    {
        Pos0 = Vector3.zero;
        V0 = Vector3.zero;
        vx0 = new Vector2(V0.x, V0.z);
        x0 = new Vector2(Pos0.x, Pos0.z);
        y0 = Pos0.y;
        vy0 = V0.y;
        t0 = 0;
        Posf = Vector3.positiveInfinity;
        Vf = Vector3.positiveInfinity;
        tf = float.PositiveInfinity;
        minV = 0;
    }
    public Path(Vector3 _pos0,Vector3 _posf, Vector3 _V0, float _t0)
    {
        Pos0 = _pos0;
        V0 = _V0;
        vx0 = new Vector2(V0.x, V0.z);
        x0 = new Vector2(Pos0.x, Pos0.z);
        y0 = Pos0.y;
        vy0 = V0.y;
        t0 = _t0;
        Posf = _posf;
        Vf = V0;
        tf = Vector3.Distance(Posf,Pos0) / V0.magnitude;
        minV = 0;
    }
    public Path(Vector3 _pos0, Vector3 _posf, Vector3 _V0,Vector3 _Vf, float _t0,float _tf)
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
    }
    public static float getT(float x, float x0,float speed)
    {

        //float A0 = angleBodyForwardDesiredVelocity;
        float d = x - x0;
        float t = d / speed;
        //print("A=" + A + " | A0=" + A0);
        return Mathf.Abs(t);
    }
    public virtual Vector3 getPositionAtTime(float t)
    {
        return Pos0 + V0 * t;
    }
    public virtual Path newPath(Vector3 _pos0, Vector3 _V0,float _t0)
    {
        return new Path(_pos0, _V0, _t0);
    }
    public virtual Vector3 getVelocityAtTime(float t)
    {
        return V0;
    }
    public virtual Vector3 getVelocityAtTime(float t,out Path path)
    {
        path = this;
        return V0;
    }
    public virtual List<float> timeToReachY(float y, int count)
    {
        Plane plane = new Plane(Vector3.up,Vector3.up*y);
        Ray ray = new Ray(Pos0,V0);
        float lenght;
        if(plane.Raycast(ray, out lenght))
        {
            List<float> list = new List<float>();
            list.Add(lenght/V0.magnitude);
            return list;
        }
        else
        {
            return new List<float>();
        }
    }
    /*public virtual List<float> timeToReachY(float y, int count)
    {
        Vector3 v1 = Vector3.up * y;
        //Vector3 v2 = (Posf-Pos0).normalized*V0.magnitude;
        Vector3 v2 = V0;
        float lenght = Vector3.Dot(v2, v1);
        Debug.Log(v1.ToString("F4") + " "+v2.ToString("F4") + " "+lenght+" "+ v1.magnitude);
        Debug.Log(lenght/V0.magnitude);
        if (lenght > 0)
        {
            float t = lenght / V0.magnitude;
            List<float> list =  new List<float>();
            list.Add(t);
            return list;
        }
        else
        {
            return new List<float>();
        }
    }*/
    protected float getPlaneRayIntersectionTime(float t0, float planeY, Vector3 pos1, Vector3 pos2)
    {
        Plane plane = new Plane(Vector3.up, Vector3.up * planeY);
        Vector3 rayOrigin = pos1;
        Vector3 rayDir = (pos2 - pos1).normalized;
        Ray ray = new Ray(rayOrigin, rayDir);
        float lenght;
        float v0 = getVelocityAtTime(t0).magnitude;
        plane.Raycast(ray, out lenght);
        float t = Mathf.Abs(lenght / v0) + t0;
        return t;
    }
    public virtual List<float> getTimeOfVelocity(float yd)
    {
        return new List<float>();
    }
    public virtual void getOptimalPointForReachTarget(Vector3 chaserPosition, float chaserSpeed,float offsetTime,out List<float> results)
    {
        Vector3 targetPosition = Pos0;
        Vector3 targetVelocity = V0;
        Vector3 vectorFromRunner = chaserPosition - targetPosition;
        float distanceToRunner = vectorFromRunner.magnitude;
        float pow2ChaserSpeed = chaserSpeed * chaserSpeed;
        float a = pow2ChaserSpeed - (targetVelocity.magnitude * targetVelocity.magnitude);
        //float b = 2 * Vector3.Dot(vectorFromRunner, targetVelocity);
        //float c = -distanceToRunner * distanceToRunner;
        float b = (2 * offsetTime* chaserSpeed * chaserSpeed) +(2 * Vector3.Dot(vectorFromRunner, targetVelocity));
        float c = (-distanceToRunner * distanceToRunner)+(offsetTime*offsetTime* pow2ChaserSpeed);
        float time1, time2;
        results = new List<float>();
        float maxDistance = Vector3.Distance(Pos0, Posf);
        if (MyFunctions.SolveQuadratic(a, b, c, out time1, out time2))
        {
            if (time1 != Mathf.Infinity && !float.IsNaN(time1) && time1 >= 0)
            {
                Vector3 optimalPoint = targetPosition + targetVelocity * time1;
                Vector3 optimalPointDir = optimalPoint - chaserPosition;
                float distance = Vector3.Distance(Pos0, optimalPoint);
                if (distance <= maxDistance)
                {
                    results.Add(time1);
                }
            }
            if (time2 != Mathf.Infinity && !float.IsNaN(time2) && time2 >= 0)
            {
                Vector3 optimalPoint = targetPosition + targetVelocity * time2;
                Vector3 optimalPointDir = optimalPoint - chaserPosition;
                float distance = Vector3.Distance(Pos0, targetPosition + targetVelocity * time2);
                if (distance <= maxDistance)
                {
                    results.Add(time2);
                    results.Sort();
                }
            }
        }
        
    }
    public void getOptimalPointForReachTarget(Vector3 chaserPosition, float chaserSpeed, float offsetTime,float scope, out List<float> results)
    {
        Vector3 targetPosition = Pos0;
        Vector3 targetVelocity = V0;
        Vector3 vectorFromRunner = chaserPosition - targetPosition;
        results = new List<float>();
        if (scope >= vectorFromRunner.magnitude && t0 == 0)
        {
            results.Add(0);
            return;
        }
        float distanceToRunner = Mathf.Clamp(vectorFromRunner.magnitude - scope,0,Mathf.Infinity);
        float pow2ChaserSpeed = chaserSpeed * chaserSpeed;
        float a = pow2ChaserSpeed - (targetVelocity.magnitude * targetVelocity.magnitude);
        //float b = 2 * Vector3.Dot(vectorFromRunner, targetVelocity);
        //float c = -distanceToRunner * distanceToRunner;
        float cos = Mathf.Cos(Vector3.Angle(vectorFromRunner, targetVelocity)*Mathf.Deg2Rad);
        float b = (2 * offsetTime * chaserSpeed * chaserSpeed) + (2 * targetVelocity.magnitude * distanceToRunner* cos);
        float c = (-distanceToRunner * distanceToRunner) + (offsetTime * offsetTime * pow2ChaserSpeed);
        float time1, time2;
        float maxDistance = Vector3.Distance(Pos0, Posf);
        if (MyFunctions.SolveQuadratic(a, b, c, out time1, out time2))
        {
            if (time1 != Mathf.Infinity && !float.IsNaN(time1) && time1 >= 0)
            {
                Vector3 optimalPoint = targetPosition + targetVelocity * time1;
                Vector3 optimalPointDir = optimalPoint - chaserPosition;
                float distance = Vector3.Distance(Pos0, optimalPoint);
                if (distance <= maxDistance)
                {
                    results.Add(time1);
                }
            }
            if (time2 != Mathf.Infinity && !float.IsNaN(time2) && time2 >= 0)
            {
                Vector3 optimalPoint = targetPosition + targetVelocity * time2;
                Vector3 optimalPointDir = optimalPoint - chaserPosition;
                float distance = Vector3.Distance(Pos0, targetPosition + targetVelocity * time2);
                if (distance <= maxDistance)
                {
                    results.Add(time2);
                    results.Sort();
                }
            }
        }

    }
    public virtual void getOptimalPointForReachTargetWhitAcceleration(ChaserData chaserData, float offsetTime,float scope,float accuracy, out List<float> results)
    {
        PublicPlayerData publicPlayerData = chaserData.publicPlayerData;
        Vector3 chaserPosition = publicPlayerData.position;
        float chaserSpeed = publicPlayerData.maxSpeed;
        getOptimalPointForReachTarget(chaserPosition, chaserSpeed, offsetTime,scope, out results);
        float t = results.Count == 0 ? 0 : results[0];

        results = new List<float>();
        float fullTime = t0 + t;
        float t4 = chaserData.getTimeToReachPoint(Pos0, scope);
        //Debug.Log("AAA t0=" + t0 + " | tf=" + tf + " | t4=" + t4 + " | t=" + t);
        if (Mathf.Abs(t0 - t4) < accuracy)
        {
            results.Add(t4 - t0);
            //Debug.Log("End BBB" + t0 + " | tf=" + tf + " | t4=" + t4);
            return;
        }
        //Debug.Log(publicPlayerData.name);
        float left = fullTime, right = tf;
        float testingT = fullTime;
        bool thereIsRight = tf == Mathf.Infinity ? false : true;
        float increase = 0.05f;
        int attempts = 0;
        float endT;

        while (right - left>0.01f && attempts<40)
        {
            
            //Vector3 testingOptimalPoint = Pos0 + V0 * (testingT - t0);
            Vector3 testingOptimalPoint = getPositionAtTime(testingT - t0);
            endT = chaserData.getTimeToReachPoint(testingOptimalPoint,scope);
            //Debug.Log("attempt=" + attempts + " | testingT=" + testingT + " | endT=" + endT + " | left=" + left + " | right=" + right + " | t0=" + t0 + " | tf=" + tf);
            
            
            if (Mathf.Abs(testingT - endT) < accuracy)
            {
                //Debug.Log("attempt=" + attempts + " | result=" + (endT - t0));
                results.Add(endT - t0);
                return;
            }
            if (endT > testingT)
            {
                left = testingT;
                if (thereIsRight)
                {
                    testingT += (right - left) / 2;
                }
                else
                {
                    testingT += increase;
                }
            }
            else
            {
                if (thereIsRight)
                {
                    thereIsRight = true;
                    right = testingT;
                    testingT -= (right - left) / 2;
                }
                else
                {
                    right = testingT;
                    testingT -= increase;
                    thereIsRight = true;
                }
            }
            attempts++;
        }
        //Debug.Log("attempt=" + attempts + " | result=" + (endT - t0));
        //results.Add(endT - t0);
    }
    public string getInfo()
    {
        return " | Velocity=" + V0.magnitude.ToString();
    }

    public virtual void DrawPath(string info,float maxVelocity,float sphereSize,bool printText)
    { 
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 10;
        style.normal.textColor = Color.yellow;
        //Handles.Label(Pos0 + Vector3.up * 0.5f, info + " | Position=" + Pos0.ToString() + " | Velocity=" + V0.magnitude.ToString(), style);
#if UNITY_EDITOR
        if(printText){
            Handles.color = Color.green;
            Handles.Label(Pos0 + Vector3.up * 0.15f,info , style);
        }
#endif
        Gizmos.color = Color.yellow;
        //Debug.Log(Pos0);
        Gizmos.DrawSphere(Pos0, sphereSize);
        Debug.DrawLine(Pos0, Posf,Color.red);
        //Debug.DrawRay(Pos0, V0*2/ maxVelocity, Color.green);
    }
}






    public class BouncyPathData
{
    public Vector3 position;
    public Vector3 velocity;
    public BouncyPathData(Vector3 _position, Vector3 _velocity)
    {
        position = _position;
        velocity = _velocity;
    }
    public BouncyPathData()
    {
    }
}
