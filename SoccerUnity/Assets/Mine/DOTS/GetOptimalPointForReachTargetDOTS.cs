using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using static FieldTriangleSpace.FieldOfTrianglesCreator;

namespace DOTS_ChaserDataCalculation
{
    
    public class GetOptimalPointForReachTargetDOTS
    {
        [BurstCompile]
        public static void getOptimalPointForReachTargetWhitAccelerationDriving(in SegmentedPathElement segmentedPathElement, ref PlayerDataComponent defenseDataComponent, ref PlayerDataComponent driverDataComponent, float offsetTime, float scope, float accuracy,PlayerSkills driverSkills, out float result, out Vector3 reachPoint, out int kickCount, out float restTime, PathDataDOTS pathData,float kickDriveTime,Vector3 kickDrivePos1,float totalKickDriveTime,out Vector3 driverLastKickPos, out float driverLastKickTime, out float differenceReach)
        {
            kickCount = 0;
            restTime = 0;
            driverLastKickPos = Vector3.zero;
            driverLastKickTime = 0;
            differenceReach = -1;
            result = Mathf.NegativeInfinity;
            Vector3 chaserPosition = defenseDataComponent.position;
            float chaserSpeed = defenseDataComponent.maxSpeed;
            float time = -1;
            //getOptimalPointForReachTarget(segmentedPathElement, chaserPosition, chaserSpeed, offsetTime, ref time);
            float t = time == -1 ? 0 : time;
            float t0 = kickDriveTime;
            float tf = totalKickDriveTime;

            Vector3 Pos0 = segmentedPathElement.Pos0;
            Vector3 V0 = segmentedPathElement.V0;
            float fullTime = t0 + t;
            //float t4 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref defenseDataComponent, Pos0);
            //Debug.Log("AAA t0=" + t0 + " | tf=" + tf + " | t4=" + t4 + " | t=" + t);
            //if (Mathf.Abs(t0 - t4) < accuracy)
            reachPoint = Vector3.zero;
           
            //Debug.Log(publicPlayerData.name);
            float left = 0, right = tf;
            float testingT = left;
            bool thereIsRight = tf == Mathf.Infinity ? false : true;
            float increase = 0.065f;
            int attempts = 0;
            float endT;
            pathData.Pos0 = kickDrivePos1;
            //while (right - left > 0.01f && attempts < 40)
            float currentResult = Mathf.Infinity;
            while (right - left > 0.01f && attempts < 40)
            {

                Vector3 testingOptimalPoint = GetTimeToReachPointDOTS.accelerationGetPositionDriving(ref driverDataComponent, segmentedPathElement.Posf, testingT, driverSkills, out kickCount, out restTime,out Vector3 kickDrivePosition12,out float kickDriveTime1, out Vector3 startAcPoint, out Vector3 startAcDir, out float startAcVel, out float startAcT);
                Vector3 posBall = Vector3.zero;
                pathData.Pos0 = kickDrivePosition12;
                StraightXZDragAndFrictionPathDOTS2.getPositionAtTime(testingT - kickDriveTime1, ref pathData,ref posBall);


                PerfectGetPositionParms perfectGetPositionParms = new PerfectGetPositionParms();
                perfectGetPositionParms.k = pathData.k;
                perfectGetPositionParms.u = MatchComponents.ballComponents.friction;
                perfectGetPositionParms.g = 9.8f;
                perfectGetPositionParms.v0 = pathData.v0Magnitude;
                perfectGetPositionParms.t = testingT - kickDriveTime1;
                //float d = StraightXZDragAndFrictionPathDOTS2.getPerfectPositionAtTime(perfectGetPositionParms);

                //reachPoint = kickDrivePosition12 + pathData.normalizedV0* d;
                reachPoint = posBall;
                //reachPoint = posBall;
                endT = GetTimeToReachPointDOTS.getTimeToReachPosition(ref defenseDataComponent, reachPoint);
                //Debug.Log("attempt=" + attempts + " | testingT=" + testingT + " | endT=" + endT + " | left=" + left + " | right=" + right + " | t0=" + t0 + " | tf=" + tf);

                float diff = endT - testingT;
                if (diff< accuracy)
                {
                    //Debug.Log("attempt=" + attempts + " | result=" + (endT - t0));
                    //results.Add(endT - t0);
                    result=testingT;
                    driverLastKickPos = kickDrivePosition12;
                    driverLastKickTime = kickDriveTime1;
                    differenceReach = diff;
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
        public static void getOptimalPointForReachTargetWhitAccelerationDriving3(in SegmentedPathElement segmentedPathElement, ref PlayerDataComponent defenseDataComponent, ref PlayerDataComponent driverDataComponent, float offsetTime, float scope, float accuracy, PlayerSkills driverSkills, out float defenseReachTime, out Vector3 reachPoint, out int kickCount, out float restTime, PathDataDOTS pathData, float kickDriveTime, Vector3 kickDrivePos1, float totalKickDriveTime, out Vector3 driverLastKickPos, out float driverLastKickTime, out float differenceReach,out Vector3 endBallReachPoint,out Vector3 endDriverTestingPoint,out float endDefenseTime, out Vector3 startAcPoint, out float startAcVel, out float startAcT,float maxDrivingDistance)
        {
            kickCount = 0;
            startAcVel = 0;
            restTime = 0;
            startAcT= 0;
            differenceReach = -1;
            driverLastKickPos = Vector3.zero;
            driverLastKickTime = 0;
            endBallReachPoint = Vector3.zero;
            endDriverTestingPoint = Vector3.zero;
            startAcPoint = Vector3.zero;
            endDefenseTime = 0;
            defenseReachTime = Mathf.NegativeInfinity;
            Vector3 chaserPosition = defenseDataComponent.position;
            float chaserSpeed = defenseDataComponent.maxSpeed;
            float time = -1;
            //getOptimalPointForReachTarget(segmentedPathElement, chaserPosition, chaserSpeed, offsetTime, ref time);
            float t = time == -1 ? 0 : time;
            float t0 = kickDriveTime;
            float tf = totalKickDriveTime;

            Vector3 Pos0 = segmentedPathElement.Pos0;
            Vector3 V0 = segmentedPathElement.V0;
            float fullTime = t0 + t;
            //float t4 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref defenseDataComponent, Pos0);
            //Debug.Log("AAA t0=" + t0 + " | tf=" + tf + " | t4=" + t4 + " | t=" + t);
            //if (Mathf.Abs(t0 - t4) < accuracy)
            reachPoint = Vector3.zero;

            //Debug.Log(publicPlayerData.name);
            float left = 0, right = tf == Mathf.Infinity ? 4 : tf;
            float testingT = left;
            bool thereIsRight = tf == Mathf.Infinity ? false : true;
            float increase = 0.075f;
            int attempts = 0;
            float endT;
            pathData.Pos0 = kickDrivePos1;
            //while (right - left > 0.01f && attempts < 40)
            float currentResult = Mathf.Infinity;
            while (testingT < right && attempts < 40)
            {

                Vector3 testingOptimalPoint = GetTimeToReachPointDOTS.accelerationGetPositionDriving(ref driverDataComponent, segmentedPathElement.Posf, testingT, driverSkills, out kickCount, out restTime, out Vector3 kickDrivePosition12, out float kickDriveTime1,out startAcPoint, out Vector3 startAcDir, out startAcVel, out startAcT);
                Vector3 posBall = Vector3.zero;
                pathData.Pos0 = kickDrivePosition12;
                //pathData.Posf = kickDrivePosition12+pathData.normalizedV0*maxDrivingDistance;
                StraightXZDragAndFrictionPathDOTS2.getPositionAtTime(Mathf.Abs(testingT - kickDriveTime1), ref pathData, ref posBall);


                PerfectGetPositionParms perfectGetPositionParms = new PerfectGetPositionParms();
                perfectGetPositionParms.k = pathData.k;
                perfectGetPositionParms.u = MatchComponents.ballComponents.friction;
                perfectGetPositionParms.g = 9.8f;
                perfectGetPositionParms.v0 = pathData.v0Magnitude;
                perfectGetPositionParms.t = Mathf.Abs(testingT - kickDriveTime1);
                //float d = StraightXZDragAndFrictionPathDOTS2.getPerfectPositionAtTime(perfectGetPositionParms);

                //reachPoint = kickDrivePosition12 + pathData.normalizedV0* d;
                reachPoint = posBall;
                //reachPoint = posBall;
                 endT = GetTimeToReachPointDOTS.getTimeToReachPosition(ref defenseDataComponent, reachPoint);
                //Debug.Log("attempt=" + attempts + " | testingT=" + testingT + " | endT=" + endT + " | left=" + left + " | right=" + right + " | t0=" + t0 + " | tf=" + tf);

                float diff = endT - testingT;
                if ( diff < accuracy)
                //if (attempts == 15)
                {
                    //Debug.Log("attempt=" + attempts + " | result=" + (endT - t0));
                    //results.Add(endT - t0);
                    
                    defenseReachTime = testingT;
                    driverLastKickPos = kickDrivePosition12;
                    driverLastKickTime = kickDriveTime1;
                    differenceReach = diff;
                    endBallReachPoint = reachPoint;
                    endDriverTestingPoint = testingOptimalPoint;
                    endDefenseTime = endT;
                    return;
                }
                else if (false && diff < currentResult)
                {
                    defenseReachTime = testingT;
                    driverLastKickPos = kickDrivePosition12;
                    driverLastKickTime = kickDriveTime1;
                    currentResult = diff;
                    differenceReach = diff;
                }
                testingT += increase;
                /*
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
                }*/
                attempts++;
            }
            //Debug.Log("attempt=" + attempts + " | result=" + (endT - t0));
            //results.Add(endT - t0);
        }


        [BurstCompile]
        public static void getOptimalPointForReachTargetWhitAccelerationDriving2(in SegmentedPathElement segmentedPathElement, ref PlayerDataComponent defenseDataComponent, ref PlayerDataComponent driverDataComponent, float offsetTime, float scope, float accuracy, PlayerSkills driverSkills, out float result, out Vector3 reachPoint, out int kickCount, out float restTime, PathDataDOTS pathData, float kickDriveTime, Vector3 kickDrivePos1, float totalKickDriveTime, out Vector3 driverLastKickPos, out float driverLastKickTime,out float differenceReach)
        {
            kickCount = 0;
            restTime = 0;
            differenceReach = -1;
            driverLastKickPos = Vector3.zero;
            driverLastKickTime = 0;
            result = Mathf.NegativeInfinity;
            Vector3 chaserPosition = defenseDataComponent.position;
            float chaserSpeed = defenseDataComponent.maxSpeed;
            float time = -1;
            //getOptimalPointForReachTarget(segmentedPathElement, chaserPosition, chaserSpeed, offsetTime, ref time);
            float t = time == -1 ? 0 : time;
            float t0 = kickDriveTime;
            float tf = totalKickDriveTime;

            Vector3 Pos0 = segmentedPathElement.Pos0;
            Vector3 V0 = segmentedPathElement.V0;
            float fullTime = t0 + t;
            //float t4 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref defenseDataComponent, Pos0);
            //Debug.Log("AAA t0=" + t0 + " | tf=" + tf + " | t4=" + t4 + " | t=" + t);
            //if (Mathf.Abs(t0 - t4) < accuracy)
            reachPoint = Vector3.zero;

            //Debug.Log(publicPlayerData.name);
            float left = 0, right = tf == Mathf.Infinity ? 4:tf;
            float testingT = left;
            bool thereIsRight = tf == Mathf.Infinity ? false : true;
            float increase = 0.075f;
            int attempts = 0;
            float endT;
            pathData.Pos0 = kickDrivePos1;
            //while (right - left > 0.01f && attempts < 40)
            float currentResult = Mathf.Infinity;
            while (testingT<right && attempts < 40)
            {

                Vector3 testingOptimalPoint = GetTimeToReachPointDOTS.accelerationGetPositionDriving(ref driverDataComponent, segmentedPathElement.Posf, testingT, driverSkills, out kickCount, out restTime, out Vector3 kickDrivePosition12, out float kickDriveTime1, out Vector3 startAcPoint, out Vector3 startAcDir, out float startAcVel, out float startAcT);
                Vector3 posBall = Vector3.zero;
                pathData.Pos0 = kickDrivePosition12;
                StraightXZDragAndFrictionPathDOTS2.getPositionAtTime(testingT - kickDriveTime1, ref pathData, ref posBall);


                PerfectGetPositionParms perfectGetPositionParms = new PerfectGetPositionParms();
                perfectGetPositionParms.k = pathData.k;
                perfectGetPositionParms.u = MatchComponents.ballComponents.friction;
                perfectGetPositionParms.g = 9.8f;
                perfectGetPositionParms.v0 = pathData.v0Magnitude;
                perfectGetPositionParms.t = testingT - kickDriveTime1;
                //float d = StraightXZDragAndFrictionPathDOTS2.getPerfectPositionAtTime(perfectGetPositionParms);

                //reachPoint = kickDrivePosition12 + pathData.normalizedV0* d;
                reachPoint = posBall + pathData.normalizedV0 * 0.25f;
                //reachPoint = posBall;
                endT = GetTimeToReachPointDOTS.getTimeToReachPosition(ref defenseDataComponent, reachPoint);
                //Debug.Log("attempt=" + attempts + " | testingT=" + testingT + " | endT=" + endT + " | left=" + left + " | right=" + right + " | t0=" + t0 + " | tf=" + tf);

                float diff = endT - testingT;
                if (diff< accuracy)
                {
                    //Debug.Log("attempt=" + attempts + " | result=" + (endT - t0));
                    //results.Add(endT - t0);
                    result = testingT;
                    driverLastKickPos = kickDrivePosition12;
                    driverLastKickTime = kickDriveTime1;
                    differenceReach = diff;
                    return;
                }
                else if (false && diff < currentResult)
                {
                    result = testingT;
                    driverLastKickPos = kickDrivePosition12;
                    driverLastKickTime = kickDriveTime1;
                    currentResult = diff;
                    differenceReach = diff;
                }
                testingT += increase;
                /*
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
                }*/
                attempts++;
            }
            //Debug.Log("attempt=" + attempts + " | result=" + (endT - t0));
            //results.Add(endT - t0);
        }

        [BurstCompile]
        public static void getOptimalPointForReachTargetWhitAcceleration(in SegmentedPathElement segmentedPathElement,ref PlayerDataComponent defenseDataComponent, float offsetTime, float scope, float accuracy, ref MyFloatArray results, out Vector3 reachPoint)
        {
            Vector3 chaserPosition = defenseDataComponent.position;
            float chaserSpeed = defenseDataComponent.maxSpeed;
            float time=-1;
            getOptimalPointForReachTarget(segmentedPathElement,chaserPosition, chaserSpeed, offsetTime,ref time);
            float t = time == -1 ? 0 : time;
            float t0 = segmentedPathElement.t0;
            float tf = segmentedPathElement.tf;
            results.Clear();
            
            Vector3 Pos0 = segmentedPathElement.Pos0;
            Vector3 V0 = segmentedPathElement.V0;
            float fullTime = t0 + t;
            float t4 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref defenseDataComponent,Pos0);
            //Debug.Log("AAA t0=" + t0 + " | tf=" + tf + " | t4=" + t4 + " | t=" + t);
            //if (Mathf.Abs(t0 - t4) < accuracy)
            reachPoint = Vector3.zero;
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
                Vector3 position = Pos0 + V0 * (testingT - t0);
                Vector3 testingOptimalPoint = position;
                
                //Vector3 position = Pos0 + V0 * (testingT - t0);
                endT = GetTimeToReachPointDOTS.getTimeToReachPosition(ref defenseDataComponent,testingOptimalPoint);
                reachPoint = testingOptimalPoint;
                //Debug.Log("attempt=" + attempts + " | testingT=" + testingT + " | endT=" + endT + " | left=" + left + " | right=" + right + " | t0=" + t0 + " | tf=" + tf);


                if (Mathf.Abs(endT-testingT)< accuracy)
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
            result = Mathf.Infinity;
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
