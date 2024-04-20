using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialPosition
{
    public static void SetAllInitPosition()
    {
        foreach (var publicPlayerData in PublicPlayerDataList.myPublicPlayerDatas.Values)
        {
            publicPlayerData.bodyTransform.position = publicPlayerData.InitPosition;
            publicPlayerData.bodyTransform.rotation = publicPlayerData.InitRotation;
            ComponentsPlayer.currentComponentsPlayer.rigBody.velocity = Vector3.zero;
            DebugsList.rules.print("InitialPosition.SetAllInitPosition() "+ publicPlayerData.name);
        }
        foreach (var publicPlayerData in PublicPlayerDataList.goalkeepers.Values)
        {
            publicPlayerData.bodyTransform.position = publicPlayerData.InitPosition;
            publicPlayerData.bodyTransform.rotation = publicPlayerData.InitRotation;
            DebugsList.rules.print("InitialPosition.SetAllInitPosition() " + publicPlayerData.name);
        }
        if (MatchData.ImResponsibleForTheBall)
        {
            DebugsList.rules.print("InitialPosition.SetAllInitPosition() ImResponsibleForTheBall");
            Vector3 midFieldPosition = SizeFootballFieldCtrl.getMidField().position;
            //MatchComponents.ballComponents.photonViewBall.RPC(nameof(MatchComponents.ballComponents.kickRPCs.SetData),RPC)
            Vector3 position = MyFunctions.setYToVector3(midFieldPosition, MatchComponents.ballComponents.radio);
            BallData args = new BallData(position, Quaternion.identity, Vector3.zero, Vector3.zero);
            MatchComponents.kickNotifier.notifySetData(args);
            /*
            MatchComponents.ballComponents.transBall.position = MyFunctions.setYToVector3(midFieldPosition, MatchComponents.ballComponents.radio);
            MatchComponents.ballComponents.rigBall.velocity = Vector3.zero;
            MatchComponents.ballComponents.rigBall.angularVelocity = Vector3.zero;*/
        }
    }
    public static bool getInitPositionAndRotation(string playerID,out Vector3 initPosition,out Quaternion initRotation)
    {
        Team team;
        if (Teams.getTeamFromPlayer(playerID, out team))
        {
            TypeFieldPosition.Type typeFieldPosition;
            team.getTypeFieldPositionOfPlayer(playerID, out typeFieldPosition);
            GameObject fieldPositionType = FieldPositionsCtrl.getTypeFieldPosition( team.choosedLineup.typeLineup, typeFieldPosition);
            FieldPosition fieldPosition = fieldPositionType.GetComponent<FieldPosition>();
            initPosition = SideOfFieldCtrl.TransformPointSideOfField(team.TeamName, fieldPosition.initPosition);
            initRotation = SideOfFieldCtrl.TransformRotationSideOfField(team.TeamName, fieldPosition.eulerAngleRotation);
            return true;
        }
        else
        {
            initPosition = Vector3.zero;
            initRotation = Quaternion.identity;
            return false;
        }
    }
    public static bool setInitPositionAndRotationInPublicPlayerData(PublicPlayerData publicPlayerData)
    {
        Vector3 initPosition;
        Quaternion initRotation;
        if (getInitPositionAndRotation(publicPlayerData.playerID, out initPosition, out initRotation))
        {
            publicPlayerData.InitPosition = initPosition;
            publicPlayerData.InitRotation = initRotation;
            return true;
        }
        else
        {
            return false;
        }
    }
}
