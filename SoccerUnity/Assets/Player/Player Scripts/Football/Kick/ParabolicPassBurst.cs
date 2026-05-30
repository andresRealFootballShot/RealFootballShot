using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public static class ParabolicPassBurst
{
    public static void GetV0(
        float3 pos0,
        float3 posf,
        ref GetV0BurstResult result,
        float maxKickForce,
        float t,
        float k,
        float g)
    {
        float3 v0 = Parabola(t, pos0, posf, k, g);

        float3 vt = Velocity(t, v0, k, g / k);

        result.ballReachTargetPositionTime = t;

        result.v0 = v0;
        result.v0Magnitude = math.length(v0);
        result.foundedResult = true;

        if (result.v0Magnitude > maxKickForce)
        {
            result.v0 =
                math.normalize(v0) * maxKickForce;

            result.v0Magnitude = maxKickForce;
            result.maxKickForceReached = true;
        }
    }

    // -------------------------------------------------

    public static float3 Parabola(
        float t,
        float3 a,
        float3 b,
        float k,
        float g)
    {
        float2 p0 = new float2(a.x, a.z);
        float2 p1 = new float2(b.x, b.z);

        float2 dir = p1 - p0;

        float d = math.distance(p0, p1);

        float e = 1f - math.exp(-k * t);

        float2 vxz =
            math.normalize(dir) * (d * k / e);

        float dy = b.y - a.y;

        float vf = g / k;

        float vy =
            ((dy + vf * t) * k / e) - vf;

        return new float3(vxz.x, vy, vxz.y);
    }

    // -------------------------------------------------

    public static float3 Velocity(
        float t,
        float3 v0,
        float k,
        float vf)
    {
        float e = math.exp(-k * t);

        return new float3(
            v0.x * e,
            -vf + (v0.y + vf) * e,
            v0.z * e);
    }

    // -------------------------------------------------
}
public struct GetV0BurstResult
{
    public bool foundedResult;

    public float3 v0;
    public float v0Magnitude;

    public float ballReachTargetPositionTime;

    public bool maxKickForceReached;
}