using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using Unity.Mathematics;
using CullPositionPoint;
using UnityEditor.Experimental.GraphView;
using System.IO;
using UnityEngine.UIElements;
using static MovimentValues;
using NextMove_Algorithm;

namespace DOTS_ChaserDataCalculation
{
    [BurstCompile]
    public class GetTimeToReachPointDOTS
    {
        public static float getTimeToReachPosition(ref PlayerDataComponent playerDataComponent, Vector3 targetPosition)
        {
            if (playerDataComponent.useAccelerationGetTimeToReachPosition)
            {
                return accelerationGetTimeToReachPosition(ref playerDataComponent, targetPosition, out Vector3 startAcPoint, out float startAcTime, out Vector3 endPoint);
            }
            else
            {
                return linearGetTimeToReachPosition(ref playerDataComponent, targetPosition);
            }
        }
        public static float linearGetTimeToReachPosition(Vector3 playerPosition, Vector3 targetPosition, float maxSpeed, float scope)
        {
            float distance = Mathf.Clamp(Vector3.Distance(MyFunctions.setYToVector3(playerPosition, targetPosition.y), targetPosition) - scope, 0, Mathf.Infinity);
            float t = distance / maxSpeed;
            return t;
        }
        public static float linearGetTimeToReachPosition(ref PlayerDataComponent playerDataComponent, Vector3 targetPosition)
        {
            float distance = Vector3.Distance(MyFunctions.setYToVector3(playerDataComponent.position, targetPosition.y), targetPosition);
            float t = distance / playerDataComponent.maxSpeed;
            return t;
        }
        public static Vector3 accelerationGetPosition(Vector3 playerPosition, float currentSpeed, Vector3 bodyForward, float speedRotation, float minSpeedForRotate, float a, float da, float maxAngleForRun, float scope, Vector3 targetPosition, float maxSpeed, float t, out float endSpeed, out Vector3 endDirection)
        {
            if (MyFunctions.Vector3IsNan(targetPosition) || targetPosition.Equals(Vector3.positiveInfinity) || targetPosition.Equals(Vector3.negativeInfinity))
            {
                endSpeed = Mathf.Infinity;
                endDirection = Vector3.positiveInfinity;
                return Vector3.positiveInfinity;
            }

            Vector3 bodyPosition = playerPosition;
            float d4 = Vector3.Distance(bodyPosition, MyFunctions.setYToVector3(targetPosition, bodyPosition.y));
            if (d4 < scope)
            {
                endSpeed = Mathf.Infinity;
                endDirection = Vector3.positiveInfinity;
                return playerPosition;
            }
            float speed = currentSpeed;

            Vector3 direction = MyFunctions.setY0ToVector3(targetPosition - bodyPosition).normalized;
            float angle = Vector3.Angle(bodyForward, direction);
            //float t1 = angle > maxAngleForRun ? t1_1 : 0;
            float t1 = 0;
            float t2 = 0;
            float v0Magnitude = speed;
            Vector3 x1 = playerPosition;
            if (angle > maxAngleForRun)
            {
                if (speed > minSpeedForRotate)
                {

                    float t1_1 = AccelerationPath.getT(minSpeedForRotate, speed, da);
                    //float xToStop = speed > minSpeedForRotate ? AccelerationPath.getX3(speed, t, -da) : 0;
                    t1 = t1_1;
                    float t11 = Mathf.Clamp(t1, 0, t);

                    float d = Mathf.Abs(AccelerationPath.getX3(speed, t11, -da));
                    x1 = playerPosition + direction * d;
                    if (t1 >= t)
                    {
                        endSpeed = AccelerationPath.getV(t11, speed, -da);
                        endDirection = Quaternion.AngleAxis(speedRotation * t, Vector3.up) * direction;
                        return x1;
                    }
                }

                direction = targetPosition - x1;
                direction.y = 0;
                direction.Normalize();
                t2 = Path.getT(maxAngleForRun, angle, speedRotation);
                t2 = Mathf.Clamp(t2, 0, t);
                v0Magnitude = 0;
            }
            t -= t1 + t2;
            float distance = 0;
            endSpeed = maxSpeed;
            endDirection = direction;
            if (v0Magnitude < maxSpeed && t > 0)
            {
                float t3 = AccelerationPath.getT(maxSpeed, v0Magnitude, a);
                if (t3 < t) endSpeed = maxSpeed;
                else endSpeed = AccelerationPath.getV(t, v0Magnitude, a);
                t3 = Mathf.Clamp(t3, 0, t);
                distance += Mathf.Abs(AccelerationPath.getX2(v0Magnitude, maxSpeed, a));
                t -= t3;
            }
            if (t > 0)
            {
                Vector3 x = x1 + direction * distance;
                float d = Vector3.Distance(x, targetPosition);
                float d2 = maxSpeed * t;
                float t4 = d / maxSpeed;
                t4 = t - t4;
                if (t4 > 0)
                {
                    endDirection = Quaternion.AngleAxis(speedRotation * t4, Vector3.up) * endDirection;
                }
                distance += Mathf.Clamp(d2, 0, d);
                if (d2 >= d)
                    endSpeed = 0;
                else
                    endSpeed = maxSpeed;
            }

            Vector3 result = x1 + direction * distance;
            return result;
        }
        public static float accelerationGetTimeToReachPosition(Vector3 playerPosition, float currentSpeed, Vector3 bodyForward, Vector3 normalizedForwardVelocity, ref PlayerGenericParams PlayerGenericParams, Vector3 targetPosition,PlayerPositionElement playerData)
        {
            if (MyFunctions.Vector3IsNan(targetPosition) || targetPosition.Equals(Vector3.positiveInfinity) || targetPosition.Equals(Vector3.negativeInfinity))
            {
                return Mathf.Infinity;
            }

            Vector3 bodyPosition = playerPosition;
            float d4 = Vector3.Distance(bodyPosition, MyFunctions.setYToVector3(targetPosition, bodyPosition.y));
            if (d4 < playerData.scope)
            {
                return 0;
            }
            float speed = currentSpeed;
            float minSpeedForRotate = playerData.minSpeedForRotate;
            float a = playerData.acceleration;
            float da = playerData.decceleration;
            float maxAngleForRun = playerData.maxAngleForRun;
            float t1_1 = speed > minSpeedForRotate ? AccelerationPath.getT(minSpeedForRotate, speed, da) : 0;
            Vector3 direction = MyFunctions.setY0ToVector3(targetPosition - bodyPosition).normalized;
            float angle = Vector3.Angle(bodyForward, direction);
            float t1 = angle > maxAngleForRun ? t1_1 : 0;
            Vector3 x1 = AccelerationPath.getX(bodyPosition, bodyForward, bodyForward * speed, t1, -da);
            float t2 = angle > maxAngleForRun ? Path.getT(maxAngleForRun, angle, playerData.maxSpeedRotation) : 0;

            Vector3 v0 = angle > maxAngleForRun ? Vector3.zero : normalizedForwardVelocity * speed;
            float v0Magnitude = v0.magnitude;
            float d2 = Vector3.Distance(MyFunctions.setY0ToVector3(x1), MyFunctions.setY0ToVector3(targetPosition));

            float d3 = d2 - playerData.scope;
            float d = AccelerationPath.getDistanceWhereStartDecelerate(v0Magnitude, playerData.maxSpeedForReachBall, a, -da, d3);
            float t3, t4, t8, result;
            float x3 = Mathf.Abs(AccelerationPath.getX2(v0Magnitude, PlayerGenericParams.maxSpeed, a));
            if (x3 < d)
            {
                float t5 = AccelerationPath.getT(PlayerGenericParams.maxSpeed, v0Magnitude, a);
                float x4 = Mathf.Abs(AccelerationPath.getX2(PlayerGenericParams.maxSpeed, playerData.maxSpeedForReachBall, da));
                float x5 = d3 - x3 - x4;
                float t6 = x5 / PlayerGenericParams.maxSpeed;
                float t7 = AccelerationPath.getT(playerData.maxSpeedForReachBall, PlayerGenericParams.maxSpeed, da);
                t8 = t5 + t6 + t7;
                result = t1 + t2 + t8;
            }
            else
            {
                AccelerationPath.getT(d, v0Magnitude, a, out t3);
                float v1 = v0Magnitude + a * t3;
                t4 = AccelerationPath.getT(playerData.maxSpeedForReachBall, v1, da);
                t8 = t3 + t4;
                result = t1 + t2 + t8;

            }
            return result;
        }
    public static float accelerationGetTimeToReachPosition2(
    Vector3 playerPosition,
    float currentSpeed,
    Vector3 bodyForward,
    Vector3 normalizedForwardVelocity,
    Vector3 targetPosition,

    // Parámetros antes en playerData
    float maxAngleForRun,
    float maxAngleForRun2,
    float minSpeedForRotate,
    float minSpeedForRotate2,
    float acceleration,
    float decceleration,
    float maxSpeedForReachBall,
    float scope,

    // Parámetro antes en playerParams
    float maxSpeed
)
        {
            if (MyFunctions.Vector3IsNan(targetPosition) ||
                targetPosition.Equals(Vector3.positiveInfinity) ||
                targetPosition.Equals(Vector3.negativeInfinity))
            {
                return Mathf.Infinity;
            }

            Vector3 playerPos = playerPosition;
            Vector3 playerDir = bodyForward;
            playerDir.y = 0f;
            playerDir.Normalize();

            float speed = currentSpeed;

            Vector3 toTargetInitial = targetPosition - playerPos;
            toTargetInitial.y = 0f;

            float angle = Vector3.Angle(playerDir, toTargetInitial.normalized);

            float tBrake = 0f;
            Vector3 posAfterBrake = playerPos;

            // =========================
            // FASE 0 – FRENO
            // =========================
            if (angle > maxAngleForRun)
            {
                float3 posTmp;
                float tTmp;

                if (PlayerInterceptionJob.EstimateBrakeMove(
                    playerPos,
                    playerDir,
                    targetPosition,
                    speed,
                    maxAngleForRun2,
                    minSpeedForRotate,
                    decceleration,
                    out posTmp,
                    out tTmp))
                {
                    posAfterBrake = posTmp;
                    tBrake = tTmp;
                    speed = Mathf.Clamp(minSpeedForRotate, 0, speed);
                }
                else
                {
                    float deltaV = Mathf.Clamp(speed - minSpeedForRotate2, 0, Mathf.Infinity);
                    tBrake = deltaV / decceleration;

                    float dBrake =
                        (speed * speed -
                         minSpeedForRotate2 * minSpeedForRotate2) /
                        (2f * decceleration);

                    posAfterBrake += playerDir * dBrake;
                    speed = Mathf.Clamp(minSpeedForRotate2, 0, speed);
                }
            }

            // =========================
            // FASE 2 – DESPLAZAMIENTO
            // =========================
            Vector3 toTargetAfterBrake = targetPosition - posAfterBrake;
            toTargetAfterBrake.y = 0f;

            float distanceToTarget =
                Mathf.Max(toTargetAfterBrake.magnitude - scope, 0f);

            float tMove = PlayerInterceptionJob.EstimateTimeToReach(
                distanceToTarget,
                speed,
                acceleration,
                decceleration,
                maxSpeed,
                maxSpeedForReachBall
            );

            return tBrake + tMove;
        }
        public static float accelerationGetTimeToReachPosition2(
            Vector3 playerPosition,
            float currentSpeed,
            Vector3 bodyForward,
            Vector3 normalizedForwardVelocity,
            ref PlayerGenericParams playerParams,
            Vector3 targetPosition,
            PlayerPositionElement playerData,float targetSpeed,float scope)
        {
            if (MyFunctions.Vector3IsNan(targetPosition) ||
                targetPosition.Equals(Vector3.positiveInfinity) ||
                targetPosition.Equals(Vector3.negativeInfinity))
            {
                return Mathf.Infinity;
            }

            Vector3 playerPos = playerPosition;
            Vector3 playerDir = bodyForward;
            playerDir.y = 0f;
            playerDir.Normalize();

            float speed = currentSpeed;

            Vector3 toTargetInitial = targetPosition - playerPos;
            toTargetInitial.y = 0f;

            float angle = Vector3.Angle(playerDir, toTargetInitial.normalized);

            float tBrake = 0f;
            Vector3 posAfterBrake = playerPos;

            // =========================
            // FASE 0 – FRENO (idéntica)
            // =========================
            if (angle > playerData.maxAngleForRun)
            {
                float3 posTmp;
                float tTmp;

                if (PlayerInterceptionJob.EstimateBrakeMove(
                    playerPos,
                    playerDir,
                    targetPosition,
                    speed,
                    playerData.maxAngleForRun2,
                    playerData.minSpeedForRotate,
                    playerData.decceleration,
                    out posTmp,
                    out tTmp))
                {
                    posAfterBrake = posTmp;
                    tBrake = tTmp;
                    speed = Mathf.Clamp(playerData.minSpeedForRotate, 0, speed);
                }
                else
                {
                    float deltaV = Mathf.Clamp(speed - playerData.minSpeedForRotate2, 0, Mathf.Infinity);
                    tBrake = deltaV / playerData.decceleration;

                    float dBrake =
                        (speed * speed -
                         playerData.minSpeedForRotate2 * playerData.minSpeedForRotate2) /
                        (2f * playerData.decceleration);

                    posAfterBrake += playerDir * dBrake;
                    speed = Mathf.Clamp(playerData.minSpeedForRotate2, 0, speed);
                }
            }

            // =========================
            // FASE 2 – DESPLAZAMIENTO
            // =========================
            Vector3 toTargetAfterBrake = targetPosition - posAfterBrake;
            toTargetAfterBrake.y = 0f;

            float distanceToTarget =
                Mathf.Max(toTargetAfterBrake.magnitude - scope, 0f);

            float tMove = PlayerInterceptionJob.EstimateTimeToReach(
                distanceToTarget,
                speed,
                playerData.acceleration,
                playerData.decceleration,
                playerParams.maxSpeed,
                targetSpeed
            );

            return tBrake + tMove;
        }
        public struct MovementResult
        {
            public Vector3 position;
            public float speed;
            public Vector3 direction;
        }
public static void accelerationGetPositionAtTime(
    Vector3 playerPosition,
    float currentSpeed,
    Vector3 bodyForward,
    Vector3 targetPosition,
    float time,

    float scope,
    float maxAngleForRun,
    float maxAngleForRun2,
    float minSpeedForRotate,
    float minSpeedForRotate2,
    float maxSpeedForReachBall,
    float acceleration,
    float decceleration,
    float maxSpeed,
    float targetSpeed,

    out Vector3 position,
    out float finalSpeed,
    out Vector3 finalDirection)
        {
            position = playerPosition;
            finalSpeed = currentSpeed;

            Vector3 playerDir = bodyForward;
            playerDir.y = 0f;
            playerDir.Normalize();

            Vector3 toTarget = targetPosition - playerPosition;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < 0.0001f)
            {
                finalDirection = playerDir;
                return;
            }

            Vector3 targetDir = toTarget.normalized;

            float angle = Vector3.Angle(playerDir, targetDir);

            float tRemaining = time;

            // =========================
            // FASE 0 – FRENO
            // =========================
            if (angle > maxAngleForRun && finalSpeed > 0f)
            {
                float3 posTmp;
                float tBrake;

                bool mustBrake = PlayerInterceptionJob.EstimateBrakeMove(
                    playerPosition,
                    playerDir,
                    targetPosition,
                    finalSpeed,
                    maxAngleForRun2,
                    minSpeedForRotate,
                    decceleration,
                    out posTmp,
                    out tBrake);

                float targetBrakeSpeed = mustBrake ? minSpeedForRotate : minSpeedForRotate2;

                float deltaV = Mathf.Max(finalSpeed - targetBrakeSpeed, 0f);
                float realBrakeTime = deltaV / decceleration;

                if (tRemaining <= realBrakeTime)
                {
                    // 🔴 Nos quedamos dentro del frenado
                    float v1 = finalSpeed;
                    float v2 = v1 - decceleration * tRemaining;

                    float d = (v1 + v2) * 0.5f * tRemaining;

                    position += playerDir * d;
                    finalSpeed = v2;
                    finalDirection = playerDir;
                    return;
                }
                else
                {
                    // 🔵 Completamos frenado
                    float dBrake = (finalSpeed * finalSpeed - targetBrakeSpeed * targetBrakeSpeed) / (2f * decceleration);

                    position += playerDir * dBrake;

                    finalSpeed = targetBrakeSpeed;
                    tRemaining -= realBrakeTime;
                }
            }

            // =========================
            // FASE 1 – MOVIMIENTO HACIA TARGET
            // =========================
            Vector3 moveDir = (targetPosition - position);
            moveDir.y = 0f;

            float distance = Mathf.Max(moveDir.magnitude - scope, 0f);

            if (distance < 0.0001f)
            {
                finalDirection = playerDir;
                return;
            }

            moveDir.Normalize();
            finalDirection = moveDir;

            float speed = finalSpeed;

            // Distancia para frenar hasta targetSpeed
            float decelDistance = (speed * speed - targetSpeed * targetSpeed) / (2f * decceleration);
            decelDistance = Mathf.Max(decelDistance, 0f);

            float dAccelToMax = Mathf.Abs(AccelerationPath.getX2(speed, maxSpeed, acceleration));
            float dDecelFromMax = Mathf.Abs(AccelerationPath.getX2(maxSpeed, targetSpeed, decceleration));

            float dCruise = Mathf.Max(distance - dAccelToMax - dDecelFromMax, 0f);

            // ===== CASO 1: NO ALCANZA MAX SPEED =====
            float dStartDecel = Mathf.Max(
                AccelerationPath.getDistanceWhereStartDecelerate(
                    speed, targetSpeed, acceleration, -decceleration, distance), 0f);

            float tAccel;
            AccelerationPath.getT(dStartDecel, speed, acceleration, out tAccel);

            if (tRemaining <= tAccel)
            {
                // 🔴 Dentro de aceleración
                float v2 = speed + acceleration * tRemaining;
                float d = (speed + v2) * 0.5f * tRemaining;

                position += moveDir * d;
                finalSpeed = v2;
                return;
            }

            // 🔵 completamos aceleración parcial
            float vAfterAccel = speed + acceleration * tAccel;
            position += moveDir * dStartDecel;

            tRemaining -= tAccel;

            // ===== DECELERACIÓN DIRECTA (SIN CRUCERO) =====
            float tDecel = AccelerationPath.getT(targetSpeed, vAfterAccel, decceleration);

            if (dAccelToMax >= dStartDecel)
            {
                if (tRemaining <= tDecel)
                {
                    float v2 = vAfterAccel - decceleration * tRemaining;
                    float d = (vAfterAccel + v2) * 0.5f * tRemaining;

                    position += moveDir * d;
                    finalSpeed = v2;
                    return;
                }
                else
                {
                    float dDecel = (vAfterAccel * vAfterAccel - targetSpeed * targetSpeed) / (2f * decceleration);

                    position += moveDir * dDecel;
                    finalSpeed = targetSpeed;
                    return;
                }
            }

            // ===== CASO 2: ALCANZA MAX SPEED =====

            // ACELERACIÓN HASTA MAX
            float tToMax = AccelerationPath.getT(maxSpeed, speed, acceleration);

            if (tRemaining <= tToMax)
            {
                float v2 = speed + acceleration * tRemaining;
                float d = (speed + v2) * 0.5f * tRemaining;

                position += moveDir * d;
                finalSpeed = v2;
                return;
            }

            position += moveDir * dAccelToMax;
            tRemaining -= tToMax;

            // CRUCERO
            float tCruise = dCruise / maxSpeed;

            if (tRemaining <= tCruise)
            {
                position += moveDir * (maxSpeed * tRemaining);
                finalSpeed = maxSpeed;
                return;
            }

            position += moveDir * dCruise;
            tRemaining -= tCruise;

            // DECELERACIÓN FINAL
            float tFinalDecel = AccelerationPath.getT(targetSpeed, maxSpeed, decceleration);

            if (tRemaining <= tFinalDecel)
            {
                float v2 = maxSpeed - decceleration * tRemaining;
                float d = (maxSpeed + v2) * 0.5f * tRemaining;

                position += moveDir * d;
                finalSpeed = v2;
                return;
            }

            float dFinal = (maxSpeed * maxSpeed - targetSpeed * targetSpeed) / (2f * decceleration);

            position += moveDir * dFinal;
            finalSpeed = targetSpeed;
        }
        static float SolveTimeForDistance(float v0, float a, float d)
        {
            // d = v0*t + 0.5*a*t^2
            float A = 0.5f * a;
            float B = v0;
            float C = -d;

            float discriminant = B * B - 4f * A * C;

            if (discriminant < 0f)
                return 0f;

            float sqrt = Mathf.Sqrt(discriminant);

            float t1 = (-B + sqrt) / (2f * A);
            float t2 = (-B - sqrt) / (2f * A);

            return Mathf.Max(t1, t2);
        }
        public static float accelerationGetVelocity(ref PlayerDataComponent playerDataComponent, float t, Vector3 targetPosition)
        {
            if (MyFunctions.Vector3IsNan(targetPosition) || targetPosition.Equals(Vector3.positiveInfinity) || targetPosition.Equals(Vector3.negativeInfinity))
            {
                return Mathf.Infinity;
            }

            Vector3 bodyPosition = playerDataComponent.position;
            float d4 = Vector3.Distance(bodyPosition, MyFunctions.setYToVector3(targetPosition, bodyPosition.y));
            if (d4 < playerDataComponent.scope)
            {
                return 0;
            }
            float speed = playerDataComponent.currentSpeed;
            float minSpeedForRotate = playerDataComponent.minSpeedForRotate;
            float a = playerDataComponent.acceleration;
            float da = playerDataComponent.decceleration;
            float maxAngleForRun = playerDataComponent.maxAngleForRun;
            float t1_1 = speed > minSpeedForRotate ? AccelerationPath.getT(minSpeedForRotate, speed, da) : 0;
            Vector3 direction = MyFunctions.setY0ToVector3(targetPosition - bodyPosition).normalized;
            float angle = Vector3.Angle(playerDataComponent.bodyY0Forward, direction);
            float t1 = angle > maxAngleForRun ? t1_1 : 0;
            Vector3 x1 = AccelerationPath.getX(bodyPosition, playerDataComponent.bodyY0Forward, playerDataComponent.normalizedBodyY0Forward * speed, t1, -da);
            float t2 = angle > maxAngleForRun ? Path.getT(maxAngleForRun, angle, playerDataComponent.maxSpeedRotation) : 0;

            Vector3 v0 = angle > maxAngleForRun ? Vector3.zero : playerDataComponent.normalizedForwardVelocity * speed;
            float v0Magnitude = v0.magnitude;
            float d2 = Vector3.Distance(MyFunctions.setY0ToVector3(x1), MyFunctions.setY0ToVector3(targetPosition));

            float d3 = d2 - playerDataComponent.scope;
            float d = AccelerationPath.getDistanceWhereStartDecelerate2(v0Magnitude, playerDataComponent.maxSpeed, da);
            float ddd = AccelerationPath.getDistanceWhereStartDecelerate(v0Magnitude, playerDataComponent.maxSpeedForReachBall, a, -da, d3);
            float dd = AccelerationPath.getDistanceWhereStartDecelerate2(playerDataComponent.maxSpeedForReachBall, playerDataComponent.maxSpeed, a);
            float t3, t4, t8, result;
            float x3 = Mathf.Abs(AccelerationPath.getX2(v0Magnitude, playerDataComponent.maxSpeed, a));

            if (t < t1)
            {
                result = AccelerationPath.getV2(speed, t, -da);

            }
            else if (t < t1 + t2)
            {
                result = Mathf.Clamp(speed, 0, minSpeedForRotate);
            }
            else if (d + dd < d3)
            {

                result = playerDataComponent.maxSpeed;
            }
            else
            {
                float tReachMax = AccelerationPath.getT(playerDataComponent.maxSpeed, v0Magnitude, a);
                float tReachMin = AccelerationPath.getT(playerDataComponent.maxSpeed, playerDataComponent.maxSpeedForReachBall, da);
                if (tReachMax <= t)
                {

                    result = v0Magnitude + a * tReachMax;
                }
                else
                {
                    result = v0Magnitude - da * tReachMin;
                }

            }
            return result;
        }

        public static float accelerationGetTimeToReachPosition(ref PlayerDataComponent playerDataComponent, Vector3 targetPosition, out Vector3 startAcPoint, out float startAcTime, out Vector3 endPoint)
        {
            if (MyFunctions.Vector3IsNan(targetPosition) || targetPosition.Equals(Vector3.positiveInfinity) || targetPosition.Equals(Vector3.negativeInfinity))
            {
                startAcPoint = Vector3.zero;
                startAcTime = 0;
                endPoint = Vector3.zero;
                return Mathf.Infinity;
            }

            Vector3 bodyPosition = playerDataComponent.position;
            float d4 = Vector3.Distance(bodyPosition, MyFunctions.setYToVector3(targetPosition, bodyPosition.y));
            if (d4 < playerDataComponent.scope)
            {

                startAcPoint = Vector3.zero;
                startAcTime = 0;
                endPoint = Vector3.zero;
                return 0;
            }
            float speed = playerDataComponent.currentSpeed;
            float minSpeedForRotate = playerDataComponent.minSpeedForRotate;
            float a = playerDataComponent.acceleration;
            float da = playerDataComponent.decceleration;
            float maxAngleForRun = playerDataComponent.maxAngleForRun;
            float t1_1 = speed > minSpeedForRotate ? AccelerationPath.getT(minSpeedForRotate, speed, da) : 0;
            Vector3 direction = MyFunctions.setY0ToVector3(targetPosition - bodyPosition).normalized;
            float angle = Vector3.Angle(playerDataComponent.bodyY0Forward, direction);
            float t1 = angle > maxAngleForRun ? t1_1 : 0;
            Vector3 x1 = bodyPosition + playerDataComponent.bodyY0Forward * AccelerationPath.getX3(speed, t1, -da);
            startAcPoint = x1;
            startAcTime = t1;
            float t2 = angle > maxAngleForRun ? Path.getT(maxAngleForRun, angle, playerDataComponent.maxSpeedRotation) : 0;

            Vector3 v0 = angle > maxAngleForRun ? Vector3.zero : playerDataComponent.normalizedForwardVelocity * speed;
            float v0Magnitude = v0.magnitude;
            float d2 = Vector3.Distance(MyFunctions.setY0ToVector3(x1), MyFunctions.setY0ToVector3(targetPosition));

            float d3 = d2 - playerDataComponent.scope;
            //float d = AccelerationPath.getX2(v0Magnitude, playerDataComponent.maxSpeed, a);
            //float dd = AccelerationPath.getX2(playerDataComponent.maxSpeed, playerDataComponent.maxSpeedForReachBall, da);
            float d = AccelerationPath.getDistanceWhereStartDecelerate2(v0Magnitude, playerDataComponent.maxSpeed, da);
            float ddd = AccelerationPath.getDistanceWhereStartDecelerate(v0Magnitude, playerDataComponent.maxSpeedForReachBall, a, -da, d3);
            float dd = AccelerationPath.getDistanceWhereStartDecelerate2(playerDataComponent.maxSpeedForReachBall, playerDataComponent.maxSpeed, a);
            Vector3 dir2 = targetPosition - x1;
            endPoint = x1;
            float t3, t4, t8, result;
            float x3 = Mathf.Abs(AccelerationPath.getX2(v0Magnitude, playerDataComponent.maxSpeed, a));
            if (d + dd < d3)
            {
                float t5 = AccelerationPath.getT(playerDataComponent.maxSpeed, v0Magnitude, a);
                float x4 = Mathf.Abs(AccelerationPath.getX2(playerDataComponent.maxSpeed, playerDataComponent.maxSpeedForReachBall, da));
                float x5 = d3 - x3 - x4 - playerDataComponent.scope;
                float t6 = x5 / playerDataComponent.maxSpeed;
                float t7 = AccelerationPath.getT(playerDataComponent.maxSpeed, playerDataComponent.maxSpeedForReachBall, da);
                t8 = t5 + t6 + t7;
                result = t1 + t2 + t8;
                endPoint += dir2 * x4;
                endPoint += dir2 * x3;
                endPoint += dir2 * x5;
            }
            else
            {
                //t3 = AccelerationPath.getT_StartToDesacelerate(v0Magnitude, playerDataComponent.maxSpeedForReachBall, a, -da, d3);
                AccelerationPath.getT(ddd, v0Magnitude, a, out t3);
                float v1 = v0Magnitude + a * t3;
                t4 = AccelerationPath.getT(playerDataComponent.maxSpeedForReachBall, v1, da);
                float x4 = Mathf.Abs(AccelerationPath.getX2(playerDataComponent.maxSpeed, v1, a));
                float x5 = Mathf.Abs(AccelerationPath.getX2(v1, playerDataComponent.maxSpeedForReachBall, da));
                t8 = t3 + t4 + t1 + t2;
                result = t8;
                endPoint += dir2 * x4;
                endPoint += dir2 * x5;
            }

            return result;
        }
        public static Vector3 accelerationGetPositionDriving(ref PlayerDataComponent playerDataComponent, Vector3 targetPosition, float t, PlayerSkills drivingSkills, out int kickCount, out float restTime, out Vector3 kickDrivePosition1, out float kickDriveTime1, out Vector3 startAcPoint, out Vector3 startAcDir, out float startAcVel,out float startAcT)
        {
            if (MyFunctions.Vector3IsNan(targetPosition) || targetPosition.Equals(Vector3.positiveInfinity) || targetPosition.Equals(Vector3.negativeInfinity))
            {
                kickCount = 0;
                restTime = 0;
                kickDrivePosition1 = Vector3.zero;
                kickDriveTime1 = 0;
                startAcPoint = Vector3.zero;
                startAcDir = Vector3.zero;
                startAcVel = 0;
                startAcT= 0;
                return Vector3.positiveInfinity;
            }

            Vector3 bodyPosition = playerDataComponent.position;
            float da = playerDataComponent.decceleration;
            float a = playerDataComponent.acceleration;
            float minSpeedForRotate = playerDataComponent.minSpeedForRotate;
            float maxAngleForRun = playerDataComponent.maxAngleForRun;
            float speed = playerDataComponent.currentSpeed;
            float t1_1 = speed > minSpeedForRotate ? AccelerationPath.getT(minSpeedForRotate, speed, da) : 0;
            Vector3 direction2 = MyFunctions.setY0ToVector3(targetPosition - bodyPosition).normalized;
            direction2.Normalize();
            float angle = Vector3.Angle(playerDataComponent.bodyY0Forward, direction2);
            float t1 = angle > maxAngleForRun ? t1_1 : 0;
            float dStartAc = AccelerationPath.getX3(speed, t1, -da);
            Vector3 x1 = bodyPosition + playerDataComponent.bodyY0Forward * dStartAc;
            startAcPoint = x1;
            
            float t22 = angle > maxAngleForRun ? Path.getT(maxAngleForRun, angle, playerDataComponent.maxSpeedRotation) : 0;
            float tStartAc = Mathf.Clamp(t1 + t22, 0, Mathf.Infinity);
            startAcT = t1 + t22;
            Vector3 direction = targetPosition - x1;
            direction.y = 0;
            direction.Normalize();
            if (t < t1) startAcDir = playerDataComponent.bodyY0Forward;
            else if (t < t22)
            {
                float endAngle = Mathf.Clamp(playerDataComponent.maxSpeedRotation * (t - t1), 0, angle);
                startAcDir = Quaternion.AngleAxis(endAngle, Vector3.up) * playerDataComponent.bodyY0Forward;
            }
            else
            {
                startAcDir = direction;
            }

            float scope = playerDataComponent.scope;
            float currentSpeed = playerDataComponent.currentSpeed;
            Vector3 bodyForward = playerDataComponent.bodyY0Forward;
            float maxSpeed = playerDataComponent.maxSpeed;
            float speedRotation = playerDataComponent.maxSpeedRotation;

            float maxDrivingDistance = drivingSkills.maxDrivingDistance;

            float dArriveMaxSpeed = Mathf.Abs(AccelerationPath.getX2(currentSpeed, maxSpeed, a));
            float dArriveMaxSpeedReachBall = Mathf.Abs(AccelerationPath.getX2(playerDataComponent.maxSpeedForReachBall, maxSpeed, da));
            float dNormal = maxDrivingDistance - dArriveMaxSpeed - dArriveMaxSpeedReachBall;

            float tRunNormal = dNormal / maxSpeed;
            float tArriveMaxSpeed = AccelerationPath.getT(maxSpeed, currentSpeed, a);
            float tArriveMaxSpeedReachBall = AccelerationPath.getT(maxSpeed, playerDataComponent.maxSpeedForReachBall, da);



            float dArriveMaxSpeed2 = Mathf.Abs(AccelerationPath.getX2(playerDataComponent.maxSpeedForReachBall, maxSpeed, a));
            float dNormal2 = maxDrivingDistance - dArriveMaxSpeed2 - dArriveMaxSpeedReachBall;
            float tRunNormal2 = dNormal2 / maxSpeed;
            float tArriveMaxSpeed2 = AccelerationPath.getT(maxSpeed, playerDataComponent.maxSpeedForReachBall, a);


            float totalT = tRunNormal + tArriveMaxSpeed + tArriveMaxSpeedReachBall;
            float totalT2 = tRunNormal2 + tArriveMaxSpeed2 + tArriveMaxSpeedReachBall;


            float t2 = (t - tStartAc) / totalT;
            float t3 = (t- tStartAc - totalT) / totalT2;

            int kickCount1 = (int)Mathf.Clamp01(Mathf.FloorToInt(t2));
            int kickCount2 = (int)Mathf.Clamp(Mathf.FloorToInt(t3), 0, Mathf.Infinity);
            kickDriveTime1 = totalT * kickCount1 + kickCount2 * totalT2 + t1 + t22;
            kickCount = kickCount1 + kickCount2;
            restTime = t - kickCount1 * totalT - kickCount2 * totalT2;
            float restTime2 = restTime;
            float dResult = kickCount * maxDrivingDistance;
            kickDrivePosition1 = x1 + direction * dResult;

            float dResult3 = Mathf.Lerp(0, dArriveMaxSpeedReachBall, restTime2 / tArriveMaxSpeedReachBall);
            restTime2 -= tArriveMaxSpeedReachBall;
            float dResult1 = Mathf.Lerp(0, dArriveMaxSpeed2, restTime2 / tArriveMaxSpeed);
            restTime2 -= tArriveMaxSpeed2;
            float dResult2 = Mathf.Lerp(0, dNormal2, restTime2 / tRunNormal2);
            dResult += dResult1 + dResult2 + dResult3;
            Vector3 result = x1 + direction * dResult;

            if (kickCount1 == 0)
            {
                if (t < t1)
                {
                    startAcVel = AccelerationPath.getV3(speed, playerDataComponent.minSpeedForRotate, t, da);
                }
                else if (t < t1 + t22)
                {
                    startAcVel = playerDataComponent.minSpeedForRotate;
                }
                else if (t < tArriveMaxSpeed - t1 - t22)
                {
                    float v00 = angle <= maxAngleForRun ? currentSpeed : playerDataComponent.minSpeedForRotate;
                    startAcVel = AccelerationPath.getV3(speed, maxSpeed, t - t1 - t22, a);
                }
                else if (t < tRunNormal - t1 - t22)
                {
                    startAcVel = maxSpeed;
                }
                else
                {
                    startAcVel = AccelerationPath.getV3(maxSpeed, playerDataComponent.maxSpeedForReachBall, t - t1 - t22 - tRunNormal - tArriveMaxSpeed, da);
                }
            }
            else
            {
                if (t < t1)
                {
                    startAcVel = AccelerationPath.getV3(speed, playerDataComponent.minSpeedForRotate, t, da);
                }
                else if (t < t1 + t22)
                {
                    startAcVel = playerDataComponent.minSpeedForRotate;
                }
                else if (t < tArriveMaxSpeed2 - t1 - t22)
                {
                    float v00 = angle <= maxAngleForRun ? currentSpeed : playerDataComponent.minSpeedForRotate;
                    startAcVel = AccelerationPath.getV3(speed, maxSpeed, t - t1 - t22, a);
                }
                else if (t < tRunNormal2 - t1 - t22)
                {
                    startAcVel = maxSpeed;
                }
                else
                {
                    startAcVel = AccelerationPath.getV3(maxSpeed, playerDataComponent.maxSpeedForReachBall, t - t1 - t22 - tRunNormal2 - tArriveMaxSpeed2, da);
                }
            }
            return result;
        }


        public static float accelerationGetTimeToReachPositionDriving(ref PlayerDataComponent playerDataComponent, Vector3 targetPosition, PlayerSkills drivingSkills, out int kickCount, out float tRest, out float kickDriveDistance1, out float reachLastKickTime, out float startAcVel)
        {
            if (MyFunctions.Vector3IsNan(targetPosition) || targetPosition.Equals(Vector3.positiveInfinity) || targetPosition.Equals(Vector3.negativeInfinity))
            {
                kickCount = 0;
                tRest = 0;
                kickDriveDistance1 = 0;
                reachLastKickTime = 0;
                startAcVel = 0;
                return -1;
            }

            float scope = playerDataComponent.scope;
            Vector3 bodyPosition = playerDataComponent.position;
            Vector3 direction = targetPosition - bodyPosition;
            float totalDistance = direction.magnitude - scope;
            direction.y = 0;
            direction.Normalize();
            float currentSpeed = playerDataComponent.currentSpeed;
            Vector3 bodyForward = playerDataComponent.bodyY0Forward;
            float da = playerDataComponent.decceleration;
            float a = playerDataComponent.acceleration;
            float maxSpeed = playerDataComponent.maxSpeed;
            float speed = playerDataComponent.currentSpeed;
            float speedRotation = playerDataComponent.maxSpeedRotation;
            float minSpeedForRotate = playerDataComponent.minSpeedForRotate;
            float maxAngleForRun = playerDataComponent.maxAngleForRun;
            float maxDrivingDistance = drivingSkills.maxDrivingDistance;
            float angle = Vector3.Angle(playerDataComponent.bodyY0Forward, direction);
            float t1_1 = speed > minSpeedForRotate ? AccelerationPath.getT(minSpeedForRotate, speed, da) : 0;
            float t11 = angle > maxAngleForRun ? t1_1 : 0;
            float t22 = angle > maxAngleForRun ? Path.getT(maxAngleForRun, angle, playerDataComponent.maxSpeedRotation) : 0;

            float dArriveMaxSpeed = Mathf.Abs(AccelerationPath.getX2(currentSpeed, maxSpeed, a));
            float dArriveMaxSpeedReachBall = Mathf.Abs(AccelerationPath.getX2(playerDataComponent.maxSpeedForReachBall, maxSpeed, da));
            float dCurrent_MaxSpeedReachBall = Mathf.Abs(AccelerationPath.getX2(playerDataComponent.maxSpeedForReachBall, currentSpeed, da));
            float dNormal = maxDrivingDistance - dArriveMaxSpeed - dArriveMaxSpeedReachBall;

            float tRunNormal = dNormal / maxSpeed;
            float tArriveMaxSpeed = AccelerationPath.getT(maxSpeed, currentSpeed, a);
            float tArriveMaxSpeedReachBall = AccelerationPath.getT(maxSpeed, playerDataComponent.maxSpeedForReachBall, da);
            float tCurrent_SpeedReachBall = AccelerationPath.getT(currentSpeed, playerDataComponent.maxSpeedForReachBall, da);



            float dArriveMaxSpeed2 = Mathf.Abs(AccelerationPath.getX2(playerDataComponent.maxSpeedForReachBall, maxSpeed, a));
            float dNormal2 = maxDrivingDistance - dArriveMaxSpeed2 - dArriveMaxSpeedReachBall;
            float tRunNormal2 = dNormal2 / maxSpeed;
            float tArriveMaxSpeed2 = AccelerationPath.getT(maxSpeed, playerDataComponent.maxSpeedForReachBall, a);


            float totalT = tRunNormal + tArriveMaxSpeed + tArriveMaxSpeedReachBall;
            float totalT2 = tRunNormal2 + tArriveMaxSpeed2 + tArriveMaxSpeedReachBall;
            float totalDriveKickDistance = dArriveMaxSpeed + dArriveMaxSpeedReachBall + dNormal;
            float totalDriveKickDistance2 = dArriveMaxSpeed2 + dArriveMaxSpeedReachBall + dNormal2;

            float d1 = (totalDistance) / totalDriveKickDistance;
            float d2 = (totalDistance - totalDriveKickDistance) / totalDriveKickDistance2;


            int kickCount1 = (int)Mathf.Clamp01(Mathf.FloorToInt(d1));
            int kickCount2 = (int)Mathf.Clamp(Mathf.FloorToInt(d2), 0, Mathf.Infinity);
            kickDriveDistance1 = totalDriveKickDistance * kickCount1 + totalDriveKickDistance2 * kickCount2;
            kickCount = kickCount1 + kickCount2;
            reachLastKickTime = totalT * kickCount1 + totalT2 * kickCount2 ;
            float dRest = totalDistance - kickDriveDistance1;
            float tResult;
            if (kickCount1 != 0)
            {
                if (dArriveMaxSpeedReachBall + dArriveMaxSpeed2 >= dRest)
                {
                    float startDaT = AccelerationPath.getT_StartToDesacelerate(playerDataComponent.maxSpeedForReachBall, playerDataComponent.maxSpeedForReachBall, a, -da, dRest);
                    float vStartDa = AccelerationPath.getV2(playerDataComponent.maxSpeedForReachBall, startDaT, a);
                    float tStartDa = Mathf.Abs(AccelerationPath.getT(playerDataComponent.maxSpeedForReachBall, vStartDa, da));
                    tRest = startDaT + tStartDa;
                    tResult = reachLastKickTime + tRest;

                }
                else
                {
                    float t2 = Mathf.Lerp(0, tArriveMaxSpeed2, dRest / dArriveMaxSpeed2);
                    float t1 = Mathf.Lerp(0, tArriveMaxSpeedReachBall, (dRest - dArriveMaxSpeed2) / dArriveMaxSpeedReachBall);
                    float t3 = Mathf.Lerp(0, tRunNormal2, (dRest - dArriveMaxSpeedReachBall - dArriveMaxSpeed2) / dNormal2);

                    tRest = t1 + t2 + t3;
                    tResult = reachLastKickTime + t1 + t2 + t3;

                }
                if (tResult < t11)
                {
                    startAcVel = AccelerationPath.getV3(speed, playerDataComponent.minSpeedForRotate, tResult, da);
                }
                else if (tResult < t11 + t22)
                {
                    startAcVel = playerDataComponent.minSpeedForRotate;
                }
                else if (tResult < tArriveMaxSpeed2)
                {
                    float v00 = angle <= maxAngleForRun ? currentSpeed : playerDataComponent.minSpeedForRotate;
                    startAcVel = AccelerationPath.getV3(speed, maxSpeed, tResult, a);
                }
                else if (tResult < tRunNormal2)
                {
                    startAcVel = maxSpeed;
                }
                else
                {
                    startAcVel = AccelerationPath.getV3(maxSpeed, playerDataComponent.maxSpeedForReachBall, tResult - tRunNormal2 - tArriveMaxSpeed2, da);
                }
            }
            else
            {
                if (dArriveMaxSpeedReachBall + dArriveMaxSpeed >= dRest)
                {
                    float startDaT = AccelerationPath.getT_StartToDesacelerate(currentSpeed, playerDataComponent.maxSpeedForReachBall, a, -da, dRest);
                    float xStartDa = AccelerationPath.getX3(currentSpeed, startDaT, a);
                    float vStartDa = AccelerationPath.getV2(currentSpeed, startDaT, a);
                    float tStartDa = Mathf.Abs(AccelerationPath.getT(playerDataComponent.maxSpeedForReachBall, vStartDa, da));
                    tRest = startDaT + tStartDa;
                    tResult = reachLastKickTime + tRest;

                }
                else
                {

                    float t2 = Mathf.Lerp(0, tArriveMaxSpeed, dRest / dArriveMaxSpeed);
                    float t1 = Mathf.Lerp(0, tArriveMaxSpeedReachBall, (dRest - dArriveMaxSpeed) / dArriveMaxSpeedReachBall);
                    float t3 = Mathf.Lerp(0, tRunNormal, (dRest - dArriveMaxSpeedReachBall - dArriveMaxSpeed) / dNormal);

                    tRest = t1 + t2 + t3;
                    tResult = reachLastKickTime + t1 + t2 + t3;


                }
                if (tResult < t11)
                {
                    startAcVel = AccelerationPath.getV3(speed, playerDataComponent.minSpeedForRotate, tResult, da);
                }
                else if (tResult < t11 + t22)
                {
                    startAcVel = playerDataComponent.minSpeedForRotate;
                }
                else if (tResult < tArriveMaxSpeed)
                {
                    float v00 = angle <= maxAngleForRun ? currentSpeed : playerDataComponent.minSpeedForRotate;
                    startAcVel = AccelerationPath.getV3(speed, maxSpeed, tResult, a);
                }
                else if (tResult < tRunNormal)
                {
                    startAcVel = maxSpeed;
                }
                else
                {
                    startAcVel = AccelerationPath.getV3(maxSpeed, playerDataComponent.maxSpeedForReachBall, tResult- tRunNormal - tArriveMaxSpeed, da);
                }


            }
            return tResult;

        }
    }
}
