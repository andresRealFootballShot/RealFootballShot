using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
public class RoomButton : MonoBehaviour
{
    public Text text;
    public RoomInfo roomInfo;
    void Start()
    {
        
    }
    public void Room_Click()
    {
        PhotonNetwork.JoinRoom(roomInfo.Name);
    }
    public void setText()
    {
        text.text = roomInfo.Name + " " + roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;
    }
}
