using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimentValues : MonoBehaviour
{
    public string info;
    public Variable<float> maxSpeed = new Variable<float>();
    //public Variable<Vector3> forwardDesiredVelocityVar = new Variable<Vector3>();
    public Variable<Vector3> forwardVelocityVar = new Variable<Vector3>();
    public Variable<Vector3> DesiredLookDirectionVar = new Variable<Vector3>();
    public Variable<Vector3> LookDirectionVar = new Variable<Vector3>();
    ///<summary>
    ///velocityObsolete is obsolete, use forwardVelocityVar.
    ///</summary>
    public Variable<float> velocityObsolete = new Variable<float>();
    public Variable<float> maximumJumpForceVar = new Variable<float>();
    public float forwardRunSpeed = 7, forwardSprintSpeed = 3.5f;
    public float maxForwardSpeed { get; set; }
    public float backRunSpeed = 4;
    public float backSprintSpeed = 2f;
    public float maxBackSpeed{get;set;}
    public float horizontalRunSpeed = 3, horizontalSprintSpeed = 1f;
    public float maxHorizontalSpeed { get; set; }
    public float adjustedForwardVelocitySpeed;
    public Vector3 TargetPosition { get; set ; }
    public Vector3 ForwardDesiredDirection { get; set; }
    public Vector3 ForwardDesiredVelocity { get => ForwardDesiredDirection.normalized * ForwardDesiredSpeed; set { ForwardDesiredDirection = value.normalized; ForwardDesiredSpeed = value.magnitude; } }
    public Vector3 DesiredLookDirection { get => DesiredLookDirectionVar.Value; set => DesiredLookDirectionVar.Value = value; }
    public Vector3 LookDirection { get => LookDirectionVar.Value; set => LookDirectionVar.Value = value; }
    public Vector3 NormalizedForwardDesiredVelocity { get => ForwardDesiredVelocity/ maxForwardSpeed; }
    public Vector3 Clamp01NormalizedForwardDesiredVelocity { get => Vector3.ClampMagnitude(NormalizedForwardDesiredVelocity,1); }
    public bool isDefensivePosition { get; set; }




    public float MinForwardSpeed
    {
        get;set;
    }
    public float ForwardDesiredSpeed
    {
        get; set;
    }
    
    public float MaxForwardRunSpeed
    {
        get { return forwardRunSpeed; }
        set { forwardRunSpeed = value; maxSpeed.Value = forwardRunSpeed + forwardSprintSpeed; }
    }
    public float MaxForwardSprintSpeed
    {
        get { return forwardSprintSpeed; }
        set { forwardSprintSpeed = value; maxSpeed.Value = forwardRunSpeed + forwardSprintSpeed; }
    }
    public float MaximumJumpForce
    {
        get { return maximumJumpForceVar.Value; }
        set { maximumJumpForceVar.Value = value;}
    }
    public float NormalMaximumJumpHeight { get; set; }
    public float speedCamera = 50, speedChangeVelocityBall = 5, maxVelocityBall = 7, adjustMaxDistance = 1, maxDistance, angleMax;
    public float initMaxJumpForce = 5;
    public AnimationCurve velocityCurveSprint, speedRotationCurve, velocityCurveSpeed, curveVelocity, curveAnimator;
    public AnimationCurve forwardAnimCurve,sprintAnimCurve,distanceCurve;
    [Tooltip("distanceStopOffset when Player aproaches ball")]
    [HideInInspector]
    public float distanceStopMoveBallPlayerOffset = 0.1f;
    
    //[HideInInspector]
    //public float maxAcceleration, minAcceleration, maxDeceleration, minDeceleration;
    [HideInInspector]
    public float forwardAcceleration, horizontalAcceleration, backAcceleration;
    [HideInInspector]
    public float forwardDeceleration, horizontalDeceleration, backDeceleration;
    [Space(5)]
    [Header("Acceleration")]
    [Space(1)]
    public float forwardAccelerationDistance = 1;public float horizontalAccelerationDistance = 1, backAccelerationDistance = 1;
    [Space(2)]
    [Header("Deceleration")]
    public float forwardDecelerationDistance = 1;public float horizontalDecelerationDistance = 1, backDecelerationDistance = 1;
    [Space(10)]
    public float rotationSpeed=600;
    public float minRotationSpeedWhileRun = 300;
    public float maxSpeedWhileRun_AngularLerp = 4;
    public float minSpeedForRotateBody;
    public float minSpeedForChangeDirection=1;
    public float directionRotationSpeed=360;

    public float maxSpeedForReachBall { get; set; }
    public float maxAngleForRun;
    public float stopOffset;
    private void Awake()
    {
        maxSpeed.Value = forwardRunSpeed + forwardSprintSpeed;
        MaximumJumpForce = initMaxJumpForce;

        calculateMaxSpeeds();
        calculateAccelerations();
    }
    void calculateMaxSpeeds()
    {
        maxBackSpeed = backRunSpeed + backSprintSpeed;
        maxHorizontalSpeed = horizontalRunSpeed + horizontalSprintSpeed;
        maxForwardSpeed = MaxForwardRunSpeed + MaxForwardSprintSpeed;
    }
    void calculateAccelerations()
    {
        /*minAcceleration = (maxSpeed.Value * maxSpeed.Value) / (2 * forwardAccelerationDistance);
        maxAcceleration = (maxSpeed.Value * maxSpeed.Value) / (2 * minAccelerationDistance);
        minDeceleration = (maxSpeed.Value * maxSpeed.Value) / (2 * maxDecelerationDistance);
        maxDeceleration = (maxSpeed.Value * maxSpeed.Value) / (2 * minDecelerationDistance);*/
        forwardAcceleration = maxForwardSpeed * maxForwardSpeed / (2 * forwardAccelerationDistance);
        horizontalAcceleration = maxHorizontalSpeed * maxHorizontalSpeed / (2 * horizontalAccelerationDistance);
        backAcceleration = maxBackSpeed * maxBackSpeed / (2 * backAccelerationDistance);

        forwardDeceleration = maxForwardSpeed * maxForwardSpeed / (2 * forwardDecelerationDistance);
        horizontalDeceleration = maxHorizontalSpeed * maxHorizontalSpeed / (2 * horizontalDecelerationDistance);
        backDeceleration = maxBackSpeed * maxBackSpeed / (2 * backDecelerationDistance);
    }
}
