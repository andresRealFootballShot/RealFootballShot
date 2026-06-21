using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static UnityEngine.GraphicsBuffer;

public partial class ShotSystem : SystemBase
{
    public CullPassPoints CullPassPoints; 
    public NativeArray<ShotCandidate> candidates = default;
    public NativeArray<float3> defenders = default;
    public NativeArray<ShotResult> results = default;
    protected override void OnUpdate()
    {
        candidates = default;
        defenders = default;
        results = default;
        try
        {
            Team defenseTeam = CullPassPoints.defenseTeam;

            candidates =
                BuildCandidates(
                    defenseTeam.SideOfField.goalComponents,
                    defenseTeam);

            defenders =
                BuildDefenders(defenseTeam);

            results =
                new NativeArray<ShotResult>(
                    candidates.Length,
                    Allocator.TempJob);

            ShotJob job = new ShotJob
            {
                candidates = candidates,
                defenders = defenders,
                results = results
            };

            JobHandle handle =
                job.Schedule(candidates.Length, 32);

            handle.Complete();

            CullPassPoints.bestShot = default;
            CullPassPoints.bestShot.score = -999999f;

            for (int i = 0; i < results.Length; i++)
            {
                if (results[i].valid &&
                    results[i].score > CullPassPoints.bestShot.score)
                {
                    CullPassPoints.bestShot = results[i];
                }
            }
        }
        finally
        {
            if (candidates.IsCreated)
                candidates.Dispose();

            if (defenders.IsCreated)
                defenders.Dispose();

            if (results.IsCreated)
                results.Dispose();
        }
    }

    NativeArray<ShotCandidate> BuildCandidates(
        GoalComponents goalComponents,
        Team team)
    {
        float3 BL = goalComponents.bottomLeft.position;
        float3 BR = goalComponents.bottomRight.position;
        float3 TL = goalComponents.topLeft.position;
        float3 TR = goalComponents.topRight.position;
        float3 center = goalComponents.centerOptimalPosition.position;
        float goalWidth = math.distance(BL, BR);
        float goalHalfWidth = goalWidth * 0.5f;
        int xSteps = 6;
        int ySteps = 4;

        int count = (xSteps + 1) * (ySteps + 1);

        NativeArray<ShotCandidate> arr =
            new NativeArray<ShotCandidate>(
                count,
                Allocator.TempJob);

        int id = 0;

        PublicGoalkeeperData goalkeeperPublicPlayerData =
            team.getGoalkeeperPublicPlayerData()
            as PublicGoalkeeperData;
        PublicPlayerData passer = CullPassPoints.attackTeam.firstReachBallPublicPlayerData;
        if (passer == null) return arr;
        if (goalkeeperPublicPlayerData != null)
        {
            for (int y = 0; y <= ySteps; y++)
            {
                float fy = y / (float)ySteps;

                float3 left =
                    math.lerp(BL, TL, fy);

                float3 right =
                    math.lerp(BR, TR, fy);

                for (int x = 0; x <= xSteps; x++)
                {
                    //if(y!=2||x!=3)continue;
                    float fx = x / (float)xSteps;
                    if (id >= arr.Length) break;
                    float3 targetPosition = math.lerp(left, right, fx);
                    arr[id] = new ShotCandidate
                    {
                        target = targetPosition,

                        ballPos =
                            MatchComponents.ballComponents.position,

                        goalkeeperPos =
                            goalkeeperPublicPlayerData.position,

                        goalkeeperSpeed =
                            goalkeeperPublicPlayerData.values.maxSpeed,

                        goalkeeperMaxHeight =
                            goalkeeperPublicPlayerData.values.maxHeightInArea,

                        goalCenter = center,
                        goalHalfWidth = goalHalfWidth,
                        maxKickForce = 35f,

                        minTime = 0.05f,
                        maxTime = 1.8f,

                        k = MatchComponents.ballRigidbody.drag,
                        vf = 9.81f / MatchComponents.ballRigidbody.drag,
                        reflex = goalkeeperPublicPlayerData.values.maxRandomReflexes,
                        passerPos = passer.position,
                        maxAngleControl = passer.playerComponents.playerSkills.MaxAngleControl,
                        maxVelocityControl = BotControl.GetMaxVelocityControl(targetPosition, MatchComponents.ballComponents.position, passer.playerComponents.playerSkills,1.25f),
                        ballSpeed = MatchComponents.ballRigidbody.velocity.magnitude,

                    };
                    id++;
                }
            }
        }

        return arr;
    }

    NativeArray<float3> BuildDefenders(Team team)
    {
        NativeArray<float3> defenders =
            new NativeArray<float3>(
                team.outfieldPublicPlayerDatas.Count,
                Allocator.TempJob);

        for (int i = 0; i < team.outfieldPublicPlayerDatas.Count; i++)
        {
            defenders[i] =
                team.outfieldPublicPlayerDatas[i].position;
        }

        return defenders;
    }
}

public struct ShotCandidate
{
    public float3 target;
    public float3 ballPos;
    public float ballSpeed;
    public float3 goalkeeperPos;
    public float goalkeeperSpeed;
    public float goalkeeperMaxHeight;
    public float reflex;
    public float3 goalCenter;
   
    public float goalHalfWidth;
    public float maxKickForce;

    public float minTime;
    public float maxTime;

    public float k;
    public float vf;
    public float3 passerPos;
    public float maxAngleControl,maxVelocityControl;
}
public struct ShotResult
{
    public bool valid;
    public float score;

    public float3 target;
    public float3 v0;
    public float time;
}