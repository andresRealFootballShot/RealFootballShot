using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
public class InstantiateCornerMessages : MonoBehaviour, IOnEventCallback
{
    public GameObject cornerPref;
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            Instantiate();
    }
    public void SetCornerPhotonView(PhotonView pv)
    {
        GameObject saqueDeBandaCtrl = GameObject.FindGameObjectWithTag("CornerCtrl");
        saqueDeBandaCtrl.GetComponent<CornerCtrlObsolete>().photonView = pv;
    }
    public GameObject Instantiate()
    {
        GameObject player = Instantiate(cornerPref);

        PhotonView pv = player.GetComponent<PhotonView>();
        SetCornerPhotonView(pv);
        if (PhotonNetwork.AllocateViewID(pv))
        {
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

            PhotonNetwork.RaiseEvent(CodeEventsNet.Corner, data, raiseEventOptions, sendOptions);
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
        if (photonEvent.Code == CodeEventsNet.Corner)
        {
            object[] data = (object[])photonEvent.CustomData;

            GameObject player = Instantiate(cornerPref, Vector3.zero, Quaternion.identity);
            PhotonView photonView = player.GetComponent<PhotonView>();
            SetCornerPhotonView(photonView);
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
