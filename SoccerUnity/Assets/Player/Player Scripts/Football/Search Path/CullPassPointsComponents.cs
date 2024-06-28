using DOTS_ChaserDataCalculation;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using FieldTriangleV2;
using andywiecko.BurstTriangulator;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

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
        public LonelyPointElement2(Point point,int index)
        {
            this.position = point.position;
            this.index = index;
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
public struct Point
{
    public Vector2 position;

    public Point(Vector2 position)
    {
        this.position = position;
    }
}
[System.Serializable]
public class SearchPlayData
{
    public class SearchPlayNode
    {
        public Triangulator triangulator;
        public NativeArray<float2> playerPositions = new NativeArray<float2>();
        public List<int> cullEntities = new List<int>();
        public int maxPointsSize;
        public SearchPlayNode(int initSize,int playerPosSize, Triangulator.GetLonelyPointParameters GetLonelyPointParameters)
        {
            maxPointsSize = initSize;
            playerPositions = new NativeArray<float2>(playerPosSize, Allocator.Persistent);
            triangulator = new Triangulator(Allocator.Persistent, GetLonelyPointParameters);
        }
        public void SetPlayerPosition(int index, Vector3 position)
        {
            playerPositions[index + 4] = new Vector2(position.x, position.z);
        }
        public NativeList<Point> GetLonelyPoints()
        {
            var lonelyPoints = triangulator.Output.LonelyPoints;

            return lonelyPoints;
        }
        public NativeArray<float2> GetPlayerPositions() => playerPositions;
        public int getCullEntity(int index) => -1;
    }

    public SearchPlayData()
    {
        for (int i = 0; i < maxSize; i++)
        {
            searchPlayNodes.Add(new SearchPlayNode(maxPointsSize, playerPosSize, GetLonelyPointParameters));
        }
    }

    public int maxSize = 30;
    public int maxPointsSize = 200;
    public int playerPosSize = 15;
    public int size;
    public Triangulator.GetLonelyPointParameters GetLonelyPointParameters;
    public List<SearchPlayNode> searchPlayNodes = new List<SearchPlayNode>();

    public int getSortedNode(int index)
    {
        return -1;
    }
    public int getFreeNode(int index)
    {
        return -1;
    }
    public int getCullEntity(int node, int index) =>searchPlayNodes[node].getCullEntity(index);
    public void UpdatePoints(int startIndex,int endIndex)
    {
        NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(endIndex-startIndex, Allocator.Temp);
        for (int i = startIndex,j=0; i < endIndex; i++,j++)
        {
            SearchPlayNode searchPlayNode = searchPlayNodes[i];
            NativeArray<float2> playerPositions = searchPlayNode.playerPositions;
            searchPlayNode.triangulator.Input.Positions = playerPositions;
            jobHandles[j] = searchPlayNode.triangulator.Schedule();
        }
        JobHandle.CompleteAll(jobHandles);
    }
    public int GetLonelyPointSize(int index) => searchPlayNodes[index].maxPointsSize;
    public bool GetLonelyPoint(int index, int pointIndex,out Point point)
    {
        SearchPlayNode node = searchPlayNodes[index];
        if (index < node.maxPointsSize)
        {

            point = node.triangulator.Output.LonelyPoints[pointIndex];
            return true;
        }
        else
        {
            point = default;
            return false;
        }
    }
    public void SetPlayerPosition(int nodeIndex, int index, Vector3 position)
    {
        searchPlayNodes[nodeIndex].SetPlayerPosition(index,position);
    }
    public NativeList<Point> GetPoints(int index)=> searchPlayNodes[index].triangulator.Output.LonelyPoints;
    public NativeArray<float2> GetPlayerPositions(int index) => searchPlayNodes[index].playerPositions;
    public Vector2 GetPlayerPosition(int node, int index) {
       float2 pos = searchPlayNodes[node].playerPositions[index];
        return new Vector2(pos.x, pos.y);
    }
    public void Dispose()
    {
        foreach (var searchPlayNode in searchPlayNodes)
        {
            searchPlayNode.playerPositions.Dispose();
            searchPlayNode.triangulator.Dispose();
        }
    }
}
