using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class SetStateSaques : MonoBehaviourPunCallbacks
{
    public PublicPlayerData publicPlayerData;
    MatchDataObsolete matchData;
    public new string tag;
    public PhotonView pVPlayer;
    public void setup()
    {
        publicPlayerData.collisionEvent.Event += OnEvent;
        //pVPlayer = GameObject.FindGameObjectWithTag("MyPlayerNet").GetComponent<PhotonView>();
        matchData = GameObject.FindGameObjectWithTag("MatchData").GetComponent<MatchDataObsolete>();
    }

    // Update is called once per frame
    void OnEvent(Collision collision)
    {
        if (collision.transform.tag == tag)
        {
            if (photonView.IsMine && (matchData.saqueCorner || matchData.saqueDeBanda))
                pVPlayer.RPC("StateSaqueCornerBanda", RpcTarget.All,false,false);
        }
    }
}
