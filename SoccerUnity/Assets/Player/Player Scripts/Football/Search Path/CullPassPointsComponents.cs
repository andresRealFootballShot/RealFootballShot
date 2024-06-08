using DOTS_ChaserDataCalculation;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using FieldTriangleV2;
namespace CullPositionPoint
{
    public struct LonelyPointElement2 : IBufferElementData
    {
        public Vector2 position;
        public int index;
        public bool straightReachBall, parabolicReachBall;
        public float weight;
        public int order;
        public float ballReachTime;
        public LonelyPointElement2(Vector2 position, int index)
        {
            this.position = position;
            this.index = index;
            straightReachBall = false;
            parabolicReachBall = false;
            weight = Mathf.Infinity;
            order = -1;
            ballReachTime = -1;
        }
        public LonelyPointElement2(LonelyPointElement lonelyPointElement)
        {
            this.position = lonelyPointElement.position;
            this.index = lonelyPointElement.index;
            straightReachBall = false;
            parabolicReachBall = false;
            weight = Mathf.Infinity;
            order = -1;
            ballReachTime = -1;
        }
    }
    public struct CullPassPointsComponent : IComponentData
    {
        public int teamASize, teamBSize;
        public bool teamA_IsAttacker;
        public int sizeLonelyPoints;
        public Vector2 post1Position, post2Position;
        public float distanceWeightLerp;
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
        public Vector3 closestPosition, lonelyPosition,defenseReachPosition;
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
