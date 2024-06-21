using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using Unity.Mathematics;
using CullPositionPoint;

namespace DOTS_ChaserDataCalculation
{
    [BurstCompile]
    public class GetTimeToReachPointDOTS
    {
        public static float getTimeToReachPosition(ref PlayerDataComponent playerDataComponent, Vector3 targetPosition)
        {
            if (playerDataComponent.useAccelerationGetTimeToReachPosition&&false)
            {
                return accelerationGetTimeToReachPosition(ref playerDataComponent, targetPosition);
            }
            else
            {
                return linearGetTimeToReachPosition(ref playerDataComponent, targetPosition);
            }
        }
        public static float linearGetTimeToReachPosition(Vector3 playerPosition, Vector3 targetPosition,float maxSpeed,float scope)
        {
            float distance = Mathf.Clamp(Vector3.Distance(MyFunctions.setYToVector3(playerPosition, targetPosition.y), targetPosition)-scope,0,Mathf.Infinity);
            float t = distance / maxSpeed;
            return t;
        }
        public static float linearGetTimeToReachPosition(ref PlayerDataComponent playerDataComponent, Vector3 targetPosition)
        {
            float distance = Vector3.Distance(MyFunctions.setYToVector3(playerDataComponent.position, targetPosition.y), targetPosition);
            float t = distance / playerDataComponent.maxSpeed;
            return t;
        }
        public static Vector3 accelerationGetPosition(Vector3 playerPosition, float currentSpeed, Vector3 bodyForward,float speedRotation,float minSpeedForRotate, float a,float da,float maxAngleForRun,float scope, Vector3 targetPosition,float maxSpeed,float t)
        {
            if (MyFunctions.Vector3IsNan(targetPosition) || targetPosition.Equals(Vector3.positiveInfinity) || targetPosition.Equals(Vector3.negativeInfinity))
            {
                return Vector3.positiveInfinity;
            }

            Vector3 bodyPosition = playerPosition;
            float d4 = Vector3.Distance(bodyPosition, MyFunctions.setYToVector3(targetPosition, bodyPosition.y));
            if (d4 < scope)
            {
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
                if(speed > minSpeedForRotate)
                {

                    float t1_1 = AccelerationPath.getT(minSpeedForRotate, speed, da);
                    //float xToStop = speed > minSpeedForRotate ? AccelerationPath.getX3(speed, t, -da) : 0;
                    t1 = t1_1;
                    float t11 = Mathf.Clamp(t1, 0, t);
                   
                    float d = Mathf.Abs(AccelerationPath.getX3(speed, t11, -da));
                    x1 = playerPosition+direction*d;
                    if (t1 >= t)
                    {
                        return x1;
                    }
                }
               
                direction = targetPosition - x1;
                direction.y = 0;
                direction.Normalize();
                t2 = Path.getT(maxAngleForRun, angle,speedRotation);
                t2 = Mathf.Clamp(t2, 0,t);
                v0Magnitude = 0;
            }
            t -= t1 + t2;
            float distance = 0;
            if (v0Magnitude < maxSpeed && t > 0)
            {
                float t3 = AccelerationPath.getT(v0Magnitude, maxSpeed, a);
                t3 = Mathf.Clamp(t3, 0, t);
                distance += Mathf.Abs(AccelerationPath.getX2(v0Magnitude, maxSpeed, a));
                t -= t3;
            }
            if (t > 0)
            {
                Vector3 x = x1 + direction * distance;
                float d = Vector3.Distance(x,targetPosition);
                distance += Mathf.Clamp(maxSpeed * t, 0, d);
            }

            Vector3 result = x1 + direction * distance;
            return result;
        }
        public static float accelerationGetTimeToReachPosition(Vector3 playerPosition,float currentSpeed,Vector3 bodyForward,Vector3 normalizedForwardVelocity, ref PlayerGenericParams PlayerGenericParams, Vector3 targetPosition)
        {
            if (MyFunctions.Vector3IsNan(targetPosition) || targetPosition.Equals(Vector3.positiveInfinity) || targetPosition.Equals(Vector3.negativeInfinity))
            {
                return Mathf.Infinity;
            }

            Vector3 bodyPosition = playerPosition;
            float d4 = Vector3.Distance(bodyPosition, MyFunctions.setYToVector3(targetPosition, bodyPosition.y));
            if (d4 < PlayerGenericParams.scope)
            {
                return 0;
            }
            float speed = currentSpeed;
            float minSpeedForRotate = PlayerGenericParams.minSpeedForRotate;
            float a = PlayerGenericParams.acceleration;
            float da = PlayerGenericParams.decceleration;
            float maxAngleForRun = PlayerGenericParams.maxAngleForRun;
            float t1_1 = speed > minSpeedForRotate ? AccelerationPath.getT(minSpeedForRotate, speed, da) : 0;
            Vector3 direction = MyFunctions.setY0ToVector3(targetPosition - bodyPosition).normalized;
            float angle = Vector3.Angle(bodyForward, direction);
            float t1 = angle > maxAngleForRun ? t1_1 : 0;
            Vector3 x1 = AccelerationPath.getX(bodyPosition, bodyForward, bodyForward * speed, t1, -da);
            float t2 = angle > maxAngleForRun ? Path.getT(maxAngleForRun, angle, PlayerGenericParams.maxSpeedRotation) : 0;

            Vector3 v0 = angle > maxAngleForRun ? Vector3.zero : normalizedForwardVelocity * speed;
            float v0Magnitude = v0.magnitude;
            float d2 = Vector3.Distance(MyFunctions.setY0ToVector3(x1), MyFunctions.setY0ToVector3(targetPosition));

            float d3 = d2 - PlayerGenericParams.scope;
            float d = AccelerationPath.getDistanceWhereStartDecelerate(v0Magnitude, PlayerGenericParams.maxSpeedForReachBall, a, -da, d3);
            float t3, t4, t8, result;
            float x3 = Mathf.Abs(AccelerationPath.getX2(v0Magnitude, PlayerGenericParams.maxSpeed, a));
            if (x3 < d)
            {
                float t5 = AccelerationPath.getT(PlayerGenericParams.maxSpeed, v0Magnitude, a);
                float x4 = Mathf.Abs(AccelerationPath.getX2(PlayerGenericParams.maxSpeed, PlayerGenericParams.maxSpeedForReachBall, da));
                float x5 = d3 - x3 - x4;
                float t6 = x5 / PlayerGenericParams.maxSpeed;
                float t7 = AccelerationPath.getT(PlayerGenericParams.maxSpeedForReachBall, PlayerGenericParams.maxSpeed, da);
                t8 = t5 + t6 + t7;
                result = t1 + t2 + t8;
            }
            else
            {
                AccelerationPath.getT(d, v0Magnitude, a, out t3);
                float v1 = v0Magnitude + a * t3;
                t4 = AccelerationPath.getT(PlayerGenericParams.maxSpeedForReachBall, v1, da);
                t8 = t3 + t4;
                result = t1 + t2 + t8;

            }
            return result;
        }
        public static float accelerationGetTimeToReachPosition(ref PlayerDataComponent playerDataComponent, Vector3 targetPosition)
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
            float v0Magnitude= v0.magnitude;
            float d2 = Vector3.Distance(MyFunctions.setY0ToVector3(x1), MyFunctions.setY0ToVector3(targetPosition));
            
            float d3 = d2 - playerDataComponent.scope;
            float d = AccelerationPath.getDistanceWhereStartDecelerate(v0Magnitude, playerDataComponent.maxSpeedForReachBall, a, -da, d3);
            float t3, t4, t8, result;
            float x3 = Mathf.Abs(AccelerationPath.getX2(v0Magnitude, playerDataComponent.maxSpeed, a));
            if (x3 < d)
            {
                float t5 = AccelerationPath.getT(playerDataComponent.maxSpeed, v0Magnitude, a);
                float x4 = Mathf.Abs(AccelerationPath.getX2(playerDataComponent.maxSpeed, playerDataComponent.maxSpeedForReachBall, da));
                float x5 = d3 - x3 - x4;
                float t6 = x5 / playerDataComponent.maxSpeed;
                float t7 = AccelerationPath.getT(playerDataComponent.maxSpeedForReachBall, playerDataComponent.maxSpeed, da);
                t8 = t5 + t6 + t7;
                result = t1 + t2 + t8;
            }
            else
            {
                AccelerationPath.getT(d, v0Magnitude, a, out t3);
                float v1 = v0Magnitude + a* t3;
                t4 = AccelerationPath.getT(playerDataComponent.maxSpeedForReachBall, v1, da);
                t8 = t3 + t4;
                result = t1 + t2 + t8;

            }
            return result;
        }
    }
}
