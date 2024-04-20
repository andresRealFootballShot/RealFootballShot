using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GetPlayersTeams : MonoBehaviourPunCallbacks
{
    Teams teams;
    /*
    void Start()
    {
        teams = GameObject.FindGameObjectWithTag("Teams").GetComponent<Teams>();
        photonView.RPC("GetPlayersTeam", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
    }
    [PunRPC]
    public void SetPlayersTeam(int[]red,int[]blue)
    {
        teams.teamRed.addPlayers(red);
        teams.teamBlue.addPlayers(blue);
    }
    [PunRPC]
    public void GetPlayersTeam(int sender)
    {
        teams = GameObject.FindGameObjectWithTag("Teams").GetComponent<Teams>();
        int[] red = teams.GetPlayers("Red");
        int[] blue = teams.GetPlayers("Blue");
        photonView.RPC("SetPlayersTeam", RpcTarget.All, red,blue);
       
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == CodeEventsNet.GetPlayersInTeam)
        {
            object[] data = (object[])photonEvent.CustomData;

            teams.teamRed.addPlayers((int[])data[0]);
            teams.teamBlue.addPlayers((int[])data[1]);
        }
    }*/
}
