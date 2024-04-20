using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class RPCSetupTeamsData : MonoBehaviourPunCallbacks, ISetupTeams
{
    string myPlayerID, teamName, typeFieldPosition;
    int maxAttemps = 10;
    float periodSendRequet = 0.5f;
    int currentMessageID;
    Dictionary<int, bool> messages = new Dictionary<int, bool>();
    public Color debugColor;
    bool teamsDataIsSetuped;
    void Start()
    {
        if (OnlineSettings.positionRequestCommunicationType == TypeOfCommunication.RPC)
        {
            MatchComponents.setupTeams = this;
            myPlayerID = ComponentsPlayer.myMonoPlayerID.getStringID();
            SetupTeams();
        }
    }
    public void SetupTeams()
    {
        if (PhotonNetwork.IsMasterClient && !teamsDataIsSetuped)
        {
            DebugsList.RPCRequestFieldPosition.print("RPCSetupTeamsData.SetupTeams Im MasterClient", MyColor.Red);
            SideOfFieldCtrl.SetRandomSideOfField(Teams.getTeamNames());
            Teams.setLineupToAllTeams(Lineup.TypeLineup.Default);
            teamsDataIsSetuped = true;
            InstantiateLocalGoalkeepers.StartProcess();
            //addGoalkeepers();
            MatchEvents.teamsSetuped.Invoke();
        }
        else
        {
            StartCoroutine(RequestTeamsDataCoroutine());
        }
    }
    /*
    void addGoalkeepers()
    {
       GameObject[] goalkeepers = GameObject.FindGameObjectsWithTag(Tags.GoalKeeper);
        foreach (var goalkeeper in goalkeepers)
        {
            GoalkeeperComponents goalkeeperComponents = goalkeeper.GetComponent<GoalkeeperComponents>();
            GoalkeeperCtrl goalkeeperCtrl = goalkeeperComponents.goalkeeperCtrl;
            SideOfFieldID sideOfFieldID = goalkeeperCtrl.sideOfField.Value;
            string teamName;
            SideOfFieldCtrl.getTeamOfSideOfField(sideOfFieldID, out teamName);
            goalkeeperComponents.addPlayerToTeam.AddToTeam(teamName, TypeFieldPosition.Type.GoalKeeper);
        }
    }*/
    IEnumerator RequestTeamsDataCoroutine()
    {
        int countAttemps = 0;
        currentMessageID++;
        messages.Add(currentMessageID, false);
        bool messageSended = false;
        while (countAttemps < maxAttemps && !messages[currentMessageID])
        {
            if (messageSended)
            {
                DebugsList.RPCRequestFieldPosition.print("Resend resquest TeamsData | messageID = " + currentMessageID + " | " + "player requester =" + myPlayerID, MyColor.Red, myPlayerID);
            }
            else
            {
                DebugsList.RPCRequestFieldPosition.print("Send request TeamsData | messageID = " + currentMessageID + " | " + "player requester =" + myPlayerID , debugColor, myPlayerID);
            }
            photonView.RPC(nameof(RequestTeamsData), RpcTarget.MasterClient, currentMessageID);
            messageSended = true;
            yield return new WaitForSeconds(periodSendRequet);
        }
        if (countAttemps == maxAttemps)
        {
            OnlineErrorHandler.OnlineError("Request field position");
        }
    }
    [PunRPC]
    void RequestTeamsData(int messageID, PhotonMessageInfo info)
    {
        StartCoroutine(waitTeamsDataIsSetuped(messageID, info));
    }
    IEnumerator waitTeamsDataIsSetuped(int messageID, PhotonMessageInfo info)
    {
        yield return new WaitUntil(() => teamsDataIsSetuped);
        sendTeamsData(messageID, info);
    }
    void sendTeamsData(int messageID, PhotonMessageInfo info)
    {
        DebugsList.RPCRequestFieldPosition.print("Receive RequestTeamsData | messageID = " + messageID, debugColor, PlayerID.getIDToPrint(myPlayerID));
        object[] teamsData = TeamsDataCtrl.getData();
        photonView.RPC(nameof(ReceiveTeamsData), info.Sender, messageID, teamsData as object);
    }
    [PunRPC]
    void ReceiveTeamsData(int messageID, object[] teamsData)
    {
        DebugsList.RPCRequestFieldPosition.print("ReceiveTeamsData | messageID = " + messageID, debugColor, PlayerID.getIDToPrint(myPlayerID));
        //Si lo recibe el que envió la solicitud lo anotará en los messages
        if (messages.ContainsKey(messageID))
        {
            if (!messages[messageID])
            {
                messages[messageID] = true;
                TeamsDataCtrl.deliverData(teamsData);
                teamsDataIsSetuped = true;
                InstantiateLocalGoalkeepers.StartProcess();
                MatchEvents.fieldPositionsChanged.Invoke();
                MatchEvents.teamsSetuped.Invoke();
            }
        }
    }
}
