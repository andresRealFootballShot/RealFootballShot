using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
public class Connect : MonoBehaviourPunCallbacks
{
    //public Button playOnline;
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }
        
    }
    public override void OnConnectedToMaster()
    {
        //playOnline.interactable = true;
        OnlineEvents.connected.Invoke();
    }
}
