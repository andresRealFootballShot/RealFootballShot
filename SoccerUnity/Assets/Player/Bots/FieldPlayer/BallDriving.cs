using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BallDriving : SoccerPlayerComponent
{
    float maxForce { get => playerSkills.maxForce; }
    float minForce { get => playerSkills.minForce; }
    float maxDistance=9;
    public float maxAngleDriving=5;
    public float angleDrivingSkill { get=> Mathf.Lerp(0, maxAngleDriving, 1 - playerSkills.drivingSkill);}
    float minHitTime { get => playerSkills.minHitTime; }
    float maxHitTime{ get => playerSkills.maxHitTime; }
    public float bodyAdjustAngle, bodyAdjustSpeed;
    public float maxSpeedBodyPercent { get => playerSkills.maxSpeedBodyPercent; }

    Coroutine coroutine;
    public bool enabled;
    public void StartProcess()
    {
        if(enabled)
        coroutine = StartCoroutine(run());
        
    }
    public void StopProcess()
    {
        if (enabled)
            StopCoroutine(coroutine);
    }
    public IEnumerator run()
    {
        
        while (true)
        {
            if (ballIsOrientedControlled)
            {
                
                if (Speed < MaxSpeed * maxSpeedBodyPercent)
                {
                    float bodyAdjust1 = Mathf.Clamp01(bodyAdjustAngle - (bodyForward_BallTargetAngle / 180));
                    float bodyAdjust2 = Mathf.Clamp01((Speed / (MaxSpeed * 0.5f)) + bodyAdjustSpeed);
                    //float bodyAdjust = bodyAdjust1 * bodyAdjust2;
                    float bodyAdjust = bodyAdjust2 * bodyAdjust1;
                    bodyAdjust = Mathf.Clamp01(bodyAdjust);
                    //print("drive " + bodyAdjust1 + " " + bodyAdjust2 + " " + bodyAdjust);
                    //print("drive "+bodyAdjust1+" "+ bodyAdjust2 + " "+bodyAdjust + " "+ (bodySpeed / (MaxForwardFullSpeed * 0.5f)) + " "+bodySpeed+" "+ (MaxForwardFullSpeed * 0.5f));
                    float _maxForce = Mathf.Lerp(0, MaxSpeed * maxForce * bodyAdjust - ballSpeed, ballTargetDistance / maxDistance);
                    float _minForce = Mathf.Lerp(0, MaxSpeed * minForce * bodyAdjust - ballSpeed, ballTargetDistance / maxDistance);
                    float randomNumber = Random.Range(0.0f, 1.0f);
                    float randomAdjustNumber = playerSkills.randomForceAdjust.Evaluate(randomNumber);
                    float randomForce = Mathf.Lerp(_minForce, _maxForce, randomAdjustNumber);
                    Vector3 direction = MyFunctions.setRandomRotateAxisY(ballTargetDirection.normalized, angleDrivingSkill);
                    //print(randomForce+ " "+ (MaxForwardFullSpeed * maxForce)+ " randomNumber=" + randomNumber + " randomAdjustNumber=" + randomAdjustNumber+" maxForce="+ _maxForce+" minForce="+ _minForce);
                    ballRigidbody.AddForce(direction* randomForce, ForceMode.VelocityChange);
                    yield return new WaitForSeconds(Random.Range(minHitTime, maxHitTime));
                }
                else
                {
                    yield return null;
                }
            }
            else
            {
                yield return null;
            }
            //yield return null;
        }
    }

    float getSpeed(float x)
    {
        float result = 0;
        float y = Mathf.Pow(x, 2);
        return result;
    }
}
