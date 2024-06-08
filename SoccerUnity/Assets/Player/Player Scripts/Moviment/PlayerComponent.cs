using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComponent : MonoBehaviour
{
    public virtual PlayerComponents playerComponents { get; set; }
    public PlayerData playerData { get => playerComponents.playerData; }
    protected MovimentValues movementValues { get => playerComponents.movementValues; }
    public PublicPlayerData publicPlayerData { get => playerComponents.publicPlayerData; }
    protected PlayerSkills playerSkills { get => playerComponents.playerSkills; }
    protected PlayerParameters playerParameters { get => playerComponents.playerParameters; }
    protected Transform ballTransform { get => MatchComponents.ballTransform; }
    protected Transform bodyTransform { get => playerComponents.bodyTransform; }
    protected Vector3 bodyForward { get => playerComponents.bodyTransform.forward; }
    protected Vector3 bodyRight { get => playerComponents.bodyTransform.right; }
    public Vector3 bodyY0Forward { get => MyFunctions.setYToVector3(bodyForward, 0); }
    public Vector3 Velocity { get => playerData.Velocity; set => playerData.Velocity = value; }
    public Vector3 VelocityDirection { get => playerData.Velocity.normalized;}
    public Vector3 Y0Velocity { get => MyFunctions.setY0ToVector3(playerData.Velocity); }
    public float Speed { get => playerData.Speed; }
    public float AngularSpeed { get => playerData.AngularSpeed; set => playerData.AngularSpeed = value; }
    public float VerticalSpeed { get => playerData.VerticalSpeed; }
    public float HorizontalSpeed { get => playerData.HorizontalSpeed; }
    public Vector3 DesiredLookDirection { get => movementValues.DesiredLookDirection; set => movementValues.DesiredLookDirection = value; }
    protected float bodyRotationSpeed { get => playerData.RotationSpeed; set => playerData.RotationSpeed = value; }
    protected Vector3 ballPosition { get => MatchComponents.ballTransform.position; set => MatchComponents.ballTransform.position = value; }
    protected Vector3 ballVelocity { get => MatchComponents.ballRigidbody.velocity; }
    protected float ballSpeed { get => MatchComponents.ballRigidbody.velocity.magnitude; }
    protected float ballRadio { get => MatchComponents.ballRadio; }
    protected float bodyRadio { get => playerComponents.playerData.bodyRadio; }
    protected float bodyHeight { get => playerComponents.playerData.height; }
    public float bodyBallRadio { get => bodyRadio + ballRadio; }
    public float MaxSpeed { get => playerComponents.getMaxSpeed(); }
    public float MinForwardSpeed { get => playerComponents.movementValues.MinForwardSpeed; set => playerComponents.movementValues.MinForwardSpeed = value; }
    public float ForwardDesiredSpeed { get => playerComponents.movementValues.ForwardDesiredSpeed; set => playerComponents.movementValues.ForwardDesiredSpeed = value; }
    public Vector3 ForwardDesiredDirection { get => movementValues.ForwardDesiredDirection; set => movementValues.ForwardDesiredDirection = value; }
    protected Vector3 bodyPosition { get => playerComponents.bodyTransform.position; }
    protected Vector3 bodyY0Position { get => MyFunctions.setY0ToVector3(playerComponents.bodyTransform.position); }
    protected Vector3 headPosition { get => playerComponents.bodyTransform.position + Vector3.up*playerComponents.playerData.height; }
    protected Vector3 bodyBallDirection { get => ballPosition - bodyPosition; }
    protected Vector3 bodyBallY0Direction { get => MyFunctions.setYToVector3(bodyBallDirection,0); }
    protected Vector3 bodyBallNormalizedDirection { get => bodyBallDirection.normalized; }
    public Vector3 TargetPosition { get => movementValues.TargetPosition; set => movementValues.TargetPosition = value; }
    public Vector3 BodyTargetDirection { get => MyFunctions.setY0ToVector3(movementValues.TargetPosition - bodyPosition).normalized; }
    protected float BodyTargetXZDistance { get => Vector3.Distance(MyFunctions.setYToVector3(bodyPosition, 0), MyFunctions.setYToVector3(TargetPosition, 0)); }
    protected float BodyBallXZDistance { get => Vector3.Distance(MyFunctions.setYToVector3(bodyPosition, 0), MyFunctions.setYToVector3(ballPosition, 0)); }
    protected Quaternion bodyRotation { get => playerComponents.bodyTransform.rotation; set => playerComponents.bodyTransform.rotation = value; }
    protected Rigidbody bodyRigidbody { get => playerComponents.rigidbody; }
    protected Rigidbody ballRigidbody { get => MatchComponents.ballRigidbody; }
    protected Vector3 bodyRigidbodyPosition { get => playerComponents.rigidbody.position; }
    protected float ballBodyAngle { get => Vector3.Angle(bodyY0Forward, bodyBallY0Direction); }
    public float scope { get => playerComponents.soccerPlayerData.scopeOffset +bodyRadio + MatchComponents.ballRadio; }
    
    public float stopOffset { get => movementValues.stopOffset; set => movementValues.stopOffset = value; }
    public new string name { get => playerComponents.root.name; }
    public float getMaxAcceleration()
    {
        //return movementValues.forwardAcceleration;
        return playerComponents.getMaxAcceleration();
    }
    public float getMaxDeceleration()
    {
        //return movementValues.forwardDeceleration;
        return playerComponents.getMaxDeceleration();
    }
}
