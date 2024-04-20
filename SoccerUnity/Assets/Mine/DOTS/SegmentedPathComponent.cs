using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;

namespace DOTS_ChaserDataCalculation
{
    [BurstCompile]
    public struct SegmentedPathElement : IBufferElementData
    {
        public int index;
        public Vector3 Pos0, Posf;
        public Vector3 V0, Vf;
        public float t0, tf;
        public float V0Magnitude;
    }
}
