using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineKickNotifier : IKickNotifier
{
    public void notifyAddForce(KickEventArgs args)
    {
        Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        Transform ballTransform = MatchComponents.ballComponents.transBall;
        Kick.AddForce(ForceMode.VelocityChange, args);
        
    }
    public void notifySetData(BallData args)
    {
        //Vector3 midFieldPosition = SizeFootballFieldCtrl.getMidField().position;
        //MatchComponents.ballComponents.transBall.position = MyFunctions.setYToVector3(midFieldPosition, MatchComponents.ballComponents.radio);
        MatchComponents.ballComponents.rigBall.position = args.position;
        MatchComponents.ballComponents.rigBall.rotation = args.rotation;
        MatchComponents.ballComponents.rigBall.velocity = args.velocity;
        MatchComponents.ballComponents.rigBall.angularVelocity = args.angularVelocity;
    }
}
