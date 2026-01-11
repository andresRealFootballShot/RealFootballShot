using DOTS_ChaserDataCalculation;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using Unity.Entities.UniversalDelegates;
using UnityEditor;
using UnityEngine;
using static MovimentValues;

public class MovementCtrl : MovementPlayerComponent
{
    public bool useRigidbody = false;
    float forwardAnim, sprintAnim;
    float targetVelocityBall;
    Vector3 previousPosition;
    float speed, forwardDesiredSpeed2;
    Vector3 lookDirection;
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
    public void BotMove(float deltaTime)
    {
        UpdateMovementPhase();
        UpdateRotation(deltaTime);
        UpdateSpeed(deltaTime);
        calculateBotVelocity(deltaTime);
        ApplyMovement(deltaTime);
    }
    void UpdateMovementPhase()
    {
        if (DesiredDirection == Vector3.zero)
            return;
        
        float angle = Vector3.Angle(VelocityDirection, DesiredDirection);
        float requiredSpeed = angle > maxAngleForRun2 ? minSpeedForRotate2: minSpeedForRotate;
        if (angle > maxAngleForRun && EndForwardSpeed > requiredSpeed + 0.01f)
        {
            phase = MovePhase.Brake;
        }
        else if (angle > 1f)
        {
            phase = MovePhase.Rotate;
        }
        else
        {
            phase = MovePhase.Move;
        }
    }
    void UpdateSpeed(float dt)
    {
        switch (phase)
        {
            case MovePhase.Brake:
                {
                    float angle = Vector3.Angle(VelocityDirection, DesiredDirection);
                    float requiredSpeed = angle > maxAngleForRun2 ? minSpeedForRotate2 : minSpeedForRotate;
                    if (EndForwardSpeed > requiredSpeed)
                    {
                        EndForwardSpeed -= movementValues.forwardDeceleration * dt;
                        EndForwardSpeed = Mathf.Max(EndForwardSpeed, requiredSpeed);
                    }
                    break;
                }

            case MovePhase.Rotate:
                

            case MovePhase.Move:
                {
                    float distance =
                        Vector3.Distance(
                            new Vector3(bodyPosition.x, 0, bodyPosition.z),
                            new Vector3(TargetPosition.x, 0, TargetPosition.z)
                        ) - scope;

                    float stopDist =
                        (EndForwardSpeed * EndForwardSpeed) /
                        (2f * movementValues.forwardDeceleration);

                    if (distance <= stopDist)
                    {
                        
                        EndForwardSpeed -= movementValues.forwardDeceleration * dt;
                        EndForwardSpeed = Mathf.Max(EndForwardSpeed, reachBallSpeed);
                    }
                    else
                    {
                        
                        EndForwardSpeed += movementValues.forwardAcceleration * dt;
                        EndForwardSpeed = Mathf.Min(EndForwardSpeed, ForwardDesiredSpeed);
                    }
                    break;
                }
        }
    }
    void UpdateRotation(float dt)
    {
        if (DesiredLookDirection == Vector3.zero)
        {
            lookDirection = bodyY0Forward;
            return;
        }

        Vector3 targetDir = DesiredLookDirection;
        targetDir.y = 0f;

        if (targetDir.sqrMagnitude < 0.0001f)
            return;

        float angle = Vector3.Angle(lookDirection, targetDir);

        float rotSpeed = phase==MovePhase.Brake ? 0 : movementValues.rotationSpeed;


        Vector3 newDir = Vector3.RotateTowards(
            lookDirection,
            targetDir,
            rotSpeed * Mathf.Deg2Rad * dt,
            1f
        ).normalized;

        lookDirection = newDir;

       
        bodyTransform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
    }
    void calculateBotVelocity(float deltaTime)
    {
        Vector3 dir= phase == MovePhase.Brake? VelocityDirection : BodyTargetDirection;
        float forwardSpeed = Vector3.Dot(bodyY0Forward, dir * EndForwardSpeed);
        float horizontalSpeed = Vector3.Dot(bodyTransform.right, dir * EndForwardSpeed);
        Velocity = dir * EndForwardSpeed;
        playerData.VerticalSpeed = forwardSpeed;
        playerData.HorizontalSpeed = horizontalSpeed;
        previousPosition = bodyPosition;
    }
    void ApplyMovement(float dt)
    {
        Vector3 dir = phase == MovePhase.Brake ? VelocityDirection : BodyTargetDirection;
        Vector3 delta = dir * EndForwardSpeed * dt;
        bodyRigidbody.MovePosition(bodyRigidbody.position + delta);
    }
    void testSpeed()
    {
        
        float maxSpeedForReachBall_rot = angleBodyForward_DesiredLookDirection >= playerComponents.movementValues.maxAngleForRun ? minSpeedForRotate:  reachBallSpeed ;
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
        float v = reachBallSpeed;
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
                lookDirection = Vector3.RotateTowards(lookDirection, targetDirection, movementValues.directionRotationSpeed * Mathf.Deg2Rad * deltaTime, 1).normalized;
            }
            else
            {
                lookDirection = EndForwardSpeed <= movementValues.minSpeedForChangeDirection ? targetDirection : lookDirection;
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
            bodyRigidbody.MovePosition(bodyRigidbodyPosition + lookDirection.normalized * EndForwardSpeed * deltaTime);
        }
        else
        {
            bodyTransform.Translate(lookDirection.normalized * EndForwardSpeed * deltaTime, Space.World);
        }
        //calculateVelocity(deltaTime);
    }
    void calculateVelocity(float deltaTime)
    {
        //movementValues.ForwardVelocity = (bodyRigidbody.position - previousPosition)/ deltaTime;

        calculateIsAccelerating();
        float forwardSpeed = Vector3.Dot(bodyY0Forward, lookDirection.normalized * EndForwardSpeed);
        float horizontalSpeed = Vector3.Dot(bodyTransform.right, lookDirection.normalized * EndForwardSpeed);
        playerData.Velocity = lookDirection.normalized * EndForwardSpeed;
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
    public void SetTargetPosition(Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - publicPlayerData.position;
        dir.y = 0;
        dir.Normalize();
        playerComponents.ForwardDesiredDirection = dir;
        playerComponents.ForwardDesiredSpeed = publicPlayerData.movimentValues.maxForwardSpeed;
        playerComponents.DesiredLookDirection = dir;
        playerComponents.MinForwardSpeed = 0;
        playerComponents.TargetPosition = targetPosition;
        playerComponents.stopOffset = 0;
    }
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && debug)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 12;
            style.normal.textColor = Color.white;
            //string info =i +"-"+ trajectory.times[i].ToString("f2");
            string info = "v="+Speed.ToString("f1") + " θ=" + angleBodyForwardDesiredVelocity.ToString("f0")+" phase="+phase+" distanceTarget="+ BodyTargetXZDistance + " distanceBall=" + BodyBallXZDistance;
            Handles.Label(bodyPosition + Vector3.up * 1.5f, info, style);
        }
    }
}
