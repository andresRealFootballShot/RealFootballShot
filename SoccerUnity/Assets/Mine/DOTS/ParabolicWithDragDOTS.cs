using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
namespace DOTS_ChaserDataCalculation
{
    public class ParabolicWithDragDOTS
    {
        [BurstCompile]
        public static void ParabolicWithDragPositionAtTime(float t,in PathDataDOTS pathData,ref Vector3 endPosition)
        {
            Vector3 V0 = pathData.V0;
            Vector3 Pos0 = pathData.Pos0;
            float k = pathData.k;
            Vector2 vx0 = new Vector2(V0.x, V0.z);
            float ekt = Mathf.Exp(-k * t);
            Vector2 x = (vx0 / k) * (1 - ekt);
            float y = ParabolicWithDragGetPosYAtTime(t, pathData);
            endPosition = new Vector3(x.x, y, x.y) + new Vector3(Pos0.x, 0, Pos0.z);
        }
        [BurstCompile]
        public static void ParabolicWithDrag_GetVelocityAtTime(float t, in PathDataDOTS pathData,ref Vector3 velocity)
        {
            float ekt = Mathf.Exp(-pathData.k * t);
            float vx = pathData.V0.x * ekt;
            float vz = pathData.V0.z * ekt;
            float vy = -pathData.vfMagnitude + (pathData.V0.y + pathData.vfMagnitude) * ekt;
            velocity = new Vector3(vx, vy, vz);
        }
        public static void GetVelocityAtTime(float t, Vector3 V0,float k,float vf, ref Vector3 velocity)
        {
            float ekt = Mathf.Exp(-k * t);
            float vx = V0.x * ekt;
            float vz = V0.z * ekt;
            float vy = -vf + (V0.y + vf) * ekt;
            velocity = new Vector3(vx, vy, vz);
        }
        [BurstCompile]
        public static float ParabolicWithDragGetPosYAtTime(float t, in PathDataDOTS pathData)
        {
            float k = pathData.k;
            float ekt = Mathf.Exp(-k * t);
            float vy0 = pathData.V0.y;
            float vfMagnitude = pathData.vfMagnitude;
            float pos0y = pathData.Pos0.y;
            float y = -vfMagnitude * t + ((vy0 + vfMagnitude) / k) * (1 - ekt);
            return y + pos0y;
        }
        [BurstCompile]
        public static bool getTimeToReachY(float y, in PathDataDOTS pathData,float tf,out float result)
        {
            float left, right,centralT;
            int attempts = 0,maxAttempts=20;
            left = 0;
            right = tf;
            centralT = (right - left) / 2;
            float precision = 0.01f;
            while (left <= right && attempts <= maxAttempts)
            {
                float centralY = ParabolicWithDragGetPosYAtTime(centralT,pathData);
                if (Mathf.Abs(centralY - y) <= precision)
                {
                    result = centralT;
                    return true;
                }else if (centralY < y)
                {
                    right = centralT;
                    centralT -= (right - left) / 2;
                    
                }
                else
                {
                    left = centralT;
                    centralT += (right - left) / 2;
                    
                }
                attempts++;
            }
            result = 0;
            return false;
        }
    }
}
