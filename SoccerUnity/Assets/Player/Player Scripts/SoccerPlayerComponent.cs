using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerPlayerComponent : PlayerComponent
{
    protected SoccerPlayerData soccerPlayerData { get => playerComponents.soccerPlayerData; }
    protected Vector3 ballTargetPosition { get=> soccerPlayerData.ballTargetPosition; set => soccerPlayerData.ballTargetPosition = value; }
    protected Vector3 ballTargetDirection { get => ballTargetPosition - ballPosition;}
    protected float ballTargetDistance { get => ballTargetDirection.magnitude; }
    protected Vector3 bodyBallTargetY0Direction { get => MyFunctions.setY0ToVector3(ballTargetPosition - bodyPosition); }
    protected float bodyForward_BallTargetAngle { get => Vector3.Angle(bodyBallTargetY0Direction,bodyY0Forward); }
    protected float ballBody_BallTargetAngle { get => Vector3.Angle(bodyBallY0Direction, bodyBallTargetY0Direction); }
    protected float ballTarget_BallVelocityAngle { get => Vector3.Angle(bodyBallTargetY0Direction,MyFunctions.setY0ToVector3(ballVelocity)); }
    protected float bodyBallTarget_BallVelocityAngle { get => Vector3.Angle(bodyBallTargetY0Direction, MyFunctions.setY0ToVector3(ballVelocity)); }
    public bool ballIsOrientedControlled { get => soccerPlayerData.ballIsOrientedControlled; set => soccerPlayerData.ballIsOrientedControlled = value; }
    protected Vector3 closestPointBodyPosition(Vector3 point)
    {
        return MyFunctions.GetClosestPointOnFiniteLine(point, bodyPosition, headPosition);
    }
    protected Vector3 ballBodyDirection { get => closestPointBodyPosition(ballPosition) - ballPosition; }
    protected float ballBody_BallVelocityAngle { get =>Vector3.Angle(ballBodyDirection,ballVelocity); }
    protected float drag { get => ballRigidbody.drag; }
    protected float m { get => ballRigidbody.mass; }
    protected float friction { get => playerComponents.friction; }
    protected float maxKickForce { get => playerComponents.soccerPlayerData.maxKickForce; }

}
