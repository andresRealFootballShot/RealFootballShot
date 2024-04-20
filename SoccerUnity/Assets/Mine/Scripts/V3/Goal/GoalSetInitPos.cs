using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class GoalSetInitPos : MonoBehaviour
{
    public GoalFavorAnimation goalFavorAnimation;
    public GoalAgainstAnimation goalAgainstAnimation;
    [HideInInspector] public string teamGoal;
    void Start()
    {
        goalFavorAnimation.EndEvent += EndAnimationGoal;
        goalAgainstAnimation.EndEvent += EndAnimationGoal;

    }

    public void EndAnimationGoal()
    {
        
        Invoke("StartAgainMatch", 0);
    }
    void StartAgainMatch()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerMessages myPlayerNet = GameObject.FindGameObjectWithTag("MyPlayerNet").GetComponent<PlayerMessages>();
            myPlayerNet.photonView.RPC("SendSetInitialPos",RpcTarget.All,teamGoal,PhotonNetwork.Time);
        }
    }
}
