
using CullPositionPoint;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

//[BurstCompile]
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
    [ReadOnly] public NativeArray<bool> isGoalkeepers;
    [WriteOnly] public NativeArray<int> reachableIndex;
    [WriteOnly] public NativeArray<float> timePlayerToReachIndex;
    [WriteOnly] public NativeArray<float3> endPlayerDirections;
    public void Execute(int index)
    {
        float bestTime = -1;
        int bestIndex = -1;
        float3 toTargetAfterBrake = float3.zero;
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
        bool isGoalkeeper = isGoalkeepers[index];

        for (int i = 0; i < ballPositions.Length; i++)
        {
            float3 ballPos = ballPositions[i];
            float ballTime = ballTimes[i];
            float totalTime = 0;
            
            float verticalDistance = ballPos.y - playerPos.y;
            
            if (verticalDistance > jumpHeight)
                continue;
            if (isGoalkeeper)
            {
                toTargetAfterBrake = ballPos - playerPos;
                totalTime = linearGetTimeToReachPosition(playerPos, ballPos, maxSpeed, scope);
            }
            else
            {
                float currentSpeed = math.length(new float2(playerVel.x, playerVel.z));
                // =========================
                // FASE 0 – FRENO + DESPLAZAMIENTO
                // =========================
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
                    if (EstimateBrakeMove(playerPos, playerDir, ballPos, currentSpeed, maxAngleForRun2, minSpeedForRotate, decel, out posAfterBrake, out tBrake))
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
                toTargetAfterBrake = ballPos - posAfterBrake;

                float2 toTargetXZAfterBrake = new float2(toTargetAfterBrake.x, toTargetAfterBrake.z);

                float angleAfterBrake = AngleBetweenXZ(playerDir, toTargetXZAfterBrake);
                float tRotate = Mathf.Clamp(angleAfterBrake - maxAngleForRun, 0, Mathf.Infinity) / rotationSpeed;

                // =========================
                // FASE 2 – DESPLAZAMIENTO REAL
                // =========================
                float distanceToTarget = math.max(
                    math.length(toTargetXZAfterBrake) - scope, 0);

                float tMove = EstimateTimeToReach(
                    distanceToTarget,
                    currentSpeed,
                    accel,
                    decel,
                    maxSpeed,
                    reachBallSpeed
                );

                //float totalTime = tBrake + tRotate + tMove;
                totalTime = tBrake + tMove;
            }
            
            //Debug.Log("bT=" + tBrake + " mT=" + tMove + " rT=" + tRotate+ " distanceToTarget="+ distanceToTarget + " angle=" + angleAfterBrake);
            if (totalTime <= ballTime)
            {
                //bestTime = ballTime == Mathf.Infinity ? totalTime : ballTime;
                bestTime = totalTime;
                bestIndex = i;
                
                break;
            }
        }
        endPlayerDirections[index] = toTargetAfterBrake;
        reachableIndex[index] = bestIndex;
        timePlayerToReachIndex[index] = bestTime;
    }
    public static float linearGetTimeToReachPosition(Vector3 playerPosition, Vector3 targetPosition, float maxSpeed, float scope)
    {
        float distance = Mathf.Clamp(Vector3.Distance(MyFunctions.setYToVector3(playerPosition, targetPosition.y), targetPosition) - scope, 0, Mathf.Infinity);
        float t = distance / maxSpeed;
        return t;
    }
    public static bool EstimateBrakeMove(float3 playerPos,float3 playerDir,float3 ballPos,float currentSpeed,float maxAngleForRun,float minSpeedForRotate, float decel,out float3 posAfterBrake,out float time)
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
    public static float EstimateTimeToReach(
    float distance,
    float currentSpeed,
    float accel,
    float decel,
    float maxSpeed,
    float targetSpeed)
    {
        float decelDistance = (currentSpeed * currentSpeed - targetSpeed * targetSpeed) / (2f * decel);
        decelDistance = Mathf.Max(decelDistance, 0);
        // Si la distancia es menor que la que necesitamos para frenar, ajustamos
        if (decelDistance > distance)
        {
            // No da tiempo a alcanzar maxSpeed, solo desaceleramos
            // Usamos fórmula de MRUA invertida: d = (v^2 - u^2) / (2a)
            return (Mathf.Sqrt(currentSpeed * currentSpeed - 2f * decel * distance) - currentSpeed) / (-decel);
        }

        float time = 0f;
        
        float d = Mathf.Max(AccelerationPath.getDistanceWhereStartDecelerate(currentSpeed, targetSpeed, accel, -decel, distance),0);
        
        float acelX = Mathf.Abs(AccelerationPath.getX2(currentSpeed, maxSpeed, accel));
        if (acelX >= d)
        {
            
            float t3;
            AccelerationPath.getT(d, currentSpeed, accel, out t3);
            float v1 = currentSpeed + accel * t3;
            float t4 = 0;
            if (d < distance)
            {

                t4 = AccelerationPath.getT(targetSpeed, v1, decel);
            }
            time = t3 + t4;

            //Debug.Log("Not Reach MaxSpeed DistanceStartDecelerate=" + d+" time_startDecelerate="+t3 + " speed_startDecelerate=" + v1);
        }
        else
        {
            float t5 = AccelerationPath.getT(maxSpeed, currentSpeed, accel);
            float decelX = Mathf.Abs(AccelerationPath.getX2(maxSpeed, targetSpeed, decel));
            float x5 = distance - acelX - decelX;
            float t6 = x5 / maxSpeed;
            float t7 = AccelerationPath.getT(targetSpeed, maxSpeed, decel);
            time = t5 + t6 + t7;
            //Debug.Log("Reach MaxSpeed DistanceStartDecelerate=" + d+" distanceTarget="+(distance - acelX) + " timeA=" + t5 + " timeMaxSpeed="+t6);
        }

        return time;
    }

    private static float AngleBetweenXZ(float3 forward, float2 toTarget)
    {
        float2 f = math.normalize(new float2(forward.x, forward.z));
        float2 t = math.normalize(toTarget);

        float dot = math.clamp(math.dot(f, t), -1f, 1f);
        return math.degrees(math.acos(dot));
    }
}