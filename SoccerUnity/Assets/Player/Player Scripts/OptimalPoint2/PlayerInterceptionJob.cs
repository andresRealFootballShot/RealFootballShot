using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct PlayerInterceptionJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> ballPositions;
    [ReadOnly] public NativeArray<float> ballTimes;

    [ReadOnly] public NativeArray<float> accelerations;
    [ReadOnly] public NativeArray<float> deccelerations;
    [ReadOnly] public NativeArray<float> maxSpeeds;
    [ReadOnly] public NativeArray<float> rotationSpeeds;
    [ReadOnly] public NativeArray<float> jumpHeights;
    [ReadOnly] public NativeArray<float> desiredSpeeds;

    [ReadOnly] public NativeArray<float3> playerPositions;
    [ReadOnly] public NativeArray<float3> playerVelocities;
    [ReadOnly] public NativeArray<float3> playerDirections;

    [WriteOnly] public NativeArray<int> reachableIndex;
    [WriteOnly] public NativeArray<float> timeToReachIndex;

    public void Execute(int index)
    {
        float bestTime = float.MaxValue;
        int bestIndex = -1;

        float3 playerPos = playerPositions[index];
        float3 playerVel = playerVelocities[index];
        float3 playerDir = playerDirections[index];
        float acceleration = accelerations[index];
        float decceleration = deccelerations[index];
        float maxSpeed = maxSpeeds[index];
        float rotationSpeed = rotationSpeeds[index];
        float jumpHeight = jumpHeights[index];
        float desiredSpeed = desiredSpeeds[index];

        for (int i = 0; i < ballPositions.Length; i++)
        {
            float3 ballPos = ballPositions[i];
            float ballTime = ballTimes[i];

            float3 toTarget = ballPos - playerPos;
            float horizontalDistance = math.length(new float2(toTarget.x, toTarget.z));
            float verticalDistance = toTarget.y;

            if (verticalDistance > jumpHeight)
                continue;

            float timeToReach = EstimateTimeToReach(horizontalDistance, math.length(new float2(playerVel.x, playerVel.z)), acceleration, decceleration, maxSpeed, desiredSpeed);

            float2 v1 = math.normalize(new float2(playerDir.x, playerDir.z));
            float2 v2 = math.normalize(new float2(toTarget.x, toTarget.z));
            float angle = math.degrees(math.acos(math.clamp(math.dot(v1, v2), -1f, 1f)));
            float timeToRotate = angle / rotationSpeed;

            float totalTime = timeToReach + timeToRotate;
            if (totalTime <= ballTime)
            {
                bestTime = totalTime;
                bestIndex = i;
                break;
            }

        }

        reachableIndex[index] = bestIndex;
        timeToReachIndex[index] = bestTime;
    }

    private float EstimateTimeToReach(float distance, float currentSpeed, float accel, float decel, float maxSpeed, float targetSpeed)
    {
        targetSpeed = math.clamp(targetSpeed, 0f, maxSpeed);
        currentSpeed = math.clamp(currentSpeed, 0f, maxSpeed);

        // Fase 1: aceleración hasta maxSpeed
        float tAccel = (maxSpeed - currentSpeed) / accel;
        float dAccel = (currentSpeed + maxSpeed) * 0.5f * tAccel;

        // Fase 3: desaceleración desde maxSpeed hasta targetSpeed
        float tDecel = (maxSpeed - targetSpeed) / decel;
        float dDecel = (maxSpeed + targetSpeed) * 0.5f * tDecel;

        float totalAccelDecelDist = dAccel + dDecel;

        if (totalAccelDecelDist >= distance)
        {
            // No hay espacio suficiente para llegar a maxSpeed
            // Entonces se calcula el punto intermedio donde acelera y luego desacelera
            // Encontramos velocidad pico alcanzable v_peak usando conservación de distancia

            // Fórmula: d = (v0 + v) * t1/2 + (v + vt) * t2/2
            // Pero v_peak es la incógnita, y queremos una transición suave: aceleración hasta v_peak, luego deceleración a targetSpeed

            float a = 1f / (2f * accel);
            float b = currentSpeed / accel + targetSpeed / decel;
            float c = -distance + (currentSpeed * currentSpeed) / (2f * accel) + (targetSpeed * targetSpeed) / (2f * decel);

            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0f)
                return float.MaxValue; // No alcanzable

            float v_peak = (-b + math.sqrt(discriminant)) / (2f * a);

            // Tiempos para llegar a v_peak y luego desacelerar a targetSpeed
            float t1 = (v_peak - currentSpeed) / accel;
            float t2 = (v_peak - targetSpeed) / decel;
            return t1 + t2;
        }
        else
        {
            // Fase 2: tramo a velocidad constante
            float dCruise = distance - totalAccelDecelDist;
            float tCruise = dCruise / maxSpeed;
            return tAccel + tCruise + tDecel;
        }
    }

}