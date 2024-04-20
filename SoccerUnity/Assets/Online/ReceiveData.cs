using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveData : MonoBehaviourPunCallbacks, IOnEventCallback
{
    List<string> receivedMessages = new List<string>();
    string info;
    bool isSetuped;
    PlayerIDMonoBehaviour myPlayerID;
    IDeliverData deliverData;
    byte receiveCode, confirmReception;
    MyTypeReliability typeReliability;
    SendOptions sendOptions;
    EventCaching eventCaching;
    public MyDebug myDebug;
    public void setup(IDeliverData deliverData, PlayerIDMonoBehaviour myPlayerID, byte receiveCode, byte confirmReception, MyTypeReliability typeReliability, SendOptions sendOptions, EventCaching eventCaching, string info)
    {
        this.deliverData = deliverData;
        this.myPlayerID = myPlayerID;
        this.receiveCode = receiveCode;
        this.info = info;
        this.typeReliability = typeReliability;
        this.sendOptions = sendOptions;
        this.confirmReception = confirmReception;
        this.eventCaching = eventCaching;
        isSetuped = true;
    }
    public void OnEvent(EventData photonEvent)
    {
        StartCoroutine(OnEventCoroutine(photonEvent));
    }
    IEnumerator OnEventCoroutine(EventData photonEvent)
    {
        yield return new WaitUntil(() => isSetuped);
        byte eventCode = photonEvent.Code;
        
        if (eventCode == receiveCode)
        {
            receiveData(photonEvent);
        }
    }
    void receiveData(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;
        string idRequester = (string)data[0];
        int[] localActorDataReceivers = (int[])data[1];
        string dataSenderID = (string)data[2];
        int idMessage = (int)data[3];
        if (idRequester.Equals(PlayerID.None) && (localActorDataReceivers.Contains(myPlayerID.playerID.localActor) || localActorDataReceivers.Length == 0))
        {
            if (!receivedMessages.Contains(dataSenderID+"-"+ idMessage))
            {
                receivedMessages.Add(dataSenderID + "-" + idMessage);
                myDebug.print("Receive unrequest Data | " + "Data Sender=" + PlayerID.getIDToPrint(dataSenderID) + " | Client=" + PlayerID.getIDToPrint(idRequester) + " | Message " + idMessage + " | " + info, MyColor.White, myPlayerID.playerID.getIDToPrint());
                object[] teamsData = (object[])data[4];
                deliverData.deliverData(teamsData);
                confirmMessage(photonEvent);
            }
            else
            {
                myDebug.print("Receive unrequest data duplicated | idMessage " + idMessage + " | " + "Data Sender=" + PlayerID.getIDToPrint(dataSenderID) + " | Client=" + PlayerID.getIDToPrint(idRequester) + " | " + info, MyColor.Red, myPlayerID.playerID.getIDToPrint());
                confirmMessage(photonEvent);
            }
        }
        else if (!localActorDataReceivers.Contains(myPlayerID.playerID.localActor) || localActorDataReceivers.Length == 0)
        {
            //MyDebug.print("Receive unsolicited data " + " | Data Sender=" + PlayerID.getIDToPrint(dataSenderID) + " | Client=" + PlayerID.getIDToPrint(idRequester) + " | idMessage " + idMessage + " | " + info, MyColor.Yellow, myPlayerID.playerID.getIDToPrint());
        }else if (!receivedMessages.Contains(dataSenderID + "-" + idMessage))
        {
            receivedMessages.Add(dataSenderID + "-" + idMessage);
            myDebug.print("Receive Data | " + "Data Sender=" + PlayerID.getIDToPrint(dataSenderID) + " | Client=" + PlayerID.getIDToPrint(idRequester) + " | Message " + idMessage + " | " + info, MyColor.White, myPlayerID.playerID.getIDToPrint());
            object[] teamsData = (object[])data[4];
            deliverData.deliverData(teamsData);
            confirmMessage(photonEvent);
        }
        else
        {
            myDebug.print("Receive data duplicated | idMessage " + idMessage + " | " + "Data Sender=" + PlayerID.getIDToPrint(dataSenderID) + " | Client=" + PlayerID.getIDToPrint(idRequester) + " | " + info, MyColor.Red, myPlayerID.playerID.getIDToPrint());
            confirmMessage(photonEvent);
        }
    }
    void confirmMessage(EventData photonEvent)
    {
        switch (typeReliability)
        {
            case MyTypeReliability.Reliable:
                object[] data = (object[])photonEvent.CustomData;
                string requesterID = (string)data[0];
                int[] localActorDataReceivers = (int[])data[1];
                string dataSenderID = (string)data[2];
                int idMessage = (int)data[3];
                object[] content = new object[] { requesterID,myPlayerID.getStringID(), dataSenderID, idMessage};
                int[] targetActors = new int[] { photonEvent.Sender };
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { TargetActors = targetActors, CachingOption = eventCaching };
                PhotonNetwork.RaiseEvent(confirmReception, content, raiseEventOptions, sendOptions);
                myDebug.print("Send confirm received message "+" | idMessage " + idMessage+" | " + "Data Sender=" + PlayerID.getIDToPrint(dataSenderID) + " | Requester=" + PlayerID.getIDToPrint(requesterID) + " | Receiver=" + myPlayerID.getIDToPrint() + " | " + info, MyColor.White, myPlayerID.playerID.getIDToPrint());
                break;
            case MyTypeReliability.Unreliable:
                break;
        }
    }
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
