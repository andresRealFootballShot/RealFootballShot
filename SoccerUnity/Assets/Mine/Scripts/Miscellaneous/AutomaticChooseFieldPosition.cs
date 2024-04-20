using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class AutomaticChooseFieldPosition : MonoBehaviourPunCallbacks
{
    public InstantiatePlayer instantiatePlayer;
    public ChooseTeamCtrl chooseTeamCtrl;
    public ChooseFieldPositionCtrl chooseFieldPositionCtrl;
    public ChooseLineupCtrl chooseLineupCtrl;

    public void StartProcess()
    {
        chooseTeamCtrl.HideMenu();
        chooseFieldPositionCtrl.HideMenu();
        MatchEvents.matchLoaded.AddListener(chooseFieldPosition);
        MatchEvents.requestedFieldPositionWasAccepted.AddListener(InstantiatePlayer);
        MatchEvents.requestedFieldPositionWasDenied.AddListener(chooseFieldPosition);
        DebugsList.RPCRequestFieldPosition.print("AutomaticChooseFieldPosition StartProcess");
    }

    public void chooseFieldPosition()
    {
        DebugsList.RPCRequestFieldPosition.print("AutomaticChooseFieldPosition-chooseFieldPosition");
        ChooseTeamCtrl.teamSelected = Teams.getAvailableRandomTeam();
        ChooseFieldPositionCtrl.typeFieldSelected = ChooseTeamCtrl.teamSelected.getAvailableRandomFieldPosition();
        ChooseModel.ChooseRandomModel();
        //ChooseModel.Choose(ModelName.Name.DefaultMale);
        MatchComponents.requestFieldPosition.RequestFieldPosition(ComponentsPlayer.myMonoPlayerID.getStringID(),ChooseTeamCtrl.teamSelected.TeamName, ChooseFieldPositionCtrl.typeFieldSelected.ToString());
    }
    public void InstantiatePlayer()
    {
        Team teamOfPlayer;
        Teams.getTeamFromPlayer(ComponentsPlayer.myMonoPlayerID.getStringID(),out teamOfPlayer);
        TypeFieldPosition.Type typeFieldPosition;
        teamOfPlayer.getTypeFieldPositionOfPlayer(ComponentsPlayer.myMonoPlayerID.getStringID(),out typeFieldPosition);
        GameObject fieldPositionType = FieldPositionsCtrl.getTypeFieldPosition(teamOfPlayer.choosedLineup.typeLineup, typeFieldPosition);
        FieldPosition fieldPosition = fieldPositionType.GetComponent<FieldPosition>();
        Vector3 initPosition = SideOfFieldCtrl.TransformPointSideOfField(ChooseTeamCtrl.teamSelected.TeamName, fieldPosition.initPosition);
        Quaternion initRotation = SideOfFieldCtrl.TransformRotationSideOfField(ChooseTeamCtrl.teamSelected.TeamName, fieldPosition.eulerAngleRotation);
        instantiatePlayer.Instantiate();
    }
}
