using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
namespace DOTS_ChaserDataCalculation
{
    public struct PathIsCalculatedComponent : IComponentData
    {
    }
    [BurstCompile]
    public struct PathComponent : IComponentData
    {
        
        public PathDataDOTS currentPath;
        public SegmentedPathCalculationData segmentedPathCalculationData;

        public PathComponent(PathDataDOTS currentPath, SegmentedPathCalculationData segmentedPathCalculationData)
        {
            this.currentPath = currentPath;
            this.segmentedPathCalculationData = segmentedPathCalculationData;
        }
    }
    public enum PathType
    {
        Parabolic,InGround
    }
    [BurstCompile]
    public struct PathDataDOTS
    {
        public int index;
        public PathType pathType;
        public float t0;
        public Vector3 Pos0,Posf;
        public Vector3 V0,normalizedV0;
        public float vfMagnitude,v0Magnitude;
        public float k;
        public float mass;
        public float groundY;
        public float bounciness,friction, slidingFriction,ballRadio,g;

        public PathDataDOTS(int index, PathType pathType, float t0,Vector3 pos0, Vector3 Posf, Vector3 v0, float k, float mass, float groundY, float bounciness, float friction, float slidingFriction, float ballRadio, float g)
        {
            this.index = index;
            this.pathType = pathType;
            this.t0 = t0;
            Pos0 = pos0;
            V0 = v0;
            normalizedV0 = V0.normalized;
            vfMagnitude = (g / k);
            v0Magnitude = V0.magnitude;
            this.k = k;
            this.mass = mass;
            this.groundY = groundY;
            this.bounciness = bounciness;
            this.friction = friction;
            this.slidingFriction = slidingFriction;
            this.ballRadio = ballRadio;
            this.g = g;
            this.Posf = Posf;
        }
    }
}

