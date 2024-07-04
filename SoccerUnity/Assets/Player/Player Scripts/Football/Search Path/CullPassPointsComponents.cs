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
using CullPositionPoint;

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
public class CalculateNextPositionComponents
{
    public NativeArray<Vector2> normalizedPositions;
    public NativeArray<float> offsideLinePosYs;
    public NativeArray<float> weightOffsideLines;
    public NativeArray<NextPositionData2> normalNextPosition;
}
[System.Serializable]
public class SearchPlayData
{
    public class SearchPlayNode
    {
        public Triangulator triangulator;
        public NativeArray<float2> playerPositions = new NativeArray<float2>();
        public List<int> cullEntities = new List<int>();
        public List<int> nextTriangulatorEntities = new List<int>();
        public List<float> speed = new List<float>();
        public List<Vector3> direction = new List<Vector3>();
        public int maxPointsSize;
        public int playerCount;
        public int nextNode;
        public int cullEntitiesSize { get => cullEntities.Count; }
        public List<LonelyPointElement2> posibleLonelyPoints;
        public int previousNode;
        public bool isSearched;
        public CalculateNextPositionComponents CalculateNextPositionComponents;
        public SearchPlayNode(int initSize,int playerPosSize,int playerSize, Triangulator.GetLonelyPointParameters GetLonelyPointParameters,int maxPosibleLonelyPoints)
        {
            maxPointsSize = initSize;
            playerPositions = new NativeArray<float2>(playerPosSize, Allocator.Persistent);
            speed = new List<float>(playerSize);
            direction = new List<Vector3>(playerSize);
            triangulator = new Triangulator(Allocator.Persistent, GetLonelyPointParameters);
            playerCount = playerSize;
            posibleLonelyPoints = new List<LonelyPointElement2>(maxPosibleLonelyPoints);
            LoadParameters(maxPosibleLonelyPoints);
        }
        public void LoadParameters(int JobSize)
        {
            CalculateNextPositionComponents.normalizedPositions = new NativeArray<Vector2>(JobSize, Allocator.Persistent);
            CalculateNextPositionComponents.offsideLinePosYs = new NativeArray<float>(JobSize, Allocator.Persistent);
            CalculateNextPositionComponents.weightOffsideLines = new NativeArray<float>(JobSize, Allocator.Persistent);
            CalculateNextPositionComponents.normalNextPosition = new NativeArray<NextPositionData2>(JobSize, Allocator.Persistent);
        }
        public void SetCalculateNextPositionParameters(int index, Vector2 normalizedPosition, float offsideLinePosY, float weightOffsideLine)
        {
            CalculateNextPositionComponents.normalizedPositions[index] = normalizedPosition;
            CalculateNextPositionComponents.offsideLinePosYs[index] = offsideLinePosY;
            CalculateNextPositionComponents.weightOffsideLines[index] = weightOffsideLine;
        }
        public void SetPlayerPosition(int index, Vector3 position, float speed, Vector3 direction)
        {
            playerPositions[index + 4] = new Vector2(position.x, position.z);
            this.speed[index] = speed;
            this.direction[index] = direction;
        }
        public void SetPosibleLonelyPoint(int index, LonelyPointElement2 lonelyPoint)
        {
            posibleLonelyPoints[index] = lonelyPoint;
        }
        
        public NativeList<Point> GetLonelyPoints()
        {
            var lonelyPoints = triangulator.Output.LonelyPoints;

            return lonelyPoints;
        }
        public NativeArray<float2> GetPlayerPositions() => playerPositions;
        public int getCullEntity(int index) => cullEntities[index];
    }

    public SearchPlayData(int maxPosibleLonelyPoints)
    {
        for (int i = 0; i < maxSize; i++)
        {
            searchPlayNodes.Add(new SearchPlayNode(maxPointsSize, playerPosSize, playerPosSize-4, GetLonelyPointParameters, maxPosibleLonelyPoints));
        }
    }

    public int maxSize = 30;
    public int maxPointsSize = 200;
    public int playerPosSize = 15;
    public int size;
    public Triangulator.GetLonelyPointParameters GetLonelyPointParameters;
    public List<SearchPlayNode> searchPlayNodes = new List<SearchPlayNode>();
    public LonelyPointElement2 GetPosibleLonelyPoint(int node,int index) => searchPlayNodes[node].posibleLonelyPoints[index];
    public CalculateNextPositionComponents GetCalculateNextPositionComponents(int node)
    {
       return searchPlayNodes[node].CalculateNextPositionComponents;
    }
    public void SetCalculateNextPositionParameters(int node,int index, Vector2 normalizedPosition, float offsideLinePosY, float weightOffsideLine)
    {
        searchPlayNodes[node].SetCalculateNextPositionParameters(index, normalizedPosition, offsideLinePosY, weightOffsideLine);
    }
    public void SetPosibleLonelyPoint(int node,int index, LonelyPointElement2 lonelyPoint)
    {
        searchPlayNodes[node].SetPosibleLonelyPoint(index, lonelyPoint);
    }
    public void SetPreviousNode(int node,int previousNode)=>searchPlayNodes[node].previousNode = previousNode;
    public bool GetIsSearched(int node) => searchPlayNodes[node].isSearched;
    public void SetIsSearched(int node,bool isSearched) => searchPlayNodes[node].isSearched= isSearched;
    public int getSortedNode(int index)
    {
        return -1;
    }
    public void getSortedNodes(ref List<int> nodes,int size)
    {
        for (int i = 0; i < size; i++)
        {
            nodes[i] = i;
        }
    }
    public void getFreeNodes(ref List<int> nodes, int size)
    {
        for (int i = 0; i < size; i++)
        {
            nodes[i] = i;
        }
    }
    public int getFreeNode(int index)
    {
        return -1;
    }
    public int getCullEntity(int node, int index) =>searchPlayNodes[node].getCullEntity(index);
    public int getCullEntityCount(int node) => searchPlayNodes[node].cullEntitiesSize;
    public void UpdatePoints(List<int> nodes, int size)
    {
        NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(size, Allocator.Temp);
        for (int i = 0; i < size; i++)
        {
            int node = nodes[i];
            SearchPlayNode searchPlayNode = searchPlayNodes[node];
            NativeArray<float2> playerPositions = searchPlayNode.playerPositions;
            searchPlayNode.triangulator.Input.Positions = playerPositions;
            jobHandles[i] = searchPlayNode.triangulator.Schedule();
        }
        JobHandle.CompleteAll(jobHandles);
        jobHandles.Dispose();
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
    public void SetPlayerPosition(int nodeIndex, int index, Vector3 position, float endSpeed, Vector3 direction)
    {
        searchPlayNodes[nodeIndex].SetPlayerPosition(index,position,endSpeed,direction);
    }
    public NativeList<Point> GetPoints(int index)=> searchPlayNodes[index].triangulator.Output.LonelyPoints;
    public NativeArray<float2> GetPlayerPositions(int index) => searchPlayNodes[index].playerPositions;
    public Vector2 GetPlayerPosition(int node, int index) {
       float2 pos = searchPlayNodes[node].playerPositions[index];
        return new Vector2(pos.x, pos.y);
    }
    public float GetPlayerSpeed(int node, int index)=> searchPlayNodes[node].speed[index];
    public Vector3 GetPlayerDirection(int node, int index) => searchPlayNodes[node].direction[index];
    public int GetPlayerCount(int node) => searchPlayNodes[node].playerCount;
    public void Dispose()
    {
        foreach (var searchPlayNode in searchPlayNodes)
        {
            searchPlayNode.playerPositions.Dispose();
            searchPlayNode.triangulator.Dispose();
        }
    }
}
