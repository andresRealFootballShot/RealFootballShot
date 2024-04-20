using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightPass : Pass
{
    SegmentedPath segmentedPath;
    public override PassResult getPassResult(PassParameters passParameters)
    {
        PassResult passResult;
        BallComponents ballComponents = MatchComponents.ballComponents;
        
       
        passParameters.receiverReachPointTime = passParameters.ReceiverChaserData.getTimeToReachPoint(passParameters.Posf, passParameters.ReceiverChaserData.scope);
        StraightXZDragAndFrictionPath straightXZDragAndFrictionPath = new StraightXZDragAndFrictionPath(passParameters.Pos0, passParameters.Posf, Vector3.zero, Vector3.zero, 0, passParameters.receiverReachPointTime, ballComponents.drag, ballComponents.radio, ballComponents.friction, ballComponents.mass);
        //straightXZDragAndFrictionPath.getV0(t, 0.1f, passParameters.maxControlSpeed, 20, out v0);
        GetV0Result getV0Result = straightXZDragAndFrictionPath.getV0(passParameters.receiverReachPointTime, 0.1f, passParameters.maxControlSpeed,5, 20);
        float v0Magnitude = Mathf.Clamp(getV0Result.v0.magnitude, 0, passParameters.maxKickForce);
        Vector3 v0 = getV0Result.v0.normalized * v0Magnitude;
        getRivalsChaserDatas(passParameters, v0);
        bool noRivalReachTheTargetBeforeMe = checkIfNoRivalReachTheTargetBeforeMe(passParameters, passParameters.receiverReachPointTime);
        bool noPartnerIsAhead = checkIfNoPartnerIsAhead(passParameters,v0, passParameters.Pos0, passParameters.Posf, passParameters.partnerIsAheadMinTime);
        passResult = new PassResult(v0, noRivalReachTheTargetBeforeMe, getV0Result.foundedResult,!noPartnerIsAhead);
        return passResult;
    }
    bool checkIfNoPartnerIsAhead(PassParameters passParameters,Vector3 v0, Vector3 pos0, Vector3 posf, float partnerIsAheadMinTime)
    {
        float minDistance = passParameters.partnerMinDistance;
        Vector3 dir = posf - pos0;
        minDistance = Mathf.Clamp(minDistance, 0, dir.magnitude);
        Vector3 posf2 = pos0 + dir.normalized * minDistance;
        BallComponents ballComponents = MatchComponents.ballComponents;
        StraightXZDragAndFrictionPath straightXZDragAndFrictionPath = new StraightXZDragAndFrictionPath(passParameters.Pos0, v0, 0, ballComponents.friction, ballComponents.drag, ballComponents.mass, ballComponents.radio);
        foreach (var item in passParameters.partnerChaserDatas)
        {
            float d = MyFunctions.DistancePointAndFiniteLine(MyFunctions.setY0ToVector3(item.position), MyFunctions.setY0ToVector3(pos0), MyFunctions.setY0ToVector3(posf));
            float t;
            if(straightXZDragAndFrictionPath.getT(v0, pos0,item.position, ballComponents.drag,0.1f,100,out t))
            {
                if (d < item.publicPlayerData.playerComponents.bodyBallRadio + 0.05f && t <= partnerIsAheadMinTime)
                {
                    return false;
                }
            }
        }
        return true;
    }

    void getRivalsChaserDatas(PassParameters passParameters,Vector3 v0)
    {
        BallComponents ballComponents = MatchComponents.ballComponents;
        StraightXZDragAndFrictionPath straightXZDragAndFrictionPath = new StraightXZDragAndFrictionPath(passParameters.Pos0,v0, 0, ballComponents.friction, ballComponents.drag, ballComponents.mass, ballComponents.radio);
        segmentedPath = new SegmentedPath(straightXZDragAndFrictionPath);
        //SegmentedPath segmentedPath = GetSegmentedPath(passParameters,v0);
        CalculateChaserDatas calculateChaserDatas = new CalculateChaserDatas(chaserDataCalculationParameters.timeRange, chaserDataCalculationParameters.timeIncrement, chaserDataCalculationParameters.minAngle, chaserDataCalculationParameters.minVelocity, chaserDataCalculationParameters.maxAngle, chaserDataCalculationParameters.maxVelocity);
        StartCoroutine(calculateChaserDatas.getChaserDatas(passParameters.rivalsChaserDatas, segmentedPath, true, 0, null));
        
    }
    SegmentedPath GetSegmentedPath(PassParameters passParameters, Vector3 v0)
    {
        BallComponents ballComponents = MatchComponents.ballComponents;
        ParabolaWithDrag trajectory = new ParabolaWithDrag(passParameters.Pos0, v0, 0, 9.81f, ballComponents.drag);
        StraightXZDragAndFrictionPath straightXZDragAndFrictionPath = new StraightXZDragAndFrictionPath(ballComponents.drag, ballComponents.radio, ballComponents.friction, ballComponents.mass);
        BouncyPath bouncyPath = new BouncyPath(trajectory, straightXZDragAndFrictionPath, ballComponents.radio, 0.1f, ballComponents.bounciness, ballComponents.friction);
        bouncyPath.info = "perfectPass";
        SegmentedPath segmentedPath = new SegmentedPath(bouncyPath);
        return segmentedPath;
    }
    private void OnDrawGizmos()
    {
        if (segmentedPath != null)
        {
            segmentedPath.DrawPath("", 20, 0.1f, false);
        }
    }
}
