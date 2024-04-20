using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinToOnlineTeam : MonoBehaviour, IJoinToTeam
{
    public PhotonView photonView;

    public event JoiningProcessFinishedDelegate joininProcessFinishedEvent;

    public void joinToTeam()
    {
        photonView.RPC("RequestJoin", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, ChooseTeamCtrl.teamSelected.TeamName);
    }
}
