using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct ShotJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<ShotCandidate> candidates;
    [ReadOnly] public NativeArray<float3> defenders;

    public NativeArray<ShotResult> results;

    public void Execute(int index)
    {
        ShotCandidate c = candidates[index];

        ShotResult best = default;
        best.score = -999999f;

        float left = c.minTime;
        float right = c.maxTime;

        for (int i = 0; i < 8; i++)
        {
            float t = (left + right) * 0.5f;

            GetV0BurstResult r = default;

            ParabolicPassBurst.GetV0(
                c.ballPos,
                c.target,
                ref r,
                c.maxKickForce,
                t,
                c.k,
                c.vf);

            if (!r.foundedResult)
            {
                left = t;
                continue;
            }

            if (r.maxKickForceReached)
            {
                right = t;
                continue;
            }

            if (DefenderBlocks(c.ballPos, c.target))
            {
                left = t;
                continue;
            }

            if (GoalkeeperBlocks(c, r.v0, t))
            {
                left = t;
                continue;
            }

            float score =
                math.abs(c.target.x) - r.v0Magnitude;

            if (score > best.score)
            {
                best.valid = true;
                best.target = c.target;
                best.v0 = r.v0;
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
        float totalTime)
    {
        for (int i = 1; i <= 10; i++)
        {
            float t = totalTime * i / 10f;

            float3 ball =
                ParabolicPassBurst.Velocity(
                    t, v0, c.k, c.vf);

            if (ball.y > c.goalkeeperMaxHeight)
                continue;

            float dist =
                math.distance(
                    new float3(c.goalkeeperPos.x, ball.y, c.goalkeeperPos.z),
                    ball);

            if (dist / c.goalkeeperSpeed <= t)
                return true;
        }

        return false;
    }
}