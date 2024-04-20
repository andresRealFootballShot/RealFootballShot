using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
public class RoomListing : MonoBehaviourPunCallbacks
{
    public Transform content;
    public GameObject roomPrefab;
    public Transform roomsParent;
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        print("eo");
        int size = roomsParent.childCount;
        for (int i = 0; i < size; i++)
        {
            Destroy(roomsParent.GetChild(0).gameObject);
        }
        foreach(RoomInfo roomInfo in roomList)
        {
            if (roomInfo.IsVisible && roomInfo.MaxPlayers!=0)
            {
               GameObject room = Instantiate(roomPrefab, content);
                RoomButton roomButton = room.GetComponent<RoomButton>();
                roomButton.roomInfo = roomInfo;
                roomButton.setText();
                if (!roomInfo.IsOpen)
                {
                    
                    room.GetComponent<Button>().interactable = false;
                }
            }
        }
    }
    public override void OnLeftLobby()
    {
        print("OnLeftLobby");
        TypedLobby sqlLobby = new TypedLobby("myLobby", LobbyType.SqlLobby);
        PhotonNetwork.JoinLobby();
    }
    public override void OnEnable()
    {
        TypedLobby sqlLobby = new TypedLobby("myLobby", LobbyType.SqlLobby);
        //PhotonNetwork.JoinLobby();
    }
}
