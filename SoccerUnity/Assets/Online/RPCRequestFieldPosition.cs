using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RPCRequestFieldPosition : MonoBehaviourPunCallbacks,IRequestFieldPosition
{
    string myPlayerID,teamName,typeFieldPosition;
    int maxAttemps = 10;
    float periodSendRequet = 0.5f;
    int currentMessageID;
    Dictionary<int, bool> messages = new Dictionary<int, bool>();
    void Start()
    {
        if (OnlineSettings.positionRequestCommunicationType == TypeOfCommunication.RPC)
        {
            MatchComponents.requestFieldPosition = this;
            myPlayerID = ComponentsPlayer.myMonoPlayerID.getStringID();
        }
    }
    public void RequestFieldPosition(string playerID, string teamName, string typeFieldPosition)
    {
        DebugsList.RPCRequestFieldPosition.print("RequestFieldPosition");
        myPlayerID = playerID;
        this.teamName = teamName;
        this.typeFieldPosition = typeFieldPosition;
        StartCoroutine(RequestFieldPositionCoroutine());
    }
    IEnumerator RequestFieldPositionCoroutine()
    {
        int countAttemps = 0;
        currentMessageID++;
        messages.Add(currentMessageID, false);
        bool messageSended=false;
        while (countAttemps < maxAttemps && !messages[currentMessageID])
        {
            if (messageSended)
            {
                DebugsList.RPCRequestFieldPosition.print("Resend request field position | messageID = " + currentMessageID + " | " + "player requester =" + myPlayerID + " | " + "team name=" + teamName + " | " + " field position=" + typeFieldPosition, MyColor.Red, myPlayerID);
            }
            else
            {
                DebugsList.RPCRequestFieldPosition.print("Send request field position | messageID = " + currentMessageID + " | " + "player requester =" + myPlayerID + " | " + "team name=" + teamName + " | " + " field position=" + typeFieldPosition, MyColor.White, myPlayerID);
            }
            photonView.RPC(nameof(RequestFieldPosition), RpcTarget.MasterClient, currentMessageID, myPlayerID, teamName, typeFieldPosition);
            messageSended = true;
            yield return new WaitForSeconds(periodSendRequet);
        }
        if (countAttemps == maxAttemps)
        {
            OnlineErrorHandler.OnlineError("Request field position");
        }
    }
    [PunRPC]
    void RequestFieldPosition(int messageID,string playerID,string teamName,string typeFieldPositionString, PhotonMessageInfo info)
    {
        Team team = Teams.getTeamByName(teamName);
        TypeFieldPosition.Type typeFieldPosition = (TypeFieldPosition.Type) System.Enum.Parse(typeof(TypeFieldPosition.Type), typeFieldPositionString);
        if (team.fieldPositionIsAvailable(typeFieldPosition))
        {
            string playerIDTeamFieldPosition = team.fieldPositionOfPlayers[typeFieldPosition];
            //MyFunctions.GetKeyByValue(team.fieldPositionOfPlayers, typeFieldPosition, out playerIDTeamFieldPosition);
            if (!playerIDTeamFieldPosition.Equals(playerID) && team.players.Contains(playerID))
            {
                DebugsList.RPCRequestFieldPosition.print("Receive request and the field position is already taken , send deny | messageID = " + messageID + " | " + "player requester =" + PlayerID.getIDToPrint(playerID) + " | " + "team name=" + teamName + " | " + " field position=" + typeFieldPositionString, MyColor.Red, PlayerID.getIDToPrint(myPlayerID));
                photonView.RPC(nameof(DenyFieldPosition), info.Sender, messageID, playerID);
            }
            else
            {
                DebugsList.RPCRequestFieldPosition.print("Receive request and send accept | messageID = " + messageID + " | " + "player requester =" + PlayerID.getIDToPrint(playerID) + " | " + "team name=" + teamName + " | " + " field position=" + typeFieldPositionString, MyColor.White, PlayerID.getIDToPrint(myPlayerID));
                //team.addPlayer(new PlayerID(playerID), typeFieldPositionString);
                photonView.RPC(nameof(AcceptFieldPosition), RpcTarget.All, messageID, playerID, teamName, typeFieldPositionString);
            }
        }
        else
        {
            DebugsList.RPCRequestFieldPosition.print("Receive request and send accept | messageID = " + messageID + " | " + "player requester =" + PlayerID.getIDToPrint(playerID) + " | " + "team name=" + teamName + " | " + " field position=" + typeFieldPositionString, MyColor.White, PlayerID.getIDToPrint(myPlayerID));
            //team.addPlayer(new PlayerID(playerID), typeFieldPositionString);
            photonView.RPC(nameof(AcceptFieldPosition), RpcTarget.All, messageID, playerID, teamName, typeFieldPositionString);
        }
    }
    [PunRPC]
    void AcceptFieldPosition(int messageID,string playerID,string teamName, string typeFieldPositionString)
    {
        Teams.AddPlayerToTeam(teamName, playerID, typeFieldPositionString);
        //FieldPositionsCtrl.addFieldPositionOfPlayer(playerID, typeFieldPositionString);
        if (playerID != null && myPlayerID.Equals(playerID))
        {
            DebugsList.RPCRequestFieldPosition.print("Receive accept | messageID = " + messageID + " | " + "player requester =" + PlayerID.getIDToPrint(playerID) + " | " + "team name=" + teamName + " | " + " field position=" + typeFieldPositionString, MyColor.White, PlayerID.getIDToPrint(myPlayerID));
            //Si lo recibe el que envió la solicitud lo anotará en los messages
            if (messages.ContainsKey(messageID))
            {
                if (!messages[messageID])
                {
                    messages[messageID] = true;
                    MatchEvents.requestedFieldPositionWasAccepted?.Invoke();
                }
            }
        }
    }
    [PunRPC]
    void DenyFieldPosition(int messageID,string playerID)
    {
        if (myPlayerID.Equals(playerID))
        {
            DebugsList.RPCRequestFieldPosition.print("Receive deny | messageID = " + messageID + " | " + "player requester =" + PlayerID.getIDToPrint(playerID), MyColor.Red, PlayerID.getIDToPrint(myPlayerID));
            if (messages.ContainsKey(messageID))
            {
                if (!messages[messageID])
                {
                    messages[messageID] = true;
                    MatchEvents.requestedFieldPositionWasDenied?.Invoke();
                }
            }
        }
    }
}
