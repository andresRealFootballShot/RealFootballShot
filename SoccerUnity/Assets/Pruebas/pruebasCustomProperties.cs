using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class pruebasCustomProperties : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void OnConnectedToMaster()
    {
        
        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        if (!customProperties.ContainsKey("Name"))
        {
            customProperties["Name"] = "Pepito";
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
        }
        else
        {
            print("yeah " + customProperties["Name"]);
        }
    }
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
            print(customProperties["Name"] as string);
            setCustomProperties();
        }
        else
        {
            ExitGames.Client.Photon.Hashtable roomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            foreach (var item in roomCustomProperties["teams"] as string[])
            {
                print(item);
            }
        }
    }
    void setCustomProperties()
    {
        ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
        hashtable.Add("teams", new string[] { "Blue", "Red" });
        PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("teams"))
        {
            foreach (var item in propertiesThatChanged["teams"] as string[])
            {
                print(item);
            }
        }
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
    }
}
