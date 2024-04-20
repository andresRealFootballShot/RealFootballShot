using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class CornerMessagesObsolete : MonoBehaviour
{
    ComponentsPlayer componentsPlayer;
    BallComponents componentsBall;
    Teams teams;
    MatchDataObsolete matchData;
    CornerCtrlObsolete cornerCtrl;

    private void Start()
    {
        componentsPlayer = GameObject.FindGameObjectWithTag("ComponentsPlayer").GetComponent<ComponentsPlayer>();
        teams = GameObject.FindGameObjectWithTag("Teams").GetComponent<Teams>();
        matchData = GameObject.FindGameObjectWithTag("MatchData").GetComponent<MatchDataObsolete>();
        cornerCtrl = GameObject.FindGameObjectWithTag("CornerCtrl").GetComponent<CornerCtrlObsolete>();
        componentsBall = GameObject.FindGameObjectWithTag("ObjectGoal").GetComponent<BallComponents>();
    }
    [PunRPC]
    public void EnableKickCorner(int actorSaque)
    {
        componentsPlayer.kickGObjt.SetActive(true);
        matchData.saqueCorner= false;
        componentsBall.rigBall.isKinematic = false;
    }
    [PunRPC]
    public void DisableKickCorner(string team)
    {
        if (teams.getTeamFromActor(PhotonNetwork.LocalPlayer.ActorNumber).name == team)
        {
            componentsPlayer.kickGObjt.SetActive(false);
        }
    }
}
