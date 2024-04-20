using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using DOTS_ChaserDataCalculation;
public struct GetPerfectPassComponent : IComponentData
{
    public GetTimeToReachPointElement getTimeToReachPoint;
    public GetPassV0Element straightGetPassV0,parabolicGetPassV0;
    public float rival_Thrower_OptimalTimeDifference, partnerIsAheadMinTime;
    
    public float rivalsHeightOffset;
    public bool isReceiverHeadPass;
    public int defenseGoalAreaIndex,fullFieldAreaIndex;
    public bool useOptimization;
    public bool useTest;
}
