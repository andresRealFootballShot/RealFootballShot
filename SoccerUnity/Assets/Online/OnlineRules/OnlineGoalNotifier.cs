using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class OnlineGoalNotifier : MonoBehaviourPunCallbacks
{
    void Start()
    {
        RulesEvents.notifyGoal.AddListenerConsiderInvoked(notify);

    }
    void notify(GoalData args)
    {
        //DebugsList.rules.print("OnlineCornerNotifier.notify()", debugColor, debug);
        //photonView.RPC(nameof(NotifyCorner), RpcTarget.Others,args.SideOfFieldID.ToString(),args.cornerID.ToString(),(byte)playerID.onlineActor,(byte)playerID.localActor);

        object[] data = args.getData();
        photonView.RPC(nameof(NotifyGoal), RpcTarget.Others, data as object);
        executeGoal(data);
        //StartCoroutine(waitExecuteGoal(data, 1));
        //CornerCtrl.dispatchCorner(args);
    }
    [PunRPC]
    void NotifyGoal(object data, PhotonMessageInfo info)
    {
        executeGoal(data);
        /*
        double time = PhotonNetwork.Time - info.SentServerTime;
        time = 1 - time;
        //Invoke(nameof(playWhistle), 0.3f);
        StartCoroutine(waitExecuteGoal(data, (float)time));*/
    }
    IEnumerator waitExecuteGoal(object data, float time)
    {
        yield return new WaitForSeconds(time);
        executeGoal(data);
    }
    void executeGoal(object data)
    {
        //DebugsList.rules.print("OnlineCornerNotifier.NotifyCorner()", debugColor, debug);
        GoalData args = new GoalData((object[])data);
        MatchComponents.rulesComponents.goalCtrl.execute(args);
    }
}
