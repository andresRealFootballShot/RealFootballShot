using Unity.Burst;
using Unity.Mathematics;

public struct GetV0DOTSResultBurst
{
    public bool foundedResult;

    public float vt;

    public float3 v0;
    public float v0Magnitude;

    public bool maximumControlSpeedReached;
    public bool maxKickForceReached;

    public bool noRivalReachTheTargetBeforeMe;
    public bool noPartnerIsAhead;

    public float differenceTimeWithRival;

    public float receiverReachTargetPositionTime;
    public float ballReachTargetPositionTime;
}
[BurstCompile]
public static class ParabolicPassBurst
{
    public static void GetV0(
        float3 pos0,
        float3 posf,
        ref GetV0DOTSResultBurst result,
        float maxKickForce, float minControlSpeed,
        float maxControlSpeed,
        float maxControlSpeedLerpDistance,
        float t,
        float k,
        float vfMagnitude,float3 ballVelocity)
    {
        float3 flatPosf =
            new float3(posf.x, pos0.y, posf.z);
        float3 dir = posf - pos0;
        if (pos0.y > 1) maxKickForce = math.length(CalculateHeaderVelocity(ballVelocity,dir, 7,0.55f,0.85f));
        float d =
            math.distance(pos0, flatPosf);

        maxControlSpeed =
            math.lerp(
                minControlSpeed,
                maxControlSpeed,
                d / maxControlSpeedLerpDistance);

        float3 v0 =
            ParabolaWithDrag_GetV0(
                t,
                pos0,
                posf,
                k,
                9.8f);

        v0.y =
            math.clamp(
                v0.y,
                0f,
                maxControlSpeed);

        float3 vt =
            GetVelocityAtTime(
                t,
                v0,
                k,
                vfMagnitude);

        result.ballReachTargetPositionTime = t;

        float3 vt2 = vt;
        vt2.y = 0f;
        result.foundedResult = true;


        if (math.length(vt2) >= maxControlSpeed || t == 0f)
        {
            float3 v02 =
                GetV0ByVt(
                    maxControlSpeed,
                    pos0,
                    posf,
                    k);

            result.v0 = v02;

            float3 v03 = result.v0;
            v03.y = 0f;

            float t2 =
                GetT(
                    pos0,
                    posf,
                    math.length(v03),
                    k);

            result.v0.y =
                ParabolaWithDrag_GetVY0(
                    t2,
                    pos0,
                    posf,
                    k,
                    9.8f);

            result.v0.y =
                math.clamp(
                    result.v0.y,
                    0f,
                    maxControlSpeed + 0.5f);

            result.v0Magnitude =
                math.length(result.v0);

            result.maximumControlSpeedReached = true;
            result.vt = maxControlSpeed;
            
        }
        else
        {
            result.v0 = v0;
            result.v0Magnitude =
                math.length(v0);
        }

        if (result.v0Magnitude > maxKickForce)
        {
            result.v0 =
                math.normalize(result.v0) *
                maxKickForce;

            result.v0Magnitude =
                maxKickForce;

            result.maxKickForceReached = true;
        }

        result.maximumControlSpeedReached =
            result.vt >= maxControlSpeed;

        if (result.maximumControlSpeedReached ||
            result.maxKickForceReached)
        {
            float3 v0mzx = result.v0;
            v0mzx.y = 0f;

            float t2 =
                GetT(
                    d,
                    math.length(v0mzx),
                    k);

            result.ballReachTargetPositionTime = t2;
        }
    }
    public static float HeadBall(float ballSpeed,float headSpeed)
    {
        float e = 0.5f;
        return (1+e)*headSpeed-e*ballSpeed;
    }
    public static float3 CalculateHeaderVelocity(
    float3 ballVelocity,
    float3 headerDirection,
    float headSpeed,
    float restitution = 0.55f,
    float lateralRetention = 0.85f)
    {
        // Normalizar la dirección del remate
        headerDirection = math.normalize(headerDirection);

        // Componente del balón en la dirección del remate
        float ballParallel = math.dot(ballVelocity, headerDirection);

        // Componente perpendicular
        float3 ballPerpendicular =
            (ballVelocity - ballParallel * headerDirection) * lateralRetention;

        // Nueva velocidad paralela
        float newParallel =
            (1f + restitution) * headSpeed - restitution * ballParallel;

        // Velocidad final
        return ballPerpendicular + newParallel * headerDirection;
    }
   
    public static float3 ParabolaWithDrag_GetV0(
        float t,
        float3 pos0,
        float3 posf,
        float k,
        float g)
    {
        float2 XZ0 =
            new float2(pos0.x, pos0.z);

        float2 XZf =
            new float2(posf.x, posf.z);

        float2 dir = XZf - XZ0;

        float distanceXZ =
            math.distance(XZ0, XZf);

        float e =
            1f - math.exp(-k * t);

        float2 vxz0 =
            math.normalize(dir) *
            (distanceXZ * k / e);

        float distanceY =
            posf.y - pos0.y;

        float vf = g / k;

        float vy0 =
            ((distanceY + vf * t) * k / e) - vf;

        return new float3(
            vxz0.x,
            vy0,
            vxz0.y);
    }
    public static float3 GetPositionAtTime(
    float t,
    float3 pos0,
    float3 v0,
    float k,
    float vf)
    {
        float e = math.exp(-k * t);

        float x =
            pos0.x +
            (v0.x / k) * (1f - e);

        float z =
            pos0.z +
            (v0.z / k) * (1f - e);

        float y =
            pos0.y
            - vf * t
            + ((v0.y + vf) / k) * (1f - e);

        return new float3(x, y, z);
    }
    public static float3 GetVelocityAtTime(
        float t,
        float3 v0,
        float k,
        float vf)
    {
        float ekt =
            math.exp(-k * t);

        return new float3(
            v0.x * ekt,
            -vf + (v0.y + vf) * ekt,
            v0.z * ekt);
    }

    public static float3 GetV0ByVt(
        float vt,
        float3 pos0,
        float3 posf,
        float k)
    {
        float3 XZ0 =
            new float3(pos0.x, 0f, pos0.z);

        float3 XZf =
            new float3(posf.x, 0f, posf.z);

        float3 dir =
            XZf - XZ0;

        float distanceXZ =
            math.distance(XZ0, XZf);

        return math.normalize(dir) *
               (distanceXZ * k + vt);
    }

    public static float GetT(
        float3 x0,
        float3 xf,
        float v0,
        float k)
    {
        float3 p0 =
            new float3(x0.x, 0f, x0.z);

        float3 p1 =
            new float3(xf.x, 0f, xf.z);

        float d =
            math.distance(p0, p1);

        return GetT(d, v0, k);
    }

    public static float GetT(
        float d,
        float v0,
        float k)
    {
        float a =
            (d * k) / v0;

        if (a >= 1f)
            return float.PositiveInfinity;

        float ln =
            math.log(1f - a);

        return math.max(
            0f,
            ln / -k);
    }

    public static float ParabolaWithDrag_GetVY0(
        float t,
        float3 pos0,
        float3 posf,
        float k,
        float g)
    {
        float e =
            1f - math.exp(-k * t);

        float distanceY =
            posf.y - pos0.y;

        float vf = g / k;

        return ((distanceY + vf * t) * k / e) - vf;
    }
}