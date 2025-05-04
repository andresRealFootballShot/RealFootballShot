using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
namespace DOTS_ChaserDataCalculation
{
    public struct PerfectGetPositionParms
    {
        public float v0;
        public float t;
        public float u;
        public float k;
        public float g;
    }
    [BurstCompile]
    public class StraightXZDragAndFrictionPathDOTS2
    {
        public static bool getT(ref PathDataDOTS pathData,Vector3 Posf, float distanceAccuracy, int maxAttempts, out float result)
        {
            float left, right;
            left = StraightXZDragPath.getT(pathData.Pos0, Posf, pathData.v0Magnitude, pathData.k);
            right = Mathf.Infinity;

            float target = Vector3.Distance(pathData.Pos0, Posf);

            int attempts = 0;
            float proofT = left;
            float testIncrement = 0.1f;
            bool thereIsRight = false;
            Vector3 pos = Vector3.zero;
            while (left <= right && attempts <= maxAttempts)
            {
                getPositionAtTime(proofT,ref pathData,ref pos);
                float distance = Vector3.Distance(pos, pathData.Pos0);
                if (Mathf.Abs(distance - target) <= distanceAccuracy)
                {
                    result = proofT;
                    return true;
                }
                if (target < distance)
                {
                    if (thereIsRight)
                    {

                        right = proofT;
                        proofT -= (right - left) / 2;
                    }
                    else
                    {
                        right = proofT;
                        proofT -= (right - left) / 2;
                        thereIsRight = true;
                    }
                }
                else
                {
                    if (thereIsRight)
                    {
                        left = proofT;
                        proofT += (right - left) / 2;
                    }
                    else
                    {

                        left = proofT;
                        proofT += testIncrement;
                    }
                }
                //Debug.Log("centralV0=" + centralV0 + "  | distance=" + distance + " |target=" + target + " | left ="+left + " |right="+ right);
                attempts++;
            }
            result = 0;
            return false;
        }
        [BurstCompile]
        public static void getVelocityAtTime(float t,ref PathDataDOTS pathData,ref Vector3 velocity)
        {
            if (pathData.v0Magnitude == 0)
            {
                velocity = Vector3.zero;
                return;
            }
            float maxWt = getTofWMax_WithRollDrag(ref pathData);
            if (pathData.k == 0)
            {
                velocity = pathData.normalizedV0* (pathData.v0Magnitude - pathData.friction * 9.81f * maxWt);
            }
            else
            {
                float vf = (pathData.friction * pathData.g) / pathData.k;
                float rollSlipV = -vf + (pathData.v0Magnitude + vf) * Mathf.Exp(-pathData.k * Mathf.Clamp(maxWt, 0, t));
                if (t <= maxWt)
                {
                    velocity = pathData.normalizedV0 * rollSlipV;
                }
                else
                {
                    float pureRollV = rollSlipV * Mathf.Exp(-pathData.k * (t - maxWt));
                    velocity = pathData.normalizedV0 * pureRollV;
                }
            }
        }
        [BurstCompile]
        public static void getPositionAtTime(float t, ref PathDataDOTS pathData,ref Vector3 positionResult)
        {

            if (pathData.v0Magnitude == 0)
            {
                positionResult = pathData.Pos0;
                return;
            }
            float t1 = getTofWMax_WithRollDrag(ref pathData);
            if (t < t1)
            {
                t1 = t;
            }
            Vector3 velocity = Vector3.zero;
            getVelocityAtTime(t1,ref pathData,ref velocity);
            float v2 = velocity.magnitude;
            if (pathData.k == 0)
            {
                float roll = pathData.friction * 9.81f;
                float t2 = (t - t1);
                float a = -0.5f * roll * t1 * t1;
                float d = pathData.v0Magnitude * t1 + a + v2 * t2;
                positionResult = pathData.normalizedV0 * d + pathData.Pos0;
            }
            else
            {
                float e = (1 - Mathf.Exp(-pathData.k * t1));
                float e2 = (1 - Mathf.Exp(-pathData.k * Mathf.Clamp((t - t1), 0, Mathf.Infinity)));
                float vf = (pathData.friction * pathData.g) / pathData.k;
                float b = (pathData.v0Magnitude + vf) / pathData.k;
                float roll = -vf * t1 + b * e;
                float drag = (v2 / pathData.k) * e2;
                //Debug.Log("getPositionAtTime " + roll+ " "+drag + " | v2=" +v2 + " |t1=" +t1+" | vf="+vf);
                positionResult = pathData.normalizedV0 * roll + pathData.normalizedV0 * drag + pathData.Pos0;
            }
        }
        [BurstCompile]
        static float getTofWMax_WithRollDrag(ref PathDataDOTS pathData)
        {
            float r = pathData.ballRadio;
            float w = Mathf.Clamp(pathData.v0Magnitude * 5.86923f, 0, 50);
            if (pathData.k == 0)
            {
                return (pathData.v0Magnitude - r * w) / (pathData.friction * pathData.g);
            }
            else
            {
                float vf = (pathData.friction * pathData.g) / pathData.k;
                float ln = Mathf.Log((r * w + vf) / (pathData.v0Magnitude+ vf));
                return ln / -pathData.k;
            }
        }

        public static void getV0(ref PathDataDOTS currentPath,ref GetV0DOTSResult result,float maxKickForce,ref GetStraightV0Params getV0Params,float t)
        {
            float d = Vector3.Distance(currentPath.Pos0, MyFunctions.setYToVector3(currentPath.Posf, currentPath.Pos0.y));
            float maxControlSpeed = Mathf.Lerp(2, getV0Params.maxControlSpeed, d / getV0Params.maxControlSpeedLerpDistance);
            getV02(ref result,ref currentPath, maxKickForce,ref getV0Params, t);
            
            if (result.vt >= maxControlSpeed || t == 0)
            {
                Vector3 v0;
                
                if (getV0WithVt(ref currentPath,ref getV0Params, maxControlSpeed, out v0))
                {
                    result.v0 = v0;
                    result.v0Magnitude = v0.magnitude;
                    result.maximumControlSpeedReached = true;
                    result.vt = maxControlSpeed;
                    result.foundedResult = true;
                }
                else
                {
                    result.foundedResult = false;
                }
            }
            if (result.v0Magnitude > maxKickForce)
            {
                result.v0 = result.v0.normalized* maxKickForce;
                result.v0Magnitude = maxKickForce;
                result.maxKickForceReached = true;
            }
            result.maximumControlSpeedReached = result.vt >= maxControlSpeed;
            if (result.maximumControlSpeedReached || result.maxKickForceReached)
            {
                float t2;
                currentPath.V0 = result.v0;
                currentPath.v0Magnitude = result.v0.magnitude;
                currentPath.normalizedV0 = result.v0.normalized;
                StraightXZDragAndFrictionPathDOTS.getT(in currentPath, currentPath.Posf, 0.1f, 100, out t2);
                result.ballReachTargetPositionTime = t2;
            }
        }
        public static void getV02(ref GetV0DOTSResult result, ref PathDataDOTS currentPath, float maxKickForce,ref GetStraightV0Params getV0Params,float t)
        {
            Vector3 dir = (currentPath.Posf - currentPath.Pos0).normalized;
            Vector3 straightXZDragPathV0 = StraightXZDragPath.getV0(t, currentPath.Pos0, currentPath.Posf, currentPath.k);
            //float v0magnitude = Mathf.Clamp(straightXZDragPathV0.magnitude, 0, maxKickForce);
            float v0magnitude = straightXZDragPathV0.magnitude;
            
            float left, right;
            left = v0magnitude;
            right = maxKickForce;
            float target = Vector3.Distance(currentPath.Posf, currentPath.Pos0);

            int attempts = 0;
            float centralV0 =v0magnitude;
            float x = 3f;
            bool thereIsRight = true;

            while (left <= right && attempts <= getV0Params.maxAttempts)
            {
                Vector3 v0 = dir * centralV0;
                
                Vector3 pos = getPositionAtTime(ref currentPath, v0,t);
                //float distance = Vector3.Distance(pos, data.Pos0);
                float distance = Vector3.Distance(pos, currentPath.Posf);
                float distance2 = Vector3.Distance(pos, currentPath.Pos0);
                //if (Mathf.Abs(distance - target) <= data.accuracy)
                if (distance<= getV0Params.accuracy)
                {
                    Vector3 vt = getVelocityAtTime(ref currentPath, t, v0);
                    result = new GetV0DOTSResult(true, vt.magnitude, dir * centralV0,false,false,false,false,t,t);
                    return;
                }
                if (target < distance2)
                {
                    if (thereIsRight)
                    {

                        right = centralV0;
                        centralV0 -= (right - left) / 2;
                    }
                    else
                    {
                        right = centralV0;
                        centralV0 -= (right - left) / 2;
                        thereIsRight = true;
                    }
                }
                else
                {
                    if (thereIsRight)
                    {
                        left = centralV0;
                        centralV0 += (right - left) / 2;
                    }
                    else
                    {

                        left = centralV0;
                        centralV0 += x;
                    }
                }
                //Debug.Log("centralV0=" + centralV0 + "  | distance=" + distance + " |target=" + target + " | left ="+left + " |right="+ right);
                attempts++;
            }
            result = new GetV0DOTSResult();
            result.v0 = dir * v0magnitude;
            result.v0Magnitude = v0magnitude;
            result.vt = Mathf.Infinity;
        }
        public static Vector3 getVelocityAtTime(ref PathDataDOTS currentPath, float t, Vector3 V0)
        {
            float v0Magnitude = V0.magnitude;
            float maxWt = getTofWMax_WithRollDrag(ref currentPath, v0Magnitude);
            if (currentPath.k == 0)
            {
                return V0.normalized * (V0.magnitude - currentPath.friction * 9.81f * maxWt);
            }
            else
            {
                float vf = (currentPath.friction * currentPath.g) / currentPath.k;
                float rollSlipV = -vf + (v0Magnitude + vf) * Mathf.Exp(-currentPath.k * Mathf.Clamp(maxWt, 0, t));
                if (t <= maxWt)
                {
                    return V0.normalized * rollSlipV;
                }
                else
                {
                    float pureRollV = rollSlipV * Mathf.Exp(-currentPath.k * (t - maxWt));
                    return V0.normalized * pureRollV;
                }
            }
        }
        public static Vector3 getPositionAtTime(ref PathDataDOTS currentPath, Vector3 V0,float t)
        {
            float v0Magnitude = V0.magnitude;
            Vector3 normalizedV0 = V0.normalized;
            if (v0Magnitude == 0)
            {
                return currentPath.Pos0;
            }
            float t1 = getTofWMax_WithRollDrag(ref currentPath, v0Magnitude);
            if (t < t1)
            {
                t1 = t;
            }
            float v2 = getVelocityAtTime(ref currentPath, t1, V0).magnitude;
            if (currentPath.k == 0)
            {
                float roll = currentPath.friction * 9.81f;
                float t2 = (t - t1);
                float a = -0.5f * roll * t1 * t1;
                float d = v0Magnitude * t1 + a + v2 * t2;
                return normalizedV0 * d + currentPath.Pos0;
            }
            else
            {
                float e = (1 - Mathf.Exp(-currentPath.k * t1));
                float e2 = (1 - Mathf.Exp(-currentPath.k * Mathf.Clamp((t - t1), 0, Mathf.Infinity)));
                float vf = (currentPath.friction * currentPath.g) / currentPath.k;
                float b = (v0Magnitude + vf) / currentPath.k;
                float roll = -vf * t1 + b * e;
                float drag = (v2 / currentPath.k) * e2;
                //Debug.Log("getPositionAtTime " + roll+ " "+drag + " | v2=" +v2 + " |t1=" +t1+" | vf="+vf);
                return normalizedV0 * roll + normalizedV0 * drag + currentPath.Pos0;
            }
        }
        public static bool getV0WithVt(ref PathDataDOTS currentPath, ref GetStraightV0Params getV0Params, float Vt, out Vector3 result)
        {
            float left, right;
            left = StraightXZDragPath.getV0ByVt(Vt, currentPath.Pos0, currentPath.Posf, currentPath.k).magnitude;
            right = Mathf.Infinity;
            float targetDistance = Vector3.Distance(currentPath.Posf, currentPath.Pos0);

            Vector3 dir = (currentPath.Posf - currentPath.Pos0).normalized;
            int attempts = 0;
            float centralV0 = left;
            float x = 3;
            bool thereIsRight = false;
            //Debug.Log("max0=" + maxV0 + " | Vt=" + Vt);
            while (left <= right && attempts <= getV0Params.maxAttempts)
            {
                Vector3 pos = getPositionAtVt(ref currentPath, Vt, dir * centralV0);
                float distance = Vector3.Distance(pos, currentPath.Pos0);
                //Debug.Log((distance - targetDistance));

                //Debug.Log("centralV0=" + centralV0 + "  | distance=" + distance + " |target=" + targetDistance + " | left =" + left + " |right=" + right);
                if (Mathf.Abs(distance - targetDistance) <= getV0Params.accuracy)
                {
                    //Debug.Log("result was founded, attempts=" + attempts);
                    result = dir * centralV0;
                    return true;
                }
                if (targetDistance < distance)
                {
                    if (thereIsRight)
                    {

                        right = centralV0;
                        centralV0 -= (right - left) / 2;
                    }
                    else
                    {
                        right = centralV0;
                        centralV0 -= (right - left) / 2;
                        thereIsRight = true;
                    }
                }
                else
                {
                    if (thereIsRight)
                    {

                        left = centralV0;
                        centralV0 += (right - left) / 2;

                    }
                    else
                    {

                        left = centralV0;

                        centralV0 += x;
                    }
                }
                attempts++;
            }
            //Debug.Log("attempts=" + attempts);
            result = dir * centralV0;
            return false;
        }
        public static Vector3 getPositionAtVt(ref PathDataDOTS currentPath, float vt,Vector3 v0)
        {

            float v0Magnitude = v0.magnitude;
            if (v0Magnitude == 0)
            {
                return currentPath.Pos0;
            }
            float maxWt = getTofWMax_WithRollDrag(ref currentPath, v0Magnitude);
            float vf = (currentPath.friction * currentPath.g) / currentPath.k;
            float v0M_vf = v0Magnitude + vf;
            float v022 = -vf + v0M_vf * Mathf.Exp(-currentPath.k * maxWt);
            float v02;
            if (vt > v022)
            {
                v02 = vt;
            }
            else
            {
                v02 = v022;
            }
            //Debug.Log("v02=" + v02 + " | vt=" + vt);
            float e11 = (v02 + vf) / v0M_vf;
            float v2 = (-vf + v0M_vf * e11) / currentPath.k;
            float e1 = (1 - e11);
            float e2 = (1 - vt / v02);
            float t1 = Mathf.Log((v02 + vf) / v0M_vf) / -currentPath.k;
            float b = v0M_vf / currentPath.k;
            float roll = -vf * t1 + b * e1;
            float drag = v2 * e2;
            //Debug.Log("getPositionAtTime " + roll+ " "+drag + " | v2=" +v2 + " |t1=" +t1+" | vf="+vf);
            Vector3 normalizedV0 = v0.normalized;
            
            if (vt > v022)
            {
                return normalizedV0 * roll + currentPath.Pos0;
            }
            else
            {
                return normalizedV0 * roll + normalizedV0 * drag + currentPath.Pos0;
            }
        }
        static float getTofWMax_WithRollDrag(ref PathDataDOTS currentPath, float v0Magnitude)
        {
            float r = currentPath.ballRadio;
            float w = Mathf.Clamp(v0Magnitude * 5.86923f, 0, 50);
            
            if (currentPath.k == 0)
            {
                return (v0Magnitude - r * w) / (currentPath.friction * currentPath.g);
            }
            else
            {
                float vf = (currentPath.friction * currentPath.g) / currentPath.k;
                float ln = Mathf.Log((r * w + vf) / (v0Magnitude + vf));
                return ln / -currentPath.k;
            }
        }






       
        [BurstCompile]
        public static float getPerfectPositionAtTime(PerfectGetPositionParms parms)
        {
            float v0 = parms.v0;
            float t = parms.t;
            float u=parms.u;
            float k=parms.k;
            float g = parms.g;
            float x = (v0/(k*g))*(1-Mathf.Exp(-k*g*t));
            return x;
        }


    }


}
