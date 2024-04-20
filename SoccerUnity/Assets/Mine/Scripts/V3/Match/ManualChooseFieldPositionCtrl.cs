using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class ManualChooseFieldPositionCtrl : MonoBehaviour
{
    public MatchDataObsolete matchData;
    //public GetTeamsInfoCtrl getTeamsInfoCtrl;
    
    public ChooseTeamCtrl chooseTeamCtrl;
    public ChooseFieldPositionCtrl chooseFieldPositionCtrl;
    public ChooseLineupCtrl chooseLineupCtrl;
    public JoinToTeam joinToTeam;
    public ButtonCtrl okChooseFieldPosition;
    public TypeFieldPosition typeFieldPositionSelected;
    public InstantiatePlayer instantiatePlayer;
    public InstantiatePhotonGameObject masterClientRPCs;
    public void StartProcess()
    {
        chooseTeamCtrl.HideMenu();
        joinToTeam.chooseFieldPositionCtrl = chooseFieldPositionCtrl;
        chooseTeamCtrl.JoinToTeam = joinToTeam;
        chooseFieldPositionCtrl.HideMenu();
        MatchEvents.matchLoaded.AddListener(endLoadMatch);
        MatchEvents.requestedFieldPositionWasAccepted.AddListener(acceptRequestFieldPosition);
        MatchEvents.requestedFieldPositionWasDenied.AddListener(denyRequestFieldPosition);
    }

    //Called by Resources/TypeMatch
    public void endLoadMatch()
    {
        //getTeamsInfoCtrl.StartProcess();
        chooseTeamCtrl.ShowMenu();
        //Para mostrar el campo de futbol
        //lineupUIList.SetActive(true);
        foreach (var item in Teams.teamsList)
        {
            chooseTeamCtrl.Deselect(item.nameTeamVar);
        }
        //SideOfFieldCtrl.SetRandomSideOfField(Teams.getTeamNames());
        ChooseModel.ChooseRandomModel();
    }
    public void acceptRequestFieldPosition()
    {
        Team team;
        chooseFieldPositionCtrl.HideMenu();
        chooseTeamCtrl.HideMenu();
        if(Teams.getTeamFromPlayer(ComponentsPlayer.myMonoPlayerID.getStringID(),out team))
        {
            
            TypeFieldPosition.Type typeFieldPosition;
            team.getTypeFieldPositionOfPlayer(ComponentsPlayer.myMonoPlayerID.getStringID(), out typeFieldPosition);
            GameObject fieldPositionType = FieldPositionsCtrl.getTypeFieldPosition(team.choosedLineup.typeLineup, typeFieldPosition);
            FieldPosition fieldPosition = fieldPositionType.GetComponent<FieldPosition>();
            Vector3 initPosition = SideOfFieldCtrl.TransformPointSideOfField(team.TeamName, fieldPosition.initPosition);
            Quaternion initRotation = SideOfFieldCtrl.TransformRotationSideOfField(team.TeamName, fieldPosition.eulerAngleRotation);
            
            instantiatePlayer.Instantiate();
        }
    }
    public void denyRequestFieldPosition()
    {
        chooseFieldPositionCtrl.ShowMenu();
        chooseTeamCtrl.ShowMenu();
        okChooseFieldPosition.Enable();
    }
}
