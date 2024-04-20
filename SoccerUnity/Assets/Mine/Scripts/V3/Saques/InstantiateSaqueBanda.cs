using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
public class InstantiateSaqueBanda : MonoBehaviour, IOnEventCallback
{
    public GameObject saqueBandaPref;
    private void Start()
    {
        if(PhotonNetwork.IsMasterClient)
            Instantiate();
    }
    public void SetSaqueDeBanda(PhotonView pv)
    {
        GameObject saqueDeBandaCtrl = GameObject.FindGameObjectWithTag("SaqueDeBandaCtrl");
        saqueDeBandaCtrl.GetComponent<SaqueBandaCtrl>().photonView = pv;
    }
    public GameObject Instantiate()
    {
        GameObject player = Instantiate(saqueBandaPref);

        PhotonView pv = player.GetComponent<PhotonView>();
        SetSaqueDeBanda(pv);
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

            PhotonNetwork.RaiseEvent(CodeEventsNet.SaqueBanda, data, raiseEventOptions, sendOptions);
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
        if (photonEvent.Code == CodeEventsNet.SaqueBanda)
        {
            object[] data = (object[])photonEvent.CustomData;

            GameObject player = Instantiate(saqueBandaPref, Vector3.zero, Quaternion.identity);
            PhotonView photonView = player.GetComponent<PhotonView>();
            SetSaqueDeBanda(photonView);
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
