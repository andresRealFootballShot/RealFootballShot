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
using System;
[BurstCompile]
public struct CullPassPointsJob : IJobEntityBatch
{
    public BufferTypeHandle<LonelyPointElement2> lonelyPointsHandle;
    [ReadOnly] public BufferTypeHandle<PlayerPositionElement> playerPositionElementHandle;
    [ReadOnly] public ComponentTypeHandle<CullPassPointsComponent> cullPassPointsParamsHandle;
    [ReadOnly] public ComponentTypeHandle<BallParamsComponent> BallParamsComponentHandle;
    public ComponentTypeHandle<TestResultComponent> TestResultComponentHandle;
    public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
    {

        BufferAccessor<LonelyPointElement2> lonelyPointsBuffer = batchInChunk.GetBufferAccessor(lonelyPointsHandle);
        BufferAccessor<PlayerPositionElement> playerPositionElementBuffer = batchInChunk.GetBufferAccessor(playerPositionElementHandle);
        NativeArray<CullPassPointsComponent> cullPassPointsParamsBuffer = batchInChunk.GetNativeArray(cullPassPointsParamsHandle);
        NativeArray<BallParamsComponent> BallParamsComponentBuffer = batchInChunk.GetNativeArray(BallParamsComponentHandle);
        NativeArray<TestResultComponent> TestResultBuffer = batchInChunk.GetNativeArray(TestResultComponentHandle);
        GetStraightV0Params VOParams = GetV0Params();
        PlayerGenericParams PlayerGenericParams = GetPlayerGenericParams();

        for (int i = 0; i < lonelyPointsBuffer.Length; i++)
        {

            DynamicBuffer<LonelyPointElement2> lonelyPoints = lonelyPointsBuffer[i];
            DynamicBuffer<PlayerPositionElement> PlayerPositions = playerPositionElementBuffer[i];
            CullPassPointsComponent CullPassPointsParams = cullPassPointsParamsBuffer[i];
            BallParamsComponent BallParams = BallParamsComponentBuffer[i];
            TestResultComponent TestResult = TestResultBuffer[i];
            if (PlayerPositions.Length == 0) continue;
            int attackIndexStart = CullPassPointsParams.teamA_IsAttacker ? 0 : CullPassPointsParams.teamASize;
            int attackIndexEnd = CullPassPointsParams.teamA_IsAttacker ? CullPassPointsParams.teamASize : CullPassPointsParams.teamASize + CullPassPointsParams.teamBSize;
            int defenseIndexStart = CullPassPointsParams.teamA_IsAttacker ? CullPassPointsParams.teamASize : 0;
            int defenseIndexEnd = CullPassPointsParams.teamA_IsAttacker ? CullPassPointsParams.teamASize + CullPassPointsParams.teamBSize : CullPassPointsParams.teamASize;
            float minDistancePlayer_Ball;
            int playerIndexStraightPass;
            PathDataDOTS PathDataDOTS = new PathDataDOTS(0, PathType.InGround, 0, Vector3.zero, Vector3.zero, Vector3.zero, BallParams.k, BallParams.mass, BallParams.groundY, BallParams.bounciness, BallParams.friction, BallParams.dynamicFriction, BallParams.ballRadio, BallParams.g);
            GetV0DOTSResult getV0DOTSResult = new GetV0DOTSResult();
            float defenseReachTimeResult;
            float vf = BallParams.g / BallParams.k;
            for (int j = 0; j < CullPassPointsParams.sizeLonelyPoints; j++)
            {
                LonelyPointElement2 lonelyPoint = lonelyPoints[j];
                Vector3 lonelyPosition = new Vector3(lonelyPoint.position.x, BallParams.BallPosition.y, lonelyPoint.position.y);
                int attackIndex = -1;
                float reachTime = GetTimeToReachPosition(ref PlayerPositions, ref PlayerGenericParams, attackIndexStart, attackIndexEnd, lonelyPosition, ref attackIndex);
                PathDataDOTS.Pos0 = BallParams.BallPosition;
                PathDataDOTS.Posf = lonelyPosition;
                getV0DOTSResult.ballReachTargetPositionTime = reachTime;
                getV0DOTSResult.receiverReachTargetPositionTime = reachTime;
                StraightXZDragAndFrictionPathDOTS2.getV0(ref PathDataDOTS, ref getV0DOTSResult, PlayerGenericParams.maxKickForce, ref VOParams, reachTime);
                PathDataDOTS.V0 = getV0DOTSResult.v0;
                PathDataDOTS.v0Magnitude = getV0DOTSResult.v0Magnitude;
                PathDataDOTS.normalizedV0 = getV0DOTSResult.v0.normalized;
                getMinReachDistance_StraightPass(true,ref PlayerPositions, ref PlayerGenericParams, defenseIndexStart, defenseIndexEnd, lonelyPosition, BallParams.BallPosition, ref PathDataDOTS, out minDistancePlayer_Ball, out playerIndexStraightPass, out defenseReachTimeResult, ref TestResult);
                //Debug.Log(defenseReachTimeResult);
                TestResult.GetV0DOTSResult1 = getV0DOTSResult;
                TestResult.defenseLonelyPointReachTime = defenseReachTimeResult;
                TestResult.defenseLonelyPointReachIndex = playerIndexStraightPass;
                TestResult.attackLonelyPointReachIndex = attackIndex;
                TestResult.attackReachTime = reachTime;
                TestResult.lonelyPosition = lonelyPosition;
                TestResult.parabolicReachBall = true;
                TestResult.straightReachBall = true;
                lonelyPoint.parabolicReachBall = true;
                lonelyPoint.straightReachBall = true;
                if (minDistancePlayer_Ball > 0)
                {
                    TestResult.straightReachBall = false;
                    lonelyPoint.straightReachBall = false;
                    bool parabolicReachBall = getParabolicPass_isPosible(ref PlayerPositions, defenseIndexStart, defenseIndexEnd, playerIndexStraightPass, ref PlayerGenericParams, lonelyPosition, BallParams.BallPosition, reachTime, defenseReachTimeResult, BallParams.k, vf, ref VOParams, PlayerGenericParams.maxKickForce,ref PathDataDOTS, ref TestResult);
                    TestResult.parabolicReachBall = parabolicReachBall;
                    lonelyPoint.parabolicReachBall = parabolicReachBall;
                    if (!parabolicReachBall)
                    {
                        //TestResult.defenseReachPosition = lonelyPosition;
                    }
                }
                else
                {
                    //Debug.Log("aaaaa ballReachTargetPositionTime=" + getV0DOTSResult.ballReachTargetPositionTime + " defenseReachTime=" + defenseReachTimeResult);
                }
                calculateWeight(ref lonelyPoint, ref CullPassPointsParams);
                lonelyPoints[j] = lonelyPoint;

            }
            TestResultBuffer[i] = TestResult;

        }
    }
    void calculateWeight(ref LonelyPointElement2 lonelyPoint, ref CullPassPointsComponent CullPassPointsParams)
    {
        if (!lonelyPoint.straightReachBall && !lonelyPoint.parabolicReachBall)
        {
            lonelyPoint.weight = Mathf.Infinity;
            lonelyPoint.order =-1;
            return;
        }
        Vector2 dir1 = CullPassPointsParams.post1Position - lonelyPoint.position;
        Vector2 dir2 = CullPassPointsParams.post2Position - lonelyPoint.position;
        float angle = Vector2.Angle(dir1, dir2);
        angle = 1- (angle / 180);

        Vector2 closestGoalPosition = MyFunctions.GetClosestPointOnFiniteLine(lonelyPoint.position, CullPassPointsParams.post1Position, CullPassPointsParams.post2Position);
        float d = Vector2.Distance(CullPassPointsParams.post1Position, CullPassPointsParams.post2Position);
        Vector2 dir3 = CullPassPointsParams.post1Position - CullPassPointsParams.post2Position;
        dir3.Normalize();
        Vector2 center = CullPassPointsParams.post2Position + dir3 * (d/2);

        float distance = Vector2.Distance(closestGoalPosition, lonelyPoint.position);
        distance = distance / CullPassPointsParams.distanceWeightLerp;
        float a = 1f;
        float weight = (angle+ distance*a)/(1+a);
        lonelyPoint.weight = weight;
    }
    bool getParabolicPass_isPosible(ref DynamicBuffer<PlayerPositionElement> PlayerPositions, int startIndex, int endIndex, int playerIndexStraightPass, ref PlayerGenericParams PlayerGenericParams, Vector3 lonelyPosition, Vector3 ballPosition, float attackReachTime, float defenseReachTime, float k, float vf, ref GetStraightV0Params VOParams, float maxKickForce,ref PathDataDOTS PathDataDOTS, ref TestResultComponent TestResult)
    {

        GetV0DOTSResult getV0DOTSResult = new GetV0DOTSResult();
        //StraightXZDragPathDOTS.getXZV0(ref getV0DOTSResult, attackReachTime, ballPosition, lonelyPosition, PlayerGenericParams.maxKickForce, ref VOParams, k);
        Vector3 controlLonelyPosition = lonelyPosition;
        controlLonelyPosition.y = PlayerGenericParams.heightBallControl;
        ParabolicPassDOTS.getV0(ballPosition, controlLonelyPosition, ref getV0DOTSResult, maxKickForce, VOParams.maxControlSpeed, VOParams.maxControlSpeedLerpDistance, attackReachTime, k, vf);
        float timeDiference = defenseReachTime - getV0DOTSResult.ballReachTargetPositionTime;
        TestResult.defenseParabolicDifferenceTime = timeDiference;
        TestResult.GetV0DOTSResult2 = getV0DOTSResult;
        Vector3 V0 = getV0DOTSResult.v0;
        PathDataDOTS.V0 = getV0DOTSResult.v0;
        PathDataDOTS.v0Magnitude = getV0DOTSResult.v0Magnitude;
        float t1, t2;
        if (!ParabolicWithDragDOTS.timeToReachHeightParabolicNoDrag(PlayerGenericParams.heightJump, 9.8f, V0.y, ballPosition.y, out t1, out t2))
        {
            return false;
        }

        for (int i = startIndex; i < endIndex; i++)     {
            PlayerPositionElement playerPositionElement = PlayerPositions[i];
            Vector3 playerPosition2 = PlayerPositions[i].position;
            Vector3 playerPosition = new Vector3(playerPosition2.x, 0, playerPosition2.y);
            float playerReachTime;
            Vector3 ballPositionAtReachTime = Vector3.zero;
            Vector3 closestPoint = MyFunctions.GetClosestPointOnFiniteLine(playerPosition, ballPosition, lonelyPosition);

            float maxSpeed = i == startIndex ? PlayerGenericParams.goalkeeperMaxSpeed : PlayerGenericParams.maxSpeed;
            if (t1 > 0 && t2 > 0)
            {

                Vector3 posReachPlayerHeightJump1 = StraightXZDragPathDOTS.getPositionAtTime(ballPosition, V0, k, t1);
                Vector3 posReachPlayerHeightJump2 = StraightXZDragPathDOTS.getPositionAtTime(ballPosition, V0, k, t2);
                float d1 = Vector3.Distance(closestPoint, ballPosition);
                float d2 = Vector3.Distance(posReachPlayerHeightJump1, ballPosition);
                float d3 = Vector3.Distance(posReachPlayerHeightJump2, ballPosition);
                if (d1 > d2 && d1 < d3)
                {
                    float PlayerReachDistance = getPlayerReachDistance_StraightPass(false, playerPosition, posReachPlayerHeightJump1, ballPosition, maxSpeed, ref PathDataDOTS, ref ballPositionAtReachTime, ref PlayerGenericParams,ref playerPositionElement, out playerReachTime);
                    if (PlayerReachDistance > 0)
                    {
                        return false;
                    }
                    PlayerReachDistance = getPlayerReachDistance_StraightPass(false, playerPosition, posReachPlayerHeightJump2, ballPosition, maxSpeed, ref PathDataDOTS, ref ballPositionAtReachTime, ref PlayerGenericParams,ref playerPositionElement, out playerReachTime);
                    if (PlayerReachDistance > 0)
                    {
                        return false;
                    }
                }
                else
                {
                    float PlayerReachDistance = getPlayerReachDistance_StraightPass(false, playerPosition, closestPoint, ballPosition, maxSpeed, ref PathDataDOTS, ref ballPositionAtReachTime, ref PlayerGenericParams,ref playerPositionElement, out playerReachTime);
                    if (PlayerReachDistance > 0)
                    {
                        return false;
                    }
                }
                float PlayerReachDistance2 = getPlayerReachDistance_StraightPass(false, playerPosition, lonelyPosition, ballPosition, maxSpeed, ref PathDataDOTS, ref ballPositionAtReachTime, ref PlayerGenericParams,ref playerPositionElement, out playerReachTime);
                if (PlayerReachDistance2 > 0)
                {
                    return false;
                }
            }
        }
        return true;

    }
    
    void getMinReachDistance_StraightPass(bool isStrightPass,ref DynamicBuffer<PlayerPositionElement> PlayerPositions, ref PlayerGenericParams PlayerGenericParams, int indexStart, int indexEnd, Vector3 lonelyPosition, Vector3 ballPosition, ref PathDataDOTS PathDataDOTS, out float distanceResult, out int playerLonelyIndexResult, out float playerReachLonelyTimeResult, ref TestResultComponent TestResult)
    {
        Vector2 playerPosition2;
        Vector3 playerPosition;
        Vector3 closestPoint;
        Vector3 ballPositionAtReachTime = Vector3.zero;
        distanceResult = -Mathf.Infinity;
        playerLonelyIndexResult = -1;
        playerReachLonelyTimeResult = Mathf.Infinity;
        Vector3 dir1 = lonelyPosition - ballPosition;
        ballPosition.y = 0;
        for (int i = indexStart; i < indexEnd; i++)
        {
            PlayerPositionElement playerPositionElement = PlayerPositions[i];
            playerPosition2 = playerPositionElement.position;
            playerPosition = new Vector3(playerPosition2.x, 0, playerPosition2.y);
            closestPoint = MyFunctions.GetClosestPointOnFiniteLine(playerPosition, ballPosition, lonelyPosition);
            float maxSpeed = i == indexStart ? PlayerGenericParams.goalkeeperMaxSpeed : PlayerGenericParams.maxSpeed;
            Vector3 dir2 = playerPosition - ballPosition;
            float angle = Vector3.Angle(dir1, dir2);
            float playerReachTime, playerReachTime2;
            if (angle >= 90)
            {
                closestPoint = lonelyPosition;
            }
            else
            {
                float PlayerReachDistance = getPlayerReachDistance_StraightPass(isStrightPass, playerPosition, closestPoint, ballPosition,maxSpeed, ref PathDataDOTS, ref ballPositionAtReachTime, ref PlayerGenericParams,ref playerPositionElement, out playerReachTime);
                if (PlayerReachDistance > distanceResult)
                {
                    distanceResult = PlayerReachDistance;
                    TestResult.closestDistanceDefenseBall = PlayerReachDistance;
                    TestResult.defenseClosestReachTime = playerReachTime;
                    TestResult.defenseReachPosition = closestPoint;
                }
            }
            

            //playerPositionElement.getTimeReach = aux ? playerReachTime : -1;
            //PlayerPositions[i] = playerPositionElement;
            
            float PlayerReachDistance2;

            float distanceClosest_LonelyPosition = Vector3.Distance(closestPoint, lonelyPosition);
            
            PlayerReachDistance2 = getPlayerReachDistance_StraightPass(isStrightPass, playerPosition, lonelyPosition, ballPosition, maxSpeed, ref PathDataDOTS, ref ballPositionAtReachTime, ref PlayerGenericParams,ref playerPositionElement, out playerReachTime2);
            if (playerReachTime2 < playerReachLonelyTimeResult)
            {
                playerReachLonelyTimeResult = playerReachTime2;
                playerLonelyIndexResult = i;

                TestResult.closestPosition = closestPoint;
                TestResult.closestDistanceDefenseBall = PlayerReachDistance2;
                TestResult.defenseClosestReachTime = playerReachTime2;
            }
            if (PlayerReachDistance2 > distanceResult)
            {
                distanceResult = PlayerReachDistance2;

                TestResult.defenseReachPosition = lonelyPosition;
            }
        }
    }
    float getPlayerReachDistance_StraightPass(bool isStraightPass, Vector3 playerPosition, Vector3 closestPoint,Vector3 ballPosition,float maxSpeed, ref PathDataDOTS PathDataDOTS, ref Vector3 ballPositionAtReachTime, ref PlayerGenericParams PlayerGenericParams,ref PlayerPositionElement playerPositionElement, out float playerReachTimeResult)
    {
        playerReachTimeResult = GetTimeToReachPosition(ref playerPositionElement, closestPoint,maxSpeed, ref PlayerGenericParams);

        if (isStraightPass)
        {
            StraightXZDragAndFrictionPathDOTS2.getPositionAtTime(playerReachTimeResult, ref PathDataDOTS, ref ballPositionAtReachTime);
        }
        else
        {
            ballPositionAtReachTime = StraightXZDragPathDOTS.getPositionAtTime(ballPosition, PathDataDOTS.V0, PathDataDOTS.k, playerReachTimeResult);
        }
        float d1 = Vector3.Distance(ballPosition, closestPoint);
        float d2 = Vector3.Distance(ballPosition, ballPositionAtReachTime);
        int sign = d1 <= d2 ? -1 : 1;
        float distancePlayer_Ball = sign * Vector3.Distance(closestPoint, ballPositionAtReachTime);

        return distancePlayer_Ball;
    }
    PathDataDOTS getPathDataDOTS(ref BallParamsComponent BallParams, Vector3 Pos0, Vector3 Posf, Vector3 V0)
    {
        PathDataDOTS pathDataDOTS = new PathDataDOTS(0, PathType.InGround, 0, Pos0, Posf, V0, BallParams.k, BallParams.mass, BallParams.groundY, BallParams.bounciness, BallParams.friction, BallParams.dynamicFriction, BallParams.ballRadio, BallParams.g);
        return pathDataDOTS;

    }
    float GetTimeToReachPosition(ref DynamicBuffer<PlayerPositionElement> PlayerPositions, ref PlayerGenericParams PlayerGenericParams, int indexStart, int indexEnd, Vector3 targetPosition, ref int attackIndex)
    {
        float reachTimeEnd = Mathf.Infinity;
        for (int i = indexStart; i < indexEnd; i++)
        {
            PlayerPositionElement playerPositionElement = PlayerPositions[i];
            Vector3 playerPosition = new Vector3(PlayerPositions[i].position.x, 0, PlayerPositions[i].position.y);

            float maxSpeed = i == indexStart ? PlayerGenericParams.goalkeeperMaxSpeed : PlayerGenericParams.maxSpeed;
            float reachTime = GetTimeToReachPosition(ref playerPositionElement, targetPosition,maxSpeed, ref PlayerGenericParams);
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
        PlayerGenericParams.goalkeeperMaxSpeed = 5f;
        PlayerGenericParams.maxKickForce = 33f;
        PlayerGenericParams.heightJump = 2.2f;
        PlayerGenericParams.heightBallControl = 1.4f;
        PlayerGenericParams.scope = 0.75f;
        PlayerGenericParams.acceleration = 13.78125f;
        PlayerGenericParams.decceleration = 55.125f;
        PlayerGenericParams.minSpeedForRotate = 2;
        PlayerGenericParams.maxAngleForRun = 95;
        PlayerGenericParams.maxSpeedRotation = 800;
        PlayerGenericParams.maxSpeedForReachBall =7;
        return PlayerGenericParams;
    }
    float GetTimeToReachPosition(ref PlayerPositionElement playerPositionElement, Vector3 closestPoint,float maxSpeed,ref  PlayerGenericParams PlayerGenericParams)
    {
        //return GetTimeToReachPointDOTS.linearGetTimeToReachPosition(playerPositionElement.position, closestPoint, maxSpeed, PlayerGenericParams.scope);
        Vector3 position = new Vector3(playerPositionElement.position.x,0, playerPositionElement.position.y);
        Vector3 normalizedPosition = new Vector3(playerPositionElement.normalizedVelocity.x,0, playerPositionElement.normalizedVelocity.y);
        Vector3 bodyForward = new Vector3(playerPositionElement.bodyForward.x,0, playerPositionElement.bodyForward.y);
        return GetTimeToReachPointDOTS.accelerationGetTimeToReachPosition(position, playerPositionElement.currentSpeed, bodyForward, normalizedPosition, ref PlayerGenericParams, closestPoint);

    }
}
