using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldPositionCustomPropertiesCtrl : MonoBehaviourPunCallbacks,IRequestFieldPosition, ILoad
{
    bool fieldPositionIsRequested;
    public static int staticLoadLevel = MatchEvents.staticLoadLevel + 1;
    public int loadLevel { get => staticLoadLevel; set => staticLoadLevel = value; }
    public void Load(int level)
    {
        if (level == loadLevel)
        {
            MatchEvents.fieldPositionsChanged.AddListener(removePlayerOfFieldPosition);
        }
    }
    void removePlayerOfFieldPosition(FieldPositionEventArgs args)
    {
        if (args.playerID.Equals("None"))
        {
            DebugsList.testing.print("removePlayerOfFieldPosition",Color.green);
            ExitGames.Client.Photon.Hashtable customProperties = RoomCustomPropertiesCtrl.createFieldPosition(args.teamName, args.typeFieldPosition, args.playerID);
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        }
    }
    public void RequestFieldPosition(string playerID, string teamName, string typeFieldPosition)
    {
        if (!fieldPositionIsRequested)
        {
            fieldPositionIsRequested = true;
            DebugsList.testing.print("RequestFieldPosition");
            ExitGames.Client.Photon.Hashtable expectedCustomProperties = RoomCustomPropertiesCtrl.createFieldPosition(teamName, typeFieldPosition, "None");
            ExitGames.Client.Photon.Hashtable customProperties = RoomCustomPropertiesCtrl.createFieldPosition(teamName, typeFieldPosition, playerID);
            //DebugsList.testing.print("expectedCustomProperties");
            RoomCustomPropertiesCtrl.printFieldPosition(expectedCustomProperties, "expectedCustomProperties");
            //DebugsList.testing.print("customProperties");
            RoomCustomPropertiesCtrl.printFieldPosition(customProperties, "customProperties");
            
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties, expectedCustomProperties);
        }
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        DebugsList.RPCRequestFieldPosition.print("OnRoomPropertiesUpdate");
        //RoomCustomPropertiesCtrl.printCustomProperties();

        
        bool isFieldPosition=false;
        bool isNone = false;
        DictionaryEntry propery;
        foreach (var item in propertiesThatChanged)
        {
            if (RoomCustomPropertiesCtrl.isFieldPosition(item))
            {
                isNone = item.Value.Equals("None");
                isFieldPosition = true;
            }
            propery = item;
        }
        
        if (isFieldPosition)
        {
            string teamName, typeFieldPosition;
            RoomCustomPropertiesCtrl.parseKeyFieldPosition(propery.Key as string, out teamName, out typeFieldPosition);
            DebugsList.RPCRequestFieldPosition.print("AddPlayerToTeam teamName="+ teamName+" | playerID="+ propery.Value as string+" | typeFieldPosition="+ typeFieldPosition);
            Teams.AddPlayerToTeam(teamName, propery.Value as string, typeFieldPosition);
            if (ChooseTeamCtrl.teamSelected != null)
            {
                string myKey = RoomCustomPropertiesCtrl.getFieldPositionKey(ChooseTeamCtrl.teamSelected.TeamName, ChooseFieldPositionCtrl.typeFieldSelected.ToString());
                if (propertiesThatChanged.ContainsKey(myKey))
                {
                    if (propertiesThatChanged[myKey].Equals(ComponentsPlayer.myMonoPlayerID.getStringID()))
                    {
                        DebugsList.RPCRequestFieldPosition.print("requestedFieldPositionWasAccepted");
                        MatchEvents.requestedFieldPositionWasAccepted.Invoke();
                    }
                    else
                    {
                        /*
                        print("requestedFieldPositionWasDenied" + " " + fieldPositionIsRequested);
                        MatchEvents.requestedFieldPositionWasDenied.Invoke();*/
                    }
                }
            }
        }
    }

    private void NetworkingClientOnOpResponseReceived(OperationResponse opResponse)
    {
        if (opResponse.OperationCode == OperationCode.SetProperties &&
            opResponse.ReturnCode == ErrorCode.InvalidOperation)
        {
            print("CAS failure");
            fieldPositionIsRequested = false;
            MatchEvents.requestedFieldPositionWasDenied.Invoke();
        }
    }
    private void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.OpResponseReceived += NetworkingClientOnOpResponseReceived;
    }

    private void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.NetworkingClient.OpResponseReceived -= NetworkingClientOnOpResponseReceived;
    }
}
