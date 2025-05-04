using DOTS_ChaserDataCalculation;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class MovementCtrl : MovementPlayerComponent
{
    public bool useRigidbody = false;
    float forwardAnim, sprintAnim;
    float targetVelocityBall;
    Vector3 previousPosition;
    float speed, forwardDesiredSpeed2;
    Vector3 direction;
    public bool debug;

    // Update is called once per frame
    private void Start()
    {
        if (!useRigidbody)
        {
            Destroy(bodyRigidbody);
        }
    }
    public void getAdjustedForwardVelocitySpeed(float deltaTime)
    {
        if (!enabled)
        {
            return;
        }
        testSpeed();
        return;

        calculateDistanceStop();
        if (angleVelocity_DesiredVelocity < maxAngleForRun && angleBodyForward_DesiredLookDirection < maxAngleForRun)
        {
            float speedRotation = Mathf.Clamp01(1 - angleVelocity_DesiredVelocity / 90);
            float targetSpeed = speedRotation * ForwardDesiredSpeed;

            float max, min;
            if (targetSpeed < EndForwardSpeed)
            {
                max = MaxSpeed;
                min = targetSpeed;
                //print("a");
            }
            else if (targetSpeed > EndForwardSpeed)
            {
                max = targetSpeed;
                min = 0;
                //print("b");
            }
            else
            {
                max = MaxSpeed;
                min = 0;
            }
            //print(targetSpeed + "  | " + adjustedForwardVelocitySpeed+ " | "+ acceleration);
            //float d = Mathf.Clamp(targetSpeed - adjustedForwardVelocitySpeed,0,Mathf.Infinity)/deltaTime;
            //float a = Mathf.Clamp(acceleration, -d , d);

            //print(ForwardDesiredSpeed + " "+ acceleration);
            float a = acceleration;
            EndForwardSpeed += a * deltaTime;
            EndForwardSpeed = Mathf.Clamp(EndForwardSpeed, min, max);
        }
        else
        {
            //print("ForwardVelocity");
            float a = acceleration;
            EndForwardSpeed += a * deltaTime;

            EndForwardSpeed = Mathf.Clamp(EndForwardSpeed, 0, MaxSpeed);
        }
        //DrawArrow.ForDebug(bodyPosition, direction.normalized);
        //print(speedRotation + )
        //print("adjustedForwardVelocitySpeed=" + adjustedForwardVelocitySpeed + " | speedRotation=" + speedRotation + " | ForwardDesiredVelocitySpeed=" + ForwardDesiredVelocitySpeed + " | acceleration="+ acceleration);
    }
    void testSpeed()
    {
        
        float maxSpeedForReachBall_rot = angleBodyForward_DesiredLookDirection >= playerComponents.movementValues.maxAngleForRun ? minSpeedForRotate:  maxSpeedForReachBall ;
        float stopDistance = Mathf.Abs(AccelerationPath.getX2(maxSpeedForReachBall_rot, playerComponents.Speed, playerComponents.movementValues.forwardDeceleration));
        float desiredSpeed_rot = angleBodyForward_DesiredLookDirection >= playerComponents.movementValues.maxAngleForRun ? minSpeedForRotate : ForwardDesiredSpeed;
        MovimentValues movimentValues = playerComponents.movementValues;
        //print(BodyTargetXZDistance+" "+stopDistance + " "+scope);
        if (BodyTargetXZDistance < stopDistance+scope)
        {

            EndForwardSpeed -= movementValues.forwardDeceleration * Time.deltaTime;
            EndForwardSpeed = Mathf.Clamp(EndForwardSpeed, maxSpeedForReachBall_rot, Mathf.Infinity);
        }
        else
        {
            if (EndForwardSpeed < desiredSpeed_rot)
            {

                EndForwardSpeed += movementValues.forwardAcceleration * Time.deltaTime;
                EndForwardSpeed = Mathf.Clamp(EndForwardSpeed, 0, desiredSpeed_rot);
            }
            else
            {
                EndForwardSpeed -= movementValues.forwardDeceleration * Time.deltaTime;
                EndForwardSpeed = Mathf.Clamp(EndForwardSpeed, desiredSpeed_rot, Mathf.Infinity);
            }
        }
        calculateVelocity(Time.deltaTime);
    }
    void calculateDistanceStop()
    {
        float v = maxSpeedForReachBall;
        if (isAccelerating)
        {
            speed = EndForwardSpeed;
            //print(speed);
        }

        float v0 = speed;
        float d = ((v * v) - (v0 * v0)) / (2 * getMaxDeceleration());
        float stopDistance = Mathf.Abs(AccelerationPath.getX2(playerComponents.movementValues.maxSpeedForReachBall, Speed, playerComponents.movementValues.forwardDeceleration));
        
        /*float da = getMaxDeceleration();
        float t1_1 = Speed > minSpeedForRotate ? AccelerationPath.getT(minSpeedForRotate, Speed, da) : 0;
        float angle = angleVelocity_DesiredVelocity;
        float t1 = angle > maxAngleForRun ? t1_1 : 0;
        Vector3 x1 = AccelerationPath.getX(bodyPosition, VelocityDirection, Velocity, t1, -da);
        float d2 = Vector3.Distance(MyFunctions.setY0ToVector3(x1), MyFunctions.setY0ToVector3(TargetPosition));
        float targetDistance = d2 - scope;
        float d = AccelerationPath.getDistanceWhereStartDecelerate(ballVelocity.magnitude, maxSpeedForReachBall, getMaxAcceleration(), -da, targetDistance);*/
        movementValues.distanceStopMoveBallPlayerOffset = Mathf.Abs(d);
        
        //float speed2 = BodyTargetXZDistance < distanceStopMoveBallPlayer&& angleBodyTarget_DesiredDirection < 5 ? MinForwardSpeed : ForwardDesiredSpeed;
        float speed2 = BodyTargetXZDistance < stopDistance+stopOffset && angleBodyTarget_DesiredDirection < 5 ? MinForwardSpeed : ForwardDesiredSpeed;

        speed2 = speed2 < 0.001f ? 0 : speed2;
        ForwardDesiredSpeed = speed2;
    }
    void printDebug(string message)
    {
        if(debug)print(message);
    }
    public void rotation(float deltaTime)
    {
        AngularSpeed = 0;
        if (DesiredLookDirection != Vector3.zero)
        {
            float angle = angleBodyForward_DesiredLookDirection;
            
            if (angle < maxAngleForRun)
            {
                float maxSpeed = movementValues.rotationSpeed;
                float minSpeed = movementValues.minRotationSpeedWhileRun;
                float speedRotation = Mathf.Lerp(maxSpeed, minSpeed, EndForwardSpeed / movementValues.maxSpeedWhileRun_AngularLerp);
               
                bodyRotationSpeed = Mathf.Lerp(0, speedRotation, angleBodyForward_DesiredLookDirection / 1f);
                Vector3 cross = Vector3.Cross(bodyY0Forward, DesiredLookDirection);
                bodyRotationSpeed = Mathf.Clamp(bodyRotationSpeed, 0, angleBodyForward_DesiredLookDirection / deltaTime);
                AngularSpeed = bodyRotationSpeed;


               
                bodyTransform.eulerAngles += Mathf.Sign(cross.y) * Vector3.up * bodyRotationSpeed * deltaTime;


                //print(angleBodyForward_DesiredLookDirection + " "+maxAngleForRun + " " + ForwardY0Velocity);
            }
            else
            {
                //print("rotation");
                float maxSpeed = movementValues.rotationSpeed;
                float speedRotation = EndForwardSpeed <= minSpeedForRotate ? maxSpeed : 0;
                //float speedRotation = Mathf.Lerp(maxSpeed,0,(adjustedForwardVelocitySpeed - 2)/2);

                bodyRotationSpeed = Mathf.Lerp(0, speedRotation, angleBodyForward_DesiredLookDirection / 0.1f);
                Vector3 cross = Vector3.Cross(bodyY0Forward, DesiredLookDirection);
                //Debug.LogError("bodyRotationSpeed=" + bodyRotationSpeed+ " | speedRotation=" + speedRotation + " | angle=" + angle + " | EndForwardSpeed=" + EndForwardSpeed + " | maxSpeed=" + maxSpeed + " | ForwardDesiredDirection=" + ForwardDesiredDirection);
                AngularSpeed = bodyRotationSpeed;
                
                bodyTransform.eulerAngles += Mathf.Sign(cross.y) * Vector3.up * bodyRotationSpeed * deltaTime;

                //print("b " + speedRotation);
            }
        }
        
        if (DesiredDirection != Vector3.zero)
        {
            Vector3 targetDirection = TargetPosition- bodyPosition;
            if (angleVelocity_DesiredVelocity < maxAngleForRun)
            {
                direction = Vector3.RotateTowards(direction, targetDirection, movementValues.directionRotationSpeed * Mathf.Deg2Rad * deltaTime, 1).normalized;
            }
            else
            {
                direction = EndForwardSpeed <= movementValues.minSpeedForChangeDirection ? targetDirection : direction;
            }
        }
    }
    public void rotationDeprecated(float deltaTime)
    {
        if (DesiredLookDirection != Vector3.zero)
        {

            if (angleBodyForwardDesiredVelocity < maxAngleForRun)
            {
                float maxSpeed = movementValues.rotationSpeed;
                float minSpeed = Mathf.Lerp(10, 30, rotAccelerationSkill);
                float speedRotation = Mathf.Lerp(maxSpeed, minSpeed, EndForwardSpeed / MaxSpeed);
                float angle = angleBodyForwardDesiredVelocity;
                bodyRotationSpeed = Mathf.Lerp(0, speedRotation, angle / 1f);
                Vector3 cross = Vector3.Cross(bodyY0Forward, DesiredLookDirection);
                bodyRotationSpeed = Mathf.Clamp(bodyRotationSpeed, 0, angle / deltaTime);
                bodyTransform.eulerAngles += Mathf.Sign(cross.y) * Vector3.up * bodyRotationSpeed * deltaTime;
                //print(angle);
            }
            else
            {
                //print("rotation");
                float maxSpeed = movementValues.rotationSpeed;
                float speedRotation = EndForwardSpeed <= minSpeedForRotate ? maxSpeed : 0;
                //float speedRotation = Mathf.Lerp(maxSpeed,0,(adjustedForwardVelocitySpeed - 2)/2);

                float angle = angleBodyForwardDesiredVelocity;
                bodyRotationSpeed = Mathf.Lerp(0, speedRotation, angle / 0.1f);
                Vector3 cross = Vector3.Cross(bodyY0Forward, DesiredLookDirection);
                //Debug.LogError("bodyRotationSpeed=" + bodyRotationSpeed+ " | speedRotation=" + speedRotation + " | angle=" + angle + " | EndForwardSpeed=" + EndForwardSpeed + " | maxSpeed=" + maxSpeed + " | ForwardDesiredDirection=" + ForwardDesiredDirection);
                bodyTransform.eulerAngles += Mathf.Sign(cross.y) * Vector3.up * bodyRotationSpeed * deltaTime;
                //print("b " + speedRotation);
            }
        }
    }

    public void movement(float deltaTime)
    {
        /*if (!playerComponents.wallRayCast.isHitting)
        {
            //print(ForwardDesiredVelocity + " " + adjustedForwardVelocitySpeed);
            
            //print(movementValues.ForwardSpeed + " | " + adjustedForwardVelocitySpeed+ " | "+ NormalizedForwardDesiredVelocity.magnitude);
            
            
        }*/
        if (useRigidbody)
        {
            bodyRigidbody.MovePosition(bodyRigidbodyPosition + direction.normalized * EndForwardSpeed * deltaTime);
        }
        else
        {
            bodyTransform.Translate(direction.normalized * EndForwardSpeed * deltaTime, Space.World);
        }
        //calculateVelocity(deltaTime);
    }
    void calculateVelocity(float deltaTime)
    {
        //movementValues.ForwardVelocity = (bodyRigidbody.position - previousPosition)/ deltaTime;

        calculateIsAccelerating();
        float forwardSpeed = Vector3.Dot(bodyY0Forward, direction.normalized * EndForwardSpeed);
        float horizontalSpeed = Vector3.Dot(bodyTransform.right, direction.normalized * EndForwardSpeed);
        playerData.Velocity = direction.normalized * EndForwardSpeed;
        playerData.VerticalSpeed = forwardSpeed;
        playerData.HorizontalSpeed = horizontalSpeed;
        previousPosition = bodyPosition;
    }
    public static float FindAngle(Vector3 fromVector, Vector3 toVector)
    {
        if (toVector == Vector3.zero)
            return 0;

        float angle = Vector3.Angle(fromVector, toVector);
        Vector3 signo = Vector3.Cross(fromVector, toVector);
        angle *= Mathf.Sign(signo.y);
        return angle;
    }
    public void animator(float deltaTime)
    {
        Animator anim = playerComponents.animator;
        //forwardAnim = Mathf.Lerp(forwardAnim, (forwardVelocity) / movementValues.MaxForwardRunSpeed, deltaTime * GeneralPlayerParameters.speedAnim);
        float vertical = VerticalSpeed;
        float horizontal = HorizontalSpeed;
        //sprintAnim = Mathf.Lerp(sprintAnim, (movementValues.ForwardDesiredSpeed - movementValues.MaxForwardRunSpeed) / (movementValues.MaxForwardSprintSpeed), deltaTime * GeneralPlayerParameters.speedAnim);
        anim.SetFloat("vertical", vertical, 0.1f, deltaTime * GeneralPlayerParameters.speedAnim);
        anim.SetFloat("horizontal", horizontal, 0.1f, deltaTime * GeneralPlayerParameters.speedAnim);
        //anim.SetFloat("sprint", horizontal, 0.1f, deltaTime * GeneralPlayerParameters.speedAnim2);

        //anim.SetFloat("vertical", movementValues.forwardAnimCurve.Evaluate(forwardAnim), 0.1f, deltaTime * GeneralPlayerParameters.speedAnim2);
        //anim.SetFloat("sprint", movementValues.sprintAnimCurve.Evaluate(sprintAnim) + 0.5f, 0.1f, deltaTime * GeneralPlayerParameters.speedAnim2);
    }

}
