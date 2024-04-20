using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
public class FindRandomMatch : MonoBehaviourPunCallbacks
{
    public OnCreateRoom onCreateRoom;
    public static TypeNormalMatch typeNormalMatch = TypeNormalMatch.OneVSOne;
    
    private void Awake()
    {
        if (SceneSetup.staticTypeMatch == TypeMatchID.Playtime)
        {

        }
    }
    public void find()
    {
        LoadScene.notifyClearBeforeLoadScene();
        SceneSetup.setStaticSetup(TypeMatchID.NormalMatch, SceneModeID.Online, false, false);
        TypeMatch.setup(typeNormalMatch.ToString(), true);
        
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = TypeMatch.getMaxPlayers(typeNormalMatch.ToString()) };
        PhotonNetwork.JoinRandomRoom(null, TypeMatch.getMaxPlayers(typeNormalMatch.ToString()));
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }
    void CreateRoom()
    {
        int randomNumber = Random.Range(0, 1000);
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = TypeMatch.getMaxPlayers(typeNormalMatch.ToString()) };
        onCreateRoom.setTypeMatchData(typeNormalMatch.ToString(), true);
        PhotonNetwork.CreateRoom("Random Room " + randomNumber, roomOptions);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        OnlineEvents.createRoomFailed.Invoke();

    }

}
