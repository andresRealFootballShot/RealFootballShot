using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace DOTS_PerfectPass
{/*
    public partial class PerfectPassSystemDOTS : SystemBase
    {
        
        EntityQuery optimalPointQuery;
        public NativeList<Entity> segmentedPath_OptimalPointConnections;
        public NativeList<int> segmentedPathToRemoveCounts;
        List<Entity> enableEntities = new List<Entity>();
        List<Entity> disableEntities = new List<Entity>();

        protected override void OnCreate()
        {
            var description2 = new EntityQueryDesc()
            {
                All = new ComponentType[]
                       {ComponentType.ReadOnly<PlayerDataComponent>()}
            };
            optimalPointQuery = this.GetEntityQuery(description2);
            segmentedPath_OptimalPointConnections = new NativeList<Entity>(Allocator.Persistent);
            segmentedPathToRemoveCounts = new NativeList<int>(Allocator.Persistent);


        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            segmentedPath_OptimalPointConnections.Dispose();
            segmentedPathToRemoveCounts.Dispose();
        }
        protected override void OnUpdate()
        {

            int k = 0;
            foreach (var segmentedPath_OptimalPointConnection in segmentedPath_OptimalPointConnections)
            {
                enableEntities.Clear();
                disableEntities.Clear();
                DynamicBuffer<SegmentedPathElement> segmentedPathBuffer = GetBuffer<SegmentedPathElement>(segmentedPath_OptimalPointConnection);
                DynamicBuffer<OptimalPointListEntityElement> optimalPointList = GetBuffer<OptimalPointListEntityElement>(segmentedPath_OptimalPointConnection);
                //segmentedPathToRemoveCounts[k] = 0;
                int optimalPointIndex = 0;
                int segmentedPathIndex = 0;
                int segmentedPathCount = 0;
                int disableCount = 0;
                for (int j = 0; j < segmentedPathBuffer.Length; j++)
                {
                    SegmentedPathElement segmentedPathElement = segmentedPathBuffer[j];
                    if (segmentedPathIndex == 0)
                    {
                        disableCount++;
                    }
                    DynamicBuffer<OptimalPointEntityElement> optimalPointEntityElements = GetBuffer<OptimalPointEntityElement>(optimalPointList[optimalPointIndex].entity);
                    int length = optimalPointEntityElements.Length;
                    for (int i = 0; i < length; i++)
                    {
                        //PlayerDataComponent playerDataComponent = GetComponent<PlayerDataComponent>(optimalPointEntityElements[i].entity);
                        //SetComponent<PlayerDataComponent>(optimalPointEntityElements[i].entity, playerDataComponent);
                        DynamicBuffer<SegmentedPathElement> playerSegmentedPathBuffer = GetBuffer<SegmentedPathElement>(optimalPointEntityElements[i].entity);
                        playerSegmentedPathBuffer.Add(segmentedPathElement);
                        if (segmentedPathIndex == 0)
                        {
                            enableEntities.Add(optimalPointEntityElements[i].entity);
                        }
                    }
                    segmentedPathIndex++;
                    segmentedPathCount++;
                    if (segmentedPathIndex >= optimalPointList[optimalPointIndex].segmentedPathSize)
                    {
                        optimalPointIndex++;
                        segmentedPathIndex = 0;
                    }
                    if (optimalPointIndex >= optimalPointList.Length)
                    {
                        break;
                    }
                }
                segmentedPathToRemoveCounts[k] = segmentedPathCount;

                //enableEntities.ForEach(x => entityManager.SetEnabled(x, true));
                optimalPointList = GetBuffer<OptimalPointListEntityElement>(segmentedPath_OptimalPointConnection);
                for (int i = disableCount; i < optimalPointList.Length; i++)
                {
                    DynamicBuffer<OptimalPointEntityElement> optimalPointEntityElements = GetBuffer<OptimalPointEntityElement>(optimalPointList[i].entity);

                    foreach (var optimalPointEntityElement in optimalPointEntityElements)
                    {
                        disableEntities.Add(optimalPointEntityElement.entity);
                    }
                }
                //disableEntities.ForEach(x => entityManager.SetEnabled(x, false));
                k++;
            }




            var optimalPointJob = new OptimalPointJob();
            var ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            optimalPointJob.playerDataHandle = this.GetComponentTypeHandle<PlayerDataComponent>(true);
            optimalPointJob.ConcurrentCommands = ecbSystem.CreateCommandBuffer().AsParallelWriter();
            optimalPointJob.areaPlanesHandle = GetBufferTypeHandle<AreaPlaneElement>(true);
            optimalPointJob.segmentedPathsHandle = GetBufferTypeHandle<SegmentedPathElement>(false);
            this.Dependency = optimalPointJob.ScheduleParallel(optimalPointQuery, 1, this.Dependency);
            ecbSystem.AddJobHandleForProducer(this.Dependency);
            //entities.Dispose();
        }
}*/
}

