using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using DOTS_ChaserDataCalculation;
using FieldTriangleV2;
namespace NextMove_Algorithm
{
    public partial class NextMoveSystem : SystemBase
    {
        EntityQuery searchLonelyPointsquery;
        public InitialNextMoveCreator InitialNextMoveCreator = new InitialNextMoveCreator();
        
        
        public SearchLonelyPointsManager NextMoveSystemManager;
        protected override void OnCreate()
        {
            var description1 = new EntityQueryDesc()
            {
                All = new ComponentType[]
                       {typeof(PointElement)}
            };
            searchLonelyPointsquery = this.GetEntityQuery(description1);
        }
        
        protected override void OnUpdate()
        {
            //InitialNextMoveCreator.Update();
            foreach (var teamName in NextMoveSystemManager.teamNames)
            {
                NextMoveSystemManager.UpdatePoints(teamName);
            }

            var searchLonelyPointsJob = new SearchLonelyPointsJob();
            searchLonelyPointsJob.pointsHandle = this.GetBufferTypeHandle<PointElement>(true);
            searchLonelyPointsJob.edgesHandle = this.GetBufferTypeHandle<EdgeElement>(false);
            searchLonelyPointsJob.trianglesHandle = this.GetBufferTypeHandle<TriangleElement>(false);
            searchLonelyPointsJob.lonelyPointsHandle = this.GetBufferTypeHandle<LonelyPointElement>(false);
            searchLonelyPointsJob.pointBufferSizeComponentHandle = this.GetComponentTypeHandle<BufferSizeComponent>(false);
            //var ecb = new EntityCommandBuffer(Allocator.TempJob);
            //EntityCommandBuffer.ParallelWriter ecbParallel = ecb.AsParallelWriter();

            //searchLonelyPointsJob.ConcurrentCommands = ecbParallel;
            Dependency = searchLonelyPointsJob.ScheduleParallel(searchLonelyPointsquery, 1, this.Dependency);
            Dependency.Complete();
            //ecb.Playback(this.EntityManager);
            //ecb.Dispose();
        }
    }
}
