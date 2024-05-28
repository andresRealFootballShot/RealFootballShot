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
    EntityQuery searchLonelyPointsquery;
    public CullPassPoints CullPassPoints;
    public SearchLonelyPointsManager SearchLonelyPointsManager;
    EntityManager entityManager;
    protected override void OnCreate()
    {
        var description1 = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(LonelyPointElement2), typeof(CullPassPointsComponent), typeof(PlayerPositionElement) }
        };
        cullPassPointsQuery = this.GetEntityQuery(description1);

        var description2 = new EntityQueryDesc()
        {
            All = new ComponentType[]
                       {typeof(PointElement)}
        };
        searchLonelyPointsquery = this.GetEntityQuery(description2);

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

        SearchLonelyPointsManager.setEntitiesEnable2(false);
        SearchLonelyPointsManager.setEntitiesEnable(false);
        UpdateNextPlayerPositions();

        CalculateLonelyPoints();

    }
    void CalculateLonelyPoints()
    {


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
    }
    void UpdateNextPlayerPositions()
    {
        int sortLonelyPointsSize = CullPassPoints.sortLonelyPointsSize[0];
        Team defenseTeam = Teams.getTeamByName(CullPassPoints.teamName_Defense);
        CullPassPoints.SetAllLonelyPointsCalculateNextPositionParameters(sortLonelyPointsSize, FieldPositionsData.HorizontalPositionType.Right,defenseTeam,0);
        CullPassPoints.calculateNextPositionShedule.SheduleJobs(sortLonelyPointsSize, defenseTeam.teamMaxFieldPlayers/2, CullPassPoints.lineupName, CullPassPoints.pressureName);

        CullPassPoints.UpdateNextPlayerPoints(sortLonelyPointsSize, 0, FieldPositionsData.HorizontalPositionType.Right, defenseTeam, defenseTeam.playersNoGoalkeeperCount / 2);
    }
}
