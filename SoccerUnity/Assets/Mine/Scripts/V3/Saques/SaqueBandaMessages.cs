using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class SaqueBandaMessages : MonoBehaviour
{
    ComponentsPlayer componentsPlayer;
    BallComponents componentsBall;
    Teams teams;
    MatchDataObsolete matchData;
    SaqueBandaCtrl saqueBandaCtrl;
    private void Start()
    {
        componentsPlayer = GameObject.FindGameObjectWithTag("ComponentsPlayer").GetComponent<ComponentsPlayer>();
        teams = GameObject.FindGameObjectWithTag("Teams").GetComponent<Teams>();
        matchData = GameObject.FindGameObjectWithTag("MatchData").GetComponent<MatchDataObsolete>();
        saqueBandaCtrl = GameObject.FindGameObjectWithTag("SaqueDeBandaCtrl").GetComponent<SaqueBandaCtrl>();
        componentsBall = GameObject.FindGameObjectWithTag("ObjectGoal").GetComponent<BallComponents>();
    }
    [PunRPC]
    public void EnableKick(int actorSaque)
    {
        componentsPlayer.kickGObjt.SetActive(true);
        saqueBandaCtrl.actorSaque = actorSaque;
        matchData.saqueDeBanda = false;
        componentsBall.rigBall.isKinematic = false;
    }
    [PunRPC]
    public void DisableKick(string team)
    {
        if (teams.getTeamFromActor(PhotonNetwork.LocalPlayer.ActorNumber).name == team)
        {
            componentsPlayer.kickGObjt.SetActive(false);
        }
    }
}
