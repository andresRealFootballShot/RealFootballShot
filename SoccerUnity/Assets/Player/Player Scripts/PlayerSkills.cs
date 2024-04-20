
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
    [Header("Driving")]
    [Range(0, 1)]
    public float drivingSkill;
    public float maxForce, minForce;
    public AnimationCurve randomForceAdjust;
    public float minHitTime, maxHitTime;
    public float maxSpeedBodyPercent;
}
