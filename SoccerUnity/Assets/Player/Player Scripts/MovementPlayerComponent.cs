using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementPlayerComponent : PlayerComponent
{
    protected MovimentValues movementValues { get => playerComponents.movementValues; }
    protected ResistanceCtrl resistanceCtrl { get => playerComponents.resistanceCtrl; }
    public ResistanceParameters resistanceParameters { get => playerComponents.resistanceParameters; }

    
    protected Vector3 NormalizedForwardDesiredVelocity { get => movementValues.NormalizedForwardDesiredVelocity; }
    public Vector3 DesiredDirection { get => movementValues.ForwardDesiredDirection; set => movementValues.ForwardDesiredDirection = value; }
    public Vector3 ForwardDesiredVelocity { get => movementValues.ForwardDesiredVelocity; set => movementValues.ForwardDesiredVelocity = value; }
    protected Vector3 ForwardY0DesiredVelocity { get => MyFunctions.setY0ToVector3(ForwardDesiredVelocity);}
    protected float ForwardDesiredVelocitySpeed { get => movementValues.ForwardDesiredSpeed; }
    protected Vector3 Clamp01NormalizedForwardDesiredVelocity { get => movementValues.Clamp01NormalizedForwardDesiredVelocity; }
    protected float Clamp01NormalizedForwardDesiredSpeed { get => movementValues.Clamp01NormalizedForwardDesiredVelocity.magnitude; }

    public float EndForwardSpeed { get => movementValues.adjustedForwardVelocitySpeed; set => movementValues.adjustedForwardVelocitySpeed = value; }
    
    protected Vector3 LookDirection { get => movementValues.LookDirection; set => movementValues.LookDirection = value; }
    
    protected float distanceStopMoveBallPlayer { get => stopOffset + movementValues.distanceStopMoveBallPlayerOffset; }
    //protected float distanceStopMoveBallPlayer { get => 3.289582f; }
    protected float angleBallForwardDesiredVelocity { get => Vector3.Angle(bodyBallDirection,ForwardDesiredVelocity); }
    protected float angleBodyForwardDesiredVelocity { get => Vector3.Angle(bodyY0Forward, ForwardY0DesiredVelocity); }
    protected float angleVelocity_DesiredVelocity { get => Vector3.Angle(Y0Velocity, ForwardY0DesiredVelocity); }
    protected float angleBodyForward_DesiredLookDirection { get => Vector3.Angle(bodyY0Forward, DesiredLookDirection); }
    protected float angleBodyTarget_DesiredDirection { get => Vector3.Angle(BodyTargetDirection, DesiredDirection); }
    protected float angleBodyForwardVelocity { get => Vector3.Angle(bodyY0Forward, VelocityDirection); }

    protected float accelerationSkill { get => playerSkills.acceleration; }
    protected float rotAccelerationSkill { get => playerSkills.acceleration; }
    protected float acceleration { get => getAcceleration(); }
    protected float maxSpeedRotation { get => playerComponents.maxSpeedRotation; }
    protected bool isAccelerating { get; set; }
    protected bool isDecelerating { get; set; }
    protected float minSpeedForRotate { get => movementValues.minSpeedForRotateBody; }
    protected float maxSpeedForReachBall { get => movementValues.maxSpeedForReachBall; }
    protected float maxAngleForRun { get => movementValues.maxAngleForRun; }
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
