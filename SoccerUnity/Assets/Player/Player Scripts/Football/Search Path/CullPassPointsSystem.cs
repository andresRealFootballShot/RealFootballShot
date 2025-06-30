using FieldTriangleV2;
using NextMove_Algorithm;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using CullPositionPoint;
using static UnityStandardAssets.Utility.TimedObjectActivator;
using Unity.Collections;
using System;
using Unity.Jobs;

[UpdateAfter(typeof(NextMoveSystem))]
public class CullPassPointsSystem : SystemBase
{
    EntityQuery cullPassPointsQuery;
    EntityQuery searchLonelyPointsquery;
    public CullPassPoints CullPassPoints;
    public SearchLonelyPointsManager SearchLonelyPointsManager;
    EntityManager entityManager;
    SearchPlayData SearchPlayData { get => CullPassPoints.searchPlayData; }
    public List<int> Snodes = new List<int>(), Fnodes = new List<int>();
    CullPassPoints.CullPassPointsParams cullPassPointsParams { get => CullPassPoints.cullPassPointsParams; }
    int nodeCalculationPerFrameTotal = 0;
    int nodeCount;
    int currentNodeSize, nextNodeSize;
    int nodeSizeIndex;
    int calculateNodeCount,calculateNodeCount2;
    int startNode;
    public bool enable;
    int nextNodeSize2;
    int previous;
    bool reset=true;
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
    bool checkParameters()
    {
        if (CullPassPoints==null||!enable||!CullPassPoints.enableCullPassPointsSystem)
        {
            enable = true;
            return false;
        }

        return true;
    }
    protected override void OnUpdate()
    {
        
        if (!checkParameters()) return;
        try
        {
            
        Team defenseTeam = Teams.getTeamByName(CullPassPoints.teamName_Defense);
        Team attackTeam = Teams.getTeamByName(CullPassPoints.teamName_Attacker);
        //SearchPlayData.ClearCullEntities();
        if (reset)
        {
            SearchPlayData.Clear();
           
            CullPassPoints.PlacePoints(0);
            Snodes.Clear();
            SearchPlayData.getSortedNodes(ref Snodes, 1);
            //SearchPlayData.getNode(Snodes, 0);
            CullPassPoints.SetBallPosition(Snodes,1);
            nodeCalculationPerFrameTotal = 1;
            SearchPlayData.posibleNodes.Clear();

            CullPassPoints.UpdateInstantPlayerPositions(defenseTeam, attackTeam, Snodes);
            CullPassPoints.UpdatePlayerPositions(Snodes, 1, 0);
            SearchPlayData.SetIsSearched(Snodes[0], true);
            SearchPlayData.SetIsBusy(Snodes[0], true);
            currentNodeSize = 1;
            nextNodeSize = 0;
            nodeCount = 1;
            nodeSizeIndex=0;
            startNode = 0;
            SearchPlayData.searchingNodesCount = 1;
            nextNodeSize2 = 1;
            calculateNodeCount = 0;
            previous = 1;
            calculateNodeCount2 = 0;
            reset = false;
        }
        
       
        // Código que podría lanzar una excepción
        int nodeCalculationPerFrame = CullPassPoints.cullPassPointsParams.nodeCalculationPerFrame;
        int i = 0;
        
        while (i< CullPassPoints.cullPassPointsParams.repetitionPerFrame)
        {

            var CullPassPointsJob = new CullPassPointsJob();

            CullPassPointsJob.lonelyPointsHandle = this.GetBufferTypeHandle<LonelyPointElement2>(false);
            CullPassPointsJob.playerPositionElementHandle = this.GetBufferTypeHandle<PlayerPositionElement>(true);
            CullPassPointsJob.cullPassPointsParamsHandle = this.GetComponentTypeHandle<CullPassPointsComponent>(true);
            CullPassPointsJob.BallParamsComponentHandle = this.GetComponentTypeHandle<BallParamsComponent>(true);
            CullPassPointsJob.TestResultComponentHandle = this.GetComponentTypeHandle<TestResultComponent>(false);
            //Dependency = CullPassPointsJob.ScheduleParallel(cullPassPointsQuery, CullPassPoints.batchesPerChunk, this.Dependency);
                //Dependency.Complete();
                JobHandle handle = CullPassPointsJob.Schedule(cullPassPointsQuery, Dependency);
                handle.Complete();
                
            
            if(nodeSizeIndex < CullPassPoints.sortLonelyPointsSize.Count-1)
            {
                    if(calculateNodeCount2 <= 0)
                    {
                        previous = previous * CullPassPoints.sortLonelyPointsSize[nodeSizeIndex];
                        nextNodeSize2 = nextNodeSize2 * CullPassPoints.sortLonelyPointsSize[nodeSizeIndex + 1];
                        calculateNodeCount2 = nextNodeSize2;
                        currentNodeSize = currentNodeSize * CullPassPoints.sortLonelyPointsSize[nodeSizeIndex];
                        nextNodeSize = CullPassPoints.sortLonelyPointsSize[nodeSizeIndex + 1];
                        nodeCount = 0;
                        nodeSizeIndex++;
                    }
                    CullPassPoints.getDebugWeightPoints(Snodes);

                    CullPassPoints.SetAllLonelyPointsCalculateNextPositionParameters(FieldPositionsData.HorizontalPositionType.Right, defenseTeam, Snodes, Fnodes, nodeCalculationPerFrameTotal, nextNodeSize, out int calculateNodeCount3, startNode, nodeCalculationPerFrame, nextNodeSize2, previous);
                    calculateNodeCount += calculateNodeCount3;

               

                startNode += currentNodeSize;
                SearchPlayData.ClearCullEntities();
            }

            if(nodeCalculationPerFrameTotal < CullPassPoints.maxNodes2)
            {
                int size = Mathf.Clamp(calculateNodeCount, 0, nodeCalculationPerFrame);
                UpdateNextPlayerPositions(Snodes, Fnodes, nodeCalculationPerFrameTotal, nextNodeSize, size, defenseTeam, nodeCalculationPerFrameTotal);
                CullPassPoints.PlacePoints2(SearchPlayData.posibleNodes, size, nodeCalculationPerFrameTotal);
                CullPassPoints.UpdatePlayerPositions(SearchPlayData.posibleNodes, size, nodeCalculationPerFrameTotal);
                CullPassPoints.SetBallPosition2(SearchPlayData.posibleNodes, size);

                SearchPlayData.SetIsSearched(SearchPlayData.posibleNodes, true);
                //Snodes = Fnodes;
                CopyNodes(Snodes, SearchPlayData.posibleNodes, size);
                SearchPlayData.searchingNodesCount += size;
                nodeCalculationPerFrameTotal += size;
                RemovePosibleNodes(size);
                nodeCount += size;
                calculateNodeCount -= size;
                calculateNodeCount2 -= size;
                i++;
            }
            if(nodeCalculationPerFrameTotal >= CullPassPoints.maxNodes2)
            {
                reset = true;
                break;
            }
            
        }
        }
        catch (Exception e)
        {
            Debug.LogError("Error: " + e.Message + "\n" + e.StackTrace);
        }
        
    }
    
    void CopyNodes(List<int> nodes, List<int> copies,int size)
    {
        //nodes.Clear();
        for (int i = 0; i < size; i++)
        {
            nodes.Add(copies[i]);
            //nodes[i]=copies[i];
        }
    }
    void RemovePosibleNodes(int size)
    {
        for (int i = 0; i < size; i++)
        {
            SearchPlayData.posibleNodes.RemoveAt(0);
        }
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
    void UpdateNextPlayerPositions(List<int> Snodes, List<int> Fnodes , int nodeSizeTotal,int newNodeSize,int nodeCalculationPerFrame, Team defenseTeam,int startNode)
    {
        if (defenseTeam.publicPlayerDatas.Count == 0) return;
        
        

        //int posibleLonelyPointSize = CullPassPoints.GetPosibleLonelyPoints(nodeSize1);
        CullPassPoints.calculateNextPositionShedule.SheduleJobs(nodeCalculationPerFrame, SearchPlayData, defenseTeam.teamMaxFieldPlayers/2, CullPassPoints.lineupName, CullPassPoints.pressureName);
        CullPassPoints.UpdateNextPlayerPoints(nodeCalculationPerFrame, FieldPositionsData.HorizontalPositionType.Right, defenseTeam, defenseTeam.playersNoGoalkeeperCount / 2);
        CullPassPoints.CompleteTriangulatorJob(nodeCalculationPerFrame);
    }
}
