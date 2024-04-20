using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseTeamCtrl : MonoBehaviour
{
    public ChooseTeamMenu chooseTeamMenu;
    public static Team teamSelected;
    IJoinToTeam joinToTeam;
    public IJoinToTeam JoinToTeam { get => joinToTeam; set => joinToTeam = value; }
    
    public void SetInteractableButtons(bool red, bool blue)
    {
        
    }
    public void HideMenu()
    {
        CursorCtrl.notifyHideMenu();
        chooseTeamMenu.HideMenu();
    }
    public void ShowMenu()
    {
        CursorCtrl.notifyShowMenu();
        chooseTeamMenu.ShowMenu();
    }
    public void Exit_Click()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }
    public void Select(Variable<string> nameTeam)
    {
        chooseTeamMenu.Select(nameTeam);
    }
    public void Deselect(Variable<string> nameTeam)
    {
        chooseTeamMenu.Deselect(nameTeam);
    }
    public void SelectTeam_Click(Team team)
    {
        teamSelected = team;
        JoinToTeam.joinToTeam();
    }
}
