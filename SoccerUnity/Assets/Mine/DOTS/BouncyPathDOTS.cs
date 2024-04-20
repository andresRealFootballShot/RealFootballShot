using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
namespace DOTS_ChaserDataCalculation
{
    public class BouncyPathDOTS
    {
        [BurstCompile]
        public static void getVelocityAtTime(float t,in PathDataDOTS currentPath,ref Vector3 velocity)
        {
            if (currentPath.pathType==PathType.InGround)
            {
                StraightXZDragAndFrictionPathDOTS.getVelocityAtTime(t - currentPath.t0, currentPath,ref velocity);
            }
            else
            {
                ParabolicWithDragDOTS.ParabolicWithDrag_GetVelocityAtTime(t - currentPath.t0, currentPath, ref velocity);
            }
        }
        [BurstCompile]
        public static void getPositionAtTime(float t, in PathDataDOTS currentPath,ref Vector3 position)
        {
            t = Mathf.Clamp(t - currentPath.t0,0,Mathf.Infinity);
            if (currentPath.pathType==PathType.InGround)
            {
                StraightXZDragAndFrictionPathDOTS.getPositionAtTime(t, currentPath, ref position);
            }
            else
            {
                ParabolicWithDragDOTS.ParabolicWithDragPositionAtTime(t, currentPath, ref position);
            }
        }
        [BurstCompile]
        public static bool getBouncePath(float t,ref PathDataDOTS currentPath,int attempt)
        {
            if (currentPath.pathType==PathType.InGround)
            {
                return false;
            }

            float minGroundYVelocity = 0.4f;
            if (Mathf.Abs(currentPath.V0.y) < minGroundYVelocity && currentPath.Pos0.y<=currentPath.groundY+0.01f)
            {
                currentPath.pathType = PathType.InGround;
                currentPath.V0.y = 0;
                currentPath.v0Magnitude = currentPath.V0.magnitude;
                currentPath.normalizedV0 = currentPath.V0.normalized;
                return false;
            }
            float pathTime = currentPath.t0;
            float t2 = t - pathTime;
            float groundY = currentPath.groundY-0.0f;
            Vector3 pos = Vector3.zero;
            Vector3 newPos = Vector3.zero;
            Vector3 newVelocity = Vector3.zero;
            ParabolicWithDragDOTS.ParabolicWithDragPositionAtTime(t2,currentPath,ref pos);
            if (pos.y < groundY)
            {
                float timeToReachY;
                bool thereIsTimeY=ParabolicWithDragDOTS.getTimeToReachY(groundY, currentPath, t2,out timeToReachY);
                if (thereIsTimeY)
                {
                    
                     ParabolicWithDragDOTS.ParabolicWithDragPositionAtTime(timeToReachY, currentPath,ref newPos);
                     newPos.y = currentPath.groundY;
                     ParabolicWithDragDOTS.ParabolicWithDrag_GetVelocityAtTime(timeToReachY, currentPath,ref newVelocity);
                    if (Mathf.Abs(newVelocity.y) < minGroundYVelocity)
                    {
                        currentPath.index++;
                        currentPath.pathType = PathType.InGround;
                        currentPath.Pos0 = newPos;
                        newVelocity.y = 0;
                        currentPath.V0 = newVelocity;
                        currentPath.normalizedV0 = currentPath.V0.normalized;
                        currentPath.v0Magnitude = newVelocity.magnitude;
                        currentPath.t0 = timeToReachY+ pathTime;
                        return true;
                    }
                    else
                    {
                        getBounceV(newVelocity, currentPath.bounciness, currentPath.slidingFriction, ref currentPath.V0);
                        currentPath.v0Magnitude = currentPath.V0.magnitude;
                        currentPath.normalizedV0 = currentPath.V0.normalized;
                        currentPath.index++;
                        currentPath.Pos0 = newPos;
                        currentPath.t0 = timeToReachY + pathTime;
                        return true;
                    }
                    if (attempt < 10)
                    {
                        attempt++;
                        return getBouncePath(t, ref currentPath,attempt);
                    }
                }
                else
                {
                    currentPath.Pos0 = pos;
                    currentPath.Pos0.y = currentPath.groundY;
                    ParabolicWithDragDOTS.ParabolicWithDrag_GetVelocityAtTime(t2, currentPath, ref newVelocity);
                    if (Mathf.Abs(newVelocity.y) < minGroundYVelocity)
                    {
                        currentPath.index++;
                        currentPath.pathType = PathType.InGround;
                        newVelocity.y = 0;
                        currentPath.V0 = newVelocity;
                        currentPath.v0Magnitude = newVelocity.magnitude;
                        currentPath.normalizedV0 = newVelocity.normalized;
                        currentPath.t0 = t;
                        return true;
                    }
                    else
                    {
                        currentPath.index++;
                        getBounceV(newVelocity, currentPath.bounciness, currentPath.slidingFriction, ref currentPath.V0);
                        currentPath.normalizedV0 = currentPath.V0.normalized;
                        currentPath.v0Magnitude = currentPath.V0.magnitude;
                        currentPath.t0 = t;
                        return true;
                    }
                }
            }
            return false;
        }
        [BurstCompile]
        public static void getBounceV(Vector3 inputV,float bounciness, float slidingFriction,ref Vector3 result)
        {
            result.x = inputV.x - (slidingFriction * (1 - bounciness) * inputV.x);
            result.y = Mathf.Abs(inputV.y) * bounciness;
            result.z = inputV.z - (slidingFriction * (1 - bounciness) * inputV.z);
        }
    }
}
