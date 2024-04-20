using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
public class CreateMatch : MonoBehaviourPunCallbacks
{
    string nameMatchCreate;
    public Button createButton;
    public InputField nameMatch;
    public FindRandomMatch findRandomMatch;
    public OnCreateRoom onCreateRoom;
    void Awake()
    {
        createButton.interactable = false;
    }
    public void SetNameMatch(string value)
    {
        
        if (value == " ")
        {
            
            nameMatch.text = "";
            nameMatch.caretPosition = 0;
            nameMatchCreate = "";
        }
        else if(value == "")
        {
            createButton.interactable = false;
            nameMatchCreate = "";
        }else
        {
            nameMatchCreate = value;
            if (onCreateRoom.typeMatchString!="")
            {
                createButton.interactable = true;
            }
        }
        
    }
    public void SetTypeMatch(string value)
    {
        onCreateRoom.setTypeMatchData(value,false);
        if (nameMatchCreate != null&&nameMatchCreate!="")
        {
            createButton.interactable = true;
        }
    }
    public void CreateMatch_Click()
    {
        findRandomMatch.enabled = false;
        RoomOptions newRoomOptions = new RoomOptions();
        newRoomOptions.IsVisible = false;
        newRoomOptions.IsOpen = true;
        newRoomOptions.MaxPlayers = TypeMatch.getMaxPlayers(onCreateRoom.typeMatchString);
        TypedLobby sqlLobby = new TypedLobby("myLobby", LobbyType.SqlLobby);
        PhotonNetwork.CreateRoom(nameMatchCreate,newRoomOptions);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        findRandomMatch.enabled = true;
    }
}
