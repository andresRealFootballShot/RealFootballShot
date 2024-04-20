using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;

namespace DOTS_ChaserDataCalculation
{
    
    public class GetOptimalPointForReachTargetDOTS
    {
        [BurstCompile]
        public static void getOptimalPointForReachTargetWhitAcceleration(in SegmentedPathElement segmentedPathElement,ref PlayerDataComponent playerDataComponent, float offsetTime, float scope, float accuracy, ref MyFloatArray results)
        {
            Vector3 chaserPosition = playerDataComponent.position;
            float chaserSpeed = playerDataComponent.maxSpeed;
            float time=-1;
            getOptimalPointForReachTarget(segmentedPathElement,chaserPosition, chaserSpeed, offsetTime,ref time);
            float t = time == -1 ? 0 : time;
            float t0 = segmentedPathElement.t0;
            float tf = segmentedPathElement.tf;
            results.Clear();
            
            Vector3 Pos0 = segmentedPathElement.Pos0;
            Vector3 V0 = segmentedPathElement.V0;
            float fullTime = t0 + t;
            float t4 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent,Pos0);
            //Debug.Log("AAA t0=" + t0 + " | tf=" + tf + " | t4=" + t4 + " | t=" + t);
            //if (Mathf.Abs(t0 - t4) < accuracy)
            if (t4 - t0 < accuracy)
            {
                results.Add(t0);
                //Debug.Log("End BBB" + t0 + " | tf=" + tf + " | t4=" + t4);
                return;
            }
            //Debug.Log(publicPlayerData.name);
            float left = fullTime, right = tf;
            float testingT = fullTime;
            bool thereIsRight = tf == Mathf.Infinity ? false : true;
            float increase = 0.05f;
            int attempts = 0;
            float endT;

            while (right - left > 0.01f && attempts < 40)
            {

                //Vector3 testingOptimalPoint = Pos0 + V0 * (testingT - t0);
                Vector3 position = Pos0 + V0 * (testingT - t0);
                Vector3 testingOptimalPoint = position;
                endT = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent,testingOptimalPoint);
                //Debug.Log("attempt=" + attempts + " | testingT=" + testingT + " | endT=" + endT + " | left=" + left + " | right=" + right + " | t0=" + t0 + " | tf=" + tf);


                if (endT-testingT< accuracy)
                {
                    //Debug.Log("attempt=" + attempts + " | result=" + (endT - t0));
                    //results.Add(endT - t0);
                    results.Add(testingT);
                    return;
                }
                if (endT > testingT)
                {
                    left = testingT;
                    if (thereIsRight)
                    {
                        testingT += (right - left) / 2;
                    }
                    else
                    {
                        testingT += increase;
                    }
                }
                else
                {
                    if (thereIsRight)
                    {
                        thereIsRight = true;
                        right = testingT;
                        testingT -= (right - left) / 2;
                    }
                    else
                    {
                        right = testingT;
                        testingT -= increase;
                        thereIsRight = true;
                    }
                }
                attempts++;
            }
            //Debug.Log("attempt=" + attempts + " | result=" + (endT - t0));
            //results.Add(endT - t0);
        }
        [BurstCompile]
        public static void getOptimalPointForReachTarget(in SegmentedPathElement segmentedPathElement,Vector3 chaserPosition, float chaserSpeed, float offsetTime, ref float result)
        {
            Vector3 targetPosition = segmentedPathElement.Pos0;
            Vector3 targetVelocity = segmentedPathElement.V0;
            Vector3 vectorFromRunner = chaserPosition - targetPosition;
            float distanceToRunner = vectorFromRunner.magnitude;
            float pow2ChaserSpeed = chaserSpeed * chaserSpeed;
            float a = pow2ChaserSpeed - (targetVelocity.magnitude * targetVelocity.magnitude);
            //float b = 2 * Vector3.Dot(vectorFromRunner, targetVelocity);
            //float c = -distanceToRunner * distanceToRunner;
            float b = (2 * offsetTime * chaserSpeed * chaserSpeed) + (2 * Vector3.Dot(vectorFromRunner, targetVelocity));
            float c = (-distanceToRunner * distanceToRunner) + (offsetTime * offsetTime * pow2ChaserSpeed);
            float time1, time2;
            //results = new List<float>();
            float maxDistance = Vector3.Distance(segmentedPathElement.Pos0, segmentedPathElement.Posf);
            if (MyFunctions.SolveQuadratic(a, b, c, out time1, out time2))
            {
                if (time1 != Mathf.Infinity && !float.IsNaN(time1) && time1 >= 0)
                {
                    Vector3 optimalPoint = targetPosition + targetVelocity * time1;
                    float distance = Vector3.Distance(segmentedPathElement.Pos0, optimalPoint);
                    if (distance <= maxDistance)
                    {
                        result = time1+segmentedPathElement.t0;
                    }
                }
                if (time2 != Mathf.Infinity && !float.IsNaN(time2) && time2 >= 0)
                {
                    float distance = Vector3.Distance(segmentedPathElement.Pos0, targetPosition + targetVelocity * time2);
                    if (distance <= maxDistance)
                    {
                        float t =time2 + segmentedPathElement.t0;
                        if (t < result)
                        {
                            result = t;
                        }
                    }
                }
            }

        }
    }
}
