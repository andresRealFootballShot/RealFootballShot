using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PorteroPosesion : MonoBehaviour
{
    public CollisionEvent collisionEvent;
    public Team team;
    public new string tag;
    PhotonView pVPlayer;
    void Start()
    {
        collisionEvent.Event += OnEvent;
        StartCoroutine(GetPVPlayer());
    }

    IEnumerator GetPVPlayer()
    {
        GameObject g = null;
        while (g == null)
        {
            g = GameObject.FindGameObjectWithTag("MyPlayerNet");
            yield return null;
        }
        pVPlayer = g.GetComponent<PhotonView>();
    }
    void OnEvent(Collision collision)
    {
        if (collision.transform.tag == tag)
        {
            //pVPlayer.RPC("ActorWithPosession", RpcTarget.All, team.actorPortero);
        }
    }
}
