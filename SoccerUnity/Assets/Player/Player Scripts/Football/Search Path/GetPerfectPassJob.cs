using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using DOTS_ChaserDataCalculation;

[BurstCompile]
public struct GetPerfectPassJob : IJobEntityBatch
{

    public ComponentTypeHandle<GetPerfectPassComponent> GetPerfectPassComponentHandle;
    [ReadOnly] public ComponentTypeHandle<PathComponent> PathComponentHandle;
    [ReadOnly] public BufferTypeHandle<PlayerAttackElement> PlayerAttackElementBufferHandle;
    [ReadOnly] public BufferTypeHandle<PlayerDefenseElement> PlayerDefenseElementBufferHandle;
    [ReadOnly] public BufferTypeHandle<AreaPlaneElement> areaPlanesHandle;
    public BufferTypeHandle<SegmentedPathElement> segmentedPathsHandle;
    public BufferTypeHandle<ChaserDataElement> ChaserDataElementBufferHandle;
    //public NativeArray<float> results;
    public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
    {
        NativeArray<GetPerfectPassComponent> GetPerfectPassComponents = batchInChunk.GetNativeArray(GetPerfectPassComponentHandle);
        NativeArray<PathComponent> PathComponents = batchInChunk.GetNativeArray(PathComponentHandle);
        BufferAccessor<PlayerAttackElement> PlayerDataAttackBufferAccesor = batchInChunk.GetBufferAccessor(PlayerAttackElementBufferHandle);
        BufferAccessor<PlayerDefenseElement> PlayerDataDefenseBufferAccesor = batchInChunk.GetBufferAccessor(PlayerDefenseElementBufferHandle);
        BufferAccessor<ChaserDataElement> ChaserDataElementBufferAccesor = batchInChunk.GetBufferAccessor(ChaserDataElementBufferHandle);
        BufferAccessor<AreaPlaneElement> areaPlanesBufferAccesor = batchInChunk.GetBufferAccessor(areaPlanesHandle);
        BufferAccessor<SegmentedPathElement> SegmentedPathElementAccesor = batchInChunk.GetBufferAccessor(segmentedPathsHandle);
        for (int i = 0; i < GetPerfectPassComponents.Length; i++)
        {
            
            GetPerfectPassComponent GetPerfectPassComponent = GetPerfectPassComponents[i];
            GetTimeToReachPointElement getTimeToReachPoint = GetPerfectPassComponent.getTimeToReachPoint;
            PathComponent PathComponent = PathComponents[i];
            DynamicBuffer<PlayerAttackElement> PlayerAttackElementBuffer = PlayerDataAttackBufferAccesor[i];
            DynamicBuffer<PlayerDefenseElement> PlayerDefenseElementBuffer = PlayerDataDefenseBufferAccesor[i];
            DynamicBuffer<ChaserDataElement> ChaserDataElementBuffer = ChaserDataElementBufferAccesor[i];
            DynamicBuffer<AreaPlaneElement> areaPlanesBuffer = areaPlanesBufferAccesor[i];
            DynamicBuffer<SegmentedPathElement> SegmentedPathElements = SegmentedPathElementAccesor[i];
            SegmentedPathElements.Clear();
           /*NativeArray<float> sortedPlayers = new NativeArray<float>(PlayerAttackElementBuffer.Length, Allocator.Temp);
            for (int j = 0; j < sortedPlayers.Length; j++)
            {
                sortedPlayers[j] = Mathf.Infinity;
            }*/
            
            resetChaserDatas(ref ChaserDataElementBuffer);
            int receiverPlayerDataIndex=0;
            float receiverReachTime=Mathf.Infinity;
            
            //Marker.Begin();
            for (int j = 0; j < PlayerAttackElementBuffer.Length; j++)
            {
                
                PlayerDataComponent PlayerAttackComponent = PlayerAttackElementBuffer[j].PlayerDataComponent;
                if (PlayerAttackElementBuffer[j].checkGetTimeToReachPosition)
                {
                    float reachTime = GetTimeToReachPointDOTS.getTimeToReachPosition(ref PlayerAttackComponent, getTimeToReachPoint.targetPosition);
                    //sortedPlayers[j] = reachTime;
                    if(reachTime< receiverReachTime)
                    {
                        receiverPlayerDataIndex = j;
                        receiverReachTime = reachTime;
                    }
                    //sortPlayerAttack(ref sortedPlayers, reachTime, j);
                }
            }
            
            //Marker.End();
            
            PlayerDataComponent ReceiverPlayerAttackComponent = PlayerAttackElementBuffer[receiverPlayerDataIndex].PlayerDataComponent;
            GetPassV0Element StraightGetPassV0Element = GetPerfectPassComponent.straightGetPassV0;
            StraightGetPassV0Element.t = receiverReachTime;
            StraightGetPassV0Element.result.receiverReachTargetPositionTime = receiverReachTime;
            StraightGetPassV0Element.result.ballReachTargetPositionTime = receiverReachTime;
            GetPerfectPassComponent.parabolicGetPassV0.result.receiverReachTargetPositionTime = receiverReachTime;
            GetPerfectPassComponent.parabolicGetPassV0.result.ballReachTargetPositionTime = receiverReachTime;
            PathComponent.currentPath.Pos0 = StraightGetPassV0Element.Pos0;
            PathComponent.currentPath.Posf = StraightGetPassV0Element.Posf;
            StraightXZDragAndFrictionPathDOTS.getV0(ref PathComponent, ref StraightGetPassV0Element, ReceiverPlayerAttackComponent.maxKickForce);
            PathComponent.currentPath.V0 = StraightGetPassV0Element.result.v0;
            PathComponent.currentPath.v0Magnitude = StraightGetPassV0Element.result.v0.magnitude;
            PathComponent.currentPath.normalizedV0 = StraightGetPassV0Element.result.v0.normalized;
            
            GetPerfectPassComponent.straightGetPassV0 = StraightGetPassV0Element;
            //NativeArray<bool> playersWithOptimalPoint = new NativeArray<bool>(PlayerDefenseElementBuffer.Length, Allocator.Temp);
            //NativeArray<AreaPlaneElement> fullAreaPlanesBuffer = new NativeArray<AreaPlaneElement>(4, Allocator.Temp);
            //getAreas(GetPerfectPassComponent.fullFieldAreaIndex, ref areaPlanesBuffer, ref fullAreaPlanesBuffer);
            //Profiler.BeginSample("My Sample");
            //Marker.Begin();
            AreaPlaneElement fullArea = areaPlanesBuffer[0];
            
            calculateChaserDatas(ref PathComponent,ref PlayerDefenseElementBuffer,ref ChaserDataElementBuffer,ref fullArea, ref SegmentedPathElements);
            //Marker.End();
            //Profiler.EndSample();
            GetPerfectPassComponent.straightGetPassV0.result.noRivalReachTheTargetBeforeMe = checkIfNoRivalReachTheTargetBeforeMe(ref ChaserDataElementBuffer, GetPerfectPassComponent.straightGetPassV0.result.ballReachTargetPositionTime, ref GetPerfectPassComponent,ref GetPerfectPassComponent.straightGetPassV0.result);
            GetPerfectPassComponent.straightGetPassV0.result.noPartnerIsAhead = checkIfNoPartnerIsAhead(ref PlayerAttackElementBuffer, ref GetPerfectPassComponent, ref PathComponent.currentPath);
            if (GetPerfectPassComponent.useTest && !GetPerfectPassComponent.straightGetPassV0.result.noRivalReachTheTargetBeforeMe)
            {


                //resetChaserDatas(ref ChaserDataElementBuffer);
                //NativeArray<AreaPlaneElement> defenseAreaPlanesBuffer = new NativeArray<AreaPlaneElement>(4, Allocator.Temp);

                //getAreas(GetPerfectPassComponent.defenseGoalAreaIndex, ref areaPlanesBuffer, ref defenseAreaPlanesBuffer);
                /*for (int j = 0; j < PlayerDefenseElementBuffer.Length; j++)
                {
                    PlayerDefenseElement playerDefenseElement = PlayerDefenseElementBuffer[j];
                    playerDefenseElement.PlayerDataComponent.has_OptimalPoint = !playerDefenseElement.PlayerDataComponent.has_OptimalPoint;
                    PlayerDefenseElementBuffer[j]= playerDefenseElement;
                }*/
                //SegmentedPathElements.Clear();
                AreaPlaneElement defenseArea1 = areaPlanesBuffer[1];
                ParabolicPassDOTS.getPassResult(ref GetPerfectPassComponent, ref PlayerAttackElementBuffer, ref PlayerDefenseElementBuffer, ref ReceiverPlayerAttackComponent, ref ReceiverPlayerAttackComponent, ref ChaserDataElementBuffer, ref defenseArea1, ref fullArea,ref PathComponent, ref SegmentedPathElements);
            }
                

            GetPerfectPassComponents[i] = GetPerfectPassComponent;
            //results[i] = i;
        }

    }
    public static void resetChaserDatas(ref DynamicBuffer<ChaserDataElement> ChaserDataElementBuffer)
    {
        for (int j = 0; j < ChaserDataElementBuffer.Length; j++)
        {
            ChaserDataElement chaserDataElement = new ChaserDataElement();
            chaserDataElement.ReachTheTarget = false;
            chaserDataElement.CalculateOptimalPoint = true;
            chaserDataElement.thereIsClosestPoint = false;
            chaserDataElement.thereIsIntercession = false;
            chaserDataElement.OptimalTime = Mathf.Infinity;
            chaserDataElement.OptimalTargetTime = Mathf.Infinity;
            chaserDataElement.differenceClosestTime = Mathf.Infinity;
            chaserDataElement.playerID = j;
            ChaserDataElementBuffer[j] = chaserDataElement;
        }

    }
    public static void resetChaserDatas2(ref DynamicBuffer<ChaserDataElement> ChaserDataElementBuffer)
    {
        for (int j = 0; j < ChaserDataElementBuffer.Length; j++)
        {
            ChaserDataElement chaserDataElement = new ChaserDataElement();
            chaserDataElement.ReachTheTarget = false;
            chaserDataElement.CalculateOptimalPoint = !ChaserDataElementBuffer[j].CalculateOptimalPointAux;
            chaserDataElement.thereIsClosestPoint = false;
            chaserDataElement.thereIsIntercession = false;
            chaserDataElement.OptimalTime = Mathf.Infinity;
            chaserDataElement.OptimalTargetTime = Mathf.Infinity;
            chaserDataElement.differenceClosestTime = Mathf.Infinity;
            chaserDataElement.playerID = j;
            ChaserDataElementBuffer[j] = chaserDataElement;
        }

    }
    public static void getAreas(int index, ref DynamicBuffer<AreaPlaneElement> areaPlanesBuffer,ref NativeArray<AreaPlaneElement> newAreaPlanesBuffer)
    {
        for (int i = 0; i < 4; i++)
        {
            newAreaPlanesBuffer[i] = areaPlanesBuffer[i + index * 4];
        }
    }
    public static void getAreas2(int index, ref DynamicBuffer<AreaPlaneElement> areaPlanesBuffer, ref AreaPlaneElement[] newAreaPlanesBuffer)
    {
        for (int i = 0; i < 4; i++)
        {
            newAreaPlanesBuffer[i] = areaPlanesBuffer[i + index * 4];
        }
    }
    void sortPlayerAttack(ref NativeArray<float> sortedPlayers,float checkReachTime,int checkPlayerIndex)
    {
        int index = 0;
        for (int i = 0; i < sortedPlayers.Length; i++)
        {
            if (checkReachTime > sortedPlayers[i])
            {
                index = i;
            }
        }
        sortedPlayers[index] = checkReachTime;
    }
    public static void calculateChaserDatas(ref PathComponent pathComponent,ref DynamicBuffer<PlayerDefenseElement> PlayerDefenseElementBuffer, ref DynamicBuffer<ChaserDataElement> ChaserDataElementBuffer,ref AreaPlaneElement fullAreaPlanesBuffer,ref DynamicBuffer<SegmentedPathElement> SegmentedPathElements)
    {
        SegmentedPathElement segmentedPath = new SegmentedPathElement();
        int count = 0;
        float t = pathComponent.segmentedPathCalculationData.startSegmentedTime;
        float timeRange = t + pathComponent.segmentedPathCalculationData.timeRange;
        float timeIncrement = pathComponent.segmentedPathCalculationData.timeIncrement;
        float maxTime = pathComponent.segmentedPathCalculationData.maxTime;
        int index = 0;
        bool exit=false;
        while (t < timeRange && t < maxTime)
        {

            count++;
            if (count > 500)
            {
                Debug.LogError("SegmentedPathJob count>100 3");
                break;
            }
            if (SegmentedPathJob.segmentedPathCalculation(t, pathComponent.segmentedPathCalculationData, ref pathComponent, ref segmentedPath))
            {
                
                t = segmentedPath.tf;
                segmentedPath.index = index;
                SegmentedPathElements.Add(segmentedPath);
                index++;
                calculateOptimalPoint(ref segmentedPath, ref PlayerDefenseElementBuffer,ref fullAreaPlanesBuffer,ref ChaserDataElementBuffer,ref exit);
                if (exit)
                {
                    return;
                }
            }
            else
            {
                //Debug.LogError("SegmentedPathJob segmentedPath not founded");
                t += timeIncrement;
            }

        }
    }
    static void calculateOptimalPoint(ref SegmentedPathElement segmentedPath, ref DynamicBuffer<PlayerDefenseElement> PlayerDefenseElementBuffer,ref AreaPlaneElement fullAreaPlanesBuffer ,ref DynamicBuffer<ChaserDataElement> ChaserDataElementBuffer,ref bool exit)
    {
        bool thereIsChaserData = false;
        
        int playerWithOptimalPointCount = 0;
        for (int i = 0; i < PlayerDefenseElementBuffer.Length; i++)
        {
            /*if (PlayerDefenseElementBuffer[i].PlayerDataComponent.has_OptimalPoint)
            {
                playerWithOptimalPointCount++;
                if (playerWithOptimalPointCount >= PlayerDefenseElementBuffer.Length)
                {
                    exit = true;
                    return;
                }
                else
                {
                    continue;
                }
            }*/
            PlayerDataComponent playerData = PlayerDefenseElementBuffer[i].PlayerDataComponent;
            //PlayerDefenseElement playerDefenseElement = PlayerDefenseElementBuffer[i];
            ChaserDataElement endChaserDataElement = ChaserDataElementBuffer[i];
            if (!endChaserDataElement.CalculateOptimalPoint)
            {
                continue;
            }
            ChaserDataElement checkChaserDataElement = new ChaserDataElement();

            OptimalPointJob.calculateChaserData(ref playerData,ref segmentedPath,ref fullAreaPlanesBuffer, ref checkChaserDataElement);

            if (checkChaserDataElement.ReachTheTarget && checkChaserDataElement.OptimalTime < endChaserDataElement.OptimalTime)
            {
                thereIsChaserData = true;
                endChaserDataElement.ReachTheTarget = true;
                endChaserDataElement.CalculateOptimalPoint = false;
                endChaserDataElement.OptimalPoint = checkChaserDataElement.OptimalPoint;
                endChaserDataElement.OptimalTime = checkChaserDataElement.OptimalTime;
                endChaserDataElement.OptimalTargetTime = checkChaserDataElement.OptimalTargetTime;
                endChaserDataElement.chaserDataCalculationIndex = i;
                //playerData.has_OptimalPoint = true;
                //playerDefenseElement.PlayerDataComponent = playerData;
                //PlayerDefenseElementBuffer[i] = playerDefenseElement;
            }
            if (checkChaserDataElement.thereIsClosestPoint && checkChaserDataElement.differenceClosestTime < endChaserDataElement.differenceClosestTime)
            {
                thereIsChaserData = true;
                endChaserDataElement.thereIsClosestPoint = true;
                endChaserDataElement.ClosestPoint = checkChaserDataElement.ClosestPoint;
                endChaserDataElement.ClosestChaserTime = checkChaserDataElement.ClosestChaserTime;
                endChaserDataElement.ClosestTargetTime = checkChaserDataElement.ClosestTargetTime;
                endChaserDataElement.TargetPositionInClosestTime = checkChaserDataElement.TargetPositionInClosestTime;
                endChaserDataElement.differenceClosestTime = checkChaserDataElement.differenceClosestTime;
            }
            if (checkChaserDataElement.thereIsIntercession && !endChaserDataElement.thereIsIntercession)
            {
                thereIsChaserData = true;
                endChaserDataElement.thereIsIntercession = true;
                endChaserDataElement.Intercession = checkChaserDataElement.Intercession;
                endChaserDataElement.TargetIntercessionTime = checkChaserDataElement.TargetIntercessionTime;
            }
            if (thereIsChaserData)
            {
                ChaserDataElementBuffer[i] = endChaserDataElement;
            }

        }
    }

    public static bool checkIfNoRivalReachTheTargetBeforeMe(ref DynamicBuffer<ChaserDataElement> ChaserDataElementBuffer,float receiverReachPointTime, ref GetPerfectPassComponent GetPerfectPassComponent,ref GetV0DOTSResult GetV0DOTSResult)
    {
        bool result = true;
        GetV0DOTSResult.differenceTimeWithRival = Mathf.Infinity;
        for (int i = 0; i < ChaserDataElementBuffer.Length; i++)
        {
            ChaserDataElement chaserDataElement = ChaserDataElementBuffer[i];
            float t = chaserDataElement.OptimalTargetTime - receiverReachPointTime;
            if (t < GetV0DOTSResult.differenceTimeWithRival)
                GetV0DOTSResult.differenceTimeWithRival = t;
            if (chaserDataElement.ReachTheTarget && t < GetPerfectPassComponent.rival_Thrower_OptimalTimeDifference)
            {
                result = false;
            }
        }
        return result;
    }
    bool checkIfNoPartnerIsAhead(ref DynamicBuffer<PlayerAttackElement> PlayerAttackElementBuffer, ref GetPerfectPassComponent getPerfectPassComponent,ref PathDataDOTS pathDataDOTS)
    {
        for (int i = 0; i < PlayerAttackElementBuffer.Length; i++)
        {
            PlayerDataComponent playerAttackComponent= PlayerAttackElementBuffer[i].PlayerDataComponent;
            float d = MyFunctions.DistancePointAndFiniteLine(MyFunctions.setY0ToVector3(playerAttackComponent.position), MyFunctions.setY0ToVector3(pathDataDOTS.Pos0), MyFunctions.setY0ToVector3(pathDataDOTS.Posf));
            float t;

            if (StraightXZDragAndFrictionPathDOTS.getT(in pathDataDOTS, playerAttackComponent.position, 0.1f, 100, out t))
            {
                if (d < playerAttackComponent.bodyBallRadio + 0.05f && t <= getPerfectPassComponent.partnerIsAheadMinTime)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
