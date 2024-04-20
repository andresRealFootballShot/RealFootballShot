using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class InstantiateMatchSequence : MonoBehaviourPunCallbacks,IOnEventCallback
{
    public GameObject matchSequencePref;
    public void Start()
    {
        
    }
    public void Instantiate()
    {
        GameObject matchSecuence = Instantiate(matchSequencePref);
        PhotonView pv = matchSecuence.GetComponent<PhotonView>();
        
        if (PhotonNetwork.AllocateViewID(pv))
        {
            object[] data = new object[]
            {
             pv.ViewID
            };

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = EventCaching.AddToRoomCacheGlobal
            };

            SendOptions sendOptions = new SendOptions
            {
                Reliability = true
            };

            PhotonNetwork.RaiseEvent(CodeEventsNet.MatchSequence, data, raiseEventOptions, sendOptions);

        }
        else
        {
            Debug.LogError("Failed to allocate a ViewId.");

            Destroy(matchSecuence);
        }
    }
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == CodeEventsNet.MatchSequence)
        {
            object[] data = (object[])photonEvent.CustomData;

            GameObject player = (GameObject)Instantiate(matchSequencePref, Vector3.zero, Quaternion.identity);
            PhotonView photonView = player.GetComponent<PhotonView>();
            photonView.ViewID = (int)data[0];
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
