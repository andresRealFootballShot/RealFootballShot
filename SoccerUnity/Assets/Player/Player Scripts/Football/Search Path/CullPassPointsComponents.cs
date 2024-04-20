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
        
    }

    public struct PlayerPositionElement : IBufferElementData
    {
        public Vector2 position;

        public PlayerPositionElement(Vector2 position)
        {
            this.position = position;
        }
    }
    public struct PlayerGenericParams
    {
        public float maxSpeed;
        public float goalkeeperMaxSpeed;
        public float maxKickForce;
        float minSpeedForRotate;
        float acceleration;
        float decceleration;
        float maxAngleForRun;
        float maxSpeedRotation;
        float scope;
    }
    public struct TestResultComponent : IComponentData
    {
        public Vector3 closestPosition,lonelyPosition;
        public float ballReachTargetPositionTime,defenseLonelyPointReachTime, closestDistanceDefenseBall,attackReachTime,defenseClosestReachTime;
        public GetV0DOTSResult GetV0DOTSResult1,GetV0DOTSResult2;
        public int defenseLonelyPointReachIndex, attackLonelyPointReachIndex;
        public bool straightReachBall, parabolicReachBall;
    }
}
public struct BallParamsComponent : IComponentData
{
    public float k, friction, ballRadio, g,mass,groundY,dynamicFriction,bounciness;
    public Vector3 BallPosition;
}
public struct GetStraightV0Params
{
    public float accuracy, maxControlSpeed, maxControlSpeedLerpDistance, searchVyIncrement;
    public int maxAttempts;
}
