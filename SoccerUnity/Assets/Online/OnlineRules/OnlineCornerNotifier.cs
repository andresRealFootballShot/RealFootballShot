using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class OnlineCornerNotifier : MonoBehaviourPunCallbacks
{
    public bool debug = true;
    public Color debugColor = new Color(0, 0.4f, 1f);
    void Start()
    {
        RulesEvents.notifyCorner.AddListenerConsiderInvoked(notify);

    }
    void notify(CornerEventArgs args)
    {
        //DebugsList.rules.print("OnlineCornerNotifier.notify()", debugColor, debug);
        string lastPlayerPossession = args.lastKickOfPlayerID;
        PlayerID playerID = new PlayerID(lastPlayerPossession);
        //photonView.RPC(nameof(NotifyCorner), RpcTarget.Others,args.SideOfFieldID.ToString(),args.cornerID.ToString(),(byte)playerID.onlineActor,(byte)playerID.localActor);
        photonView.RPC(nameof(NotifyCorner), RpcTarget.All, args.getData() as object);
        //CornerCtrl.dispatchCorner(args);
    }
    [PunRPC]
    void NotifyCorner(object data, PhotonMessageInfo info)
    {
        double time = PhotonNetwork.Time - info.SentServerTime;
        time = 1 - time;
        Invoke(nameof(playWhistle), 0.3f);
        StartCoroutine(waitDispatchCorner(data,(float)time));
    }
    void playWhistle()
    {
        MatchComponents.rulesComponents.whistleAnimation.Play();
    }
    IEnumerator waitDispatchCorner(object data,float time)
    {
        yield return new WaitForSeconds(time);
        dispatchCorner(data);
    }
    void dispatchCorner(object data)
    {
        //DebugsList.rules.print("OnlineCornerNotifier.NotifyCorner()", debugColor, debug);
        CornerEventArgs args = new CornerEventArgs((object[])data);
        MatchComponents.rulesComponents.cornerCtrl.dispatchCorner(args);
    }
}
