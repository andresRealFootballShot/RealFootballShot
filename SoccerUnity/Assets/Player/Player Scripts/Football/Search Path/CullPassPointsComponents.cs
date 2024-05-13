using DOTS_ChaserDataCalculation;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
namespace CullPositionPoint
{
    public struct CullPassPointsComponent : IComponentData
    {
        public int teamASize, teamBSize;
        public bool teamA_IsAttacker;
        public int sizeLonelyPoints;
    }

    public struct PlayerPositionElement : IBufferElementData
    {
        public Vector2 position;
        public Vector2 bodyForward,normalizedVelocity;
        public float currentSpeed;
        public PlayerPositionElement(Vector2 position, Vector2 bodyForward, Vector2 normalizedVelocity, float currentSpeed)
        {
            this.position = position;
            this.bodyForward = bodyForward;
            this.normalizedVelocity = normalizedVelocity;
            this.currentSpeed = currentSpeed;
        }
    }
    public struct PlayerGenericParams
    {
        public float maxSpeed;
        public float goalkeeperMaxSpeed;
        public float maxKickForce;
        public float minSpeedForRotate;
        public float acceleration;
        public float decceleration;
        public float maxAngleForRun;
        public float maxSpeedRotation;
        public float scope;
        public float heightJump;
        public float heightBallControl;
        public float maxSpeedForReachBall;
    }
    public struct TestResultComponent : IComponentData
    {
        public Vector3 closestPosition, lonelyPosition;
        public float defenseLonelyPointReachTime, closestDistanceDefenseBall, attackReachTime, defenseClosestReachTime;
        public GetV0DOTSResult GetV0DOTSResult1, GetV0DOTSResult2;
        public float defenseParabolicDifferenceTime;
        public int defenseLonelyPointReachIndex, attackLonelyPointReachIndex;
        public bool straightReachBall, parabolicReachBall;
    }
}
public struct BallParamsComponent : IComponentData
{
    public float k, friction, ballRadio, g, mass, groundY, dynamicFriction, bounciness;
    public Vector3 BallPosition;
}
public struct GetStraightV0Params
{
    public float accuracy, maxControlSpeed, maxControlSpeedLerpDistance, searchVyIncrement;
    public int maxAttempts;
}
