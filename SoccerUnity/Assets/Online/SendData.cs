using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SendData : MonoBehaviourPunCallbacks, IOnEventCallback
{
    Dictionary<string, List<int>> requestedMessages = new Dictionary<string, List<int>>();
    List<string> confirmedMessages = new List<string>();
    string info;
    float time;
    bool isSetuped;
    PlayerIDMonoBehaviour myPlayerID;
    IGetData getData;
    SendOptions sendOptions;
    EventCaching eventCaching;
    byte requestCode, receiveCode,confirmReception;
    int tryCount;
    MyTypeReliability typeReliability;
    public MyDebug myDebug;
    public UnityEvent errorEvent;
    public void setup(IGetData getData, PlayerIDMonoBehaviour myPlayerID, byte requestCode,byte receiveCode,byte confirmReception, MyTypeReliability typeReliability, SendOptions sendOptions, EventCaching eventCaching, int tryCount, string info)
    {
        this.getData = getData;
        this.myPlayerID = myPlayerID;
        this.requestCode = requestCode;
        this.receiveCode = receiveCode;
        this.confirmReception = confirmReception;
        this.info = info;
        this.typeReliability = typeReliability;
        this.sendOptions = sendOptions;
        this.eventCaching = eventCaching;
        this.tryCount = tryCount;
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
        if (eventCode == requestCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            string idRequester = (string)data[0];
            bool sendToReceiverGroup = (bool)data[1];
            int[] localActorDataSenders = (int[])data[2];
            int[] localActorDataReceivers = (int[])data[3];
            int idMessage = (int)data[4];
            if (localActorDataSenders.Contains(myPlayerID.playerID.localActor) || localActorDataSenders.Length == 0)
            {
                //Es el onlineActor y localActor del targetID
                if (requestedMessages.ContainsKey(idRequester))
                {
                    if (requestedMessages[idRequester].Contains(idMessage))
                    {
                        //Ya ha envíado sendTeamsInfo anteriormente
                        float waitTime = PhotonNetwork.GetPing() * 4;
                        waitTime /= 1000;
                        if (Time.realtimeSinceStartup - time > waitTime)
                        {
                            //Si ha pasado mucho tiempo desde que envió el ultimo mensaje, lo reenvía
                            //myDebug.print("Resend data " + " | " + " idMessage " + idMessage + " | " + "Data Sender=" + myPlayerID.getIDToPrint() + " | Client=" + PlayerID.getIDToPrint(idRequester) + " | " + info, MyColor.Red, myPlayerID.playerID.getIDToPrint());
                            sendRequestedData(photonEvent);
                        }
                        else
                        {
                            //Si ha pasado poco tiempo desde que envió el ultimo mensaje, no lo reenvía
                            myDebug.print("Request duplicated,waitTime is small | " + " idMessage " + idMessage + " | " + "Data Sender=" + myPlayerID.getIDToPrint() + " | Client=" + PlayerID.getIDToPrint(idRequester) + " | " + info, MyColor.Red, myPlayerID.playerID.getIDToPrint());
                        }
                    }
                    else
                    {
                        //MyDebug.print("Contains actor " + idSenderActor + " - but not message " + idMessage, Color.red, myPlayerID.playerID.localActor);
                        requestedMessages[idRequester].Add(idMessage);
                        time = Time.realtimeSinceStartup;
                        sendRequestedData(photonEvent);
                    }
                }
                else
                {
                    //MyDebug.print("Not contains actor " + idSenderActor, Color.red, myPlayerID.playerID.localActor);
                    requestedMessages.Add(idRequester, new List<int>() { idMessage });
                    sendRequestedData(photonEvent);
                }
            }
        }else if(eventCode == confirmReception)
        {
            object[] data = (object[])photonEvent.CustomData;
            string dataRequesterID = (string)data[0];
            string dataReceiverID= (string)data[1];
            string dataSenderID = (string)data[2];
            int idMessage = (int)data[3];
            if (myPlayerID.playerID.Equals(dataSenderID))
            {
                if (!confirmedMessages.Contains(dataRequesterID+"-"+idMessage))
                {
                    confirmedMessages.Add(dataRequesterID + "-" + idMessage);
                    myDebug.print("Receive confirmation of idMessage " + idMessage + " of " + PlayerID.getIDToPrint(dataReceiverID) +" | " + info, MyColor.White, myPlayerID.playerID.getIDToPrint());
                }
                else
                {
                    myDebug.print("Receive duplicated confirmation of idMessage " + idMessage + " of " + PlayerID.getIDToPrint(dataReceiverID) + " | " + info, MyColor.Red, myPlayerID.playerID.getIDToPrint());
                }
            }
            else
            {
                //MyDebug.print("Receive unsolicited confirmation of idMessage " + idMessage + " | " + info, MyColor.Yellow, myPlayerID.playerID.getIDToPrint());
            }
        }
    }
    IEnumerator sendDataCoroutine(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;
        string idRequester = (string)data[0];
        bool sendToReceiverGroup = (bool)data[1];
        int[] localActorDataSender = (int[])data[2];
        int[] localActorDataReceivers= (int[])data[3];
        int idMessage = (int)data[4];
        object[] content = new object[] { idRequester, localActorDataReceivers, myPlayerID.getStringID(), idMessage, getData.getData() };
        //object[] content = new object[] { idRequester, myPlayerID.getStringID(), idMessage};
        int[] targetActors = new int[] { photonEvent.Sender };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { TargetActors = targetActors, CachingOption = eventCaching };
        float waitTime = PhotonNetwork.GetPing() * 4;
        waitTime /= 1000;
        bool messageSended = false;
        if(!confirmedMessages.Contains(idRequester + "-" + idMessage))
        {
            int count = 0;
            do
            {
                count ++;
                if (!messageSended)
                {
                    myDebug.print("Send Requested Data | " + "Data Sender=" + myPlayerID.getIDToPrint() + " | Client=" + PlayerID.getIDToPrint(idRequester) + " | idMessage " + idMessage + " | " + info, MyColor.White, myPlayerID.playerID.getIDToPrint());
                }
                else
                {
                    myDebug.print("Resend Requested Data | " + "Data Sender=" + myPlayerID.getIDToPrint() + " | Client=" + PlayerID.getIDToPrint(idRequester) + " | idMessage " + idMessage + " | " + info, MyColor.Red, myPlayerID.playerID.getIDToPrint());
                }
                PhotonNetwork.RaiseEvent(receiveCode, content, raiseEventOptions, sendOptions);
                messageSended = true;
                yield return new WaitForSeconds(waitTime);
            } while (typeReliability == MyTypeReliability.Reliable && !confirmedMessages.Contains(idRequester + "-" + idMessage) && count < tryCount);
            if (count == tryCount)
            {
                errorEvent.Invoke();
            }
        }
    }
    IEnumerator sendDataCoroutine(string idRequester,List<PlayerID> receiverIDs, int[] localActorDataReceivers, ReceiverGroup receiverGroup, bool sendToReceiverGroup, int idMessage)
    {
        object[] content = new object[] { idRequester, localActorDataReceivers, myPlayerID.getStringID(),idMessage, getData.getData() };
        RaiseEventOptions raiseEventOptions;
        if (sendToReceiverGroup)
        {
            raiseEventOptions = new RaiseEventOptions { Receivers = receiverGroup, CachingOption = eventCaching };
        }
        else
        {
            raiseEventOptions = new RaiseEventOptions { TargetActors = PlayerID.getOnlineActorsOfMultipleIDs(receiverIDs), CachingOption = eventCaching };
        }
        float waitTime = PhotonNetwork.GetPing() * 4;
        waitTime /= 1000;
        bool messageSended = false;
        string requester = idRequester == PlayerID.None ? "Unrequested" : "Requested";
        if(!confirmedMessages.Contains(idRequester + "-" + idMessage))
        {
            int count = 0;
            do
            {
                count++;
                if (!messageSended)
                {
                    myDebug.print("Send " + requester + " Data | " + "Data Sender=" + myPlayerID.getIDToPrint() + " | Client=" + PlayerID.getIDToPrint(idRequester) + " | idMessage " + idMessage + " | " + info, MyColor.White, myPlayerID.playerID.getIDToPrint());
                }
                else
                {
                    myDebug.print("Resend " + requester + " Data | " + "Data Sender=" + myPlayerID.getIDToPrint() + " | Client=" + PlayerID.getIDToPrint(idRequester) + " | idMessage " + idMessage + " | " + info, MyColor.Red, myPlayerID.playerID.getIDToPrint());
                }
                PhotonNetwork.RaiseEvent(receiveCode, content, raiseEventOptions, sendOptions);
                messageSended = true;
                yield return new WaitForSeconds(waitTime);
            } while (typeReliability == MyTypeReliability.Reliable && !confirmedMessages.Contains(idRequester + "-" + idMessage) && count < tryCount);
            if (count == tryCount)
            {
                errorEvent.Invoke();
            }
        }
    }
    void sendRequestedData(EventData photonEvent)
    {
        StartCoroutine(sendDataCoroutine(photonEvent));
    }
    public void sendUnrequestedData(string idRequester, List<PlayerID> receiverIDs, int[] localActorDataReceivers, ReceiverGroup receiverGroup, bool sendToReceiverGroup, int idMessage)
    {
        StartCoroutine(sendDataCoroutine(idRequester, receiverIDs, localActorDataReceivers, receiverGroup, sendToReceiverGroup,idMessage));
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
