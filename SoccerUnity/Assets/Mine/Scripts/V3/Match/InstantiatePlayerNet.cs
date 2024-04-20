using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class InstantiatePlayerNet : MonoBehaviourPunCallbacks,IOnEventCallback
{
    public GameObject playerPref;
    public Transform parent;
    public void Start()
    {
        
    }

    public GameObject Instantiate()
    {


        GameObject g = PhotonNetwork.InstantiateSceneObject(playerPref.name,Vector3.zero,Quaternion.identity,0,null);
        g.transform.parent = parent;
        return g;
        GameObject player = Instantiate(playerPref);
        
        PhotonView pv = player.GetComponent<PhotonView>();

        if (PhotonNetwork.AllocateViewID(pv))
        {
            player.transform.parent = parent;
            object[] data = new object[]
            {
             pv.ViewID
            };

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = EventCaching.AddToRoomCache
            };

            SendOptions sendOptions = new SendOptions
            {
                Reliability = true
            };

            PhotonNetwork.RaiseEvent(CodeEventsNet.Player, data, raiseEventOptions, sendOptions);
            return player;
        }
        else
        {
            Debug.LogError("Failed to allocate a ViewId.");
            Destroy(player);
            return null;
        }
    }
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == CodeEventsNet.Player)
        {
            object[] data = (object[])photonEvent.CustomData;

            GameObject player = (GameObject)Instantiate(playerPref, Vector3.zero, Quaternion.identity);
            PhotonView photonView = player.GetComponent<PhotonView>();
            photonView.ViewID = (int)data[0];
            player.transform.parent = parent;
        }
    }
    
    void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    

}
