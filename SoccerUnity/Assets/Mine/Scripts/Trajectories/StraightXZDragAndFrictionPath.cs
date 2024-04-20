using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightXZDragAndFrictionPath : Path
{
    float k;
    float timeToReachYIncrement;
    float timeToReachYMaxFind;
    float ballRadio;
    float friction;
    float g { get => -Physics.gravity.y; }
    float m;
    public StraightXZDragAndFrictionPath(Vector3 _pos0, Vector3 _posf, Vector3 _V0, Vector3 _Vf, float _t0, float _tf, float _drag,float ballRadio,float friction,float m) : base(_pos0, _posf, _V0, _Vf, _t0, _tf)
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
        this.ballRadio = ballRadio;
        this.friction = friction;
        this.m = m;
    }
    public StraightXZDragAndFrictionPath(Vector3 _pos0, Vector3 _V0, float _t0, float _drag,float friction,float m,float radio) : base(_pos0, _V0, _t0)
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
        this.friction = friction;
        this.m = m;
        ballRadio = radio;
    }
    public StraightXZDragAndFrictionPath(float _drag, float ballRadio, float friction, float m) : base()
    {
        k = _drag;
        timeToReachYIncrement = 0.1f;
        this.ballRadio = ballRadio;
        this.friction = friction;
        this.m = m;
    }
    public override Path newPath(Vector3 _pos0, Vector3 _V0, float _t0)
    {
        return new StraightXZDragAndFrictionPath(_pos0, Posf, _V0, Vf, _t0, tf, k, ballRadio, friction, m);
    }
    public GetV0Result getV0(float t,float accuracy, float maxControlSpeed,float maxControlSpeedLerpDistance, int maxAttempts)
    {
        float d = Vector3.Distance(Pos0, MyFunctions.setYToVector3(Posf,Pos0.y));
        maxControlSpeed = Mathf.Lerp(2, maxControlSpeed, d/maxControlSpeedLerpDistance);
        GetV0Result getV0Result = getV0(t, accuracy, maxAttempts);
        if (getV0Result.vt >= maxControlSpeed)
        {
            Vector3 v0;
            if(getV0WithVt(maxControlSpeed,accuracy,maxAttempts, out v0))
            {
                getV0Result.v0 = v0;
                getV0Result.maximumControlSpeedReached = true;
            }
            else
            {
                getV0Result.foundedResult = false;
            }
        }
        getV0Result.maximumControlSpeedReached = getV0Result.vt >= maxControlSpeed;
        return getV0Result;
    }
    public GetV0Result getV0(float t,float accuracy,int maxAttempts)
    {
        Vector3 dir = (Posf - Pos0).normalized;
        Vector3 straightXZDragPathV0 = StraightXZDragPath.getV0(t, Pos0, Posf, k);
        float left, right;
        left = straightXZDragPathV0.magnitude;
        right = Mathf.Infinity;
        float target = Vector3.Distance(Posf,Pos0);
        
        int attempts = 0;
        float centralV0 = straightXZDragPathV0.magnitude;
        float x = 3f;
        bool thereIsRight = false;
        
        while (left <= right && attempts <= maxAttempts)
        {
            Vector3 v0 = dir * centralV0;
            Vector3 vt = getVelocityAtTime(t, v0);
            Vector3 pos = getPositionAtTime(t,v0);
            float distance = Vector3.Distance(pos, Pos0);
            if (Mathf.Abs(distance - target) <= accuracy)
            {
                GetV0Result getV0Result = new GetV0Result(true,vt.magnitude, dir * centralV0);
                return getV0Result;
            }
            if (target < distance)
            {
                if (thereIsRight)
                {

                    right = centralV0;
                    centralV0 -= (right - left) / 2;
                }
                else
                {
                    right = centralV0;
                    centralV0 -= (right - left) / 2;
                    thereIsRight = true;
                }
            }
            else
            {
                if(thereIsRight)
                {
                    left = centralV0;
                    centralV0 += (right - left) / 2;
                }
                else
                {

                    left = centralV0;
                    centralV0 += x;
                }
            }
            //Debug.Log("centralV0=" + centralV0 + "  | distance=" + distance + " |target=" + target + " | left ="+left + " |right="+ right);
            attempts++;
        }
        return new GetV0Result(false);
    }
    public bool getT(Vector3 v0,Vector3 pos0,Vector3 posf,float k, float distanceAccuracy, int maxAttempts,out float result)
    {
        float left, right;
        left = StraightXZDragPath.getT(pos0, posf, v0.magnitude, k);
        right = Mathf.Infinity;
        
        float target = Vector3.Distance(pos0, posf);

        int attempts = 0;
        float proofT = left;
        float testIncrement = 0.1f;
        bool thereIsRight = false;
        while (left <= right && attempts <= maxAttempts)
        {
            Vector3 pos = getPositionAtTime(proofT, v0);
            float distance = Vector3.Distance(pos, pos0);
            if (Mathf.Abs(distance - target) <= distanceAccuracy)
            {
                result = proofT;
                return true;
            }
            if (target < distance)
            {
                if (thereIsRight)
                {

                    right = proofT;
                    proofT -= (right - left) / 2;
                }
                else
                {
                    right = proofT;
                    proofT -= (right - left) / 2;
                    thereIsRight = true;
                }
            }
            else
            {
                if (thereIsRight)
                {
                    left = proofT;
                    proofT += (right - left) / 2;
                }
                else
                {

                    left = proofT;
                    proofT += testIncrement;
                }
            }
            //Debug.Log("centralV0=" + centralV0 + "  | distance=" + distance + " |target=" + target + " | left ="+left + " |right="+ right);
            attempts++;
        }
        result = 0;
        return false;
    }
    public bool getV0WithVt(float Vt, float accuracy, int maxAttempts, out Vector3 result)
    {
        float left, right;
        left = StraightXZDragPath.getV0ByVt(Vt, Pos0, Posf, k).magnitude;
        right = Mathf.Infinity;
        float targetDistance = Vector3.Distance(Posf, Pos0);

        Vector3 dir = (Posf - Pos0).normalized;
        int attempts = 0;
        float centralV0 =left;
        float x=3;
        bool thereIsRight = false;
        //Debug.Log("max0=" + maxV0 + " | Vt=" + Vt);
        while (left <= right && attempts <= maxAttempts)
        {
            Vector3 pos = getPositionAtVt(Vt, dir * centralV0);
            float distance = Vector3.Distance(pos, Pos0);
            //Debug.Log((distance - targetDistance));

            //Debug.Log("centralV0=" + centralV0 + "  | distance=" + distance + " |target=" + targetDistance + " | left =" + left + " |right=" + right);
            if (Mathf.Abs(distance - targetDistance) <= accuracy)
            {
                //Debug.Log("result was founded, attempts=" + attempts);
                result = dir * centralV0;
                return true;
            }
            if (targetDistance < distance)
            {
                if (thereIsRight)
                {

                    right = centralV0;
                    centralV0 -= (right - left) / 2;
                }
                else
                {
                    right = centralV0;
                    centralV0 -= (right - left) / 2;
                    thereIsRight = true;
                }
            }
            else
            {
                if (thereIsRight)
                {

                    left = centralV0;
                    centralV0 += (right - left) / 2;

                }
                else
                {

                    left = centralV0;

                    centralV0 += x;
                }
            }
            attempts++;
        }
        //Debug.Log("attempts=" + attempts);
        result = dir * centralV0;
        return false;
    }
    public override Vector3 getVelocityAtTime(float t, out Path path)
    {
        path = this;
        return getVelocityAtTime(t);
    }
    
    public override Vector3 getVelocityAtTime(float t)
    {
        return getVelocityAtTime(t,V0);
    }
    public Vector3 getVelocityAtTime(float t,Vector3 V0)
    {
        float maxWt = getTofWMax_WithRollDrag(V0);
        if (k == 0)
        {
            return V0.normalized * (V0.magnitude - friction * 9.81f * maxWt);
        }
        else
        {
            float vf = (friction * g) / k;
            float rollSlipV = -vf + (V0.magnitude + vf) * Mathf.Exp(-k * Mathf.Clamp(maxWt, 0, t));
            if (t <= maxWt)
            {
                return V0.normalized * rollSlipV;
            }
            else
            {
                float pureRollV = rollSlipV * Mathf.Exp(-k * (t - maxWt));
                return V0.normalized * pureRollV;
            }
        }
    }
    public float getTime(Vector3 V0,float vt)
    {
        float maxWt = getTofWMax_WithRollDrag(V0);
        float vf = (friction * g) / k;
        float v0 = V0.magnitude;
        float v3 = getVelocityAtTime(maxWt, V0).magnitude;
        //Debug.Log("v3=" + v3+ " vt="+vt);
        if(vt < v3)
        {
            float v02 = -vf + (v0+vf)*Mathf.Exp(-k* maxWt);
            float t1 = Mathf.Log((v02 + vf) / (v0 + vf)) / -k;
            float t2 = Mathf.Log(vt/ v02) / -k;
            //Debug.Log("a t1="+t1+" | t2="+t2 + " "+maxWt+" "+v02);
            return t1 + t2;
        }
        else
        {
            float v02 = vt;
            float t1 = Mathf.Log((v02 + vf) / (v0 + vf)) / -k;
            //Debug.Log("b t1=" + t1 + " " + maxWt + " " + v02);
            return t1;
        }
            
    }
    public override void getOptimalPointForReachTargetWhitAcceleration(ChaserData chaserData, float offsetTime, float scope, float accuracy, out List<float> results)
    {
        PublicPlayerData publicPlayerData = chaserData.publicPlayerData;
        Vector3 chaserPosition = publicPlayerData.position;
        float chaserSpeed = publicPlayerData.maxSpeed;

        getOptimalPointForReachTarget(chaserPosition, chaserSpeed, offsetTime, scope, out results);
        float t = results.Count == 0 ? 0 : results[0];
        results = new List<float>();
        float t4 = chaserData.getTimeToReachPoint(Pos0, scope);
        if (Mathf.Abs(t0 - t4) < accuracy)
        {

            //Debug.Log("bb t0=" +t0 + " | tf=" + tf + " | t4=" + t4);
            results.Add(t4 - t0);
            return;
        }
        float left = t0 + t, right = tf;
        float testingT = t0;
        bool thereIsRight = tf == Mathf.Infinity ? false : true;
        float increase = 0.05f;
        int attempts = 0;
        float endT = 0;
        float lastEndT;
        bool computer = false;
        while (right - left > accuracy && attempts < 100)
        {

            //Vector3 testingOptimalPoint = Pos0 + V0 * (testingT - t0);
            Vector3 testingOptimalPoint = getPositionAtTime(testingT - t0);
            //Debug.Log("attempt=" + attempts + " | testingT=" + testingT + " | endT=" + endT + " | left=" + left + " | right=" + right + " | t0=" + t0 + " | tf=" + tf + " | scope=" + scope);
            endT = chaserData.getTimeToReachPoint(testingOptimalPoint, scope);
            
            if (Mathf.Abs(testingT - endT) < accuracy)
            {
                //Debug.Log("RESULT=" + (endT - t0)+" | attempt=" + attempts );
                results.Add(endT - t0);
                return;
            }
            if (endT > testingT)
            {
                left = testingT;
                if (thereIsRight && computer)
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
                if (thereIsRight && computer)
                {
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
        //Debug.Log("End-result dont founded");
        //results.Add(endT - t0);
    }
    public void getOptimalPointForReachTargetWhitAcceleration2(PublicPlayerData publicPlayerData, float offsetTime, float scope, float accuracy, out List<float> results)
    {
        Vector3 chaserPosition = publicPlayerData.position;
        float chaserSpeed = publicPlayerData.maxSpeed;
        getOptimalPointForReachTarget(chaserPosition, chaserSpeed, offsetTime, scope, out results);
        float t = results.Count == 0 ? 0 : results[0];
        results = new List<float>();
        float t4 = publicPlayerData.getTimeToReachPosition(Pos0, scope);
        if (Mathf.Abs(t0 - t4) < accuracy)
        {

            Debug.Log("bb t0=" + t0 + " | tf=" + tf + " | t4=" + t4);
            results.Add(t4 - t0);
            return;
        }
        float left = t0 + t, right = tf;
        float testingT = t0;
        bool thereIsRight = tf == Mathf.Infinity ? false : true;
        float increase = 0.1f;
        int attempts = 0;
        float endT = 0;
        float minDistance = 0.05f;

        float maxAttempts = 50;
        while (right - left > accuracy && attempts < 200)
        {
            float leftResult, rightResult, timeResult;
            bool thereIsTimeResult;
            bool thereIsSegment = getSegmentWithPossibleSolution(publicPlayerData,left,right,increase,accuracy,minDistance,scope, maxAttempts, out leftResult, out rightResult, out thereIsTimeResult, out timeResult);


            attempts++;
        }
        //Debug.Log("attempt=" + attempts + " | result=" + (endT - t0));
        Debug.Log("End-result dont founded");
        //results.Add(endT - t0);
    }
    bool getSegmentWithPossibleSolution(PublicPlayerData publicPlayerData,float left,float right,float increase,float accuracy,float minDistance,float scope,float maxAttempts,out float leftResult,out float rightResult,out bool thereIsTimeResult,out float timeResult)
    {
        leftResult = 0;
        rightResult = 0;
        int attempts = 0;
        float targetTestT=left,chaserTestT,lastChaserTestT = 0;
        timeResult = -1;
        thereIsTimeResult = false;
        bool isIncreasing=false, isDecreasing=false;
        bool firstTest = true;
        while (right - targetTestT > minDistance && attempts <= maxAttempts)
        {
            Vector3 testingOptimalPoint = getPositionAtTime(targetTestT - t0);
            //Debug.Log("attempt=" + attempts + " | testingT=" + testingT + " | endT=" + endT + " | left=" + left + " | right=" + right + " | t0=" + t0 + " | tf=" + tf + " | scope=" + scope);
            chaserTestT = publicPlayerData.getTimeToReachPosition(testingOptimalPoint, scope);

            if (Mathf.Abs(targetTestT - chaserTestT) < accuracy)
            {
                Debug.Log("RESULT=" + (chaserTestT - t0) + " | attempt=" + attempts);
                timeResult = chaserTestT - t0;
                thereIsTimeResult = true;
                return false;
            }
            else
            {
                if (firstTest)
                {
                    lastChaserTestT = chaserTestT;
                    leftResult = targetTestT;
                    targetTestT += increase;
                    firstTest = false;
                }
                else
                {
                    if (chaserTestT < lastChaserTestT)
                    {
                        isDecreasing = true;
                        leftResult = targetTestT;
                    }else if(chaserTestT == lastChaserTestT)
                    {
                        isDecreasing = true;
                    }
                    else
                    {
                        if (isDecreasing)
                        {
                            rightResult = targetTestT;
                            return true;
                        }
                        else
                        {
                            isDecreasing = false;
                        }
                    }
                    targetTestT += increase;
                    lastChaserTestT = chaserTestT;
                }
            }
            attempts++;
        }
        return false;
    }
    public override Vector3 getPositionAtTime(float t)
    {
        return getPositionAtTime(t, V0);
    }
    public Vector3 getPositionAtVt(float vt, Vector3 V0)
    {

        float v0 = V0.magnitude;
        if (v0 == 0)
        {
            return Pos0;
        }
        float maxWt = getTofWMax_WithRollDrag(V0);
        float vf = (friction * g) / k;
        float v022 = -vf + (v0 + vf) * Mathf.Exp(-k * maxWt);
        float v02;
        if(vt > v022)
        {
            v02 = vt;
        }
        else
        {
            v02 = v022;
        }
            //Debug.Log("v02=" + v02 + " | vt=" + vt);
        float e11 = (v02 + vf) / (v0 + vf);
        float v2 = (-vf + (v0+vf)* e11)/k;
        float e1 = (1 - e11);
        float e2 = (1 - vt/v02);
        float t1 = Mathf.Log((v02 + vf) / (v0 + vf)) / -k;
        float b = (v0 + vf) / k;
        float roll = -vf * t1 + b * e1;
        float drag = v2  * e2;
        //Debug.Log("getPositionAtTime " + roll+ " "+drag + " | v2=" +v2 + " |t1=" +t1+" | vf="+vf);
        if (vt > v022)
        {
            return V0.normalized * roll+Pos0;
        }
        else
        {
            return V0.normalized * roll + V0.normalized * drag + Pos0;
        }
    }
    public Vector3 getPositionAtTime(float t,Vector3 V0)
    {
        
        if (V0.magnitude == 0)
        {
            return Pos0;
        }
        float t1 = getTofWMax_WithRollDrag(V0);
        if (t < t1)
        {
            t1 = t;
        }
        float v2 = getVelocityAtTime(t1,V0).magnitude;
        if (k == 0)
        {
            float roll = friction * 9.81f;
            float t2 = (t - t1);
            float a = -0.5f * roll * t1 * t1;
            float d = V0.magnitude * t1 + a + v2 * t2;
            return V0.normalized * d + Pos0;
        }
        else
        {
            float e = (1 - Mathf.Exp(-k * t1));
            float e2 = (1 - Mathf.Exp(-k * Mathf.Clamp((t - t1), 0, Mathf.Infinity)));
            float vf = (friction * g) / k;
            float b = (V0.magnitude + vf) / k;
            float roll = -vf * t1 + b * e;
            float drag = (v2 / k) * e2;
            //Debug.Log("getPositionAtTime " + roll+ " "+drag + " | v2=" +v2 + " |t1=" +t1+" | vf="+vf);
            return V0.normalized * roll + V0.normalized * drag + Pos0;
        }
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
                }
                else if (posf.y == y)
                {
                    results.Add(tf);
                    return results;
                }
                else if ((pos0.y < y && posf.y > y) || (pos0.y > y && posf.y < y))
                {
                    Plane plane = new Plane(Vector3.up, Vector3.up * y);
                    Ray ray = new Ray(pos0, v0);
                    float lenght;
                    if (plane.Raycast(ray, out lenght))
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
    float getI()
    {
        return (2.0f / 5.0f) * m * ballRadio * ballRadio;
    }
    float getWt(float t)
    {
        return (friction * m * g * ballRadio * t) / getI();
    }
    float getMaxW(Vector3 V0)
    {
        float v0 = V0.magnitude;
        float r = ballRadio;
        float I = getI();
        float a = 1 + ((m * (r * r)) / I);
        return Mathf.Clamp(v0 * 5.86923f, 0, 50);
    }
    float getTofWMax_WithRollDrag(Vector3 V0)
    {
        float r = ballRadio;
        float w = getMaxW(V0);
        //float w = 49.18345f;
        if (k == 0)
        {
            return (V0.magnitude - r * w) / (friction * g);
        }
        else
        {
            float vf = (friction * g) / k;
            float ln = Mathf.Log((r * w + vf) / (V0.magnitude + vf));
            return ln / -k;
        }
    }
    
}
public class GetV0Result
{
    public bool foundedResult;
    public float vt;
    public Vector3 v0;
    public bool maximumControlSpeedReached;
    public bool maxKickForceReached;
    public GetV0Result(bool foundedResult, float vt, Vector3 v0)
    {
        this.foundedResult = foundedResult;
        this.vt = vt;
        this.v0 = v0;
    }
    public GetV0Result(bool foundedResult, float vt, Vector3 v0,bool maximumControlSpeedReached,bool maxKickForceReached)
    {
        this.foundedResult = foundedResult;
        this.vt = vt;
        this.v0 = v0;
        this.maximumControlSpeedReached = maximumControlSpeedReached;
        this.maxKickForceReached = maxKickForceReached;
    }
    public GetV0Result(bool foundedResult)
    {
        this.foundedResult = foundedResult;
    }
}
