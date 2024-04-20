using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class OnlineStartPartNotifier : MonoBehaviourPunCallbacks
{
    List<Player> readyPlayers = new List<Player>();
    public bool debug = true;
    public Color debugColor = new Color(0, 0.4f, 1f);
    void Start()
    {
        //RulesEvents.enableStartMach.AddListenerConsiderInvoked(Enable);
        Enable();
    }
    public void Enable()
    {
        RulesEvents.notifyStartPart.AddListenerConsiderInvoked(notify);
    }

    public void Disable()
    {
        RulesEvents.notifyStartPart.RemoveListener(notify);
    }
    public void notify()
    {
        DebugsList.rules.print("OnlineStartPartNotifier notify", debugColor, debug);
        photonView.RPC(nameof(NotifyReadPlayer), RpcTarget.All);
    }
    [PunRPC]
    void NotifyReadPlayer(PhotonMessageInfo info)
    {
        if (!readyPlayers.Exists(x => x.ActorNumber == info.Sender.ActorNumber))
        {
            DebugsList.rules.print("Player "+ info.Sender.NickName+" is Ready", debugColor, debug);
            readyPlayers.Add(info.Sender);
            checkStartPart();
        }
    }
    void checkStartPart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (readyPlayers.Count == PhotonNetwork.PlayerList.Length)
            {
                DebugsList.rules.print("I am Master Client-Send StartMatch", debugColor, debug);
                photonView.RPC(nameof(StartPart), RpcTarget.All);
            }
        }
    }
    [PunRPC]
    void StartPart(PhotonMessageInfo info)
    {
        double time = PhotonNetwork.Time - info.SentServerTime;
        time = MatchComponents.rulesSettings.timeWaitToStart - time;
        DebugsList.rules.print("StartPart PhotonNetwork.Time="+ PhotonNetwork.Time+" | Sendtime="+ info.SentServerTime + " | t="+time, debugColor, debug);
        readyPlayers.Clear();
        Invoke(nameof(startMatchExecutor), (float)time);
    }
    void startMatchExecutor()
    {
        DebugsList.rules.print("startPartExecutor PhotonNetwork.Time=" + PhotonNetwork.Time, debugColor, debug);
        RulesEvents.startPart.Invoke();
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {

    }
}
