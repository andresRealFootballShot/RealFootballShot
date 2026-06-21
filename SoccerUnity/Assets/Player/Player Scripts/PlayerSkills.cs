
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSkills", menuName = "ScriptableObjects/PlayerSkills", order = 1)]
public class PlayerSkills : ScriptableObject
{
    public string typePlayerSkills;
    [Header("Movement")]
    [Range(0, 1)]
    public float acceleration;

    [Header("Control")]
    [Range(0, 1)]
    public float ballControl;
    [Range(0, 1)]
    public float ballForceControl;
    [Range(0, 1)]
    public float ballTimeControl;
    [Header("Control 2")]
    [Range(0, 180)]
    public int MaxAngleControl=120;
    [Range(0, 100)]
    public float MinVelocityControl = 5;
    [Range(0, 100)]
    public float MaxVelocityControl = 20;
    [Range(0, 100)]
    public float MaxVelocityDistanceControl=50;
    [Header("Driving")]
    [Range(0, 1)]
    public float drivingSkill;
    public float maxForce, minForce;
    public AnimationCurve randomForceAdjust;
    public float minHitTime, maxHitTime;
    public float maxSpeedBodyPercent;
    public float maxDrivingDistance = 5;
    public float minDrivingDistance = 0.5f;
    public float lastKickDistanceOffset = 3.0f;

}
