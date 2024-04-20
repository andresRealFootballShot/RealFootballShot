using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MatchDataObsolete : MonoBehaviourPunCallbacks
{
    
    public SortedDictionary<int, PhotonView> pVBodyes = new SortedDictionary<int, PhotonView>();
    public SortedDictionary<int, PlayerModel> players = new SortedDictionary<int, PlayerModel>();
    public bool chooseTeam=true;
    public float startMatch;
    public bool matchStarted;
    public bool endMatch;
    public bool enableGoals;
    public bool saqueDeBanda;
    public bool saqueCorner;
    public int part;
    public int actorWithPosession;
    public int timeStampPosession=-1;
    public bool fueraDeJuego;
    PhotonView pVPlayer;
    void Awake()
    {
        Player[] list = PhotonNetwork.PlayerList;
        foreach(Player player in list)
        {
            players.Add(player.ActorNumber, new PlayerModel(player));
        }
    }
   
    public void SetPosession(int actor,int timeStamp)
    {
        actorWithPosession = actor;
        timeStampPosession = timeStamp;

    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
       
        players.Remove(otherPlayer.ActorNumber);
        if (pVBodyes.ContainsKey(otherPlayer.ActorNumber))
        {
            pVBodyes.Remove(otherPlayer.ActorNumber);
        }
        Teams teams = GameObject.FindGameObjectWithTag("Teams").GetComponent<Teams>();
        //teams.RemovePlayer(otherPlayer.ActorNumber);
        
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        players.Add(newPlayer.ActorNumber, new PlayerModel(newPlayer));
        
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        
    }
}
