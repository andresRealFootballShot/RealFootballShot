using DOTS_ChaserDataCalculation;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
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
    public bool debug,debugMove,debugMoveTimes;
    MovePhase previousPhase;
    float previousSpeed;
    [HideInInspector]
    public float breakTime, moveTime, rotationTime;
    bool startMove;
    bool startDecelerate;
    // Update is called once per frame
    private void Start()
    {
        if (!useRigidbody)
        {
            Destroy(bodyRigidbody);
        }
    }
    private void OnEnable()
    {
        //lookDirection = bodyY0Forward;
    }
    public void getAdjustedForwardVelocitySpeed(float deltaTime)
    {
        
        testSpeed();
        return;
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
        calculateTimesMove();
    }

    void UpdateMovementPhase()
    {
        if (DesiredDirection == Vector3.zero)
            return;
        
        float angle = Vector3.Angle(VelocityY0Direction, DesiredY0Direction);
        float angle3 = Vector3.Angle(LookY0Direction, DesiredLookDirection);
        float angle2 = Vector3.Angle(bodyY0Forward, DesiredY0Direction)-maxAngleForRun;
        float requiredSpeed = angle > maxAngleForRun2 ? minSpeedForRotate2: minSpeedForRotate;
        string n = publicPlayerData.playerID;
        if (angle > maxAngleForRun && EndForwardSpeed > requiredSpeed || StopMove && angle3>1&& EndForwardSpeed > requiredSpeed)
        {
            phase = MovePhase.Brake;
        }
        else if (angle2 > 0.1f || StopMove&& angle3 > 1)
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
                    float angle = Vector3.Angle(VelocityY0Direction, DesiredY0Direction);
                    float requiredSpeed = angle > maxAngleForRun2 ? minSpeedForRotate2 : minSpeedForRotate;

                    EndForwardSpeed -= movementValues.forwardDeceleration * dt;
                    EndForwardSpeed = Mathf.Max(EndForwardSpeed, requiredSpeed);
                    
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
                        (EndForwardSpeed * EndForwardSpeed - reachBallSpeed * reachBallSpeed) /
                        (2f * movementValues.forwardDeceleration);

                    if (distance < stopDist)
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


        float maxRadiansThisFrame =
        Mathf.Min(angle * Mathf.Deg2Rad, rotSpeed * Mathf.Deg2Rad * dt);

        Vector3 newDir = Vector3.RotateTowards(
            bodyY0Forward,
            targetDir,
            maxRadiansThisFrame,
            0f
        ).normalized;

        lookDirection = newDir;

        
        bodyTransform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        LookDirection = lookDirection;
    }

    void calculateBotVelocity(float deltaTime)
    {
        Vector3 dir= phase == MovePhase.Brake ? VelocityDirection : DesiredY0Direction;
        Velocity = dir * EndForwardSpeed;
        previousPosition = bodyPosition;
    }
    public void ApplyMovement(float dt)
    {
        Vector3 dir = phase == MovePhase.Brake ? VelocityDirection : DesiredY0Direction;
        
        Vector3 delta = dir * EndForwardSpeed * dt;
        if (useRigidbody)
        {
            bodyRigidbody.MovePosition(bodyRigidbody.position + delta);
        }
        else
        {
            bodyTransform.Translate(delta, Space.World);
        }
        
    }
    void testSpeed()
    {
        
        float maxSpeedForReachBall_rot = angleBodyForward_DesiredLookDirection >= playerComponents.movementValues.maxAngleForRun ? minSpeedForRotate:  reachBallSpeed ;
        float stopDistance = Mathf.Abs(AccelerationPath.getX2(maxSpeedForReachBall_rot, playerComponents.Speed, playerComponents.movementValues.forwardDeceleration));
        float desiredSpeed_rot = angleBodyForward_DesiredLookDirection >= playerComponents.movementValues.maxAngleForRun ? minSpeedForRotate : ForwardDesiredSpeed;
        MovimentValues movimentValues = playerComponents.movementValues;
        //print(BodyTargetXZDistance+" "+stopDistance + " "+scope);
        if (BodyTargetXZDistance < stopDistance + scope)
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
        playerComponents.ForwardDesiredSpeed = publicPlayerData.movimentValues.maxForwardSpeed;
        
        StopMove = false;
        LookTarget = true;
        playerComponents.MinForwardSpeed = 0;
        playerComponents.TargetPosition = targetPosition;
        playerComponents.stopOffset = 0;
        startMoveTimes();

    }
    public void SetInstantVelocity(Vector3 dir,float speed)
    {
        playerComponents.ForwardDesiredSpeed = speed;
        EndForwardSpeed = speed;
        playerComponents.LookTarget = true;
        playerComponents.MinForwardSpeed = 0;
        playerComponents.TargetPosition =bodyPosition + dir*100;
        playerComponents.stopOffset = 0;
        playerComponents.Velocity = dir*speed;
    }
    void calculateTimesMove()
    {
        if (!debug) return;
        if (BodyTargetXZDistance <= scope && startMove)
        {
            print("MovementCtrl"+"\nTotal Time= " + (moveTime + breakTime) + " mT=" + moveTime + " bT=" + breakTime + " rT=" + rotationTime + " Speed="+EndForwardSpeed+" TargetDistance="+ BodyTargetXZScpDistance + " BallDistance=" + BodyBallXZScpDistance);
            startMove = false;
            startDecelerate = false;
        }
        if (startMove)
        {
            if (previousPhase == MovePhase.Brake && phase != MovePhase.Brake)
            {
                //print("Angle= " + angleBodyForwardDesiredVelocity + " speed="+Speed + " distanceTarget="+(BodyTargetXZDistance-scope));
            }
            previousPhase = phase;
            if (phase == MovePhase.Brake)
            {
                breakTime += Time.deltaTime;
            }
            else
            {
                /*
                if (Speed >= MaxSpeed && previousSpeed < MaxSpeed)
                {
                    print("Reach MaxSpeed | speed=" + Speed + " distanceTarget=" + (BodyTargetXZDistance - scope) + " currentTime=" + moveTime);
                }else if (previousSpeed > Speed && !startDecelerate)
                {
                    print("Start Decelerate | speed=" + Speed + " distanceTarget=" + (BodyTargetXZDistance - scope)+ " currentTime=" + moveTime);
                    startDecelerate = true;
                }*/
                previousSpeed = Speed;
                if (phase == MovePhase.Rotate)
                {
                    rotationTime += Time.deltaTime;
                    moveTime += Time.deltaTime;
                    //print("speed="+Speed);
                }
                else
                {
                    //print("speed=" + Speed);
                    moveTime += Time.deltaTime;
                }
            }

        }

    }
    public void startMoveTimes()
    {
        startMove = true;
        breakTime = 0;
        moveTime = 0;
        rotationTime = 0;
    }
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && debug)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 10;
            style.normal.textColor = Color.white;
            //string info =i +"-"+ trajectory.times[i].ToString("f2");
            string info="";
            if (debugMove)
            {
                info = "v=" + Speed.ToString("f1") + " θ=" + angleBodyForwardDesiredVelocity.ToString("f0") + " phase=" + phase + " dT=" + (BodyTargetXZDistance - scope).ToString("f2") + " dB=" + BodyBallXZDistance.ToString("f2")+" LookTarget="+LookTarget+" StopMove="+StopMove;
            }
            if (debugMoveTimes)
            {
                info += "Total time=" + (breakTime+ moveTime).ToString("f3")+ " bT=" + breakTime.ToString("f3") + " mT=" + moveTime.ToString("f3") + " rT=" + rotationTime.ToString("f3");
            }
            Handles.Label(bodyPosition + Vector3.up * 1.5f, info, style);
        }
    }
    public void SetStopped_LookTarget(Vector3 targetPosition)
    {
        StopMove = true;
        LookTarget = false;
        Vector3 dir = targetPosition - bodyPosition;
        dir.y = 0;
        dir.Normalize();
        DesiredLookDirection = dir;
    }
}
