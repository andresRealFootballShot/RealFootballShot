using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class EndMatchObsolete : MonoBehaviour
{
    public Teams teams;
    public Animator animVictory, animDefeat, animTie;
    public Canvas exitMatch;
    public MenuNet menuNet;
    public void End()
    {
        exitMatch.enabled = true;
        menuNet.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Team myTeam = teams.getTeamFromActor(PhotonNetwork.LocalPlayer.ActorNumber);
        Team otherTeam = teams.getTeamNotContainActor(PhotonNetwork.LocalPlayer.ActorNumber);
        /*
        if (myTeam.goals>otherTeam.goals)
        {
            animVictory.SetBool("start", true);
        }
        else if (myTeam.goals < otherTeam.goals)
        {
            animDefeat.SetBool("start", true);
        }
        else
        {
            animTie.SetBool("start", true);
        }*/
    }
    
}
