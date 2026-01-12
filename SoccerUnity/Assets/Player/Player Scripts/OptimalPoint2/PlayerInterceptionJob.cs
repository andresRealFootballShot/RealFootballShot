
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

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
    [ReadOnly] public NativeArray<float> reachBallSpeeds;
    [ReadOnly] public NativeArray<float> maxAngleForRuns;
    [ReadOnly] public NativeArray<float> maxAngleForRuns2;
    [ReadOnly] public NativeArray<float> minSpeedForRotates;
    [ReadOnly] public NativeArray<float> minSpeedForRotates2;
    [ReadOnly] public NativeArray<float> scopes;

    [ReadOnly] public NativeArray<float3> playerPositions;
    [ReadOnly] public NativeArray<float3> playerVelocities;
    [ReadOnly] public NativeArray<float3> playerDirections;

    [WriteOnly] public NativeArray<int> reachableIndex;
    [WriteOnly] public NativeArray<float> timePlayerToReachIndex;

    public void Execute(int index)
    {
        float bestTime = float.MaxValue;
        int bestIndex = -1;

        float3 playerPos = playerPositions[index];
        float3 playerVel = playerVelocities[index];
        float3 playerDir = math.normalizesafe(new float3(playerDirections[index].x, 0f, playerDirections[index].z));

        float accel = accelerations[index];
        float decel = deccelerations[index];
        float maxSpeed = maxSpeeds[index];
        float rotationSpeed = rotationSpeeds[index];
        float jumpHeight = jumpHeights[index];
        float reachBallSpeed = reachBallSpeeds[index];
        float maxAngleForRun = maxAngleForRuns[index];
        float maxAngleForRun2 = maxAngleForRuns2[index];
        float minSpeedForRotate = minSpeedForRotates[index];
        float minSpeedForRotate2 = minSpeedForRotates2[index];
        float scope = scopes[index];

       

        for (int i = 0; i < ballPositions.Length; i++)
        {
            float3 ballPos = ballPositions[i];
            float ballTime = ballTimes[i];
            float verticalDistance = ballPos.y - playerPos.y;
            float currentSpeed = math.length(new float2(playerVel.x, playerVel.z));
            if (verticalDistance > jumpHeight)
                continue;

            // =========================
            // FASE 0 – FRENO + DESPLAZAMIENTO
            // ======================w===
            float3 toTargetInitial = ballPos - playerPos;
            float2 toTargetXZ = new float2(toTargetInitial.x, toTargetInitial.z);


            

            float angle = AngleBetweenXZ(playerDir, toTargetXZ);
            bool mustBrakeBeforeRotate = angle > maxAngleForRun;

            float tBrake = 0f;
            float dBrake = 0f;
            float3 posAfterBrake = playerPos;
            //if (mustBrakeBeforeRotate && currentSpeed > minSpeedForRotate)
            if (mustBrakeBeforeRotate)
            {
                if(EstimateBrakeMove(playerPos, playerDir, ballPos, currentSpeed, maxAngleForRun2, minSpeedForRotate, decel, out posAfterBrake, out tBrake))
                {
                    currentSpeed = Mathf.Clamp(minSpeedForRotate, 0, currentSpeed);
                }
                else
                {

                    float deltaV = Mathf.Clamp(currentSpeed - minSpeedForRotate2, 0, Mathf.Infinity);
                    tBrake = (deltaV / decel);
                    dBrake = (deltaV * deltaV) / (2f * decel);

                    posAfterBrake = playerPos + playerDir * dBrake;
                    currentSpeed = Mathf.Clamp(minSpeedForRotate2, 0, currentSpeed);
                }
            }

            // =========================
            // FASE 1 – ROTACIÓN
            // =========================
            float3 toTargetAfterBrake = ballPos - posAfterBrake;
            float2 toTargetXZAfterBrake = new float2(toTargetAfterBrake.x, toTargetAfterBrake.z);

            float angleAfterBrake = AngleBetweenXZ(playerDir, toTargetXZAfterBrake);
            float tRotate = Mathf.Clamp(angleAfterBrake - maxAngleForRun,0,Mathf.Infinity) / rotationSpeed;

            // =========================
            // FASE 2 – DESPLAZAMIENTO REAL
            // =========================
            float distanceToTarget =
                math.length(toTargetXZAfterBrake) - scope;

            float tMove = EstimateTimeToReach(
                distanceToTarget,
                currentSpeed,
                accel,
                decel,
                maxSpeed,
                reachBallSpeed
            );

            //float totalTime = tBrake + tRotate + tMove;
            float totalTime = tBrake + tMove;

            Debug.Log("bT=" + tBrake + " mT=" + tMove + " rT=" + tRotate+ " distanceToTarget="+ distanceToTarget + " angle=" + angleAfterBrake);
            if (totalTime <= ballTime)
            {
                bestTime = ballTime == Mathf.Infinity ? totalTime : ballTime;
                bestIndex = i;
                break;
            }
        }

        reachableIndex[index] = bestIndex;
        timePlayerToReachIndex[index] = bestTime;
    }
    bool EstimateBrakeMove(float3 playerPos,float3 playerDir,float3 ballPos,float currentSpeed,float maxAngleForRun,float minSpeedForRotate, float decel,out float3 posAfterBrake,out float time)
    {

        float deltaV = Mathf.Clamp(currentSpeed, 0, Mathf.Infinity);
        float dBrake = (deltaV * deltaV - minSpeedForRotate* minSpeedForRotate) / (2f * decel);
        posAfterBrake = playerPos + playerDir*dBrake;
        float3 toTargetInitial = ballPos - posAfterBrake;
        float2 toTargetXZ = new float2(toTargetInitial.x, toTargetInitial.z);
        float angle = AngleBetweenXZ(playerDir, toTargetXZ);
        time = (deltaV- minSpeedForRotate) / decel;
        bool mustBrakeBeforeRotate = angle <= maxAngleForRun;
        return mustBrakeBeforeRotate;
    }
    // =====================================================
    // CINEMÁTICA LINEAL (misma que tu Speed())
    // =====================================================
    private float EstimateTimeToReach(
    float distance,
    float currentSpeed,
    float accel,
    float decel,
    float maxSpeed,
    float targetSpeed)
    {
        // --- 1. Calcular la distancia necesaria para desacelerar hasta targetSpeed ---
        float decelDistance = (currentSpeed * currentSpeed - targetSpeed * targetSpeed) / (2f * decel);
        decelDistance = Mathf.Max(decelDistance, 0);
        // Si la distancia es menor que la que necesitamos para frenar, ajustamos
        if (decelDistance > distance)
        {
            // No da tiempo a alcanzar maxSpeed, solo desaceleramos
            // Usamos fórmula de MRUA invertida: d = (v^2 - u^2) / (2a)
            return (Mathf.Sqrt(currentSpeed * currentSpeed - 2f * decel * distance) - currentSpeed) / (-decel);
        }

        // --- 2. Distancia disponible para acelerar y velocidad máxima ---
        float remainingDistance = Mathf.Max(distance - decelDistance,0);

        // Distancia para alcanzar maxSpeed desde currentSpeed
        float accelDistance = (maxSpeed * maxSpeed - currentSpeed * currentSpeed) / (2f * accel);
        accelDistance = Mathf.Max(accelDistance, 0);
        float time = 0f;

        if (accelDistance >= remainingDistance)
        {
            // No alcanza velocidad máxima, solo acelera hasta un punto y luego frena
            float peakSpeed = Mathf.Sqrt(currentSpeed * currentSpeed + 2f * accel * remainingDistance);
            time += Mathf.Max(peakSpeed - currentSpeed,0) / accel;          // Tiempo acelerando
            time += Mathf.Max(peakSpeed - targetSpeed,0) / decel;           // Tiempo desacelerando
        }
        else
        {
            // Alcanzamos velocidad máxima
            time += Mathf.Max(maxSpeed - currentSpeed,0) / accel;           // Tiempo acelerando
            float cruiseDistance = Mathf.Max(remainingDistance - accelDistance,0);
            time += cruiseDistance / maxSpeed;                  // Tiempo a velocidad constante
            time += Mathf.Max(maxSpeed - targetSpeed,0) / decel;           // Tiempo desacelerando
        }

        return time;
    }

    private float AngleBetweenXZ(float3 forward, float2 toTarget)
    {
        float2 f = math.normalize(new float2(forward.x, forward.z));
        float2 t = math.normalize(toTarget);

        float dot = math.clamp(math.dot(f, t), -1f, 1f);
        return math.degrees(math.acos(dot));
    }
}


/*using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

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
    [WriteOnly] public NativeArray<float> timePlayerToReachIndex;

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
                bestTime = ballTime==Mathf.Infinity ? totalTime : ballTime;
                bestIndex = i;
                break;
            }

        }

        reachableIndex[index] = bestIndex;
        timePlayerToReachIndex[index] = bestTime;
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

}*/