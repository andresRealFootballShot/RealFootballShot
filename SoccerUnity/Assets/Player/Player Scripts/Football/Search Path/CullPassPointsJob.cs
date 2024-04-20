using FieldTriangleV2;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using CullPositionPoint;
using DOTS_ChaserDataCalculation;
using TMPro;
using Unity.Entities.UniversalDelegates;
[BurstCompile]
public struct CullPassPointsJob : IJobEntityBatch
{
    [ReadOnly] public BufferTypeHandle<LonelyPointElement> lonelyPointsHandle;
    [ReadOnly] public BufferTypeHandle<PlayerPositionElement> playerPositionElementHandle;
    [ReadOnly] public ComponentTypeHandle<CullPassPointsComponent> cullPassPointsParamsHandle;
    [ReadOnly] public ComponentTypeHandle<BallParamsComponent> BallParamsComponentHandle;
               public ComponentTypeHandle<TestResultComponent> TestResultComponentHandle;
    public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
    {

        BufferAccessor<LonelyPointElement> lonelyPointsBuffer = batchInChunk.GetBufferAccessor(lonelyPointsHandle);
        BufferAccessor<PlayerPositionElement> playerPositionElementBuffer = batchInChunk.GetBufferAccessor(playerPositionElementHandle);
        NativeArray<CullPassPointsComponent> cullPassPointsParamsBuffer = batchInChunk.GetNativeArray(cullPassPointsParamsHandle);
        NativeArray<BallParamsComponent> BallParamsComponentBuffer = batchInChunk.GetNativeArray(BallParamsComponentHandle);
        NativeArray<TestResultComponent> TestResultBuffer = batchInChunk.GetNativeArray(TestResultComponentHandle);
        GetStraightV0Params VOParams = GetV0Params();
        PlayerGenericParams PlayerGenericParams = GetPlayerGenericParams();
        for (int i = 0; i < lonelyPointsBuffer.Length; i++)
        {
            DynamicBuffer<LonelyPointElement> lonelyPoints = lonelyPointsBuffer[i];
            DynamicBuffer<PlayerPositionElement> PlayerPositions = playerPositionElementBuffer[i];
            CullPassPointsComponent CullPassPointsParams = cullPassPointsParamsBuffer[i];
            BallParamsComponent BallParams = BallParamsComponentBuffer[i];
            TestResultComponent TestResult = TestResultBuffer[i];
            if (PlayerPositions.Length == 0) continue;

            int attackIndexStart = CullPassPointsParams.teamA_IsAttacker ? 0 : CullPassPointsParams.teamASize;
            int attackIndexEnd = CullPassPointsParams.teamA_IsAttacker ? CullPassPointsParams.teamASize : CullPassPointsParams.teamASize + CullPassPointsParams.teamBSize;
            int defenseIndexStart = CullPassPointsParams.teamA_IsAttacker ? CullPassPointsParams.teamASize : 0;
            int defenseIndexEnd = CullPassPointsParams.teamA_IsAttacker ? CullPassPointsParams.teamASize + CullPassPointsParams.teamBSize : CullPassPointsParams.teamBSize;
            float minDistancePlayer_Ball;
            int playerIndexStraightPass;
            PathDataDOTS PathDataDOTS = new PathDataDOTS(0, PathType.InGround, 0, Vector3.zero, Vector3.zero, Vector3.zero, BallParams.k, BallParams.mass, BallParams.groundY, BallParams.bounciness, BallParams.friction, BallParams.dynamicFriction, BallParams.ballRadio, BallParams.g);
            GetV0DOTSResult getV0DOTSResult = new GetV0DOTSResult();
            float defenseReachTimeResult;
            float vf = BallParams.g / BallParams.k;
            for (int j = 0; j < lonelyPoints.Length; j++)
            {
                LonelyPointElement lonelyPoint = lonelyPoints[j];
                Vector3 lonelyPosition = new Vector3(lonelyPoint.position.x, 0, lonelyPoint.position.y);
                int attackIndex=-1;
                float reachTime = GetTimeToReachPosition(ref PlayerPositions, ref PlayerGenericParams, attackIndexStart, attackIndexEnd, lonelyPosition,ref attackIndex);
                PathDataDOTS.Pos0 = BallParams.BallPosition;
                PathDataDOTS.Posf = lonelyPosition;
                getV0DOTSResult.ballReachTargetPositionTime = reachTime;
                getV0DOTSResult.receiverReachTargetPositionTime = reachTime;
                StraightXZDragAndFrictionPathDOTS2.getV0(ref PathDataDOTS, ref getV0DOTSResult, PlayerGenericParams.maxKickForce, ref VOParams, reachTime);
                PathDataDOTS.V0 = getV0DOTSResult.v0;
                PathDataDOTS.v0Magnitude = getV0DOTSResult.v0Magnitude;
                PathDataDOTS.normalizedV0 = getV0DOTSResult.v0.normalized;
                getMinReachDistance_StraightPass(ref PlayerPositions, ref PlayerGenericParams, defenseIndexStart, defenseIndexEnd, lonelyPosition, BallParams.BallPosition, ref PathDataDOTS, out minDistancePlayer_Ball, out playerIndexStraightPass,out defenseReachTimeResult,ref TestResult);
                //Debug.Log(defenseReachTimeResult);
                TestResult.GetV0DOTSResult1 = getV0DOTSResult;
                TestResult.defenseLonelyPointReachTime = defenseReachTimeResult;
                TestResult.ballReachTargetPositionTime = getV0DOTSResult.ballReachTargetPositionTime;
                TestResult.defenseLonelyPointReachIndex = playerIndexStraightPass;
                TestResult.attackLonelyPointReachIndex = attackIndex;
                TestResult.attackReachTime = reachTime;
                TestResult.lonelyPosition = lonelyPosition;
                if (minDistancePlayer_Ball > 0 || true)
                {
                    if(defenseReachTimeResult > getV0DOTSResult.ballReachTargetPositionTime || true)
                    {
                        getParabolicPass_isPosible(ref PlayerPositions, playerIndexStraightPass, ref PlayerGenericParams, lonelyPosition, BallParams.BallPosition, reachTime, defenseReachTimeResult, BallParams.k,vf, ref VOParams, PlayerGenericParams.maxKickForce, ref TestResult);
                    }
                }
                else
                {
                    //Debug.Log("aaaaa ballReachTargetPositionTime=" + getV0DOTSResult.ballReachTargetPositionTime + " defenseReachTime=" + defenseReachTimeResult);
                }
                TestResultBuffer[i] = TestResult;


            }


        }
    }
    void getParabolicPass_isPosible(ref DynamicBuffer<PlayerPositionElement> PlayerPositions,int playerIndexStraightPass, ref PlayerGenericParams PlayerGenericParams, Vector3 lonelyPosition, Vector3 ballPosition, float attackReachTime, float defenseReachTime, float k,float vf, ref GetStraightV0Params VOParams,float maxKickForce, ref TestResultComponent TestResult)
    {

        GetV0DOTSResult getV0DOTSResult = new GetV0DOTSResult();
        //StraightXZDragPathDOTS.getXZV0(ref getV0DOTSResult, attackReachTime, ballPosition, lonelyPosition, PlayerGenericParams.maxKickForce, ref VOParams, k);
        ParabolicPassDOTS.getV0(ballPosition,lonelyPosition,ref getV0DOTSResult, maxKickForce, VOParams.maxControlSpeed, VOParams.maxControlSpeedLerpDistance, defenseReachTime, k, vf);
        float timeDiference = defenseReachTime - getV0DOTSResult.ballReachTargetPositionTime;
        TestResult.GetV0DOTSResult2 = getV0DOTSResult;
    }
    void getMinReachDistance_StraightPass(ref DynamicBuffer<PlayerPositionElement> PlayerPositions, ref PlayerGenericParams PlayerGenericParams, int indexStart, int indexEnd, Vector3 lonelyPosition, Vector3 ballPosition, ref PathDataDOTS PathDataDOTS, out float distanceResult, out int playerLonelyIndexResult, out float playerReachLonelyTimeResult,ref TestResultComponent TestResult)
    {
        Vector2 playerPosition2;
        Vector3 playerPosition;
        Vector3 closestPoint;
        Vector3 ballPositionAtReachTime = Vector3.zero;
        distanceResult = -Mathf.Infinity;
        playerLonelyIndexResult = -1;
        playerReachLonelyTimeResult = Mathf.Infinity;
        for (int i = indexStart; i < indexEnd; i++)
        {
            playerPosition2 = PlayerPositions[i].position;
            playerPosition = new Vector3(playerPosition2.x, 0, playerPosition2.y);
            closestPoint = MyFunctions.GetClosestPointOnFiniteLine(playerPosition, ballPosition, lonelyPosition);
            float maxSpeed = i == indexStart ? PlayerGenericParams.goalkeeperMaxSpeed : PlayerGenericParams.maxSpeed;

            if (Vector3.Distance(ballPosition, closestPoint) < 2)
            {
                closestPoint = lonelyPosition;
            }
            float playerReachTime, playerReachTime2 ;
            float PlayerReachDistance = getPlayerReachDistance_StraightPass(playerPosition, closestPoint, maxSpeed, ballPosition, lonelyPosition, ref PathDataDOTS, ref ballPositionAtReachTime, out playerReachTime);
            TestResult.defenseClosestReachTime = playerReachTime;
            float PlayerReachDistance2;

            float distanceClosest_LonelyPosition = Vector3.Distance(closestPoint, lonelyPosition);
            if (PlayerReachDistance > distanceResult)
            {

                distanceResult = PlayerReachDistance;
                TestResult.closestDistanceDefenseBall = PlayerReachDistance;
                TestResult.closestPosition = closestPoint;
            }
            if (distanceClosest_LonelyPosition < 0.5f)
            {
                if (playerReachTime < playerReachLonelyTimeResult)
                {
                    playerReachLonelyTimeResult = playerReachTime;
                    playerLonelyIndexResult = i;
                    TestResult.closestPosition = closestPoint;
                }
                
            }
            else
            {
                //closestPoint = lonelyPosition;
                PlayerReachDistance2 = getPlayerReachDistance_StraightPass(playerPosition, lonelyPosition, maxSpeed, ballPosition, lonelyPosition, ref PathDataDOTS, ref ballPositionAtReachTime, out playerReachTime2);
                if(PlayerReachDistance2 > distanceResult)
                {
                    distanceResult = PlayerReachDistance2;
                    TestResult.closestDistanceDefenseBall = PlayerReachDistance;
                    TestResult.closestPosition = closestPoint;
                }
                if (playerReachTime2 < playerReachLonelyTimeResult)
                {
                    playerReachLonelyTimeResult = playerReachTime2;
                    playerLonelyIndexResult = i;
                }
            }
            if (i != indexStart)
            {
                //Debug.Log(Vector3.Distance(closestPoint, lonelyPosition)+ " PlayerReachDistance="+ PlayerReachDistance+" distance2="+ PlayerReachDistance2);
            }
        }
    }
    float getPlayerReachDistance_StraightPass(Vector3 playerPosition, Vector3 closestPoint, float maxSpeed, Vector3 ballPosition, Vector3 lonelyPosition, ref PathDataDOTS PathDataDOTS, ref Vector3 ballPositionAtReachTime, out float playerReachTimeResult)
    {
        playerReachTimeResult = GetTimeToReachPosition(playerPosition, closestPoint, maxSpeed);

        StraightXZDragAndFrictionPathDOTS2.getPositionAtTime(playerReachTimeResult, ref PathDataDOTS, ref ballPositionAtReachTime);

        Vector3 dir1 = ballPosition - closestPoint;
        Vector3 dir2 = ballPositionAtReachTime - closestPoint;
        float sign = Mathf.Sign(Vector3.Dot(dir1, dir2));
        float distancePlayer_Ball = sign * Vector3.Distance(closestPoint, ballPositionAtReachTime);

        return distancePlayer_Ball;
    }
    PathDataDOTS getPathDataDOTS(ref BallParamsComponent BallParams, Vector3 Pos0, Vector3 Posf, Vector3 V0)
    {
        PathDataDOTS pathDataDOTS = new PathDataDOTS(0, PathType.InGround, 0, Pos0, Posf, V0, BallParams.k, BallParams.mass, BallParams.groundY, BallParams.bounciness, BallParams.friction, BallParams.dynamicFriction, BallParams.ballRadio, BallParams.g);
        return pathDataDOTS;

    }
    float GetTimeToReachPosition(ref DynamicBuffer<PlayerPositionElement> PlayerPositions, ref PlayerGenericParams PlayerGenericParams, int indexStart, int indexEnd, Vector3 targetPosition,ref int attackIndex)
    {
        float reachTimeEnd = Mathf.Infinity;
        for (int i = indexStart; i < indexEnd; i++)
        {
            float maxSpeed = i == indexStart ? PlayerGenericParams.goalkeeperMaxSpeed : PlayerGenericParams.maxSpeed;
            Vector3 playerPosition = new Vector3(PlayerPositions[i].position.x, 0, PlayerPositions[i].position.y);

            float reachTime = GetTimeToReachPosition(playerPosition, targetPosition, maxSpeed);
            if (reachTime < reachTimeEnd)
            {
                reachTimeEnd = reachTime;
                attackIndex = i;
            }
        }
        return reachTimeEnd;
    }
   
    GetStraightV0Params GetV0Params()
    {
        GetStraightV0Params getV0Params = new GetStraightV0Params();
        getV0Params.maxAttempts = 20;
        getV0Params.maxControlSpeed = 15;
        getV0Params.accuracy = 0.1f;
        getV0Params.maxControlSpeedLerpDistance = 5f;
        getV0Params.searchVyIncrement = 0.5f;
        return getV0Params;
    }
    PlayerGenericParams GetPlayerGenericParams()
    {
        PlayerGenericParams PlayerGenericParams = new PlayerGenericParams();
        PlayerGenericParams.maxSpeed = 10.5f;
        PlayerGenericParams.goalkeeperMaxSpeed = 4.5f;
        PlayerGenericParams.maxKickForce = 33f;
        return PlayerGenericParams;
    }
    float GetTimeToReachPosition(Vector3 playerPosition, Vector3 closestPoint, float maxSpeed)
    {
        return GetTimeToReachPointDOTS.linearGetTimeToReachPosition(playerPosition, closestPoint, maxSpeed);
    }
}
