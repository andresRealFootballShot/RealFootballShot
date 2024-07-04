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
    SearchPlayData SearchPlayData { get => CullPassPoints.searchPlayData; }
    public List<int> Snodes,Fnodes;
    CullPassPoints.CullPassPointsParams cullPassPointsParams { get => CullPassPoints.cullPassPointsParams; }
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
        if (SearchPlayData.size==0)
        {
            if (CullPassPoints.test)
            {
                CullPassPoints.PlaceTestLonelyPoint();
            }
            else
            {
                CullPassPoints.PlacePoints(0);
            }
        }
        
        for (int i = 0; i < 2; i++)
        {
            int nodeSize = CullPassPoints.sortLonelyPointsSize[i];
            int nextNodeSize = CullPassPoints.sortLonelyPointsSize[i];
            SearchPlayData.getSortedNodes(ref Snodes, nodeSize);
            SearchPlayData.getFreeNodes(ref Fnodes, nodeSize);
            CullPassPoints.UpdatePlayerPositions(Snodes,Fnodes, nodeSize);
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

            UpdateNextPlayerPositions(Snodes, nodeSize, nextNodeSize);
        }
        
    }
    void DistribuiteLonelyPointsToNodes()
    {

    }
    void CalculateLonelyPoints()
    {


        var searchLonelyPointsJob = new SearchLonelyPointsJob();
        searchLonelyPointsJob.pointsHandle = this.GetBufferTypeHandle<PointElement>(true);
        searchLonelyPointsJob.edgesHandle = this.GetBufferTypeHandle<EdgeElement>(false);
        searchLonelyPointsJob.trianglesHandle = this.GetBufferTypeHandle<TriangleElement>(false);
        searchLonelyPointsJob.lonelyPointsHandle = this.GetBufferTypeHandle<LonelyPointElement>(false);
        searchLonelyPointsJob.pointBufferSizeComponentHandle = this.GetComponentTypeHandle<BufferSizeComponent>(false);
        Dependency = searchLonelyPointsJob.ScheduleParallel(searchLonelyPointsquery, 1, this.Dependency);
        Dependency.Complete();
        //SearchLonelyPointsManager.setEntitiesEnable(false);
    }
    void UpdateNextPlayerPositions(List<int> nodes, int nodeSize,int nextNodeSize)
    {
        int calculationIndex = CullPassPoints.debugOrderLonelyPointIndex;
        

        Team defenseTeam = Teams.getTeamByName(CullPassPoints.teamName_Defense);
        if (defenseTeam.publicPlayerDatas.Count == 0) return;
        CullPassPoints.SetAllLonelyPointsCalculateNextPositionParameters(FieldPositionsData.HorizontalPositionType.Right,defenseTeam, nodes,nodeSize, nextNodeSize);

        //int posibleLonelyPointSize = CullPassPoints.GetPosibleLonelyPoints(nodeSize1);
        CullPassPoints.calculateNextPositionShedule.SheduleJobs(nodes, nodeSize, nextNodeSize, SearchPlayData, defenseTeam.teamMaxFieldPlayers/2, CullPassPoints.lineupName, CullPassPoints.pressureName);
        CullPassPoints.UpdateNextPlayerPoints(ref nodes, nodeSize, FieldPositionsData.HorizontalPositionType.Right, defenseTeam, defenseTeam.playersNoGoalkeeperCount / 2);
        CullPassPoints.CompleteTriangulatorJob(nodes, nodeSize);
    }
}
