using DOTS_ChaserDataCalculation;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

public struct GetPassV0Element : IBufferElementData
{
    public float t,accuracy,maxControlSpeed, maxControlSpeedLerpDistance,searchVyIncrement;
    public int maxAttempts;
    
    public Vector3 Pos0,Posf;
    public float k;
    public float friction, ballRadio, g;
    public GetV0DOTSResult result;
}
public struct GetV0DOTSResult
{
    public bool foundedResult;
    public float vt;
    public Vector3 v0;
    public float v0Magnitude;
    public bool maximumControlSpeedReached;
    public bool maxKickForceReached;
    public bool noRivalReachTheTargetBeforeMe, noPartnerIsAhead;
    public float differenceTimeWithRival;
    public float receiverReachTargetPositionTime,ballReachTargetPositionTime;
    public GetV0DOTSResult(bool foundedResult, float vt, Vector3 v0, bool maximumControlSpeedReached, bool maxKickForceReached, bool noRivalReachTheTargetBeforeMe,bool noPartnerIsAhead)
    {
        this.foundedResult = foundedResult;
        this.vt = vt;
        this.v0 = v0;
        this.maximumControlSpeedReached = maximumControlSpeedReached;
        this.maxKickForceReached = maxKickForceReached;
        this.noRivalReachTheTargetBeforeMe = noRivalReachTheTargetBeforeMe;
        this.noPartnerIsAhead = noPartnerIsAhead;
        v0Magnitude = v0.magnitude;
        differenceTimeWithRival = 0;
        receiverReachTargetPositionTime = Mathf.Infinity;
        ballReachTargetPositionTime = Mathf.Infinity;
    }
}
[BurstCompile]
public struct GetPassV0Job : IJobEntityBatch
{
    public BufferTypeHandle<GetPassV0Element> GetPassV0ElementBufferHandle;
    public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
    {
        BufferAccessor<GetPassV0Element> GetPassV0ElementBufferHandleBufferAccesor = batchInChunk.GetBufferAccessor(GetPassV0ElementBufferHandle);

        for (int i = 0; i < GetPassV0ElementBufferHandleBufferAccesor.Length; i++)
        {
            DynamicBuffer<GetPassV0Element> GetPassV0ElementBuffer= GetPassV0ElementBufferHandleBufferAccesor[i];
            for (int j = 0; j < GetPassV0ElementBuffer.Length; j++)
            {
                GetPassV0Element GetPassV0Element = GetPassV0ElementBuffer[j];
                StraightXZDragAndFrictionPathDOTS.getV0(ref GetPassV0Element,0);
                GetPassV0ElementBuffer[j] = GetPassV0Element;
            }
        }
    }
}
