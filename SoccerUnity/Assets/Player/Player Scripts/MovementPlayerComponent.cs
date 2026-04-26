using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MovimentValues;

public class MovementPlayerComponent : PlayerComponent
{
    protected MovimentValues movementValues { get => playerComponents.movementValues; }
    protected ResistanceCtrl resistanceCtrl { get => playerComponents.resistanceCtrl; }
    public ResistanceParameters resistanceParameters { get => playerComponents.resistanceParameters; }

    
    protected Vector3 NormalizedForwardDesiredVelocity { get => movementValues.NormalizedForwardDesiredVelocity; }
    public Vector3 DesiredDirection { get => movementValues.DesiredDirection;}
    public Vector3 DesiredY0Direction { get => movementValues.DesiredY0Direction;}
    public Vector3 ForwardDesiredVelocity { get => movementValues.DesiredVelocity;}
    protected Vector3 ForwardY0DesiredVelocity { get => MyFunctions.setY0ToVector3(ForwardDesiredVelocity);}
    protected float ForwardDesiredVelocitySpeed { get => movementValues.ForwardDesiredSpeed; }
    protected Vector3 Clamp01NormalizedForwardDesiredVelocity { get => movementValues.Clamp01NormalizedForwardDesiredVelocity; }
    protected float Clamp01NormalizedForwardDesiredSpeed { get => movementValues.Clamp01NormalizedForwardDesiredVelocity.magnitude; }

    public float EndForwardSpeed { get => movementValues.adjustedForwardVelocitySpeed; set => movementValues.adjustedForwardVelocitySpeed = value; }
    public bool StopMove { get => movementValues.StopMove; set => movementValues.StopMove = value; }
    public MovePhase phase { get => movementValues.phase; set => movementValues.phase = value; }
    
    protected Vector3 LookDirection { get => movementValues.LookDirection; set => movementValues.LookDirection = value; }
    protected Vector3 LookY0Direction { get => MyFunctions.setY0ToVector3( movementValues.LookDirection);}
    
    protected float distanceStopMoveBallPlayer { get => stopOffset + movementValues.distanceStopMoveBallPlayerOffset; }
    //protected float distanceStopMoveBallPlayer { get => 3.289582f; }
    protected float angleBallForwardDesiredVelocity { get => Vector3.Angle(bodyBallDirection,ForwardDesiredVelocity); }
    protected float angleBodyForwardDesiredVelocity { get => Vector3.Angle(bodyY0Forward, DesiredY0Direction); }
    protected float angleVelocity_DesiredVelocity { get => Vector3.Angle(Y0Velocity, ForwardY0DesiredVelocity); }
    protected float angleBodyForward_DesiredLookDirection { get => Vector3.Angle(bodyY0Forward, DesiredLookDirection); }
    protected float angleBodyForwardVelocity { get => Vector3.Angle(bodyY0Forward, VelocityDirection); }

    protected float accelerationSkill { get => playerSkills.acceleration; }
    protected float rotAccelerationSkill { get => playerSkills.acceleration; }
    protected float acceleration { get => getAcceleration(); }
    protected float maxSpeedRotation { get => playerComponents.maxSpeedRotation; }
    protected bool isAccelerating { get; set; }
    protected bool isDecelerating { get; set; }
    protected float minSpeedForRotate { get => movementValues.minSpeedForRotateBody; }
    protected float minSpeedForRotate2 { get => movementValues.minSpeedForRotateBody2; }
    protected float reachBallSpeed { get => movementValues.maxSpeedForReachBall; }
    protected float maxAngleForRun { get => movementValues.maxAngleForRun; }
    protected float maxAngleForRun2 { get => movementValues.maxAngleForRun2; }
    protected void calculateIsAccelerating()
    {
        float angleBodyBall = getVelocity_DesiredDirectionAngle();
        if (angleBodyBall < maxAngleForRun)
        {
            float speedRotation = Mathf.Clamp01(1 - angleBodyBall / 90);
            float targetSpeed = speedRotation * EndForwardSpeed;
            if (targetSpeed < Speed - 0f)
            {
                isAccelerating = false;
                isDecelerating = true;
            }
            else if (targetSpeed > Speed - 0f)
            {
                isAccelerating = true;
                isDecelerating = false;
            }
            else
            {
                isDecelerating = false;
                isAccelerating = false;
            }
        }
        else
        {
            isAccelerating = false;
        }
    }
    float getAcceleration()
    {
        float angleBodyBall = getVelocity_DesiredDirectionAngle();


        if (angleBodyBall < maxAngleForRun && angleBodyForward_DesiredLookDirection< maxAngleForRun)
        {
            float speedRotation = Mathf.Clamp01(1 - angleBodyBall / 90);
            float targetSpeed = speedRotation * ForwardDesiredSpeed;
            float accelerationResult;
            if (targetSpeed < EndForwardSpeed - 0f)
            {
                accelerationResult = getMaxDeceleration();
                //print("a");
            }
            else if (targetSpeed > EndForwardSpeed - 0f)
            {
                accelerationResult = getMaxAcceleration();
                //print("b");
            }
            else
            {
                accelerationResult = 0;
            }
            float lerpAcceleration = Mathf.Lerp(0, accelerationResult, Mathf.Abs(targetSpeed - EndForwardSpeed) / 0.01f);
            //float lerpAcceleration = accelerationResult;
            float sign = Mathf.Sign(targetSpeed - EndForwardSpeed);
            //print(targetSpeed + "  | " + adjustedForwardVelocitySpeed+ " | "+ acceleration);
            return sign * lerpAcceleration;
        }
        else
        {
            float targetSpeed = 0;
            float decelerationSkill = getMaxDeceleration();
            //float acceleration = Mathf.Lerp(0, decelerationSkill, Mathf.Abs(targetSpeed - Speed) / 0.1f);
            float acceleration = decelerationSkill;
            return -acceleration;
        }
    }
    protected float getVelocity_DesiredDirectionAngle()
    {
        float angleBodyBall = angleVelocity_DesiredVelocity;
        if (angleBodyBall < 1)
        {
            return 0;
        }
        else
        {
            return angleBodyBall;
        }
    }
    protected float getAngleBodyBallDeprecated()
    {
        float angleBodyBall = Vector3.Angle(bodyForward, MyFunctions.setY0ToVector3(DesiredDirection));
        if (angleBodyBall < 1)
        {
            return 0;
        }
        else
        {
            return angleBodyBall;
        }
    }
    
}
