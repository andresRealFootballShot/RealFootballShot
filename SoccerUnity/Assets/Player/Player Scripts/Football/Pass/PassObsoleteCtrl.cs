using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassObsoleteCtrl : SoccerPlayerComponent
{
    public Transform direction;
    public Transform targetTrans;
    public Transform partnerTrans;
    public float partnerSpeed;
    public float t;
    public float force;
    public float force2;
    public Vector3 impulse { get => direction.forward * force; }
    public PhysicMaterial physicMaterial;
    float k { get => ballRigidbody.drag; }
    Vector3 targetPosition { get => targetTrans.position; }
    float x0 { get => ballPosition.x; }
    float y0 { get => ballPosition.y; }
    float z0 { get => ballPosition.z; }
    float g { get => -Physics.gravity.y; }
    float friction { get => physicMaterial.staticFriction; }
    float m { get => ballRigidbody.mass; }
    float w { get => ballRigidbody.angularVelocity.magnitude; }
    void Start()
    {

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)&&false)
        {
            Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
            //ballPosition = testKickDirection.position;
            StartCoroutine(prueba1());
        }
    }
    
    Vector3 getPosWithDragRoll(float t,float v0)
    {
        float t1 = getTofWMax_WithRollDrag(v0);
        if (t < t1)
        {
            t1 = t;
        }
        float v2 = getVt(t1,v0);
        if (k == 0)
        {
            float roll = friction * 9.81f;
            float t2 = (t - t1);
            float a = -0.5f * roll * t1 * t1;
            float d =v0* t1 + a + v2* t2;
            return impulse.normalized * d + new Vector3(x0, 0, z0);
        }
        else
        {

            float e = (1 - Mathf.Exp(-k * t1));
            float e2 = (1 - Mathf.Exp(-k * Mathf.Clamp((t - t1),0,Mathf.Infinity)));
            float vf = v0 == 0 ? 0 : (friction * g) / k;
            float b = (v0 + vf) / k;
            float roll = - vf * t1 + b * e;
            float drag = (v2 / k) * e2;
            return impulse.normalized*roll + impulse.normalized * drag + new Vector3(x0,0,z0);
        }
    }
    
    float getV0(float t,Vector3 x0,Vector3 x)
    {
        float d = Vector3.Distance(x0, x);
        float vf = friction * g / k;
        float t1 = Mathf.Clamp(getTofWMax_WithRollDrag(ballSpeed),0,t);
        float t2 = Mathf.Clamp(t - t1,0,Mathf.Infinity);
        float e1 = (1 - Mathf.Exp(-k * t1));
        float e2 = (1 - Mathf.Exp(-k * t2));
        float e3 = Mathf.Exp(-k * t1);
        float a = (d + vf * t1) * k;
        float b = e1 + e3 * e2;
        float result = (a - vf * e1 + vf * e2 - vf * e3 * e2) / b;
        return result;
    }
    
    float getVt(float t,float v0)
    {
        float maxWt = getTofWMax_WithRollDrag(v0);
        if (k == 0)
        {
            return (v0 - friction * 9.81f * maxWt);
        }
        else
        {
            //v = -vf + (initBallSpeed + vf) * Mathf.Exp(-ballRigidbody.drag * _t);
            float vf = (friction * g) / k;
            float rollSlipV = -vf + (v0 + vf) * Mathf.Exp(-k * Mathf.Clamp(maxWt, 0, t));
            if (t <= maxWt)
            {
                return rollSlipV;
            }
            else
            {
                float pureRollV = rollSlipV * Mathf.Exp(-k * (t - maxWt));
                return pureRollV;
            }
        }
    }
    IEnumerator prueba1()
    {
        ballTransform.position = MyFunctions.setYToVector3(direction.position, ballRadio);
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = direction.forward * force2;

        yield return new WaitForSeconds(1);

        //t = distance / vx0;


        //print("static friction=" + physicMaterial.staticFriction + " | dynamic friction=" + physicMaterial.dynamicFriction + " | time=" + t);
        //print("static friction adjust=" + staticFrictionAdjust + " | dynamic friction adjust=" + dynamicFrictionAdjust + " | time adjust=" + timeAdjust);
        //ballRigidbody.angularVelocity = Vector3.zero;
        float previousSpeed = ballRigidbody.velocity.magnitude;
        ballRigidbody.velocity += direction.forward * force;

        float initBallSpeed = ballSpeed;
        StraightXZDragAndFrictionPath straightXZDragAndFrictionPath = new StraightXZDragAndFrictionPath(ballPosition,Vector3.positiveInfinity,ballVelocity, Vector3.positiveInfinity,0,15, k, ballRadio, friction, ballRigidbody.mass);
        Vector3 targetPosition2 = getPosWithDragRoll(t, initBallSpeed);
        Vector3 targetPosition = straightXZDragAndFrictionPath.getPositionAtTime(t);
        partnerTrans.position = targetPosition;

        print("Execution " + " friction=" + friction + " drag=" + k + " initSpeed=" + ballSpeed + " previousSpeed=" + previousSpeed);
        float _t = 0;
        float w0 = w;
        float t2 = 0;
        bool computer = false;
        while (_t < t)
        {
            float vf = k == 0 ? 0 : (friction * g) / k;
            float v;
            if (k == 0)
            {
                v = (initBallSpeed - friction * 9.81f * _t);
            }
            else
            {
                //v = -vf + (initBallSpeed + vf) * Mathf.Exp(-ballRigidbody.drag * _t);
                v = getVt(_t, initBallSpeed);
            }
            float w = ballRigidbody.angularVelocity.magnitude;
            float vcm = ballSpeed / ballRadio;
            //print(_t + " | realVelocity=" + ballRigidbody.velocity.magnitude + " | calculated velocity=" + v + " | distanceVelocity=" + (ballRigidbody.velocity.magnitude - v) + " | (w-v)=" + (w - vcm) + " |  w=" + (w) + " | calculated w=" + (w0 + getWt(_t)));
            if (Vector3.Distance(MyFunctions.setYToVector3(targetPosition, ballRadio), ballPosition) < 0.1f && !computer)
            {
                t2 = _t;
                computer = true;
            }
            yield return new WaitForFixedUpdate();
            _t += Time.fixedDeltaTime;
        }
        Vector3 dir2 = targetPosition - ballTransform.position;
        Vector3 cross = Vector3.Cross(dir2, Vector3.Cross(Vector3.up, ballRigidbody.velocity));
        print("end ballDistance=" + Vector3.Distance(ballTransform.position, targetPosition) * -Mathf.Sign(cross.y) + " | distance T=" + (t - t2));
    }
    IEnumerator prueba3()
    {
        ballTransform.position = MyFunctions.setYToVector3(direction.position, ballRadio);
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = direction.forward * force2;
        
        yield return new WaitForSeconds(1);

        //t = distance / vx0;


        //print("static friction=" + physicMaterial.staticFriction + " | dynamic friction=" + physicMaterial.dynamicFriction + " | time=" + t);
        //print("static friction adjust=" + staticFrictionAdjust + " | dynamic friction adjust=" + dynamicFrictionAdjust + " | time adjust=" + timeAdjust);
        //ballRigidbody.angularVelocity = Vector3.zero;
        float previousSpeed = ballRigidbody.velocity.magnitude;
        ballRigidbody.velocity += direction.forward * force;

        float initBallSpeed = ballSpeed;
        Vector3 targetPosition = getPosWithDragRoll(t,initBallSpeed);
        partnerTrans.position = targetPosition;

        print("Execution "+ " friction=" + friction + " drag=" + k + " initSpeed=" + ballSpeed + " previousSpeed="+ previousSpeed );
        float _t = 0;
        float w0 = w;
        float t2=0;
        bool computer = false;
        while (_t < t)
        {
            float vf = k == 0 ? 0 : (friction * g) / k;
            float v;
            if (k == 0)
            {
                v = (initBallSpeed - friction * 9.81f * _t);
            }
            else
            {
                //v = -vf + (initBallSpeed + vf) * Mathf.Exp(-ballRigidbody.drag * _t);
                v = getVt(_t,initBallSpeed);
            }
            float w = ballRigidbody.angularVelocity.magnitude;
            float vcm = ballSpeed / ballRadio;
            print(_t + " | realVelocity=" + ballRigidbody.velocity.magnitude + " | calculated velocity=" + v + " | distanceVelocity=" + (ballRigidbody.velocity.magnitude - v)+" | (w-v)="+(w-vcm)+" |  w="+(w)+" | calculated w="+ (w0+getWt(_t)));
            if (Vector3.Distance(MyFunctions.setYToVector3(targetPosition,ballRadio), ballPosition) < 0.1f && !computer)
            {
                t2 = _t;
                computer = true;
            }
            yield return new WaitForFixedUpdate();
            _t += Time.fixedDeltaTime;
        }
        Vector3 dir2 = targetPosition - ballTransform.position;
        Vector3 cross = Vector3.Cross(dir2, Vector3.Cross(Vector3.up, ballRigidbody.velocity));
        print("end ballDistance=" + Vector3.Distance(ballTransform.position, targetPosition) * -Mathf.Sign(cross.y) + " | distance T="+(t-t2));
    }

    IEnumerator prueba2()
    {
        Vector3 partnerDir = targetTrans.position - partnerTrans.position;
        t = partnerDir.magnitude / partnerSpeed;
        //ballTransform.position = MyFunctions.setYToVector3(direction.position, ballRadio);
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = direction.forward * force2;

        yield return new WaitForSeconds(0);

        float v0 = getV0(t, ballTransform.position, targetTrans.position);

        float previousSpeed = ballRigidbody.velocity.magnitude;
        Vector3 ballDir = MyFunctions.setYToVector3(targetTrans.position, ballRadio) - ballPosition;

        ballRigidbody.velocity += ballDir.normalized * (v0 - ballSpeed);
        print(v0);
        float initBallSpeed = ballSpeed;
        Vector3 targetPosition = targetTrans.position;

        //print("Execution " + i + " friction=" + friction + " drag=" + k + " initSpeed=" + ballSpeed + " previousSpeed=" + previousSpeed + " | tr=" + tr + " | vr=" + vr + " | maxV0=" + maxV0);
        float _t = 0;
        float t2 = 0;
        bool computer = false;
        while (_t < t)
        {

            if (Vector3.Distance(MyFunctions.setYToVector3(targetPosition, ballRadio), ballPosition) < 0.1f && !computer)
            {
                t2 = _t;
                computer = true;
            }
            yield return new WaitForFixedUpdate();
            _t += Time.fixedDeltaTime;
            partnerTrans.position += partnerDir.normalized * partnerSpeed * Time.fixedDeltaTime;
        }
        Vector3 dir2 = targetPosition - ballTransform.position;
        Vector3 cross = Vector3.Cross(dir2, Vector3.Cross(Vector3.up, ballRigidbody.velocity));
        print("end ballDistance=" + Vector3.Distance(ballTransform.position, targetPosition) * -Mathf.Sign(cross.y) + " | distance T=" + (t - t2));
    }
    float getI()
    {
        return (2.0f / 5.0f) * ballRigidbody.mass * ballRadio * ballRadio;
    }
    float getWt(float t)
    {
        return (friction * m * g * ballRadio * t) / getI();
    }
    float getMaxW(float v0)
    {
        float r = ballRadio;
        float I = getI();
        float a = 1 + ((m * (r * r)) / I);
        return Mathf.Clamp(v0 * 5.86923f, 0, 50);
    }
    float getTofWMax_WithRollDrag(float v0)
    {
        float r = ballRadio;
        float w = getMaxW(v0);
        //float w = 49.18345f;
        if (k == 0)
        {
            return (v0 - r * w) / (friction * g);
        }
        else
        {
            float vf = (friction * g) / k;
            float ln = Mathf.Log((r * w + vf) / (v0 + vf));
            return ln / -k;
        }
    }
}
