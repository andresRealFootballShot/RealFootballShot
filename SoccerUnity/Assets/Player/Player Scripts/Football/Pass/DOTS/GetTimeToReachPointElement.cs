using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using DOTS_ChaserDataCalculation;
public struct GetTimeToReachPointElement : IBufferElementData
{
    public int playerID;
    public Vector3 targetPosition;
}
public struct PlayerAttackElement : IBufferElementData
{
    public bool checkGetTimeToReachPosition;
    public PlayerDataComponent PlayerDataComponent;
}
public struct PlayerDefenseElement : IBufferElementData
{
    public PlayerDataComponent PlayerDataComponent;
}

