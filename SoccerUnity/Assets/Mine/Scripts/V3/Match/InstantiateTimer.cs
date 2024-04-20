using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class InstantiateTimer : MonoBehaviour,IOnEventCallback
{
    
    public GameObject timerPref;
    public Transform parent;
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            AllocateView();
    }

    void AllocateView()
    {
        GameObject timer = Instantiate(timerPref,timerPref.transform.position,timerPref.transform.rotation,parent);
        RectTransform rectTransform = timer.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector3.zero;
        PhotonView pv = timer.GetComponent<PhotonView>();
        
        if(PhotonNetwork.AllocateViewID(pv))
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

            PhotonNetwork.RaiseEvent(CodeEventsNet.Timer, data, raiseEventOptions, sendOptions);
        }
        else
        {
            Debug.LogError("Failed to allocate a ViewId.");
        }
    }
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == CodeEventsNet.Timer)
        {
            object[] data = (object[])photonEvent.CustomData;
            GameObject timer = Instantiate(timerPref, timerPref.transform.position, timerPref.transform.rotation, parent);
            RectTransform rectTransform = timer.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector3.zero;
            PhotonView pv = timer.GetComponent<PhotonView>();
            pv.ViewID = (int)data[0];
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
