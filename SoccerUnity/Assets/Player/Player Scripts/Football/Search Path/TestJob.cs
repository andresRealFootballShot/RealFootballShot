using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using DOTS_ChaserDataCalculation;

[BurstCompile]
public struct PathDataDOTS_V2 : IComponentData
{
    public int index;
    public PathType pathType;
    public float t0;
    public Vector3 Pos0, Posf;
    public Vector3 V0, normalizedV0;
    public float v0Magnitude;
    public PathDataDOTS_V2(int index, PathType pathType, float t0, Vector3 pos0, Vector3 Posf, Vector3 v0)
    {
        this.index = index;
        this.pathType = pathType;
        this.t0 = t0;
        Pos0 = pos0;
        V0 = v0;
        normalizedV0 = V0.normalized;
        v0Magnitude = V0.magnitude;
        this.Posf = Posf;
    }
}
[BurstCompile]
public struct SegmentedPathCalculationData_V2 : IComponentData
{
    public float k;
    public float mass;
    public float groundY;
    public float bounciness, friction, slidingFriction, ballRadio, g;
    public float timeRange, timeIncrement, startSegmentedTime, maxTime;
    public float minAngle, minVelocity, maxAngle, maxVelocity, vfMagnitude;

    public SegmentedPathCalculationData_V2(float k, float mass, float groundY, float bounciness, float friction, float slidingFriction, float ballRadio, float g, float timeRange, float timeIncrement, float startSegmentedTime, float maxTime, float minAngle, float minVelocity, float maxAngle, float maxVelocity) : this()
    {
        this.k = k;
        this.mass = mass;
        this.groundY = groundY;
        this.bounciness = bounciness;
        this.friction = friction;
        this.slidingFriction = slidingFriction;
        this.ballRadio = ballRadio;
        this.g = g;
        this.timeRange = timeRange;
        this.timeIncrement = timeIncrement;
        this.startSegmentedTime = startSegmentedTime;
        this.maxTime = maxTime;
        this.minAngle = minAngle;
        this.minVelocity = minVelocity;
        this.maxAngle = maxAngle;
        this.maxVelocity = maxVelocity;
        vfMagnitude = (g / k);
    }
}
[BurstCompile]
public struct TestJob : IJobEntityBatch
{
    //public NativeArray<float> results;
    [ReadOnly] public BufferTypeHandle<AreaPlaneElement> areaPlanesHandle;
    //[ReadOnly] public ComponentTypeHandle<PathComponent> PathComponentHandle;
    //[ReadOnly] public SharedComponentTypeHandle<SegmentedPathCalculationData_V2> SegmentedPathCalculationDataHandle;
    //[ReadOnly] public ComponentTypeHandle<SegmentedPathCalculationData_V2> SegmentedPathCalculationDataHandle;
    public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
    {
        BufferAccessor<AreaPlaneElement> areaPlanesBufferAccesor = batchInChunk.GetBufferAccessor(areaPlanesHandle);
        //NativeArray<PathComponent> PathComponents = batchInChunk.GetNativeArray(PathComponentHandle);
        //NativeArray<SegmentedPathCalculationData_V2> SegmentedPathCalculationDatas = batchInChunk.GetNativeArray<SegmentedPathCalculationData_V2>(SegmentedPathCalculationDataHandle);
        for (int i = 0; i < areaPlanesBufferAccesor.Length; i++)
        {
            DynamicBuffer<AreaPlaneElement> areaPlanesBuffer = areaPlanesBufferAccesor[i];
            //PathComponent pathComponent = PathComponents[i];
           //SegmentedPathCalculationData_V2 SegmentedPathCalculationData = SegmentedPathCalculationDatas[i];
            
            //NativeArray<AreaPlaneElement> fullAreaPlanesBuffer = new NativeArray<AreaPlaneElement>(4, Allocator.Temp);
            //GetPerfectPassJob.getAreas(0, ref areaPlanesBuffer, ref fullAreaPlanesBuffer);
            SegmentedPathElement segmentedPath = new SegmentedPathElement();
            int count = 0;
            float t;
            /*float t = pathComponent.segmentedPathCalculationData.startSegmentedTime;
            float timeRange = t + pathComponent.segmentedPathCalculationData.timeRange;
            float timeIncrement = pathComponent.segmentedPathCalculationData.timeIncrement;
            float maxTime = pathComponent.segmentedPathCalculationData.maxTime;*/
            int index = 0;
            bool exit = false;

                count++;
                if (count > 500)
                {
                    //Debug.LogError("SegmentedPathJob count>100 3");
                    break;
                }
               //if (SegmentedPathJob.segmentedPathCalculation(t, pathComponent.segmentedPathCalculationData, ref pathComponent, ref segmentedPath))
               if(true)
                {

                segmentedPath.Pos0 = Vector3.zero;
                segmentedPath.Posf = Vector3.one;
                segmentedPath.V0 = Vector3.forward;
                segmentedPath.V0Magnitude = 10;
                segmentedPath.Vf = Vector3.forward;
                segmentedPath.t0 = 0;
                segmentedPath.tf = 1;

                t = segmentedPath.tf;
                    segmentedPath.index = index;
                    index++;
                    Vector3 intercession;
                    float targetIntercessionTime;
                    for (int k = 0; k < 100; k++)
                    {
                        for (int j = 0; j < 11; j++)
                        {
                            //checkIntersecctionArea2(segmentedPath, areaPlanesBuffer, out intercession, out targetIntercessionTime);
                        }
                    }
                    
                }
                else
                {
                    //t += timeIncrement;
                }
            
        }
        
    }
    
}
