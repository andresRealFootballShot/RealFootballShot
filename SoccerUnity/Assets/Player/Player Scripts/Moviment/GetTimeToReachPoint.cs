using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetTimeToReachPoint
{
    PublicPlayerData publicPlayerData;
    MovimentValues movementValues { get => publicPlayerData.movimentValues; }
    Vector3 bodyY0Forward { get => publicPlayerData.playerComponents.bodyY0Forward; }
    float maxSpeedForReachBall { get => movementValues.maxSpeedForReachBall; }
    float MaxForwardFullSpeed { get => movementValues.maxForwardSpeed; }
    public getTimeToReachPointDelegate getTimeToReachPointDelegate;

    public GetTimeToReachPoint(PublicPlayerData publicPlayerData, getTimeToReachPointDelegate getTimeToReachPointDelegate)
    {
        this.publicPlayerData = publicPlayerData;
        this.getTimeToReachPointDelegate = getTimeToReachPointDelegate;
    }

    public GetTimeToReachPoint(PublicPlayerData publicPlayerData)
    {
        this.publicPlayerData = publicPlayerData;
    }

    public float linearGetTimeToReachPosition(Vector3 targetPosition,float scope)
    {
        float distance = Vector3.Distance(MyFunctions.setYToVector3(publicPlayerData.bodyTransform.position, targetPosition.y), targetPosition);
        float t = distance / publicPlayerData.maxSpeed;
        return t;
    }
    public float accelerationGetTimeToReachPosition(Vector3 targetPosition,float scope)
    {
        //bodyRotation = Quaternion.LookRotation(MyFunctions.setY0ToVector3(ballPosition - bodyPosition));
        if(MyFunctions.Vector3IsNan(targetPosition) || targetPosition.Equals(Vector3.positiveInfinity) || targetPosition.Equals(Vector3.negativeInfinity))
        {
            return Mathf.Infinity;
        }
        Vector3 bodyPosition = publicPlayerData.position;
        float d4 = Vector3.Distance(bodyPosition,MyFunctions.setYToVector3(targetPosition,bodyPosition.y));
        if (d4 < scope)
        {
            return 0;
        }
        float speed=publicPlayerData.speed;
        float minSpeedForRotate = movementValues.minSpeedForRotateBody;
        PlayerComponents playerComponents = publicPlayerData.playerComponents;
        float a = publicPlayerData.playerComponents.getMaxAcceleration();
        float da = publicPlayerData.playerComponents.getMaxDeceleration();
        float maxAngleForRun = movementValues.maxAngleForRun;
        float t1_1 = speed > minSpeedForRotate ? AccelerationPath.getT(minSpeedForRotate, speed,da) : 0;
        Vector3 direction = MyFunctions.setY0ToVector3(targetPosition - bodyPosition).normalized;
        float angle = Vector3.Angle(bodyY0Forward, direction);
        float t1 = angle > maxAngleForRun ? t1_1 : 0;
        Vector3 x1 = AccelerationPath.getX(bodyPosition, bodyY0Forward, bodyY0Forward.normalized * speed, t1, -da);
        float t2 = angle > maxAngleForRun ? Path.getT(maxAngleForRun, angle,playerComponents.maxSpeedRotation) : 0;
        //float t3 = angleBodyForwardDesiredVelocity > 45 ? getT(MaxForwardFullSpeed, 0, getAccelerationSkill()) : getT(MaxForwardFullSpeed, bodySpeed, getAccelerationSkill());
        Vector3 v0 = angle > maxAngleForRun ? Vector3.zero : playerComponents.Velocity.normalized * speed;
        //Vector3 x2 =  getX(x1 , ballPosition, v0, t3, getAccelerationSkill());
        float d2 = Vector3.Distance(MyFunctions.setY0ToVector3(x1), MyFunctions.setY0ToVector3(targetPosition));
        
        float d3 = d2 - scope ;
        float d = AccelerationPath.getDistanceWhereStartDecelerate(v0.magnitude, maxSpeedForReachBall, getAccelerationSkill(), -getDecelerationSkill(), d3);
        //Vector3 direction2 = MyFunctions.setY0ToVector3(targetPosition - x1).normalized;
        float t3, t4, t8,result;
        //Vector3 x2 = x1 + direction2 * d;
        float x3 = Mathf.Abs(AccelerationPath.getX2(v0.magnitude, MaxForwardFullSpeed, getAccelerationSkill()));
        if (x3 < d)
        {
            float t5 = AccelerationPath.getT(MaxForwardFullSpeed, v0.magnitude, getAccelerationSkill());
            float x4 = Mathf.Abs(AccelerationPath.getX2(MaxForwardFullSpeed, maxSpeedForReachBall, getDecelerationSkill()));
            float x5 = d3 - x3 - x4;
            float t6 = x5 / MaxForwardFullSpeed;
            float t7 = AccelerationPath.getT(maxSpeedForReachBall, MaxForwardFullSpeed, getDecelerationSkill());
            t8 = t5 + t6 + t7;
            result = t1 + t2 + t8;
            //string debug = "A name=" + publicPlayerData.name + " | t1=" + t1 + " | t2=" + t2 + " | t5=" + (t5 + t1 + t2) + " | t6=" + (t6 + t5 + t1 + t2) + " | t7=" + (t7 + t6 + t5 + t1 + t2) + " | t8=" + (t8 + t1 + t2) + " | d=" + d + " | dStop=" + x4 + " | d3=" + d3 + " | x1=" + x1 + " | d2=" + d2 + " | v0=" + v0.magnitude + " | angle=" + angle + " | speed=" + speed + " | result=" + result;
            //DebugsList.testing.print(debug,Color.white);

            //print(debug);
        }
        else
        {
            //getT(x2, x1, v0, getAccelerationSkill(), out t3);
            //d = 3.007097f;
            AccelerationPath.getT(d, v0.magnitude, getAccelerationSkill(), out t3);
            float x5 = AccelerationPath.getX3(v0.magnitude, t3, getAccelerationSkill());
            float v1 = v0.magnitude + getAccelerationSkill() * t3;
            float x4 = Mathf.Abs(AccelerationPath.getX2(v1, maxSpeedForReachBall, getDecelerationSkill()));
            AccelerationPath.getT(d3 - d, v1, getDecelerationSkill(), out t4);
            t4 = AccelerationPath.getT(maxSpeedForReachBall, v1, getDecelerationSkill());
            t8 = t3 + t4;
            //print((x5 + x4)+" "+d3);
            result = t1 + t2 + t8;
            //string debug = "B name=" + publicPlayerData.name + " | t1=" + t1 + " | t2=" + t2 + " | t3=" + t3 + " | t4=" + t4 + " | t8=" + t8 + " | d=" + d + " | d3=" + d3 + " | dStop=" + x4 + " | dStop2=" + (d3 - d) + " | v0=" + v0.magnitude + " | x1=" + x1 + " | v1=" + v1 + " | angle=" + angle + " | speed=" + speed + " | result=" + result;
            //print(debug);
            //DebugsList.testing.print(debug, Color.white);

        }
        //float t4 = Vector3.Distance(MyFunctions.setY0ToVector3(x2), MyFunctions.setY0ToVector3(ballPosition)) / MaxForwardFullSpeed;
        return result;
    }
    float getAccelerationSkill()
    {
        return publicPlayerData.playerComponents.getMaxAcceleration();
    }
    float getDecelerationSkill()
    {
        return publicPlayerData.playerComponents.getMaxDeceleration();
    }
}
