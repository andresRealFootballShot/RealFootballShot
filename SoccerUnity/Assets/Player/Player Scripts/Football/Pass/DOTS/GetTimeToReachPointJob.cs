using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using DOTS_ChaserDataCalculation;

[BurstCompile]
public struct GetTimeToReachPointJob : IJobEntityBatch
{
    public BufferTypeHandle<GetTimeToReachPointElement> GetTimeToReachPointElementBufferHandle;
    public BufferTypeHandle<PlayerAttackElement> PlayerDataComponentElementBufferHandle;
    public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
    {
        BufferAccessor<GetTimeToReachPointElement> GetTimeToReachPointElementBufferAccesor = batchInChunk.GetBufferAccessor(GetTimeToReachPointElementBufferHandle);
        BufferAccessor<PlayerAttackElement> PlayerDataComponentElementBufferAccesor = batchInChunk.GetBufferAccessor(PlayerDataComponentElementBufferHandle);
        
        for (int i = 0; i < GetTimeToReachPointElementBufferAccesor.Length; i++)
        {
            DynamicBuffer<GetTimeToReachPointElement> GetTimeToReachPointElementBuffer = GetTimeToReachPointElementBufferAccesor[i];
            DynamicBuffer<PlayerAttackElement> PlayerDataComponentElementBuffer = PlayerDataComponentElementBufferAccesor[i];
            for (int j = 0; j < GetTimeToReachPointElementBuffer.Length; j++)
            {
                GetTimeToReachPointElement GetTimeToReachPointElement = GetTimeToReachPointElementBuffer[j];
                int playerIndex = 0;
                for (int k = 0; k < PlayerDataComponentElementBuffer.Length; k++)
                {
                    if (PlayerDataComponentElementBuffer[k].PlayerDataComponent.id == GetTimeToReachPointElement.playerID)
                    {
                        playerIndex = k;
                    }
                }
                PlayerDataComponent playerDataComponent = PlayerDataComponentElementBuffer[playerIndex].PlayerDataComponent;
                //GetTimeToReachPointElement.receiverReachTime = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent, GetTimeToReachPointElement.targetPosition);
                GetTimeToReachPointElementBuffer[j] = GetTimeToReachPointElement;
            }
        }
     }
}
