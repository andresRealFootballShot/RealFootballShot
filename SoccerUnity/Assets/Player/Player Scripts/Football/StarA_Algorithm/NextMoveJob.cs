using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
namespace NextMove_Algorithm
{
    public class NextMoveJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<float> a;
        public void Execute(int index)
        {
            throw new System.NotImplementedException();
        }
    }
}
