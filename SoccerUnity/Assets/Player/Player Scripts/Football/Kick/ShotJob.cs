using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct ShotJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<ShotCandidate> candidates;
    [ReadOnly] public NativeArray<float3> defenders;

    public NativeArray<ShotResult> results;

    public void Execute(int index)
    {
        ShotCandidate c = candidates[index];
        if (BotControl.CheckBallControl(c))
        {
            return;
        }
        ShotResult best = default;
        best.score = -999999f;

        float left = c.minTime;
        float right = c.maxTime;

        for (int i = 0; i < 8; i++)
        {
            float t = (left + right) * 0.5f;

            GetV0DOTSResultBurst r2 = default;

            ParabolicPassBurst.GetV0(
                c.ballPos,
                c.target,
                ref r2,
                c.maxKickForce,1000,0.1f,
                t,
                c.k,
                c.vf,c.ballSpeed);
            if (!r2.foundedResult)
            {
                left = t;
                continue;
            }

            if (r2.maxKickForceReached)
            {
                right = t;
                continue;
            }

            if (DefenderBlocks(c.ballPos, c.target))
            {
                left = t;
                continue;
            }

            if (GoalkeeperBlocks(c, r2.v0, t,out float goalkeeperBallDistance))
            {
                left = t;
                continue;
            }
            
            float distToCenter =
    math.distance(c.target, c.goalCenter);
            float centerScore =
    1f - math.saturate(
        distToCenter / c.goalHalfWidth);

            float speedScore =
                1f - math.saturate(
                    r2.v0Magnitude / c.maxKickForce);
            if (r2.v0Magnitude < 7 ) speedScore = 0;
            float goalkeeperBallDistanceScore = Mathf.Clamp01(goalkeeperBallDistance / 5);
            float score =
                centerScore * 100f +
                speedScore * 20f+ goalkeeperBallDistanceScore*20;

            if (score > best.score)
            {
                best.valid = true;
                best.target = c.target;
                best.v0 = r2.v0;
                best.time = t;
                best.score = score;
            }

            right = t;
        }

        results[index] = best;
    }

    bool DefenderBlocks(float3 ball, float3 target)
    {
        for (int i = 0; i < defenders.Length; i++)
        {
            float dist =
                DistancePointSegment(
                    defenders[i],
                    ball,
                    target);

            if (dist < 0.45f)
                return true;
        }

        return false;
    }

    float DistancePointSegment(float3 p, float3 a, float3 b)
    {
        float3 ab = b - a;
        float3 ap = p - a;

        float t =
            math.saturate(
                math.dot(ap, ab) /
                math.dot(ab, ab));

        float3 closest = a + ab * t;

        return math.distance(p, closest);
    }

    bool GoalkeeperBlocks(
      ShotCandidate c,
      float3 v0,
      float totalTime,out float distance)
    {
        distance = Mathf.Infinity;
        for (int i = 1; i <= 10; i++)
        {
            float t =
                totalTime * i / 10f;

            float3 ball =
                ParabolicPassBurst.GetPositionAtTime(
                    t,
                    c.ballPos,
                    v0,
                    c.k,
                    c.vf);

            if (ball.y > c.goalkeeperMaxHeight)
                continue;

            float dist =
                math.distance(
                    c.goalkeeperPos,
                    ball);
            if ((dist / c.goalkeeperSpeed)+c.reflex <= t)
            {
                distance = dist;
                return true;
            }
        }

        return false;
    }
}