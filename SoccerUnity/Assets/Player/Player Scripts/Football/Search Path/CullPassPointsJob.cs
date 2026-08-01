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
using static CullPassPoints;
using static FieldTriangleSpace.FieldOfTrianglesCreator;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Unity.Mathematics;
//[BurstCompile]
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
            float straightMinDistancePlayer_Ball, parabolicMinDistancePlayer_Ball;
            int defenseReachIndex;
            
            
            PathDataDOTS PathDataDOTS = new PathDataDOTS(0, PathType.InGround, 0, Vector3.zero, Vector3.zero, Vector3.zero, BallParams.k, BallParams.mass, BallParams.groundY, BallParams.bounciness, BallParams.friction, BallParams.dynamicFriction, BallParams.ballRadio, BallParams.g);
            GetV0DOTSResult getV0DOTSResult = new GetV0DOTSResult();
            float defenseStraightReachTimeResult;
            float vf = BallParams.g / BallParams.k;
            Vector2 ballPosition = new Vector2(BallParams.BallPosition.x, BallParams.BallPosition.z);
            
            float maxFieldDistance = Vector2.Distance(CullPassPointsParams.defenseGoalPosition, CullPassPointsParams.midfield)*2;
           
            for (int j = 0; j < CullPassPointsParams.sizeLonelyPoints; j++)
            {
                float t0 = BallParams.t0;
                LonelyPointElement2 lonelyPoint = lonelyPoints[j];
                Vector3 lonelyPosition = new Vector3(lonelyPoint.position.x, BallParams.BallPosition.y, lonelyPoint.position.y);
                int attackIndex = -1;
                Vector2 offsidePos = GetOffsideLine(ballPosition, CullPassPointsParams.defenseGoalPosition, CullPassPointsParams.midfield, defenseIndexStart, defenseIndexEnd, ref PlayerPositions);
                float reachTime2 = GetTimeToReachPosition(ref PlayerPositions, ref PlayerGenericParams, attackIndexStart , attackIndexEnd, lonelyPosition, ref attackIndex,offsidePos, CullPassPointsParams.defenseGoalPosition,t0,CullPassPointsParams.passerIndex,ballPosition,lonelyPosition, lonelyPoint);
                
                float reachTime = attackIndex==CullPassPointsParams.passerIndex ? reachTime2 : Mathf.Max(reachTime2-t0,0);
                if (attackIndex != -1)
                {
                    reachTime += PlayerPositions[attackIndex].timePrecision;
                }
                lonelyPoint.attackReachIndex = attackIndex;
                lonelyPoint.attackReachTime = reachTime2;
                PathDataDOTS.Pos0 = BallParams.BallPosition;
                PathDataDOTS.Posf = lonelyPosition;
                getV0DOTSResult.ballReachTargetPositionTime = reachTime;
                getV0DOTSResult.receiverReachTargetPositionTime = reachTime;
                StraightXZDragAndFrictionPathDOTS2.getV0(ref PathDataDOTS, ref getV0DOTSResult, PlayerGenericParams.maxKickForce, ref VOParams, reachTime);
                PathDataDOTS.V0 = getV0DOTSResult.v0;
                PathDataDOTS.v0Magnitude = getV0DOTSResult.v0Magnitude;
                PathDataDOTS.normalizedV0 = getV0DOTSResult.v0.normalized;
                lonelyPoint.straightPassData.ballReachTime = getV0DOTSResult.ballReachTargetPositionTime;
                
                getMinReachDistance_StraightPass(true,ref PlayerPositions, ref PlayerGenericParams, defenseIndexStart, defenseIndexEnd, lonelyPosition, BallParams.BallPosition, ref PathDataDOTS, out straightMinDistancePlayer_Ball, out defenseReachIndex, out defenseStraightReachTimeResult,0,out Vector3 straightDefenseReachPosition, lonelyPoint);
                //Debug.Log(defenseReachTimeResult);
                TestResult.GetV0DOTSResult1 = getV0DOTSResult;
                TestResult.defenseLonelyPointReachTime = defenseStraightReachTimeResult;
                TestResult.defenseLonelyPointReachIndex = defenseReachIndex;
                TestResult.attackLonelyPointReachIndex = attackIndex;
                TestResult.attackReachTime = reachTime;
                TestResult.lonelyPosition = lonelyPosition;
                TestResult.parabolicReachBall = true;
                TestResult.straightReachBall = true;
                lonelyPoint.straightPassData.defenseReachIndex = defenseReachIndex;
                lonelyPoint.straightPassData.defenseReachTime = defenseStraightReachTimeResult;
                lonelyPoint.straightPassData.defenseReachPosition = new Vector2(straightDefenseReachPosition.x, straightDefenseReachPosition.z);
                lonelyPoint.straightPassData.passVelocity = PathDataDOTS.V0;
                lonelyPoint.straightPassData.distanceDefenseReachBall = straightMinDistancePlayer_Ball;
                parabolicMinDistancePlayer_Ball = Mathf.NegativeInfinity;
                lonelyPoint.parabolicPassData.Clear();
                bool reachPlayerIsUser = lonelyPoint.attackReachIndex == CullPassPointsParams.userIndex;
                lonelyPoint.parabolicWeight = Mathf.NegativeInfinity;
                lonelyPoint.straightWeight = Mathf.NegativeInfinity;
                if (straightMinDistancePlayer_Ball > 0)
                {
                    TestResult.straightReachBall = false;
                    float ballReachTime;
                    bool parabolicReachBall = getParabolicPass_isPosible(ref PlayerPositions, defenseIndexStart, defenseIndexEnd, defenseReachIndex, ref PlayerGenericParams, lonelyPosition, BallParams.BallPosition, reachTime, defenseStraightReachTimeResult, BallParams.k, vf, ref VOParams, PlayerGenericParams.maxKickForce,ref PathDataDOTS,out parabolicMinDistancePlayer_Ball,out ballReachTime, ref TestResult,t0,out int defenseParabolicReachIndex,out float defenseParabolicReachTime,out Vector3 parabolicDefenseReachPosition,out Vector3 passVelocity, straightDefenseReachPosition,BallParams.ballVelocity);

                    TestResult.parabolicReachBall = parabolicReachBall;
                    lonelyPoint.parabolicPassData.passVelocity = passVelocity;
                    lonelyPoint.parabolicPassData.ballReachTime = ballReachTime;
                    lonelyPoint.parabolicPassData.defenseReachIndex = defenseParabolicReachIndex;
                    lonelyPoint.parabolicPassData.defenseReachTime = defenseParabolicReachTime;
                    lonelyPoint.parabolicPassData.distanceDefenseReachBall = parabolicMinDistancePlayer_Ball;
                    lonelyPoint.parabolicPassData.defenseReachPosition = new Vector2(parabolicDefenseReachPosition.x, parabolicDefenseReachPosition.z);
                    
                    float parabolicWeight = EvaluatePosition(lonelyPoint.position, CullPassPointsParams.post1Position, CullPassPointsParams.post2Position, ballPosition, parabolicMinDistancePlayer_Ball, maxFieldDistance, reachPlayerIsUser, CullPassPointsParams.isCorner);
                    lonelyPoint.parabolicWeight = parabolicWeight;
                }
                float straightWeight = EvaluatePosition(lonelyPoint.position, CullPassPointsParams.post1Position, CullPassPointsParams.post2Position, ballPosition, straightMinDistancePlayer_Ball, maxFieldDistance, reachPlayerIsUser, false);
                lonelyPoint.straightWeight = straightWeight;
                //calculateWeight(ref lonelyPoint, ref CullPassPointsParams, ballPosition,straightMinDistancePlayer_Ball, parabolicMinDistancePlayer_Ball);


                lonelyPoints[j] = lonelyPoint;

            }
            TestResultBuffer[i] = TestResult;

        }
    }
    Vector2 GetOffsideLine(Vector2 ballPosition,Vector2 teamGoalPos,Vector2 midfieldPos,int startDefense,int endDefense,ref DynamicBuffer<PlayerPositionElement> PlayerPositions)
    {
        ballPosition.x = teamGoalPos.x;
        midfieldPos.x = teamGoalPos.x;

        Vector2 forward = (teamGoalPos - midfieldPos).normalized;

        float max1 = float.MinValue; // defensa más cercano a portería
        float max2 = float.MinValue; // segundo más cercano

        // Buscar los dos defensas más retrasados
        for (int i = startDefense; i < endDefense; i++)
        {
            Vector2 playerPos = new Vector2(teamGoalPos.x, PlayerPositions[i].position.y);
            float projection = Vector2.Dot(forward, playerPos - midfieldPos);

            if (projection > max1)
            {
                max2 = max1;
                max1 = projection;
            }
            else if (projection > max2)
            {
                max2 = projection;
            }
        }

        // Si hay menos de 2 defensas
        if (max2 == float.MinValue)
            return midfieldPos;

        // Proyección del balón
        float ballProjection = Vector2.Dot(forward, ballPosition - midfieldPos);
        if (ballProjection <= 0f && max2 <= 0f)
        {
            return midfieldPos;
        }
        // La línea es el más cercano a portería entre:
        // - el balón
        // - el penúltimo defensa (max2)
        float finalProjection = Mathf.Max(ballProjection, max2);

        return midfieldPos + forward * finalProjection;
    }
    void calculateWeight(ref LonelyPointElement2 lonelyPoint, ref CullPassPointsComponent CullPassPointsParams,Vector2 ballPosition,float straightMinDistancePlayer_Ball,float parabolicMinDistancePlayer_Ball)
    {
        //Debug.Log(straightMinDistancePlayer_Ball + " " + parabolicMinDistancePlayer_Ball);
        if (!lonelyPoint.straightReachBall && !lonelyPoint.parabolicReachBall&&false)
        {
            lonelyPoint.straightWeight = Mathf.Infinity;
            lonelyPoint.order =-1;
            return;
        }
        Vector2 dir1 = CullPassPointsParams.post1Position - lonelyPoint.position;
        Vector2 dir2 = CullPassPointsParams.post2Position - lonelyPoint.position;
        float angle = Vector2.Angle(dir1, dir2);
        angle = 1- (angle / 90);

        Vector2 closestGoalPosition = MyFunctions.GetClosestPointOnFiniteLine(lonelyPoint.position, CullPassPointsParams.post1Position, CullPassPointsParams.post2Position);
        float d = Vector2.Distance(CullPassPointsParams.post1Position, CullPassPointsParams.post2Position);
        Vector2 dir3 = CullPassPointsParams.post1Position - CullPassPointsParams.post2Position;
        dir3.Normalize();
        Vector2 center = CullPassPointsParams.post2Position + dir3 * (d/2);

        float distance = Vector2.Distance(closestGoalPosition, lonelyPoint.position);
        float distance_ball_lonely= Vector2.Distance(ballPosition, lonelyPoint.position);
        distance = (distance + distance_ball_lonely) / CullPassPointsParams.distanceWeightLerp;
        float a = 1f;
        float weight = (angle+ distance*a)/(1+a);
        lonelyPoint.straightWeight = weight;
    }
    public static float EvaluatePosition(
        Vector2 lonelyPoint,
        Vector2 post1Position,
        Vector2 post2Position,
        Vector2 ballPosition,
        float MinDistanceRival_Ball,
        float maxFieldDistance,bool reachPlayerIsUser,bool isCorner
    )
    {
        // --- DISTANCIA A LA PORTERÍA ---
        // Calculamos el punto medio de la portería
        Vector2 goalCenter = (post1Position + post2Position) / 2f;
        float distanceToGoal = Vector2.Distance(lonelyPoint, goalCenter);
        // Normalizamos la distancia respecto al campo: más cerca = mejor
        float distanceWeight;
        if (!isCorner)
        {

            distanceWeight = 1f - Mathf.Clamp01(distanceToGoal / maxFieldDistance);
        }
        else
        {
            float d = Mathf.Clamp01(distanceToGoal / maxFieldDistance);
            d = Mathf.Lerp(1, d, (distanceToGoal-5) / 2);
            distanceWeight = 1f - d;
        }

        // --- ÁNGULO DE LA PORTERÍA ---
        // Vector desde la posición al primer y segundo poste
        Vector2 toPost1 = post1Position - lonelyPoint;
        Vector2 toPost2 = post2Position - lonelyPoint;
        // Ángulo entre los vectores al poste1 y al poste2 (en radianes)
        float angle = Vector2.Angle(toPost1, toPost2);
        // Normalizamos ángulo: máximo ángulo = 1, mínimo = 0
        float angleWeight = angle / 180f;

        float angle2 = Vector3.Angle(goalCenter - ballPosition, lonelyPoint - ballPosition);
        // --- DISTANCIA AL BALÓN ---
        float distanceToBall = Vector2.Distance(lonelyPoint, ballPosition);
        // Penalizamos posiciones muy lejos del balón, pero no demasiado cerca
        float ballWeight = Mathf.Clamp01(1f - Mathf.Clamp01(distanceToBall / 30));
        ballWeight = Mathf.Lerp(0, ballWeight, Mathf.Clamp01((distanceToBall-5) / 5));
        ballWeight = Mathf.Lerp(ballWeight,0,angle2/180);
        float radio = Mathf.Lerp(0.01f, 5, Mathf.Clamp01(distanceToBall / 30));
        radio = Mathf.Lerp(0.01f, radio, Mathf.Clamp01((distanceToBall - 5) / 5));
        float radioWeight = Mathf.Lerp(1,0,(radio+MinDistanceRival_Ball)/radio);
        float l = Mathf.Lerp(0.1f, 7, distanceToGoal / maxFieldDistance);
        float l2 = Mathf.Clamp(MinDistanceRival_Ball / l, -1, 1);
        float reachWeight =(1- l2)/2;
        // --- PRESIÓN DEL RIVAL ---
        // Si MinDistanceRival_Ball > 0 significa que el rival puede llegar antes: penalizamos

        
        // --- PESO FINAL ---
        // Combinamos los factores. Ajusta los coeficientes según tu necesidad.
        float isUserWeight = reachPlayerIsUser ? 1 : 0;
        float finalWeight =
            0.6f * distanceWeight +    // importancia de la distancia a portería
            0.3f * angleWeight +       // importancia de estar centrado
            0.3f * isUserWeight;
            //+ ballWeight * 0.2f + radioWeight * 0.1f
        ;

        if (MinDistanceRival_Ball > 0&&!isCorner)
        {
            finalWeight += reachWeight * 0.2f;
        }
        finalWeight = MinDistanceRival_Ball > 0 &&!isCorner ? finalWeight - 2f : finalWeight;
        // Clamp opcional para mantenerlo entre -1 y 1
        return Mathf.Clamp(finalWeight, -2f, 1f);
    }
    bool getParabolicPass_isPosible(ref DynamicBuffer<PlayerPositionElement> PlayerPositions, int startIndex, int endIndex, int defenseIndexStraightPass, ref PlayerGenericParams PlayerGenericParams, Vector3 lonelyPosition, Vector3 ballPosition, float attackReachTime, float defenseStraightReachTime, float k, float vf, ref GetStraightV0Params VOParams, float maxKickForce,ref PathDataDOTS PathDataDOTS,out float parabolicMinDistance_BallPlayer,out float ballReachTime, ref TestResultComponent TestResult, float t0,out int defenseParabolicReachIndex,out float defenseParabolicReachTime,out Vector3 defenseReachPosition,out Vector3 passVelocity,Vector3 straightDefenseReachPosition,Vector3 ballVelocity)
    {

        GetV0DOTSResult getV0DOTSResult = new GetV0DOTSResult();
        //StraightXZDragPathDOTS.getXZV0(ref getV0DOTSResult, attackReachTime, ballPosition, lonelyPosition, PlayerGenericParams.maxKickForce, ref VOParams, k);
        Vector3 controlLonelyPosition = lonelyPosition;
        controlLonelyPosition.y = PlayerGenericParams.heightBallControl;
        ParabolicPassDOTS.getV0(ballPosition, controlLonelyPosition, ref getV0DOTSResult, maxKickForce, VOParams.maxControlSpeed, VOParams.maxControlSpeedLerpDistance, attackReachTime, k, vf, ballVelocity);
        ballReachTime = getV0DOTSResult.ballReachTargetPositionTime;
        float timeDiference = defenseStraightReachTime - getV0DOTSResult.ballReachTargetPositionTime;
        TestResult.defenseParabolicDifferenceTime = timeDiference;
        TestResult.GetV0DOTSResult2 = getV0DOTSResult;
        Vector3 V0 = getV0DOTSResult.v0;
        passVelocity = V0;
        PathDataDOTS.V0 = getV0DOTSResult.v0;
        PathDataDOTS.v0Magnitude = getV0DOTSResult.v0Magnitude;
        parabolicMinDistance_BallPlayer = Mathf.NegativeInfinity;
        defenseParabolicReachIndex = -1;
        defenseParabolicReachTime = Mathf.Infinity;
        defenseReachPosition = Vector3.positiveInfinity;
        float t1, t2;
        if (!ParabolicWithDragDOTS.timeToReachHeightParabolicNoDrag(PlayerGenericParams.heightJump, 9.8f,V0.y, ballPosition.y, out t1, out t2))
        {

            PlayerPositionElement playerPositionElement = PlayerPositions[defenseIndexStraightPass];
            Vector3 playerPosition2 = PlayerPositions[defenseIndexStraightPass].position;
            Vector3 playerPosition = new Vector3(playerPosition2.x, 0, playerPosition2.y);
            float playerReachTime;
            Vector3 ballPositionAtReachTime = Vector3.zero;
            Vector3 closestPoint = MyFunctions.GetClosestPointOnFiniteLine(playerPosition, ballPosition, lonelyPosition);
            bool isGoalkeeper = defenseIndexStraightPass == startIndex;
            float maxSpeed = isGoalkeeper ? PlayerGenericParams.goalkeeperMaxSpeed : playerPositionElement.maxSpeed;
            Vector3 closestPoint2 = MyFunctions.GetClosestPointOnFiniteLine(playerPosition, ballPosition, lonelyPosition);
            parabolicMinDistance_BallPlayer = getPlayerReachDistance_StraightPass2(false, playerPosition, straightDefenseReachPosition, ballPosition, maxSpeed, ref PathDataDOTS, ref ballPositionAtReachTime, ref PlayerGenericParams, ref playerPositionElement, out playerReachTime,0, isGoalkeeper,lonelyPosition);
            defenseParabolicReachIndex = defenseIndexStraightPass;
            defenseParabolicReachTime = playerReachTime;
            defenseReachPosition = straightDefenseReachPosition;
            if (parabolicMinDistance_BallPlayer >= 0)
                return false;
            else
                return true;

        }
        bool result = true; ;
        for (int i = startIndex; i < endIndex; i++)     {
            PlayerPositionElement playerPositionElement = PlayerPositions[i];
            Vector3 playerPosition2 = PlayerPositions[i].position;
            Vector3 playerPosition = new Vector3(playerPosition2.x, 0, playerPosition2.y);
            float playerReachTime;
            Vector3 ballPositionAtReachTime = Vector3.zero;
            Vector3 closestPoint = MyFunctions.GetClosestPointOnFiniteLine(playerPosition, ballPosition, lonelyPosition);
            bool isGoalkeeper = i == startIndex;
            float maxSpeed = isGoalkeeper ? PlayerGenericParams.goalkeeperMaxSpeed : playerPositionElement.maxSpeed;
            if (t1 > 0 && t2 > 0)
            {

                Vector3 posReachPlayerHeightJump1 = StraightXZDragPathDOTS.getPositionAtTime(ballPosition, V0, k, t1);
                Vector3 posReachPlayerHeightJump2 = StraightXZDragPathDOTS.getPositionAtTime(ballPosition, V0, k, t2);
                float d1 = Vector3.Distance(closestPoint, ballPosition);
                float d2 = Vector3.Distance(posReachPlayerHeightJump1, ballPosition);
                float d3 = Vector3.Distance(posReachPlayerHeightJump2, ballPosition);
                if (d1 > d2 && d1 < d3)
                {
                    float PlayerReachDistance = getPlayerReachDistance_StraightPass2(false, playerPosition, posReachPlayerHeightJump1, ballPosition, maxSpeed, ref PathDataDOTS, ref ballPositionAtReachTime, ref PlayerGenericParams,ref playerPositionElement, out playerReachTime, 0, isGoalkeeper, lonelyPosition);
                    if (PlayerReachDistance > parabolicMinDistance_BallPlayer)
                    {
                        parabolicMinDistance_BallPlayer = PlayerReachDistance;
                        defenseParabolicReachIndex = i;
                        defenseParabolicReachTime = playerReachTime;
                        defenseReachPosition = posReachPlayerHeightJump1;
                        if (PlayerReachDistance>=0)
                            result = false;
                    }
                    PlayerReachDistance = getPlayerReachDistance_StraightPass2(false, playerPosition, posReachPlayerHeightJump2, ballPosition, maxSpeed, ref PathDataDOTS, ref ballPositionAtReachTime, ref PlayerGenericParams,ref playerPositionElement, out playerReachTime, 0, isGoalkeeper, lonelyPosition);
                    if (PlayerReachDistance > parabolicMinDistance_BallPlayer)
                    {
                        parabolicMinDistance_BallPlayer = PlayerReachDistance;
                        defenseParabolicReachIndex = i;
                        defenseParabolicReachTime = playerReachTime;
                        defenseReachPosition = posReachPlayerHeightJump2;
                        if (PlayerReachDistance >= 0)
                            result = false;
                    }
                }
                else
                {
                    float PlayerReachDistance = getPlayerReachDistance_StraightPass2(false, playerPosition, closestPoint, ballPosition, maxSpeed, ref PathDataDOTS, ref ballPositionAtReachTime, ref PlayerGenericParams,ref playerPositionElement, out playerReachTime,0, isGoalkeeper, lonelyPosition);
                    if (PlayerReachDistance > parabolicMinDistance_BallPlayer)
                    {
                        parabolicMinDistance_BallPlayer = PlayerReachDistance;
                        defenseParabolicReachIndex = i;
                        defenseParabolicReachTime = playerReachTime;
                        defenseReachPosition = closestPoint;
                        if (PlayerReachDistance >= 0)
                            result = false;
                    }
                }
                float PlayerReachDistance2 = getPlayerReachDistance_StraightPass2(false, playerPosition, lonelyPosition, ballPosition, maxSpeed, ref PathDataDOTS, ref ballPositionAtReachTime, ref PlayerGenericParams,ref playerPositionElement, out playerReachTime,0, isGoalkeeper, lonelyPosition);
                if (PlayerReachDistance2 > parabolicMinDistance_BallPlayer)
                {
                    parabolicMinDistance_BallPlayer = PlayerReachDistance2;
                    defenseParabolicReachIndex = i;
                    defenseParabolicReachTime = playerReachTime;
                    defenseReachPosition = lonelyPosition;
                    if (PlayerReachDistance2 >= 0)
                        result = false;
                }
            }
        }
        return result;

    }
    
    void getMinReachDistance_StraightPass(bool isStrightPass,ref DynamicBuffer<PlayerPositionElement> PlayerPositions, ref PlayerGenericParams PlayerGenericParams, int indexStart, int indexEnd, Vector3 lonelyPosition, Vector3 ballPosition, ref PathDataDOTS PathDataDOTS, out float distanceResult, out int defenseReachIndex, out float defenseReachTime, float t0,out Vector3 defenseReachPosition, LonelyPointElement2 lonelyPoint)
    {
        Vector2 playerPosition2;
        Vector3 playerPosition;
        Vector3 closestPoint;
        Vector3 ballPositionAtReachTime = Vector3.zero;
        distanceResult = -Mathf.Infinity;
        defenseReachIndex = -1;
        defenseReachTime = Mathf.Infinity;
        defenseReachPosition = Vector3.positiveInfinity;
        Vector3 dir1 = lonelyPosition - ballPosition;
        ballPosition.y = 0;
        for (int i = indexStart; i < indexEnd; i++)
        {
            PlayerPositionElement playerPositionElement = PlayerPositions[i];
            playerPosition2 = playerPositionElement.position;
            playerPosition = new Vector3(playerPosition2.x, 0, playerPosition2.y);
            closestPoint = MyFunctions.GetClosestPointOnFiniteLine(playerPosition, ballPosition, lonelyPosition);
            bool isGolkeeper = i == indexStart;
            float maxSpeed = isGolkeeper ? PlayerGenericParams.goalkeeperMaxSpeed : playerPositionElement.maxSpeed;
            Vector3 dir2 = playerPosition - ballPosition;
            float angle = Vector3.Angle(dir1, dir2);
            float playerReachTime, playerReachTime2;

            if (angle >= 90)
            {
                closestPoint = lonelyPosition;
            }
            else
            {


                float PlayerReachDistance = getPlayerReachDistance_StraightPass(isStrightPass, playerPosition, closestPoint, ballPosition,maxSpeed, ref PathDataDOTS, ref ballPositionAtReachTime, ref PlayerGenericParams,ref playerPositionElement, out playerReachTime, t0, isGolkeeper, lonelyPosition);
                if (PlayerReachDistance > distanceResult)
                {
                    distanceResult = PlayerReachDistance;
                    defenseReachTime = playerReachTime;
                    defenseReachIndex = i;
                    defenseReachPosition = closestPoint;
                }
            }
            

            //playerPositionElement.getTimeReach = aux ? playerReachTime : -1;
            //PlayerPositions[i] = playerPositionElement;
            
            float PlayerReachDistance2;

            float distanceClosest_LonelyPosition = Vector3.Distance(closestPoint, lonelyPosition);
            
            PlayerReachDistance2 = getPlayerReachDistance_StraightPass2(isStrightPass, playerPosition, lonelyPosition, ballPosition, maxSpeed, ref PathDataDOTS, ref ballPositionAtReachTime, ref PlayerGenericParams,ref playerPositionElement, out playerReachTime2, t0, isGolkeeper, lonelyPosition);
            
            if (PlayerReachDistance2 > distanceResult)
            {
                distanceResult = PlayerReachDistance2;
                defenseReachTime = playerReachTime2;
                defenseReachIndex = i;
                defenseReachPosition = lonelyPosition;
            }
        }
    }
    void getMinReachDistance_StraightPass2(bool isStrightPass, ref DynamicBuffer<PlayerPositionElement> PlayerPositions, ref PlayerGenericParams PlayerGenericParams, int indexStart, int indexEnd, Vector3 lonelyPosition, Vector3 ballPosition, ref PathDataDOTS PathDataDOTS, out float distanceResult, out int defenseReachIndex, out float defenseReachTime, ref TestResultComponent TestResult, float t0, out Vector3 defenseReachPosition)
    {
        Vector2 playerPosition2;
        Vector3 playerPosition;
        Vector3 closestPoint;
        Vector3 ballPositionAtReachTime = Vector3.zero;
        distanceResult = -Mathf.Infinity;
        defenseReachIndex = -1;
        defenseReachTime = Mathf.Infinity;
        defenseReachPosition = Vector3.positiveInfinity;
        Vector3 dir1 = lonelyPosition - ballPosition;
        ballPosition.y = 0;
        for (int i = indexStart; i < indexEnd; i++)
        {
            PlayerPositionElement playerPositionElement = PlayerPositions[i];
            playerPosition2 = playerPositionElement.position;
            playerPosition = new Vector3(playerPosition2.x, 0, playerPosition2.y);
            closestPoint = MyFunctions.GetClosestPointOnFiniteLine(playerPosition, ballPosition, lonelyPosition);
            bool isGolkeeper = i == indexStart;
            float maxSpeed = isGolkeeper ? PlayerGenericParams.goalkeeperMaxSpeed : playerPositionElement.maxSpeed;
            Vector3 dir2 = playerPosition - ballPosition;
            float angle = Vector3.Angle(dir1, dir2);
            float playerReachTime, playerReachTime2;

            if (angle >= 90)
            {
                closestPoint = lonelyPosition;
            }
            else
            {


                float PlayerReachDistance = getPlayerReachDistance_StraightPass(isStrightPass, playerPosition, closestPoint, ballPosition, maxSpeed, ref PathDataDOTS, ref ballPositionAtReachTime, ref PlayerGenericParams, ref playerPositionElement, out playerReachTime, t0, isGolkeeper, lonelyPosition);
                if (PlayerReachDistance > distanceResult)
                {
                    distanceResult = PlayerReachDistance;
                    defenseReachTime = playerReachTime;
                    defenseReachIndex = i;
                    TestResult.closestDistanceDefenseBall = PlayerReachDistance;
                    TestResult.defenseClosestReachTime = playerReachTime;
                    TestResult.defenseReachPosition = closestPoint;
                    defenseReachPosition = closestPoint;
                }
            }


            //playerPositionElement.getTimeReach = aux ? playerReachTime : -1;
            //PlayerPositions[i] = playerPositionElement;

            float PlayerReachDistance2;

            float distanceClosest_LonelyPosition = Vector3.Distance(closestPoint, lonelyPosition);

            PlayerReachDistance2 = getPlayerReachDistance_StraightPass3(isStrightPass, playerPosition, lonelyPosition, ballPosition, maxSpeed, ref PathDataDOTS, ref ballPositionAtReachTime, ref PlayerGenericParams, ref playerPositionElement, out playerReachTime2, t0, isGolkeeper, lonelyPosition);
            if (playerReachTime2 < defenseReachTime)
            {


                TestResult.closestPosition = closestPoint;
                TestResult.closestDistanceDefenseBall = PlayerReachDistance2;
                TestResult.defenseClosestReachTime = playerReachTime2;
            }
            if (PlayerReachDistance2 > distanceResult)
            {
                distanceResult = PlayerReachDistance2;
                defenseReachTime = playerReachTime2;
                defenseReachIndex = i;
                defenseReachPosition = lonelyPosition;
                TestResult.defenseReachPosition = lonelyPosition;
            }
        }
    }
    float getPlayerReachDistance_StraightPass(bool isStraightPass, Vector3 playerPosition, Vector3 closestPoint,Vector3 ballPosition,float maxSpeed, ref PathDataDOTS PathDataDOTS, ref Vector3 ballPositionAtReachTime, ref PlayerGenericParams PlayerGenericParams,ref PlayerPositionElement playerPositionElement, out float playerReachTimeResult, float t0,bool isGoalkeeper,Vector3 lonelyPosition)
    {
        playerReachTimeResult = GetTimeToReachPosition(ref playerPositionElement,playerPositionElement.position, closestPoint,maxSpeed,playerPositionElement.maxSpeedForReachBall,playerPositionElement.currentSpeed, ref PlayerGenericParams, isGoalkeeper,playerPositionElement.scope, ballPosition,lonelyPosition,out Vector3 closestPoint2);
        
        float playerReachTimeResult2 = isGoalkeeper ? playerReachTimeResult : playerReachTimeResult - t0;
        if (isStraightPass)
        {
            StraightXZDragAndFrictionPathDOTS2.getPositionAtTime(Mathf.Max(playerReachTimeResult2, 0), ref PathDataDOTS, ref ballPositionAtReachTime);
        }
        else
        {
            ballPositionAtReachTime = StraightXZDragPathDOTS.getPositionAtTime(ballPosition, PathDataDOTS.V0, PathDataDOTS.k, Mathf.Max(playerReachTimeResult2,0));
        }
        float d1 = Vector3.Distance(ballPosition, closestPoint2);
        float d2 = Vector3.Distance(ballPosition, ballPositionAtReachTime);
        int sign = d1 <= d2 ? -1 : 1;
        float distancePlayer_Ball = sign * Vector3.Distance(closestPoint2, ballPositionAtReachTime);

        return distancePlayer_Ball;
    }
    float getPlayerReachDistance_StraightPass2(bool isStraightPass, Vector3 playerPosition, Vector3 closestPoint, Vector3 ballPosition, float maxSpeed, ref PathDataDOTS PathDataDOTS, ref Vector3 ballPositionAtReachTime, ref PlayerGenericParams PlayerGenericParams, ref PlayerPositionElement playerPositionElement, out float playerReachTimeResult, float t0, bool isGoalkeeper, Vector3 lonelyPosition)
    {
        playerReachTimeResult = GetTimeToReachPosition2(ref playerPositionElement, playerPositionElement.position, closestPoint, maxSpeed, playerPositionElement.maxSpeedForReachBall, playerPositionElement.currentSpeed, ref PlayerGenericParams, isGoalkeeper, playerPositionElement.scope, ballPosition, lonelyPosition);

        float playerReachTimeResult2 = isGoalkeeper ? playerReachTimeResult : playerReachTimeResult - t0;
        if (isStraightPass)
        {
            StraightXZDragAndFrictionPathDOTS2.getPositionAtTime(Mathf.Max(playerReachTimeResult2, 0), ref PathDataDOTS, ref ballPositionAtReachTime);
        }
        else
        {
            ballPositionAtReachTime = StraightXZDragPathDOTS.getPositionAtTime(ballPosition, PathDataDOTS.V0, PathDataDOTS.k, Mathf.Max(playerReachTimeResult2, 0));
        }
        float d1 = Vector3.Distance(ballPosition, closestPoint);
        float d2 = Vector3.Distance(ballPosition, ballPositionAtReachTime);
        int sign = d1 <= d2 ? -1 : 1;
        float distancePlayer_Ball = sign * Vector3.Distance(closestPoint, ballPositionAtReachTime);

        return distancePlayer_Ball;
    }
    void getMinReachDistance_StraightPass2(
    bool isStrightPass,
    ref DynamicBuffer<PlayerPositionElement> PlayerPositions,
    ref PlayerGenericParams PlayerGenericParams,
    int indexStart,
    int indexEnd,
    Vector3 lonelyPosition,
    Vector3 ballPosition,
    ref PathDataDOTS PathDataDOTS,
    out float distanceResult,
    out int defenseReachIndex,
    out float defenseReachTime,
    float t0,
    out Vector3 defenseReachPosition)
    {
        Vector2 playerPosition2;
        Vector3 playerPosition;
        Vector3 ballPositionAtReachTime = Vector3.zero;

        distanceResult = -Mathf.Infinity;
        defenseReachIndex = -1;
        defenseReachTime = Mathf.Infinity;
        defenseReachPosition = Vector3.positiveInfinity;

        ballPosition.y = 0;
        lonelyPosition.y = 0;

        Vector3 passDirection = lonelyPosition - ballPosition;

        // Distancia entre muestras (metros)
        const float sampleStep = 2f;

        float totalDistance = Vector3.Distance(ballPosition, lonelyPosition);
        int sampleCount = Mathf.Max(1, Mathf.CeilToInt(totalDistance / sampleStep));

        for (int i = indexStart; i < indexEnd; i++)
        {
            PlayerPositionElement playerPositionElement = PlayerPositions[i];

            playerPosition2 = playerPositionElement.position;
            playerPosition = new Vector3(playerPosition2.x, 0, playerPosition2.y);

            bool isGolkeeper = i == indexStart;

            float maxSpeed = isGolkeeper
                ? PlayerGenericParams.goalkeeperMaxSpeed
                : playerPositionElement.maxSpeed;

            Vector3 dirToPlayer = playerPosition - ballPosition;
            float angle = Vector3.Angle(passDirection, dirToPlayer);

            // Si está detrás del balón, sólo comprobamos el destino final.
            int startSample = angle >= 90f ? sampleCount : 0;

            for (int s = startSample; s <= 10; s++)
            {
                float t = (float)s / sampleCount;

                Vector3 targetPoint = Vector3.Lerp(
                    ballPosition,
                    lonelyPosition,
                    t);

                float playerReachTime;

                float playerReachDistance =
                    getPlayerReachDistance_StraightPass(
                        isStrightPass,
                        playerPosition,
                        targetPoint,
                        ballPosition,
                        maxSpeed,
                        ref PathDataDOTS,
                        ref ballPositionAtReachTime,
                        ref PlayerGenericParams,
                        ref playerPositionElement,
                        out playerReachTime,
                        t0,
                        isGolkeeper,
                        lonelyPosition);

                if (playerReachDistance > distanceResult)
                {
                    distanceResult = playerReachDistance;
                    defenseReachTime = playerReachTime;
                    defenseReachIndex = i;
                    defenseReachPosition = targetPoint;
                }
            }
        }
    }
    float getPlayerReachDistance_StraightPass3(bool isStraightPass, Vector3 playerPosition, Vector3 closestPoint, Vector3 ballPosition, float maxSpeed, ref PathDataDOTS PathDataDOTS, ref Vector3 ballPositionAtReachTime, ref PlayerGenericParams PlayerGenericParams, ref PlayerPositionElement playerPositionElement, out float playerReachTimeResult, float t0, bool isGoalkeeper, Vector3 lonelyPosition)
    {
        playerReachTimeResult = GetTimeToReachPosition2(ref playerPositionElement, playerPositionElement.position, closestPoint, maxSpeed, playerPositionElement.maxSpeedForReachBall, playerPositionElement.currentSpeed, ref PlayerGenericParams, isGoalkeeper, playerPositionElement.scope, ballPosition, lonelyPosition);

        float playerReachTimeResult2 = isGoalkeeper ? playerReachTimeResult : playerReachTimeResult - t0;
        if (isStraightPass)
        {
            StraightXZDragAndFrictionPathDOTS2.getPositionAtTime(Mathf.Max(playerReachTimeResult2, 0), ref PathDataDOTS, ref ballPositionAtReachTime);
        }
        else
        {
            ballPositionAtReachTime = StraightXZDragPathDOTS.getPositionAtTime(ballPosition, PathDataDOTS.V0, PathDataDOTS.k, Mathf.Max(playerReachTimeResult2, 0));
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
    float GetTimeToReachPositionDeprecated(ref DynamicBuffer<PlayerPositionElement> PlayerPositions, ref PlayerGenericParams PlayerGenericParams, int indexStart, int indexEnd, Vector3 targetPosition, ref int attackIndex,Vector2 offside,Vector2 defenseGoalPos,float reachBallTime,Vector3 ballPosition,Vector3 lonelyPosition)
    {
        float reachTimeEnd = Mathf.Infinity;
        Vector2 forward = defenseGoalPos - offside;
        forward.x = 0;
        for (int i = indexStart; i < indexEnd; i++)
        {
            PlayerPositionElement playerPositionElement = PlayerPositions[i];
            Vector3 playerPosition = new Vector3(PlayerPositions[i].position.x, 0, PlayerPositions[i].position.y);
            Vector2 dir = playerPositionElement.position - offside;
            dir.x = 0;
            if (Vector2.Dot(forward, dir) <= 0)
            {
                bool isGolkeeper = i == indexStart;
                float maxSpeed = isGolkeeper ? PlayerGenericParams.goalkeeperMaxSpeed : playerPositionElement.maxSpeed;

                float reachTime = GetTimeToReachPosition(ref playerPositionElement, playerPositionElement.position, targetPosition, maxSpeed, playerPositionElement.maxSpeedForReachBall, playerPositionElement.currentSpeed, ref PlayerGenericParams, isGolkeeper, playerPositionElement.scope, ballPosition, lonelyPosition, out Vector3 closestPoint2);
                if (reachTime < reachTimeEnd)
                {
                    reachTimeEnd = reachTime;
                    attackIndex = i;
                }
            }
        }
        return reachTimeEnd;
    }
    float GetTimeToReachPosition(ref DynamicBuffer<PlayerPositionElement> PlayerPositions, ref PlayerGenericParams PlayerGenericParams, int indexStart, int indexEnd, Vector3 targetPosition, ref int attackIndex, Vector2 offside, Vector2 defenseGoalPos, float reachBallTime,int passerPlayer,Vector3 ballPosition,Vector3 lonelyPosition, LonelyPointElement2 lonelyPoint)
    {
        float reachTimeEnd = Mathf.Infinity;
        Vector2 forward = defenseGoalPos - offside;
        Vector2 forward2 = new Vector2(targetPosition.x,targetPosition.y) - offside;
        forward.x = 0;
        forward2.x = 0;
        Vector3 offside3D = new Vector3(offside.x,0, offside.y);
        for (int i = indexStart; i < indexEnd; i++)
        {
            PlayerPositionElement playerPositionElement = PlayerPositions[i];
            Vector3 playerPosition = new Vector3(playerPositionElement.position.x, 0, playerPositionElement.position.y);

            Vector2 dir = playerPositionElement.position - offside;
            dir.x = 0;
            float totalTime;
            bool isGolkeeper = i == indexStart;
            float maxSpeed = isGolkeeper ? PlayerGenericParams.goalkeeperMaxSpeed : playerPositionElement.maxSpeed;
            if (Vector2.Dot(forward, forward2) <= 0 && Vector2.Dot(forward, dir) <= 0||true)
            {
                totalTime = GetTimeToReachPosition2(ref playerPositionElement, playerPositionElement.position, targetPosition, maxSpeed, playerPositionElement.maxSpeedForReachBall, playerPositionElement.currentSpeed, ref PlayerGenericParams, isGolkeeper, playerPositionElement.scope, ballPosition, lonelyPosition);
            }else if (Vector2.Dot(forward, dir) <= 0)
            {
                if (SegmentLineIntersectionXZ(playerPosition, targetPosition, offside3D, offside3D + Vector3.right, out Vector3 offsidePoint))
                {
                    if (i != passerPlayer)
                    {
                        float targetSpeed = maxSpeed;
                        float decrement = 2f;
                        bool enterInOffside = false;
                        float timeToOffside = 0;
                        while (targetSpeed > 0)
                        {
                            // Tiempo hasta ese punto
                            timeToOffside = GetTimeToReachPosition2(ref playerPositionElement, playerPositionElement.position, offsidePoint, maxSpeed, targetSpeed, playerPositionElement.currentSpeed, ref PlayerGenericParams, isGolkeeper,0.1f, ballPosition, lonelyPosition);
                            if (timeToOffside < reachBallTime)
                            {
                                targetSpeed = Mathf.Max(targetSpeed - decrement, 0);
                                enterInOffside = true;
                            }
                            else
                            {
                                break;
                            }

                        }
                        if (enterInOffside)
                        {
                            // ❌ Llegaría antes → tiene que esperar
                            float waitTime = reachBallTime - timeToOffside;

                            // Tiempo desde la línea hasta el objetivo
                            float timeFromLine = GetTimeToReachPosition2(ref playerPositionElement, new Vector2(offsidePoint.x, offsidePoint.z), targetPosition, maxSpeed, playerPositionElement.maxSpeedForReachBall,targetSpeed, ref PlayerGenericParams, isGolkeeper, playerPositionElement.scope, ballPosition, lonelyPosition);

                            totalTime = timeToOffside + waitTime + timeFromLine;
                        }
                        else
                        {
                            // ✔️ No rompe fuera de juego
                            totalTime = GetTimeToReachPosition2(ref playerPositionElement, playerPositionElement.position, targetPosition, maxSpeed, playerPositionElement.maxSpeedForReachBall, playerPositionElement.currentSpeed, ref PlayerGenericParams, isGolkeeper, playerPositionElement.scope, ballPosition, lonelyPosition);
                        }
                    }
                    else
                    {
                        totalTime = GetTimeToReachPosition2(ref playerPositionElement, playerPositionElement.position, targetPosition, maxSpeed, playerPositionElement.maxSpeedForReachBall, playerPositionElement.currentSpeed, ref PlayerGenericParams, isGolkeeper, playerPositionElement.scope, ballPosition, lonelyPosition);
                    }
                   
                }
                else
                {
                    // ✔️ No rompe fuera de juego
                    totalTime = GetTimeToReachPosition2(ref playerPositionElement, playerPositionElement.position, targetPosition, maxSpeed, playerPositionElement.maxSpeedForReachBall, playerPositionElement.currentSpeed, ref PlayerGenericParams, isGolkeeper, playerPositionElement.scope, ballPosition, lonelyPosition);
                }
            }
            else
            {
                continue;
            }
            if (totalTime < reachTimeEnd)
            {
                reachTimeEnd = totalTime;
                attackIndex = i;
            }
        }

        return reachTimeEnd;
    }
    Vector3 GetPointOnOffsideLine(Vector3 playerPosition, Vector3 targetPosition, Vector2 offside)
    {
        // Línea horizontal (x ignorada, usas eje Z realmente)
        float offsideZ = offside.y;

        Vector3 dir = (targetPosition - playerPosition);

        if (Mathf.Abs(dir.z) < 0.001f)
            return targetPosition;

        float t = (offsideZ - playerPosition.z) / dir.z;

        t = Mathf.Clamp01(t);

        return playerPosition + dir * t;
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
        PlayerGenericParams.goalkeeperMaxSpeed = 5f;
        PlayerGenericParams.maxKickForce = 33f;
        PlayerGenericParams.heightJump = 1.8f;
        PlayerGenericParams.heightBallControl = 1.4f;
        return PlayerGenericParams;
    }
    float GetTimeToReachPosition(ref PlayerPositionElement playerPositionElement,Vector2 playerPosition, Vector3 closestPoint,float maxSpeed,float targetSpeed,float currentSpeed,ref  PlayerGenericParams PlayerGenericParams,bool isGoalkeeper,float scope,Vector3 ballPosition,Vector3 lonelyPosition,out Vector3 closestPoint2)
    {
        //return GetTimeToReachPointDOTS.linearGetTimeToReachPosition(playerPositionElement.position, closestPoint, maxSpeed, PlayerGenericParams.scope);
        Vector3 position = new Vector3(playerPosition.x,0, playerPosition.y);
        Vector3 normalizedVelocity = new Vector3(playerPositionElement.normalizedVelocity.x,0, playerPositionElement.normalizedVelocity.y);
        Vector3 bodyForward = new Vector3(playerPositionElement.bodyForward.x,0, playerPositionElement.bodyForward.y);
        closestPoint2 = closestPoint;
        if (isGoalkeeper)
        {
            return GetTimeToReachPointDOTS.linearGetTimeToReachPosition(position, closestPoint, PlayerGenericParams.goalkeeperMaxSpeed, playerPositionElement.scope);
        }
        else
        {
            float t = GetTimeToReachPointDOTS.accelerationGetTimeToReachPosition2(position, currentSpeed, bodyForward, normalizedVelocity, ref PlayerGenericParams, closestPoint, playerPositionElement, targetSpeed,scope,out Vector3 posAfterBrake,out float tBrake,out float brakeSpeed);
            
            Vector3 closestPoint3 = MyFunctions.GetClosestPointOnFiniteLine(posAfterBrake, ballPosition, lonelyPosition);
            float t2 = GetTimeToReachPointDOTS.accelerationGetTimeToReachPosition2(position, brakeSpeed, bodyForward, normalizedVelocity, ref PlayerGenericParams, closestPoint3, playerPositionElement, targetSpeed, scope, out Vector3 posAfterBrake2, out float tBrake2, out brakeSpeed);
            if(t < t2)
            {
                
                return  t;
            }
            else
            {
                closestPoint2 = closestPoint3;
                return t2;
            }
        }

    }
    float GetTimeToReachPosition2(ref PlayerPositionElement playerPositionElement, Vector2 playerPosition, Vector3 closestPoint, float maxSpeed, float targetSpeed, float currentSpeed, ref PlayerGenericParams PlayerGenericParams, bool isGoalkeeper, float scope, Vector3 ballPosition, Vector3 lonelyPosition)
    {
        //return GetTimeToReachPointDOTS.linearGetTimeToReachPosition(playerPositionElement.position, closestPoint, maxSpeed, PlayerGenericParams.scope);
        Vector3 position = new Vector3(playerPosition.x, 0, playerPosition.y);
        Vector3 normalizedVelocity = new Vector3(playerPositionElement.normalizedVelocity.x, 0, playerPositionElement.normalizedVelocity.y);
        Vector3 bodyForward = new Vector3(playerPositionElement.bodyForward.x, 0, playerPositionElement.bodyForward.y);
        if (isGoalkeeper)
        {
            return GetTimeToReachPointDOTS.linearGetTimeToReachPosition(position, closestPoint, PlayerGenericParams.goalkeeperMaxSpeed, playerPositionElement.scope);
        }
        else
        {
            float t = GetTimeToReachPointDOTS.accelerationGetTimeToReachPosition2(position, currentSpeed, bodyForward, normalizedVelocity, ref PlayerGenericParams, closestPoint, playerPositionElement, targetSpeed, scope, out Vector3 posAfterBrake, out float tBrake,out float brakeSpeed);
            return t;
        }

    }
    public static bool SegmentLineIntersectionXZ(
    Vector3 segA, Vector3 segB,     // segmento (finito)
    Vector3 lineA, Vector3 lineB,   // línea infinita
    out Vector3 intersection)
    {
        intersection = Vector3.zero;

        // Convertimos a 2D (XZ)
        Vector2 p1 = new Vector2(segA.x, segA.z);
        Vector2 p2 = new Vector2(segB.x, segB.z);
        Vector2 p3 = new Vector2(lineA.x, lineA.z);
        Vector2 p4 = new Vector2(lineB.x, lineB.z);

        Vector2 r = p2 - p1;
        Vector2 s = p4 - p3;

        float cross = r.x * s.y - r.y * s.x;

        // Paralelas
        if (math.abs(cross) < 1e-6f)
            return false;

        Vector2 diff = p3 - p1;

        float t = (diff.x * s.y - diff.y * s.x) / cross;
        // float u = (diff.x * r.y - diff.y * r.x) / cross;
        // u no hace falta porque la línea es infinita

        // ✅ Aquí está la clave: solo comprobamos el segmento
        if (t < 0f || t > 1f)
            return false;

        Vector2 hit2D = p1 + t * r;

        intersection = new Vector3(hit2D.x, 0f, hit2D.y);
        return true;
    }
   
}
