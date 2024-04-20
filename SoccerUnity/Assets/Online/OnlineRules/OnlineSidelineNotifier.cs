using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class OnlineSidelineNotifier : MonoBehaviourPunCallbacks
{
    void Start()
    {
        RulesEvents.notifySideline.AddListenerConsiderInvoked(notify);

    }
    void notify(SidelineEventArgs args)
    {
        //DebugsList.rules.print("OnlineCornerNotifier.notify()", debugColor, debug);
        object data = args.getData() as object;
        photonView.RPC(nameof(NotifySideline), RpcTarget.Others, data);
        StartCoroutine(waitExecuteSideline(data,0));
    }
    [PunRPC]
    void NotifySideline(object data, PhotonMessageInfo info)
    {
        DebugsList.rules.print("OnlineSidelineNotifier.NotifySideline()");
        double time = PhotonNetwork.Time - info.SentServerTime;
        time = 1 - time;
        StartCoroutine(waitExecuteSideline(data, (float)time));
    }
    IEnumerator waitExecuteSideline(object data, float time)
    {
        yield return new WaitForSeconds(time);
        executeSideline(data);
    }
    void executeSideline(object data)
    {
        //DebugsList.rules.print("OnlineCornerNotifier.NotifyCorner()", debugColor, debug);
        SidelineEventArgs args = new SidelineEventArgs((object[])data);
        MatchComponents.rulesComponents.sidelinesCtrl.execute(args);
    }

}
