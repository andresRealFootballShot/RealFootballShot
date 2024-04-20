using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
public class PruebaRPC : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        print("Start");
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < 100; i++)
            {
                photonView.RPC(nameof(pruebaRPC), RpcTarget.AllBufferedViaServer);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    void pruebaRPC(PhotonMessageInfo info)
    {
        print("pruebaRPC");
    }
}
