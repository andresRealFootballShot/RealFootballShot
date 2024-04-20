using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class SetActorPosessionCollision : MonoBehaviourPunCallbacks
{
    public PublicPlayerData publicPlayerData;
    public PhotonView pVPlayer;
    public void setup()
    {
        publicPlayerData.collisionEvent.Event += OnEvent;
    }

    // Update is called once per frame
    void OnEvent(Collision collision)
    {
        if (collision.transform.tag == Tags.ObjectGoal)
        {
            if (photonView.IsMine)
                pVPlayer.RPC("ActorWithPosession", RpcTarget.All, photonView.Owner.ActorNumber);
        }
    }
}
