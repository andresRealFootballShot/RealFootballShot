using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
//Importante usar System.Linq
using System.Linq;
public class OtherPlayerLeftRoom : MonoBehaviourPunCallbacks
{
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
       
        List<string> allPlayerIDs = Teams.getPlayersOfAllTeams();
        foreach (var playerID in allPlayerIDs)
        {
            if(PlayerID.getOnlineActor(playerID) == otherPlayer.ActorNumber)
            {
                DebugsList.testing.print("OtherPlayerLeftRoom.OnPlayerLeftRoom()", Color.red);

                MatchEvents.otherPlayerLeftRoom.Invoke(playerID);
            }
        }
        LeaveRoom.leaveRoom();
        return;
    }
}
