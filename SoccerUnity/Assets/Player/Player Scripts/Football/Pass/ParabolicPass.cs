using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParabolicPass : Pass
{
    public override PassResult getPassResult(PassParameters passParameters)
    {
        PassResult passResult;
        Vector3 firstPoint;
        float firstTime;
        ParabolicPassParameters parabolicPassParameters = (ParabolicPassParameters)passParameters;
        bool somePartnerIsAhead;
        if (!getFirstPoint(parabolicPassParameters, out firstPoint,out firstTime,out somePartnerIsAhead))
        {
            passResult = new PassResult(passParameters.initV0, true, true, somePartnerIsAhead);
            return passResult;
        }
        Vector3 v0 = getFirstMinVy(firstPoint, firstTime, parabolicPassParameters);
        float v0Magnitude = Mathf.Clamp(v0.magnitude, 0, passParameters.maxKickForce);
        passResult = searchMinVy(parabolicPassParameters, v0.normalized*v0Magnitude);
        passResult.somePartnerIsAhead = somePartnerIsAhead;
        return passResult;
    }
    PassResult searchMinVy(ParabolicPassParameters passParameters, Vector3 firstVy)
    {
        float left, right;
        left = firstVy.y;

        float drag = MatchComponents.ballComponents.drag;
        StraightXZDragPath straightXZDragPath = new StraightXZDragPath(passParameters.Pos0, passParameters.Posf, Vector3.zero, Vector3.zero, 0, 0, drag);
        GetV0Result getV0Result = straightXZDragPath.getXZV0(passParameters.receiverReachPointTime, passParameters.maxControlSpeed, passParameters.maxControlSpeedLerpDistance, passParameters.maxKickForce);

        ParabolaWithDrag parabolaWithDrag = new ParabolaWithDrag(passParameters.Pos0, Vector3.zero, 0, 9.81f, drag);
        Vector3 heightPoint = MyFunctions.setY0ToVector3(passParameters.Posf) + Vector3.up*passParameters.maxReceiverHeight;
        float t = StraightXZDragPath.getT(passParameters.Pos0, passParameters.Posf, getV0Result.v0.magnitude, drag);
        float maxVy = parabolaWithDrag.getV0(t, passParameters.Pos0, heightPoint).y;

        right = maxVy;

        int attempts = 0;
        float testVy = left;
        float increment = passParameters.searchVyIncrement;
        Vector3 testV0;
        while (left <= right && attempts <= passParameters.maxAttempts)
        {
            testV0 = MyFunctions.setY0ToVector3(getV0Result.v0) + Vector3.up * testVy;
            calculateChaserDatas(passParameters, testV0);
            bool noRivalReachTheTargetBeforeMe = checkIfNoRivalReachTheTargetBeforeMe(passParameters, passParameters.receiverReachPointTime);
            if (noRivalReachTheTargetBeforeMe)
            {
                PassResult passResult = new PassResult(testV0, noRivalReachTheTargetBeforeMe, true);
                return passResult;
            }
            left = testVy;
            testVy += increment;
            if (testVy > right)
            {
                break;
            }
            //Debug.Log("centralV0=" + centralV0 + "  | distance=" + distance + " |target=" + target + " | left ="+left + " |right="+ right);
            attempts++;
        }
        return new PassResult(MyFunctions.setYToVector3(getV0Result.v0, maxVy), false, false);
    }
    void calculateChaserDatas(ParabolicPassParameters passParameters, Vector3 v0)
    {
        BallComponents ballComponents = MatchComponents.ballComponents;
        SegmentedPath segmentedPath = GetSegmentedPath(passParameters,v0);
        CalculateChaserDatas calculateChaserDatas = new CalculateChaserDatas(chaserDataCalculationParameters.timeRange, chaserDataCalculationParameters.timeIncrement, chaserDataCalculationParameters.minAngle, chaserDataCalculationParameters.minVelocity, chaserDataCalculationParameters.maxAngle, chaserDataCalculationParameters.maxVelocity);
        StartCoroutine(calculateChaserDatas.getChaserDatas(passParameters.rivalsChaserDatas, segmentedPath, true, 0, null));
    }
    SegmentedPath GetSegmentedPath(PassParameters passParameters, Vector3 v0)
    {
        BallComponents ballComponents = MatchComponents.ballComponents;
        ParabolaWithDrag trajectory = new ParabolaWithDrag(passParameters.Pos0, v0, 0, 9.81f, ballComponents.drag);
        StraightXZDragAndFrictionPath straightXZDragAndFrictionPath = new StraightXZDragAndFrictionPath(ballComponents.drag, ballComponents.radio, ballComponents.friction, ballComponents.mass);
        BouncyPath bouncyPath = new BouncyPath(trajectory, straightXZDragAndFrictionPath, 0, 0.1f, ballComponents.bounciness, ballComponents.friction);
        bouncyPath.info = "perfectPass";
        SegmentedPath segmentedPath = new SegmentedPath(bouncyPath);
        return segmentedPath;
    }
    Vector3 getFirstMinVy(Vector3 firstPoint,float time,ParabolicPassParameters passParameters)
    {
        float drag = MatchComponents.ballComponents.drag;
        ParabolaWithDrag parabolaWithDrag = new ParabolaWithDrag(passParameters.Pos0, Vector3.zero, 0, 9.81f, drag);
        Vector3 v0 = parabolaWithDrag.getV0(time, passParameters.Pos0, firstPoint);
        return v0;
    }
    Vector3 getJumpHeightPoint(ChaserData chaserData, ParabolicPassParameters passParameters)
    {
        float chaserHeight = chaserData.publicPlayerData.getMaximumJumpHeightOfPoint(chaserData.OptimalPoint);
        float d = chaserHeight + passParameters.rivalsHeightOffset - chaserData.OptimalPoint.y;
        return chaserData.OptimalPoint + Vector3.up * d;
    }
    bool getFirstPoint(ParabolicPassParameters passParameters,out Vector3 point,out float time,out bool somePartnerIsAhead)
    {
        Vector3 pos0 = passParameters.Pos0;
        Vector3 posf = passParameters.Posf;
        float lastDistance = Mathf.Infinity;
        bool thereIsResult = false;
        float drag = MatchComponents.ballComponents.drag;
        point = Vector3.zero;
        time = 0;
        somePartnerIsAhead = false;
        foreach (var partnerChaserData in passParameters.partnerChaserDatas)
        {
            float d = MyFunctions.DistancePointAndFiniteLine(MyFunctions.setY0ToVector3(partnerChaserData.position), MyFunctions.setY0ToVector3(pos0), MyFunctions.setY0ToVector3(posf));
            if (d < partnerChaserData.bodyBallRadio+0.05f)
            {
                StraightXZDragPath straightXZDragPath = new StraightXZDragPath(passParameters.Pos0, passParameters.Posf, Vector3.zero, Vector3.zero, 0, 0, drag);
                GetV0Result getV0Result = straightXZDragPath.getXZV0(passParameters.receiverReachPointTime, passParameters.maxControlSpeed, passParameters.maxControlSpeedLerpDistance, passParameters.maxKickForce);
                float t = StraightXZDragPath.getT(passParameters.Pos0, partnerChaserData.position, getV0Result.v0.magnitude, drag);
                if (t < passParameters.partnerIsAheadMinTime)
                {
                    float height = partnerChaserData.height + MatchComponents.ballRadio;
                    
                    time = t;
                    Vector3 p = MyFunctions.GetClosestPointOnFiniteLine(MyFunctions.setY0ToVector3(partnerChaserData.position), MyFunctions.setY0ToVector3(pos0), MyFunctions.setY0ToVector3(posf));
                    point = p + Vector3.up * height;
                    lastDistance = Vector3.Distance(MyFunctions.setY0ToVector3(pos0), MyFunctions.setY0ToVector3(p));
                    thereIsResult = true;
                    somePartnerIsAhead = true;
                }
            }
        }
        foreach (var rivalChaserData in passParameters.rivalsChaserDatas)
        {
            float d = Vector3.Distance(MyFunctions.setY0ToVector3(pos0), MyFunctions.setY0ToVector3(rivalChaserData.OptimalPoint));
            if(rivalChaserData.ReachTheTarget && d < lastDistance)
            {
                point = getJumpHeightPoint(rivalChaserData, passParameters);
                time = rivalChaserData.OptimalTime;
                lastDistance = d;
                thereIsResult = true;
            }
        }
        return thereIsResult;
    }
}
