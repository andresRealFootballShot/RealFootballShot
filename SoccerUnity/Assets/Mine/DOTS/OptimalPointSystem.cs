using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Profiling;
namespace DOTS_ChaserDataCalculation
{
    public class OptimalPointSystem
    {
        public NativeList<Entity> segmentedPath_OptimalPointConnections;
        public NativeList<int> segmentedPathToRemoveCounts;
        List<Entity> enableEntities = new List<Entity>();
        List<Entity> disableEntities = new List<Entity>();
        public GetPerfectPassManager SearchPathManager;
        EntityManager entityManager;
        public void OnCreate()
        {
            segmentedPath_OptimalPointConnections = new NativeList<Entity>(Allocator.Persistent);
            segmentedPathToRemoveCounts = new NativeList<int>(Allocator.Persistent);
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }
        public void OnDestroy()
        {
            segmentedPath_OptimalPointConnections.Dispose();
            segmentedPathToRemoveCounts.Dispose();
        }
        public void OnUpdate(SystemBase systemBase)
        {
            int k = 0;
            foreach (var segmentedPath_OptimalPointConnection in segmentedPath_OptimalPointConnections)
            {
                enableEntities.Clear();
                disableEntities.Clear();
                DynamicBuffer<SegmentedPathElement> segmentedPathBuffer = systemBase.GetBuffer<SegmentedPathElement>(segmentedPath_OptimalPointConnection);
                DynamicBuffer<OptimalPointListEntityElement> optimalPointList = systemBase.GetBuffer<OptimalPointListEntityElement>(segmentedPath_OptimalPointConnection);
                //segmentedPathToRemoveCounts[k] = 0;
                int optimalPointIndex = 0;
                int segmentedPathIndex = 0;
                int segmentedPathCount = 0;
                int disableCount = 0;
                DynamicBuffer<OptimalPointEntityElement> optimalPointEntityElements;
                for (int j = 0; j < segmentedPathBuffer.Length; j++)
                {
                   
                    SegmentedPathElement segmentedPathElement = segmentedPathBuffer[j];
                    if (segmentedPathIndex == 0)
                    {
                        disableCount++;
                    }
                    optimalPointEntityElements = systemBase.GetBuffer<OptimalPointEntityElement>(optimalPointList[optimalPointIndex].entity);
                    int length = optimalPointEntityElements.Length;
                    for (int i = 0; i < length; i++)
                    {
                        DynamicBuffer<SegmentedPathElement> playerSegmentedPathBuffer = systemBase.GetBuffer<SegmentedPathElement>(optimalPointEntityElements[i].entity);
                        playerSegmentedPathBuffer[j] = segmentedPathElement;
                        
                        if (segmentedPathIndex == 0)
                        {
                            //enableEntities.Add(optimalPointEntityElements[i].entity);
                        }
                    }
                    segmentedPathIndex++;
                    segmentedPathCount++;
                    if (segmentedPathIndex >= optimalPointList[optimalPointIndex].segmentedPathSize)
                    {
                        SetSegmentedPathSize(ref optimalPointEntityElements, segmentedPathIndex);
                        optimalPointIndex++;
                        segmentedPathIndex = 0;
                    }
                    if (optimalPointIndex >= optimalPointList.Length)
                    {
                        break;
                    }
                }
                optimalPointEntityElements = systemBase.GetBuffer<OptimalPointEntityElement>(optimalPointList[optimalPointIndex].entity);
                SetSegmentedPathSize(ref optimalPointEntityElements, segmentedPathIndex);

                segmentedPathToRemoveCounts[k] = segmentedPathCount;
                
                optimalPointList = systemBase.GetBuffer<OptimalPointListEntityElement>(segmentedPath_OptimalPointConnection);
                for (int i = disableCount; i < optimalPointList.Length; i++)
                {
                    optimalPointEntityElements = systemBase.GetBuffer<OptimalPointEntityElement>(optimalPointList[i].entity);

                    foreach (var optimalPointEntityElement in optimalPointEntityElements)
                    {
                        //disableEntities.Add(optimalPointEntityElement.entity);
                    }
                }
                //disableEntities.ForEach(x => entityManager.SetEnabled(x, false));
                k++;
            }
            
            //entities.Dispose();
        }
        void SetSegmentedPathSize(ref DynamicBuffer<OptimalPointEntityElement> optimalPointEntityElements,int segmentedPathSize)
        {
            int length = optimalPointEntityElements.Length;
            for (int i = 0; i < length; i++)
            {
                PlayerDataComponent playerDataComponent = entityManager.GetComponentData<PlayerDataComponent>(optimalPointEntityElements[i].entity);
                playerDataComponent.segmentedPathSize = segmentedPathSize;
                entityManager.SetComponentData<PlayerDataComponent>(optimalPointEntityElements[i].entity, playerDataComponent);
                //PlayerDataComponent playerDataComponent = systemBase.GetComponentDataFromEntity<PlayerDataComponent>(optimalPointEntityElements[i].entity);
            }
        }
    }
}
