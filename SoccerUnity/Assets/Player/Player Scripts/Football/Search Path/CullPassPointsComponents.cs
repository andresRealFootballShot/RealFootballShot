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
using System;

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
    public NativeArray<Vector2> normalizedBallPositions;
    public NativeArray<float> offsideLinePosYs;
    public NativeArray<float> weightOffsideLines;
    public NativeArray<NextPositionData2> normalNextPosition;
}
public class CalculateNextPositionComponents2
{
    public Vector2 normalizedBallPosition;
    public float offsideLinePosY;
    public float weightOffsideLine;
    public NextPositionData2 normalNextPosition;
}
[System.Serializable]
public class SearchPlayData
{
    public class SearchPlayNode
    {
        public Triangulator triangulator;
        public NativeArray<float2> playerPositions = new NativeArray<float2>();
        public List<int> cullEntities = new List<int>();
        public List<int> cullEntityLonelyPointCount = new List<int>();
        public List<int> nextTriangulatorEntities = new List<int>();
        public List<float> speed = new List<float>();
        public List<Vector3> direction = new List<Vector3>();
        public Vector3 ballPosition { get => new Vector3(ballLonelyPoint.position.x, 0, ballLonelyPoint.position.y); }
        public int maxPointsSize;
        public int playerCount;
        public int cullEntitiesSize { get => cullEntities.Count; }
        public LonelyPointElement2 ballLonelyPoint;
        public int previousNode;
        public bool isBusy,isSearched;
        public CalculateNextPositionComponents2 CalculateNextPositionComponents = new CalculateNextPositionComponents2();
        public List<int> nextNodes;
        public int index;
        public int cullEntityBusySize;
        public SearchPlayNode(int initSize,int playerSize, int index)
        {
            maxPointsSize = initSize;
            //playerPositions = new NativeArray<float2>(playerPosSize, Allocator.Persistent);
            speed = new List<float>(new float[playerSize]);
            direction = new List<Vector3>(new Vector3[playerSize]);
            //triangulator = new Triangulator(Allocator.Persistent, GetLonelyPointParameters);
            playerCount = playerSize-4;
            nextNodes = new List<int>();
            this.index = index;
        }
        public void SetCalculateNextPositionParameters(Vector2 normalizedBallPosition, float offsideLinePosY, float weightOffsideLine)
        {
            CalculateNextPositionComponents.normalizedBallPosition = normalizedBallPosition;
            CalculateNextPositionComponents.offsideLinePosY= offsideLinePosY;
            CalculateNextPositionComponents.weightOffsideLine = weightOffsideLine;
        }
        public void SetPlayerPosition(int index, Vector3 position, float speed, Vector3 direction)
        {
            playerPositions[index + 4] = new Vector2(position.x, position.z);
            this.speed[index] = speed;
            this.direction[index] = direction;
        }
        public void SetPosibleSortLonelyPoint(LonelyPointElement2 ballLonelyPoint)
        {
            this.ballLonelyPoint = ballLonelyPoint;
        }
        public NativeList<Point> GetLonelyPoints()
        {
            var lonelyPoints = triangulator.Output.LonelyPoints;

            return lonelyPoints;
        }
        public NativeArray<float2> GetPlayerPositions() => playerPositions;
        public int getCullEntity(int index) => cullEntities[index];
        public int getLastCullEntity() => cullEntities[(int)Mathf.Clamp(cullEntities.Count-1,0,Mathf.Infinity)];
        public void Clear()
        {
            nextNodes.Clear();
            isBusy = false;
            isSearched = false;
            cullEntities.Clear();
        }
    }

    public void Load(int posibleNodeSize)
    {
        //posibleNodes = new List<int>(new int[posibleNodeSize]);
        for (int i = 0; i < maxSize; i++)
        {
            searchPlayNodes.Add(new SearchPlayNode(maxPointsSize, playerPosSize,i));
        }
    }
    public int maxSize;
    public int maxPointsSize;
    public int playerPosSize;
    public int searchingNodesCount;
    public Triangulator.GetLonelyPointParameters GetLonelyPointParameters;
    public List<SearchPlayNode> searchPlayNodes = new List<SearchPlayNode>();
    public List<int> posibleNodes = new List<int>();
    public int nextCullEntity;
    public int nextFreeNode;
    public LonelyPointElement2 GetPosibleSortLonelyPoint(int node) => searchPlayNodes[node].ballLonelyPoint;
    public void AddPosibleNode(int posibleNode) => posibleNodes.Add(posibleNode);
    public CalculateNextPositionComponents2 GetCalculateNextPositionComponents(int node)
    {
       return searchPlayNodes[node].CalculateNextPositionComponents;
    }
    public void SetCalculateNextPositionParameters(int node,Vector2 normalizedBallPosition, float offsideLinePosY, float weightOffsideLine)
    {
        searchPlayNodes[node].SetCalculateNextPositionParameters( normalizedBallPosition, offsideLinePosY, weightOffsideLine);
    }
    public void SetPosibleSortLonelyPoint(int node,LonelyPointElement2 lonelyPoint)
    {
        searchPlayNodes[node].SetPosibleSortLonelyPoint(lonelyPoint);
    }
    public void SetPreviousNode(int node,int previousNode)=>searchPlayNodes[node].previousNode = previousNode;
    public void AddNextNode(int node, int nextNode) => searchPlayNodes[node].nextNodes.Add(nextNode);
    public void SetIsSearched(int node, bool isSearched) => searchPlayNodes[node].isSearched = isSearched;
    public void SetIsBusy(int node, bool isBusy) => searchPlayNodes[node].isBusy = isBusy;
    
    public void SetIsSearched(List<int> nodes, bool isSearched)
    {
        foreach (var node in nodes)
        {
            SetIsSearched(node, isSearched);
        }
    }
    public void SetIndex(int node, int index) => searchPlayNodes[node].index = index;
    public int GetIndex(int node) => searchPlayNodes[node].index;
    public int GetPreviousNode(int node) => searchPlayNodes[node].previousNode;
    public void SetBallPosition(int node, Vector3 ballPosition) => searchPlayNodes[node].ballLonelyPoint.position = new Vector2(ballPosition.x,ballPosition.z);
    public int getSortedNode(int index)
    {
        return -1;
    }
    public void getNode(List<int> nodes, int index)
    {
        nodes[index]=index;
    }
    public void getSortedNodes(ref List<int> nodes,int size)
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < searchPlayNodes.Count; j++)
            {
                if (!searchPlayNodes[i].isSearched)
                {
                    nextFreeNode = j + 1;
                    nodes.Add(j);
                    break;
                }
            }
            
        }
    }
    public int getFreeNode(int index)
    {
        return -1;
    }
    public int getNextFreeNode()
    {
        int result = nextFreeNode;
        nextFreeNode++;
        return result;
    }
    public int getCullEntity(int node, int index) =>searchPlayNodes[node].getCullEntity(index);
    public int getNextCullEntity() => nextCullEntity;
    public int getCullEntityCount(int node) => searchPlayNodes[node].cullEntitiesSize;
    public void UpdatePoints(int size)
    {
        NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(size, Allocator.Temp);
        for (int i = 0; i <size; i++)
        {
            int node = posibleNodes[i];
            SearchPlayNode searchPlayNode = searchPlayNodes[node];
            NativeArray<float2> playerPositions = searchPlayNode.playerPositions;
            searchPlayNode.triangulator.Input.Positions = playerPositions;
            jobHandles[i] = searchPlayNode.triangulator.Schedule();
        }
        JobHandle.CompleteAll(jobHandles);
        jobHandles.Dispose();
    }
    public int GetMaxPointSize(int index) => searchPlayNodes[index].maxPointsSize;
    public int GetNextNode(int node,int index) => searchPlayNodes[node].nextNodes[index];
    public List<int> GetNextNodes(int node) => searchPlayNodes[node].nextNodes;
    public int GetNextNodeByOrder(int node, int order)
    {
        for (int i = 0; i < searchPlayNodes[node].nextNodes.Count; i++)
        {
            int nextNode = searchPlayNodes[node].nextNodes[i];
            if (searchPlayNodes[nextNode].ballLonelyPoint.order == order) return nextNode;
        }
        return -1;
    }
    public int GetNextNodeCount(int node) => searchPlayNodes[node].nextNodes.Count;
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
    public NativeList<Point> GetLonelyPoints(int index)=> searchPlayNodes[index].triangulator.Output.LonelyPoints;
    public int GetLonelyPointsCount(int index) => searchPlayNodes[index].triangulator.Output.LonelyPoints.Length;
    public NativeArray<float2> GetPlayerPositions(int index) => searchPlayNodes[index].playerPositions;
    public Vector2 GetPlayerPosition(int node, int index) {
       float2 pos = searchPlayNodes[node].playerPositions[index+4];
        return new Vector2(pos.x, pos.y);
    }
    public float GetPlayerSpeed(int node, int index)=> searchPlayNodes[node].speed[index];
    public Vector3 GetPlayerDirection(int node, int index) => searchPlayNodes[node].direction[index];
    public int GetPlayerCount(int node) => searchPlayNodes[node].playerCount;
    public Vector3 GetBallPosition(int node) => searchPlayNodes[node].ballPosition;
    public void Dispose()
    {
        foreach (var searchPlayNode in searchPlayNodes)
        {
            searchPlayNode.playerPositions.Dispose();
            searchPlayNode.triangulator.Dispose();
        }
    }
    public void SetPlayerPositions(int node, NativeArray<float2> playerPositions) => searchPlayNodes[node].playerPositions = playerPositions;
    public void SetTriangulator(int node, Triangulator triangulator) => searchPlayNodes[node].triangulator = triangulator;
   
    public void SetCullEntity(int node,int cullEntity)
    {
       searchPlayNodes[node].cullEntities.Add(cullEntity);
       nextCullEntity = cullEntity+1;
    }
    public void ClearCullEntities()
    {
        foreach (var searchPlayNode in searchPlayNodes)
        {
            searchPlayNode.cullEntities.Clear();
        }
        nextCullEntity = 0;
    }
    internal void Clear()
    {
        searchingNodesCount = 0;
        nextFreeNode = 0;
        foreach (var searchPlayNode in searchPlayNodes)
        {
            searchPlayNode.Clear();
        }
    }
}
