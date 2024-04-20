using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Collections;
using Unity.Jobs;
using DOTS_ChaserDataCalculation;
public class SearchPathSystem : SystemBase
{
    EntityQuery searchPathQuery,getPassV0Query,optimalPointQuery,segmentedPathQuery,testQuery;
    EntityQuery getPerfectPassQuery;
    public GetPerfectPassManager SearchPathManager;
    public Entity GetPassV0Entity;
    EntityManager entityManager;
    public Entity GetTimeToReachPointEntity;
    OptimalPointDOTSCreator OptimalPointDOTSCreator { get => SearchPathManager.OptimalPointDOTSCreator; }
    OptimalPointSystem OptimalPointSystem { get => SearchPathManager.OptimalPointSystem; }
    public List<OptimalPointReference> OptimalPointReferences { get => SearchPathManager.OptimalPointReferences; }
    public SegmentedPathParams SegmentedPathParams { get => SearchPathManager.segmentedPathParams; }
    bool test;
    bool test2;
    protected override void OnCreate()
    {
        var description1 = new EntityQueryDesc()
        {
            All = new ComponentType[]
                       {typeof(GetTimeToReachPointElement),ComponentType.ReadOnly<PlayerAttackElement>()}
        };
        var description2 = new EntityQueryDesc()
        {
            All = new ComponentType[]
                       {typeof(GetPassV0Element)}
        };
        var description3 = new EntityQueryDesc()
        {
            All = new ComponentType[]
                       {typeof(PlayerDataComponent),typeof(SegmentedPathElement)}
        };
        var description4 = new EntityQueryDesc()
        {
            All = new ComponentType[]
                       {typeof(PathComponent)}
        };
        var description5 = new EntityQueryDesc()
        {
            All = new ComponentType[]
                       {typeof(GetPerfectPassComponent)}
        };
        var description6 = new EntityQueryDesc()
        {
            All = new ComponentType[]
                       {ComponentType.ReadOnly<AreaPlaneElement>()}
        };
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        searchPathQuery = this.GetEntityQuery(description1);
        getPassV0Query = this.GetEntityQuery(description2);
        optimalPointQuery = this.GetEntityQuery(description3);
        segmentedPathQuery = this.GetEntityQuery(description4);
        getPerfectPassQuery = this.GetEntityQuery(description5);
        testQuery = this.GetEntityQuery(description6);

    }
    protected override void OnDestroy()
    {
        //OptimalPointSystem.OnDestroy();
    }
    protected override void OnUpdate()
    {
        
        if (SearchPathManager.v1)
        {
            searchPathV1();
        }
        else
        {
            searchPathV2();
        }


        //Profiler.BeginSample("MyTest");
        //Profiler.EndSample();
        
    }
    void TestJob()
    {
        TestJob testJob = new TestJob();
        //testJob.PathComponentHandle = GetComponentTypeHandle<PathComponent>(true);
        testJob.areaPlanesHandle = GetBufferTypeHandle<AreaPlaneElement>(true);
        Dependency = testJob.ScheduleParallel(testQuery, SearchPathManager.batchesPerChunk, this.Dependency);
    }
    void searchPathV2()
    {
        if (!test)
            SearchPathManager.updateGetTimeToReachPoint();
        var getTimeToReachPointJob = new GetTimeToReachPointJob();

        //var ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        getTimeToReachPointJob.GetTimeToReachPointElementBufferHandle = GetBufferTypeHandle<GetTimeToReachPointElement>(false);
        getTimeToReachPointJob.PlayerDataComponentElementBufferHandle = GetBufferTypeHandle<PlayerAttackElement>(true);
        Dependency = getTimeToReachPointJob.ScheduleParallel(searchPathQuery, 1, this.Dependency);
        Dependency.Complete();

        getPassV0();
        //ecbSystem.AddJobHandleForProducer(Dependency);
        //test = true;
        //test2 = true;
    }
    void searchPathV1()
    {
        PlayGetPerfectPassJob();
    }
    void PlayGetPerfectPassJob()
    {
        //NativeArray<float> results = new NativeArray<float>(20, Allocator.TempJob);

        //TestJob testJob = new TestJob();
        //testJob.results = results;
        //testJob.TestComponentHandle = GetComponentTypeHandle<TestComponent>(false);
        //JobHandle jobHandle2 = testJob.ScheduleParallel(testQuery, 1);
        //Dependency = testJob.ScheduleParallel(testQuery, 1);
        //Dependency.Complete();

        var GetPerfectPassJob = new GetPerfectPassJob();
        //GetPerfectPassJob.results = results;
        //var ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        GetPerfectPassJob.GetPerfectPassComponentHandle = GetComponentTypeHandle<GetPerfectPassComponent>(false);
        GetPerfectPassJob.PathComponentHandle = GetComponentTypeHandle<PathComponent>(true);
        GetPerfectPassJob.PlayerAttackElementBufferHandle = GetBufferTypeHandle<PlayerAttackElement>(true);
        GetPerfectPassJob.PlayerDefenseElementBufferHandle = GetBufferTypeHandle<PlayerDefenseElement>(true);
        GetPerfectPassJob.areaPlanesHandle = GetBufferTypeHandle<AreaPlaneElement>(true);
        GetPerfectPassJob.ChaserDataElementBufferHandle = GetBufferTypeHandle<ChaserDataElement>(false);
        GetPerfectPassJob.segmentedPathsHandle = GetBufferTypeHandle<SegmentedPathElement>(false);
        //Dependency = GetPerfectPassJob.ScheduleParallel(getPerfectPassQuery, 1, this.Dependency);
        Dependency = GetPerfectPassJob.ScheduleParallel(getPerfectPassQuery, SearchPathManager.batchesPerChunk, this.Dependency);

        Dependency.Complete();
        //jobHandle2.Complete();
        //results.Dispose();
        //Dependency.Complete();
    }
    void playSegmentedPathJobs()
    {
        
        var segmentedPathJob = new SegmentedPathJob();
        segmentedPathJob.pathComponentHandle = this.GetComponentTypeHandle<PathComponent>(false);
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        EntityCommandBuffer.ParallelWriter ecbParallel = ecb.AsParallelWriter();

        segmentedPathJob.ConcurrentCommands = ecbParallel;
        Dependency = segmentedPathJob.ScheduleParallel(segmentedPathQuery, 1, this.Dependency);
        Dependency.Complete();
        ecb.Playback(this.EntityManager);

        ecb.Dispose();
    }
    void updatePathComponents()
    {
        DynamicBuffer<GetPassV0Element> GetPassV0Elements = entityManager.GetBuffer<GetPassV0Element>(GetPassV0Entity);
        GetPassV0Element getPassV0Element = GetPassV0Elements[0];
        UpdateOptimalPointData updateOptimalPointData = new UpdateOptimalPointData();
        updateOptimalPointData.v0 = getPassV0Element.result.v0;
        updateOptimalPointData.pos0 = getPassV0Element.Pos0;

        foreach (var OptimalPointReference in OptimalPointReferences)
        {
            OptimalPointDOTSCreator.updatePathComponents(OptimalPointReference, SegmentedPathParams, updateOptimalPointData);
        }

    }
    void updateOptimalPoints()
    {
        foreach (var OptimalPointReference in OptimalPointReferences)
        {
            OptimalPointDOTSCreator.updatePlayerDataComponents(OptimalPointReference);
        }
        //Profiler.BeginSample("MyTest4");
            OptimalPointSystem.OnUpdate(this);
        //Profiler.EndSample();
        
        
    }
    void playOptimalPointJobs()
    {
        var optimalPointJob = new OptimalPointJob();
        var ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        optimalPointJob.playerDataHandle = GetComponentTypeHandle<PlayerDataComponent>(false);
        optimalPointJob.ConcurrentCommands = ecbSystem.CreateCommandBuffer().AsParallelWriter();
        optimalPointJob.areaPlanesHandle = GetBufferTypeHandle<AreaPlaneElement>(true);
        optimalPointJob.segmentedPathsHandle = GetBufferTypeHandle<SegmentedPathElement>(false);
        Dependency = optimalPointJob.ScheduleParallel(optimalPointQuery, 1, Dependency);
        ecbSystem.AddJobHandleForProducer(Dependency);
        Dependency.Complete();
    }
    void updateChaserDatas()
    {
        foreach (var OptimalPointReference in OptimalPointReferences)
        {
            OptimalPointDOTSCreator.updateChaserDataOfPublicPlayerData(OptimalPointReference);
        }
        foreach (var OptimalPointReference in OptimalPointReferences)
        {
            OptimalPointDOTSCreator.clearSegmentedPaths(OptimalPointReference);
        }
    }
    void updateGetPassV0()
    {
        

        DynamicBuffer<GetTimeToReachPointElement> GetTimeToReachPointElements = entityManager.GetBuffer<GetTimeToReachPointElement>(GetTimeToReachPointEntity);
        GetTimeToReachPointElement GetTimeToReachPointElement = GetTimeToReachPointElements[0];

        DynamicBuffer<GetPassV0Element> GetPassV0Elements = entityManager.GetBuffer<GetPassV0Element>(GetPassV0Entity);
        GetPassV0Element getPassV0Element = GetPassV0Elements[0];

        
        getPassV0Element.Pos0 = MyFunctions.setY0ToVector3(MatchComponents.ballPosition);
        getPassV0Element.Posf = SearchPathManager.targetPosition.position;
        GetPassV0Elements[0] = getPassV0Element;
    }
    void getPassV0()
    {
        if (!test)
            updateGetPassV0();
        var getPassV0Job = new GetPassV0Job();
        getPassV0Job.GetPassV0ElementBufferHandle = GetBufferTypeHandle<GetPassV0Element>(false);
        Dependency = getPassV0Job.ScheduleParallel(getPassV0Query, 1, this.Dependency);
        Dependency.Complete();
        if (!test)
            updatePathComponents();
        playSegmentedPathJobs();
        if (!test && !test2)
        {
            updateOptimalPoints();
        }
        playOptimalPointJobs();
        if (!test)
            updateChaserDatas();
        
    }
}
