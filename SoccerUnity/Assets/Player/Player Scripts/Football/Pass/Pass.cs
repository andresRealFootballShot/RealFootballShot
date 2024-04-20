using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class Pass : MonoBehaviour
{
    public ChaserDataCalculationParameters chaserDataCalculationParameters { get; set; }

    protected bool checkIfNoRivalReachTheTargetBeforeMe(PassParameters passParameters,float receiverReachPointTime)
    {
        foreach (var item in passParameters.rivalsChaserDatas)
        {
            float t = item.OptimalTime - receiverReachPointTime;
            if (item.ReachTheTarget && t < passParameters.rival_Thrower_OptimalTimeDifference)
            {
                return false;
            }
        }
        return true;
    }
    
    public abstract PassResult getPassResult(PassParameters passData);
}
public class PassParameters
{
    public ChaserData ReceiverChaserData;
    public List<ChaserData> partnerChaserDatas;
    public List<ChaserData> rivalsChaserDatas;
    public Vector3 Pos0, Posf;
    public float maxControlSpeed;
    public float maxKickForce;
    public float rival_Thrower_OptimalTimeDifference;
    public float partnerMinDistance;
    public Vector3 initV0;
    public float receiverReachPointTime;
    public float maxControlSpeedLerpDistance;
    public float partnerIsAheadMinTime;
    public PassParameters(ChaserData receiverChaserData, List<ChaserData> partnerChaserDatas, List<ChaserData> rivalsChaserDatas, Vector3 pos0, Vector3 posf, float maxControlSpeed, float maxKickForce, float rival_Thrower_OptimalTimeDifference, float partnerMinDistance, Vector3 initV0,float maxControlSpeedLerpDistance, float receiverReachPointTime, float partnerIsAheadMinTime)
    {
        ReceiverChaserData = receiverChaserData;
        this.partnerChaserDatas = partnerChaserDatas;
        this.rivalsChaserDatas = rivalsChaserDatas;
        Pos0 = pos0;
        Posf = posf;
        this.maxControlSpeed = maxControlSpeed;
        this.maxKickForce = maxKickForce;
        this.rival_Thrower_OptimalTimeDifference = rival_Thrower_OptimalTimeDifference;
        this.partnerMinDistance = partnerMinDistance;
        this.initV0 = initV0;
        this.maxControlSpeedLerpDistance = maxControlSpeedLerpDistance;
        this.receiverReachPointTime = receiverReachPointTime;
        this.partnerIsAheadMinTime = partnerIsAheadMinTime;
    }
}
public class ParabolicPassParameters : PassParameters
{
    public float rivalsHeightOffset;
    public int maxAttempts;
    public float maxReceiverHeight;
    public float searchVyIncrement;
    public ParabolicPassParameters(ChaserData receiverChaserData, List<ChaserData> partnerChaserDatas, List<ChaserData> rivalsChaserDatas, Vector3 pos0, Vector3 posf, float maxControlSpeed, float maxKickForce, float rival_Thrower_OptimalTimeDifference, float partnerMinDistance, Vector3 initV0,float rivalsHeightOffset, int maxAttempts, float maxReceiverHeight, float searchVyIncrement,float maxControlSpeedLerpDistance,float receiverReachPointTime, float partnerIsAheadMinTime) : base(receiverChaserData, partnerChaserDatas, rivalsChaserDatas, pos0, posf, maxControlSpeed, maxKickForce, rival_Thrower_OptimalTimeDifference, partnerMinDistance, initV0, maxControlSpeedLerpDistance, receiverReachPointTime, partnerIsAheadMinTime)
    {
        this.rivalsHeightOffset = rivalsHeightOffset;
        this.maxAttempts = maxAttempts;
        this.maxReceiverHeight = maxReceiverHeight;
        this.searchVyIncrement = searchVyIncrement;
    }
}
public class PassResult
{
    public Vector3 v0 = Vector3.zero;
    public bool noRivalReachTheTargetBeforeMe;
    public bool resultFounded;
    public bool somePartnerIsAhead;
    public PassResult(Vector3 v0, bool noRivalReachTheTargetBeforeMe, bool resultFounded)
    {
        this.v0 = v0;
        this.noRivalReachTheTargetBeforeMe = noRivalReachTheTargetBeforeMe;
        this.resultFounded = resultFounded;
    }
    public PassResult(bool resultFounded)
    {
        this.resultFounded = resultFounded;
    }

    public PassResult(Vector3 v0, bool noRivalReachTheTargetBeforeMe, bool resultFounded, bool somePartnerIsAhead)
    {
        this.v0 = v0;
        this.noRivalReachTheTargetBeforeMe = noRivalReachTheTargetBeforeMe;
        this.resultFounded = resultFounded;
        this.somePartnerIsAhead = somePartnerIsAhead;
    }
    public override string ToString()
    {
        return "resultFounded=" + resultFounded + " noRivalReachTheTargetBeforeMe=" + noRivalReachTheTargetBeforeMe + " somePartnerIsAhead="+ somePartnerIsAhead + " v0Speed=" + v0.magnitude + " v0=" + v0;
    }
}
