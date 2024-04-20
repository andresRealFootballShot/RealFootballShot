using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBall : MovementPlayerComponent
{

    public float minDistanceSpeedInterpolationOffset = 1;
    public float maxDistanceSpeedInterpolationOffset = 1;
    public float distanceSpeedInterpolationOffset { get => Mathf.Lerp(minDistanceSpeedInterpolationOffset, maxDistanceSpeedInterpolationOffset,1-playerSkills.drivingSkill); }
    public float minFollowSpeed = 1;

    public float minOffsetSpeed = 1;
    public float maxOffsetSpeed = 1;
    public float offsetSpeed { get => Mathf.Lerp(minOffsetSpeed, maxOffsetSpeed,playerSkills.drivingSkill); }
    float speed;
    
    public void runFollowBall(float deltaTime)
    {
        /*
        Vector3 direction = ballPosition - bodyPosition;
        direction = MyFunctions.setYToVector3(direction, 0);
        ForwardDesiredDirection = direction.normalized * movementValues.MaxForwardRunSpeed;
        float aux = BodyBallXZDistance < distanceStopMoveBallPlayer ? 0 : 1;
        ForwardDesiredVelocity *= aux;*/
    }
    public void runOptimalPointToReachBall(float deltaTime)
    {
        ChaserData chaserData;
        publicPlayerData.getFirstChaserData(out chaserData);
        Vector3 direction;
        Vector3 targetPosition;
        if (chaserData.ReachTheTarget)
        {
            direction = chaserData.OptimalPoint - bodyPosition;
            targetPosition = chaserData.OptimalPoint;
        }
        else if(chaserData.thereIsClosestPoint)
        {
            direction = chaserData.ClosestPoint - bodyPosition;
            targetPosition = chaserData.ClosestPoint;
        }
        else
        {
            direction = ballPosition - bodyPosition;
            targetPosition = ballPosition;
        }
        direction = MyFunctions.setYToVector3(direction, 0);
        //float distanceInterpolation = bodyBallRadio + distanceSpeedInterpolationOffset;
        //float distanceInterpolation = distanceSpeedInterpolationOffset;
        float distanceInterpolation = distanceSpeedInterpolationOffset;
        float clampBallSpeed = Mathf.Clamp(ballSpeed + offsetSpeed, 0, MaxSpeed);
        float minSpeed = Mathf.Lerp(minFollowSpeed, clampBallSpeed, (ballSpeed) / 2f);
        minSpeed = BodyBallXZDistance <= bodyBallRadio + 0.1f ? 0 : minSpeed;
        //calculateDistanceStop();
        //print(getAccelerationSkill() + " "+getDecelerationSkill());
        float lerp = Mathf.Clamp01((BodyBallXZDistance - distanceStopMoveBallPlayer) / distanceInterpolation);
        //float speed = Mathf.Lerp(adjustedForwardVelocitySpeed, MaxForwardFullSpeed, lerp);
        //float speed = Mathf.Clamp(MaxForwardFullSpeed, 0, Mathf.Clamp(BodyBallXZDistance - distanceStopMoveBallPlayer,0,Mathf.Infinity));
        //float speed = BodyBallXZDistance <= distanceStopMoveBallPlayer ? minSpeed : MaxForwardFullSpeed;
        //float speed = BodyBallXZDistance < distanceStopMoveBallPlayer ? ballSpeed : MaxForwardFullSpeed;
        //speed = speed < 0.001f ? 0 : speed;
        DesiredDirection = direction.normalized ;
        ForwardDesiredSpeed = MaxSpeed;
        MinForwardSpeed = ballSpeed;
        //print("speed=" + speed);
        TargetPosition = targetPosition;
        //print(speed+" | "+ BodyBallXZDistance+" | "+ distanceStopMoveBallPlayer);
        DesiredLookDirection = direction.normalized;
        stopOffset = bodyBallRadio + 0.0f;
        //print(DesiredLookDirection.magnitude);
        //print(BodyBallXZDistance + " | " + distanceStopMoveBallPlayer + " | " + lerp + " | " + ForwardDesiredVelocity.magnitude);
        //float aux = BallBodyXZDistance < distanceStopMoveBallPlayer ? 0 : 1;
    }
    void calculateDistanceStop()
    {
        float v = maxSpeedForReachBall;
        
        if (isAccelerating)
        {
            speed = EndForwardSpeed;

        }
        float v0 = speed;
        //print(v0);
        float d = ((v * v) - (v0 * v0)) / (2 * getMaxDeceleration());
        movementValues.distanceStopMoveBallPlayerOffset = Mathf.Abs(d);
        //print(d + " ");
    }
}
