using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticConnect : MonoBehaviourPunCallbacks
{
    public GameObject typeMatchDataPref;
    public static TypeNormalMatch typeMatchName;
    public static bool publicMatch;
    public void StartProcess()
    {
        if (!PhotonNetwork.IsConnected)
        {
            DebugsList.testing.print("AutomaticConnect StartProcess");
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public override void OnConnectedToMaster()
    {
       FindRandomMatch();
       
        DebugsList.testing.print("OnConnectedToMaster");
    }
    public void FindRandomMatch()
    {
        RoomOptions roomOptions = new RoomOptions() { IsVisible = publicMatch, IsOpen = true, MaxPlayers = TypeMatch.maxPlayersDictionary[typeMatchName] };
        TypeMatch.setup(typeMatchName.ToString(),true);
        PhotonNetwork.JoinRandomRoom(null, TypeMatch.maxPlayersDictionary[typeMatchName]);
        //PhotonNetwork.JoinOrCreateRoom("prueba", roomOptions, TypedLobby.Default, null);
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //Debug.LogError("Join random room failed");
        DebugsList.testing.print("OnJoinRandomFailed");
        CreateRoom();
    }
    void CreateRoom()
    {
        int randomNumber = Random.Range(0, 1000);
        RoomOptions roomOptions = new RoomOptions() { IsVisible = publicMatch, IsOpen = true, MaxPlayers = TypeMatch.maxPlayersDictionary[typeMatchName] };

        PhotonNetwork.CreateRoom("Random Room " + randomNumber, roomOptions);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Create room failed | "+ returnCode +" | "+ message);
        CreateRoom();
    }
    public override void OnCreatedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            /*GameObject gameObject = PhotonNetwork.InstantiateSceneObject(typeMatchDataPref.name, Vector3.zero, Quaternion.identity, 0, null);
            DontDestroyOnLoad(gameObject);
            TypeMatch.SendTypeMatchData(typeMatchName.ToString(), publicMatch, TypeMatch.sizeFootballFieldDictionary[typeMatchName]);*/
            //PhotonNetwork.LoadLevel("Scenes/" + TypeMatch.getNameScene(typeMatch));
        }
    }
    
}
