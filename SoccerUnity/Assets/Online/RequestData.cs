using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum MyTypeReliability
{
    Reliable,Unreliable
}
public class RequestData : MonoBehaviourPunCallbacks,IOnEventCallback
{
    string info;
    int idMessage;
    bool isSetuped;
    bool sendToReceiverGroup;
    PlayerIDMonoBehaviour myPlayerID;
    List<PlayerID> dataSenderPlayerIDs = new List<PlayerID>();
    int[] localActorDataReceivers, localActorDataSenders;
    byte requestCode,receiveCode;
    ReceiverGroup receiverGroup;
    MyTypeReliability typeReliability;
    SendOptions sendOptions;
    EventCaching eventCaching;
    List<int> receivedMessages = new List<int>();
    int tryCount;
    public MyDebug myDebug;
    public UnityEvent errorEvent;
    public void setup(List<PlayerID> dataSenderPlayerIDs, int[] localActorDataSenders, PlayerIDMonoBehaviour myPlayerID, int[] localActorDataReceivers, byte requestCode,byte receiveCode, ReceiverGroup receiverGroup,bool sendToReceiverGroup, MyTypeReliability typeReliability, SendOptions sendOptions, EventCaching eventCaching, int tryCount, string info)
    {
        this.dataSenderPlayerIDs = dataSenderPlayerIDs;
        this.myPlayerID = myPlayerID;
        this.requestCode = requestCode;
        this.info = info;
        this.receiverGroup = receiverGroup;
        this.receiveCode = receiveCode;
        this.sendToReceiverGroup = sendToReceiverGroup;
        this.typeReliability = typeReliability;
        this.sendOptions = sendOptions;
        this.eventCaching = eventCaching;
        this.localActorDataReceivers = localActorDataReceivers;
        this.localActorDataSenders = localActorDataSenders;
        this.tryCount = tryCount;
        isSetuped = true;
    }
    public void StartProcess()
    {
        StartCoroutine(SendRequest());
    }
    IEnumerator SendRequest()
    {
        yield return new WaitUntil(() => isSetuped);
        idMessage++;
        int thisIdMessage = idMessage;
        //ping=tiempo de ida y vuelta
        float waitTime = PhotonNetwork.GetPing() * 4;
        waitTime /= 1000;
        bool messageSended = false;
        if (!receivedMessages.Contains(thisIdMessage))
        {
            int count = 0;
            do
            {
                count ++;
                List<string> dataSendersId = new List<string>();
                List<int> dataSendersOnlineActor = new List<int>();
                foreach (var item in dataSenderPlayerIDs)
                {
                    dataSendersId.Add(item.getStringID());
                    dataSendersOnlineActor.Add(item.onlineActor);
                }
                string dataSender;
                if (sendToReceiverGroup)
                {
                    dataSender = receiverGroup.ToString();
                }
                else
                {
                    dataSender = PlayerID.getMultipleIDsToPrint(dataSendersId.ToArray());
                }
                if (messageSended)
                {
                    myDebug.print("Resend Request " + idMessage + " to " + dataSender + " | " + info, MyColor.Red, myPlayerID.playerID.getIDToPrint());
                }
                else
                {
                    myDebug.print("Send Request " + idMessage + " to " + dataSender + " | " + info, MyColor.White, myPlayerID.playerID.getIDToPrint());
                }

                object[] content = new object[] { myPlayerID.playerID.getStringID(), sendToReceiverGroup, localActorDataSenders, localActorDataReceivers, idMessage };
                int[] targetActors = dataSendersOnlineActor.ToArray();
                RaiseEventOptions raiseEventOptions;
                if (sendToReceiverGroup)
                {
                    raiseEventOptions = new RaiseEventOptions { Receivers = receiverGroup, CachingOption = eventCaching };
                }
                else
                {
                    raiseEventOptions = new RaiseEventOptions { TargetActors = targetActors, CachingOption = eventCaching };
                }
                PhotonNetwork.RaiseEvent(requestCode, content, raiseEventOptions, sendOptions);
                messageSended = true;
                yield return new WaitForSeconds(waitTime);
            } while (!receivedMessages.Contains(thisIdMessage) && typeReliability == MyTypeReliability.Reliable && count < tryCount);
            if (count == tryCount)
            {
                errorEvent.Invoke();
            }
        }
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
            object[] data = (object[])photonEvent.CustomData;
            string idRequester = (string)data[0];
            int[] localActorDataReceivers = (int[])data[1];
            string dataSenderID = (string)data[2];
            int idMessage = (int)data[3];
            if (idRequester.Equals(PlayerID.None) && (localActorDataReceivers.Contains(myPlayerID.playerID.localActor) || localActorDataReceivers.Length == 0))
            {
                if (!receivedMessages.Contains(idMessage))
                {
                    receivedMessages.Add(idMessage);
                    myDebug.print("Receive unrequest Data (RequestData)| " + "Data Sender=" + PlayerID.getIDToPrint(dataSenderID) + " | Client=" + PlayerID.getIDToPrint(idRequester) + " | Message " + idMessage + " | " + info, MyColor.White, myPlayerID.playerID.getIDToPrint());
                }
                else
                {
                    myDebug.print("Receive unrequest data duplicated (RequestData) | idMessage " + idMessage + " | " + "Data Sender=" + PlayerID.getIDToPrint(dataSenderID) + " | Client=" + PlayerID.getIDToPrint(idRequester) + " | " + info, MyColor.Red, myPlayerID.playerID.getIDToPrint());
                }
                
            }
            else if (!localActorDataReceivers.Contains(myPlayerID.playerID.localActor) || localActorDataReceivers.Length == 0)
            {
                //myDebug.print("Receive unsolicited data (RequestData)" + " | Data Sender=" + PlayerID.getIDToPrint(dataSenderID) + " | Client=" + PlayerID.getIDToPrint(idRequester) + " | idMessage " + idMessage + " | " + info, MyColor.Yellow, myPlayerID.playerID.getIDToPrint());
            }
            else if (!receivedMessages.Contains(idMessage))
            {
                receivedMessages.Add(idMessage);
                myDebug.print("Receive Data (RequestData)| " + "Data Sender=" + PlayerID.getIDToPrint(dataSenderID) + " | Client=" + PlayerID.getIDToPrint(idRequester) + " | Message " + idMessage + " | " + info, MyColor.White, myPlayerID.playerID.getIDToPrint());
            }
            else
            {
                myDebug.print("Receive data duplicated (RequestData) | idMessage " + idMessage + " | " + "Data Sender=" + PlayerID.getIDToPrint(dataSenderID) + " | Client=" + PlayerID.getIDToPrint(idRequester) + " | " + info, MyColor.Red, myPlayerID.playerID.getIDToPrint());
            }
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (isSetuped)
        {
            List<PlayerID> listForRemove = new List<PlayerID>();
            foreach (var item in dataSenderPlayerIDs)
            {
                if (item.onlineActor == otherPlayer.ActorNumber)
                {
                    listForRemove.Add(item);
                }
            }
            foreach (var item in listForRemove)
            {
                dataSenderPlayerIDs.Remove(item);
            }
            if (sendToReceiverGroup)
            {
                if (receiverGroup == ReceiverGroup.Others && PhotonNetwork.CountOfPlayersInRooms == 1 && dataSenderPlayerIDs.Count == 0)
                {
                    myDebug.print("Stop 1 Request Data because onlineActor " + otherPlayer.ActorNumber + " left the game and targets is empty", MyColor.Red);
                    StopAllCoroutines();
                }
            }
            else if (dataSenderPlayerIDs.Count == 0)
            {
                myDebug.print("Stop 2 Request Data because onlineActor " + otherPlayer.ActorNumber + " left the game and targets is empty", MyColor.Red);
                StopAllCoroutines();
            }
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
