using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class OnlineKickNotifier : IKickNotifier
{
    public void notifyAddForce(KickEventArgs args)
    {
        Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        Transform ballTransform = MatchComponents.ballComponents.transBall;
        string rpcName = nameof(MatchComponents.ballComponents.kickRPCs.AddForceRPC);
        PlayerID playerID = new PlayerID(args.playerID);
        MatchComponents.ballComponents.photonViewBall.RPC(rpcName, RpcTarget.All, ballRigidbody.position, ballTransform.eulerAngles, args.kickDirection, args.previousVelocity, args.previousAngularVelocity, playerID.onlineActor, playerID.localActor);

    }

    public void notifySetData(BallData args)
    {
        string rpcName = nameof(MatchComponents.ballComponents.kickRPCs.SetData);
        MatchComponents.ballComponents.photonViewBall.RPC(rpcName, RpcTarget.All, args.position, args.rotation, args.velocity, args.angularVelocity);
    }
}
