using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class RPCGetMatchData : MonoBehaviourPunCallbacks
{
    public bool debug;
    public Color debugColor;
    void Start()
    {

        MatchEvents.matchLoaded.AddListenerConsiderInvoked(matchIsLoaded);
    }
    void matchIsLoaded()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            DebugsList.testing.print("matchIsLoaded I'm not MasterClient", debugColor, debug);
            photonView.RPC(nameof(RequestData), RpcTarget.MasterClient);
        }
        else
        {
            DebugsList.testing.print("matchIsLoaded I'm MasterClient", debugColor, debug);
            //MatchData.matchState = MatchState.WaitingForWarmUp;
            MatchEvents.matchDataIsLoaded.Invoke();
        }
    }
    [PunRPC]
    void RequestData(PhotonMessageInfo info)
    {
        DebugsList.testing.print("RequestData " + info.Sender.NickName, debugColor, debug);
        object[] data = MatchData.getData();
        photonView.RPC(nameof(SendMatchData), info.Sender, data as object);
    }
    [PunRPC]
    void SendMatchData(object[] data, PhotonMessageInfo info)
    {
        DebugsList.testing.print("SendData " + info.Sender.NickName, debugColor, debug);
        MatchData.setData(data);
        MatchEvents.matchDataIsLoaded.Invoke();
    }
}
