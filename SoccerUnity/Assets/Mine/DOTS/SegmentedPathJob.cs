using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;

namespace DOTS_ChaserDataCalculation
{
    [BurstCompile]
    public struct SegmentedPathCalculationData
    {
        public float timeRange, timeIncrement,startSegmentedTime,maxTime;
        public float minAngle, minVelocity, maxAngle, maxVelocity;

        public SegmentedPathCalculationData(float timeRange, float timeIncrement, float startSegmentedTime, float minAngle, float minVelocity, float maxAngle, float maxVelocity, float maxTime)
        {
            this.timeRange = timeRange;
            this.timeIncrement = timeIncrement;
            this.startSegmentedTime = startSegmentedTime;
            this.minAngle = minAngle;
            this.minVelocity = minVelocity;
            this.maxAngle = maxAngle;
            this.maxVelocity = maxVelocity;
            this.maxTime = maxTime;
        }
    }
    [BurstCompile]
    public struct SegmentedPathJob : IJobEntityBatch
    {
        public ComponentTypeHandle<PathComponent> pathComponentHandle;
        public EntityCommandBuffer.ParallelWriter ConcurrentCommands;
        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            NativeArray<PathComponent> pathComponents = batchInChunk.GetNativeArray(pathComponentHandle);
            for (int i = 0; i < pathComponents.Length; i++)
            {
                SegmentedPathElement segmentedPath = new SegmentedPathElement();
                //segmentedPathbufferAccessor[i].Clear();
                PathComponent pathComponent = pathComponents[i];
                
                int count = 0;
                float t = pathComponents[i].segmentedPathCalculationData.startSegmentedTime;
                float timeRange = t + pathComponent.segmentedPathCalculationData.timeRange;
                float timeIncrement = pathComponents[i].segmentedPathCalculationData.timeIncrement;
                float maxTime = pathComponents[i].segmentedPathCalculationData.maxTime;
                int index = 0;
                while (t < timeRange && t < maxTime)
                {
                    count++;
                    if (count > 500)
                    {
                        Debug.LogError("SegmentedPathJob count>100 3");
                        break;
                    }
                    if(segmentedPathCalculation(t, pathComponents[i].segmentedPathCalculationData, ref pathComponent, ref segmentedPath))
                    {
                        t = segmentedPath.tf;
                        segmentedPath.index = index;
                        index++;
                        //ConcurrentCommands.AppendToBuffer<SegmentedPathElement>(batchIndex, pathComponent.segmentedPathBufferEntity, segmentedPath);
                    }
                    else
                    {
                        //Debug.LogError("SegmentedPathJob segmentedPath not founded");
                        t += timeIncrement;
                    }

                }
            }
            
        }
        
        [BurstCompile]
        public static bool segmentedPathCalculation(float t,in SegmentedPathCalculationData segmentedPathCalculationData,ref PathComponent pathComponent,ref SegmentedPathElement segmentedPathResult)
        {
            ref PathDataDOTS currentPath = ref pathComponent.currentPath;
            PathDataDOTS newPath = currentPath;
            float tf = t;
            float t0 =tf;
            float timeRange = segmentedPathCalculationData.startSegmentedTime +  segmentedPathCalculationData.timeRange;
            float timeIncrement = segmentedPathCalculationData.timeIncrement;
            float minAngle = segmentedPathCalculationData.minAngle;
            float maxAngle = segmentedPathCalculationData.maxAngle;
            float minVelocity = segmentedPathCalculationData.minVelocity;
            float maxVelocity = segmentedPathCalculationData.maxVelocity;
            Vector3 v0 = currentPath.V0;
            Vector3 v = Vector3.zero;
            
            int count = 0;
            int pathIndex = currentPath.index;
            float vm;
            float angle;
            Vector3 pos0 = Vector3.zero;
            Vector3 posf = Vector3.zero;
            BouncyPathDOTS.getVelocityAtTime(t, currentPath, ref v0);
            float v0m = v0.magnitude;
            do
            {
                if (count > 100)
                {
                    //Debug.LogError("SegmentedPathJob count>100 1");
                }
                
                tf = Mathf.Clamp(tf + timeIncrement, 0, timeRange);
                if (pathComponent.currentPath.pathType==PathType.Parabolic)
                {
                    if(BouncyPathDOTS.getBouncePath(tf, ref newPath,0))
                    {
                        BouncyPathDOTS.getPositionAtTime(t0, currentPath, ref pos0);
                        segmentedPathResult.Pos0 = pos0;
                        segmentedPathResult.Posf = newPath.Pos0;
                        segmentedPathResult.V0 = v0;
                        segmentedPathResult.V0Magnitude = v0m;
                        segmentedPathResult.t0 = t0;
                        segmentedPathResult.tf = newPath.t0;
                        BouncyPathDOTS.getVelocityAtTime(newPath.t0, currentPath, ref v);
                        segmentedPathResult.Vf = v;
                        currentPath = newPath;

                        return true;
                    }
                    else
                    {
                        currentPath = newPath;
                    }
                }
                BouncyPathDOTS.getVelocityAtTime(tf, currentPath, ref v);
                angle = Vector3.Angle(v0, v);
                vm = v.magnitude;
            } while (angle < minAngle && Mathf.Abs(vm - v0m) < minVelocity && tf < timeRange && pathIndex == currentPath.index);
            
            Vector3 dir;
            Vector3 newV0;
            if(tf >= timeRange)
            {
                BouncyPathDOTS.getPositionAtTime(t0, currentPath, ref pos0);
                BouncyPathDOTS.getPositionAtTime(tf, currentPath, ref posf);
                segmentedPathResult.Pos0 = pos0;
                segmentedPathResult.Posf = posf;
                segmentedPathResult.V0 = v0;
                segmentedPathResult.V0Magnitude = v0m;
                segmentedPathResult.Vf = v;
                segmentedPathResult.t0 = t0;
                segmentedPathResult.tf = tf;
                return true;
            }
            if (angle == 0 && Mathf.Abs(vm - v0m) ==0)
            {
                BouncyPathDOTS.getPositionAtTime(t0,currentPath, ref pos0);
                BouncyPathDOTS.getPositionAtTime(tf, currentPath, ref posf);
                segmentedPathResult.Pos0 = pos0;
                segmentedPathResult.Posf = posf;
                segmentedPathResult.V0 = v0;
                segmentedPathResult.V0Magnitude = v0m;
                segmentedPathResult.Vf = v;
                segmentedPathResult.t0 = t0;
                segmentedPathResult.tf = tf;
                return true;
            }
            if(pathIndex != currentPath.index)
            {
                BouncyPathDOTS.getPositionAtTime(t0, currentPath, ref pos0);
                BouncyPathDOTS.getPositionAtTime(tf, currentPath, ref posf);
                dir = posf - pos0;
                newV0 = dir / (tf - t0);
                segmentedPathResult.Pos0 = pos0;
                segmentedPathResult.Posf = posf;
                segmentedPathResult.V0 = newV0;
                segmentedPathResult.V0Magnitude = v0m;
                segmentedPathResult.Vf = v;
                segmentedPathResult.t0 = t0;
                segmentedPathResult.tf = tf;
                return true;
            }
            while (t0 <= tf && tf < timeRange && Mathf.Abs(v.magnitude - v0m) > 0)
            {
                count++;

                if (count > 100)
                {
                    //Debug.LogError("SegmentedPathJob count>100 2");
                    break;
                }
                timeIncrement /= 2;
                if (Vector3.Angle(v0, v) > maxAngle || Mathf.Abs(v.magnitude - v0m) > maxVelocity)
                {
                    //El segmento no sigue de forma adecuada la trayectoria ó la velocidad varía mucho durante el segmento
                    tf -= timeIncrement;
                    BouncyPathDOTS.getVelocityAtTime(tf,currentPath,ref v);
                }
                else if (Vector3.Angle(v0, v) < minAngle && Mathf.Abs(v.magnitude - v0m) < minVelocity)
                {
                    //El segmento es casi identico a la trayectoria y la velocidad no varía lo suficiente así que buscaremos en un instante posterior para intentar tener menos segmentos de la trayectoria

                    tf += timeIncrement;
                    BouncyPathDOTS.getVelocityAtTime(tf, currentPath, ref v);
                }
                else
                {
                    BouncyPathDOTS.getPositionAtTime(t0,currentPath,ref pos0);
                    BouncyPathDOTS.getPositionAtTime(tf,currentPath,ref posf);
                    dir = posf - pos0;
                    newV0 = dir / (tf - t0);
                    segmentedPathResult.Pos0 = pos0;
                    segmentedPathResult.Posf = posf;
                    segmentedPathResult.V0 = newV0;
                    segmentedPathResult.V0Magnitude = v0m;
                    segmentedPathResult.Vf = v;
                    segmentedPathResult.t0 = t0;
                    segmentedPathResult.tf = tf;
                    return true;
                }
            }

            return false;

        }
        
        

    }
}
