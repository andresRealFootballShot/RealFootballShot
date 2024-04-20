using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class OnlineOffsideNotifier : MonoBehaviourPunCallbacks
{
    void Start()
    {
        RulesEvents.notifyOffside.AddListenerConsiderInvoked(notify);

    }
    void notify(OffsideData args)
    {
        DebugsList.rules.print("OnlineOffsideNotifier.notify()");
        object[] data = args.getData();
        //photonView.RPC(nameof(NotifyCorner), RpcTarget.Others,args.SideOfFieldID.ToString(),args.cornerID.ToString(),(byte)playerID.onlineActor,(byte)playerID.localActor);
        photonView.RPC(nameof(NotifyOffside), RpcTarget.Others, args.getData() as object);
        execute(data);
    }
    [PunRPC]
    void NotifyOffside(object data, PhotonMessageInfo info)
    {
        double time = PhotonNetwork.Time - info.SentServerTime;
        time = 1 - time;
        StartCoroutine(waitExecute(data, 0));
    }
    IEnumerator waitExecute(object data, float time)
    {
        yield return new WaitForSeconds(time);
        execute(data);
    }
    void execute(object data)
    {
        DebugsList.rules.print("OnlineOffsideNotifier.execute()");
        OffsideData args = new OffsideData((object[])data);
        MatchComponents.rulesComponents.offsideCtrl.execute(args);
    }
}
