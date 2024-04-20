using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pruebasPhoton : MonoBehaviourPunCallbacks, IOnEventCallback
{
    // Start is called before the first frame update
    void Start()
    {
        
       
    }
    public override void OnJoinedRoom()
    {
        print("OnJoinedRoom");
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("PruebaRPC",Vector3.zero,Quaternion.identity);
            object[] content = new object[] { };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCache };
            for (int i = 0; i < 100; i++)
            {
                PhotonNetwork.RaiseEvent(CodeEventsNet.Pruebas, content, raiseEventOptions, SendOptions.SendReliable);
            }
        }
    }
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (CodeEventsNet.Pruebas == eventCode)
        {
            print("OnEvent");
        }
    }
    
}
