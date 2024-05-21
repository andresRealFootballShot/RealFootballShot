using FieldTriangleV2;
using NextMove_Algorithm;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using CullPositionPoint;
using static UnityStandardAssets.Utility.TimedObjectActivator;
[UpdateAfter(typeof(NextMoveSystem))]
public class CullPassPointsSystem : SystemBase
{
    EntityQuery cullPassPointsQuery;
    public CullPassPoints CullPassPoints;
    EntityManager entityManager;
    protected override void OnCreate()
    {
        var description1 = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(LonelyPointElement2), typeof(CullPassPointsComponent), typeof(PlayerPositionElement) }
        };
        cullPassPointsQuery = this.GetEntityQuery(description1);
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
    protected override void OnUpdate()
    {
        if (CullPassPoints.test)
        {
            //CullPassPoints.PlacePoints();
            CullPassPoints.PlaceTestLonelyPoint();
        }
        else
        {
            CullPassPoints.PlacePoints2();
        }
        CullPassPoints.UpdatePlayerPositions();
        foreach (var entity in CullPassPoints.entities)
        {
            BallParamsComponent BallParamsComponent = entityManager.GetComponentData<BallParamsComponent>(entity);
            CullPassPoints.SetBallPosition(ref BallParamsComponent);
            entityManager.SetComponentData<BallParamsComponent>(entity, BallParamsComponent);
        }
        var CullPassPointsJob = new CullPassPointsJob();

        CullPassPointsJob.lonelyPointsHandle = this.GetBufferTypeHandle<LonelyPointElement2>(false);
        CullPassPointsJob.playerPositionElementHandle = this.GetBufferTypeHandle<PlayerPositionElement>(true);
        CullPassPointsJob.cullPassPointsParamsHandle = this.GetComponentTypeHandle<CullPassPointsComponent>(true);
        CullPassPointsJob.BallParamsComponentHandle = this.GetComponentTypeHandle<BallParamsComponent>(true);
        CullPassPointsJob.TestResultComponentHandle = this.GetComponentTypeHandle<TestResultComponent>(false);
        Dependency = CullPassPointsJob.ScheduleParallel(cullPassPointsQuery, CullPassPoints.batchesPerChunk, this.Dependency);
        Dependency.Complete();
        CullPassPoints.SortAllLonelyPoints();
    }
}
