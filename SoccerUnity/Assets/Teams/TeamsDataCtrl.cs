using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TeamsDataCtrl : MonoBehaviour
{
    PlayerIDMonoBehaviour myPlayerID;
    public RequestData requestData;
    public SendData sendData;
    public ReceiveData receiveData;
    public UnityEvent endProcess;
    public MyDebug myDebug;
    public OnlineErrorHandler errorHandelr;
    static Color debugColor = new Color(0.5f, 0.2f, 0.7f);
    void Awake()
    {
        myPlayerID = FindObjectOfType<PlayerIDMonoBehaviour>();
    }
    /*
    public void StartProcess()
    {
        PlayerID targetID = new PlayerID(TypePlayer.PlayerNet, PhotonNetwork.MasterClient.ActorNumber, 0);
        List<PlayerID> list = new List<PlayerID>();
        list.Add(targetID);
        requestData.setup(list, new int[] { 0 }, myPlayerID,new int[] { myPlayerID.playerID.localActor}, CodeEventsNet.RequestTeamData, CodeEventsNet.ReceiveTeamData, ReceiverGroup.MasterClient, true,MyTypeReliability.Reliable,SendOptions.SendReliable,EventCaching.DoNotCache, 10,"Teams Data");
        sendData.setup(this, myPlayerID, CodeEventsNet.RequestTeamData, CodeEventsNet.ReceiveTeamData,CodeEventsNet.ConfirmTeamDataReception, MyTypeReliability.Reliable, SendOptions.SendReliable, EventCaching.DoNotCache,10, "Teams Data");
        receiveData.setup(this, myPlayerID, CodeEventsNet.ReceiveTeamData, CodeEventsNet.ConfirmTeamDataReception, MyTypeReliability.Reliable, SendOptions.SendReliable, EventCaching.DoNotCache, "Teams Data");
        if (PhotonNetwork.IsMasterClient && myPlayerID.playerID.localActor == PlayerID.mainLocalActor)
        {
            //sendData.sendUnrequestedData(PlayerID.None, new List<PlayerID>(), new int[] {1}, ReceiverGroup.MasterClient, true, 1);
            deliverData(getData());
        }
        else
        {
            requestData.StartProcess();
        }
    }*/
    public void requestDataError()
    {
        OnlineErrorHandler.OnlineError("Get teams data");
    }
    public static void deliverData(object[] teamsData)
    {
        DebugsList.RPCRequestFieldPosition.print("Deliver Teams Data", debugColor, ComponentsPlayer.myMonoPlayerID);
        
        foreach (object[] teamData in teamsData)
        {
            int index = 0;
            string teamName = (string)teamData[index++];
            string teamLineup = (string)teamData[index++];
            string sideOfFieldID = (string)teamData[index++];
            SideOfFieldCtrl.setTeamSide(teamName, sideOfFieldID);
            Teams.setLineupToTeam(teamName, teamLineup);
            DebugsList.RPCRequestFieldPosition.print("TeamName=" + teamName + "Lineup="+ teamLineup, debugColor, ComponentsPlayer.myMonoPlayerID);
            object[] fieldPositions = (object[])teamData[index++];
            foreach (object[] fieldPosition in fieldPositions)
            {
                string playerID = (string)fieldPosition[0];
                if (PlayerID.getTypePlayer(playerID).Equals(TypePlayer.PlayerNet))
                {
                    string typeFieldPosition = (string)fieldPosition[1];
                    Teams.AddPlayerToTeam(teamName, playerID, typeFieldPosition);
                    DebugsList.RPCRequestFieldPosition.print("PlayerID=" + playerID + " | FieldPosition=" + typeFieldPosition, debugColor, ComponentsPlayer.myMonoPlayerID);
                }
                else
                {
                    SideOfField sideOfField;
                    SideOfFieldCtrl.getSideOfField(sideOfFieldID, out sideOfField);
                    GameObject goalkeeperGObj = sideOfField.goalComponents.goalkeeper;
                    PublicPlayerData publicPlayerData = goalkeeperGObj.GetComponent<PublicPlayerData>();
                    publicPlayerData.playerID = playerID;
                }
            }
        }
        //endProcess.Invoke();
    }
    public static object[] getData()
    {
        DebugsList.RPCRequestFieldPosition.print("Get Teams Data", debugColor, ComponentsPlayer.myMonoPlayerID);
        List<object> teamsData = new List<object>();
        foreach (var item in Teams.teamsList)
        {
            teamsData.Add(getTeamData(item));
        }
        return teamsData.ToArray();
    }
    static object[] getTeamData(Team team)
    {
        
        List<object> teamData = new List<object>();
        teamData.Add(team.TeamName);
        DebugsList.RPCRequestFieldPosition.print("getTeamData TeamName=" + team.TeamName, debugColor, ComponentsPlayer.myMonoPlayerID);
        object[] fieldPositions = new object[team.players.Count];
        int i = 0;
        teamData.Add(team.choosedLineup.typeLineup.ToString());
        SideOfFieldID sideOfFieldID;
        SideOfFieldCtrl.getSideOfFieldIDOfTeam(team.TeamName, out sideOfFieldID);
        teamData.Add(sideOfFieldID.ToString());
        foreach (var item in team.players)
        {
            List<object> fieldPosition = new List<object>();
            fieldPosition.Add(item);
            TypeFieldPosition.Type typeFieldPosition;
            team.getTypeFieldPositionOfPlayer(item, out typeFieldPosition);
            fieldPosition.Add(typeFieldPosition.ToString());
            DebugsList.RPCRequestFieldPosition.print("PlayerID=" + item + " | " + "FieldPosition=" + typeFieldPosition.ToString(), debugColor, ComponentsPlayer.myMonoPlayerID);
            fieldPositions[i] = fieldPosition.ToArray();
            i++;
        }
        teamData.Add(fieldPositions);
        return teamData.ToArray();
    }
}
