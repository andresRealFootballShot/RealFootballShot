using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MatchEvents : MonoBehaviour,IClearBeforeLoadScene
{
    public static int staticLoadLevel = 0;
    public int loadLevel { get => staticLoadLevel; set => staticLoadLevel = value; }
    public static MyEvent<FieldPositionEventArgs> fieldPositionsChanged = new MyEvent<FieldPositionEventArgs>(nameof(fieldPositionsChanged));
    public static MyEvent<PlayerAddedToTeamEventArgs> playerAddedToTeam = new MyEvent<PlayerAddedToTeamEventArgs>(nameof(playerAddedToTeam));
    public static MyEvent<PlayerAddedToTeamEventArgs> publicPlayerDataOfAddedPlayerToTeamIsAvailable = new MyEvent<PlayerAddedToTeamEventArgs>(nameof(publicPlayerDataOfAddedPlayerToTeamIsAvailable));
    public static MyEvent publicPlayerDataOfFieldPositionsAreAvailable = new MyEvent(nameof(publicPlayerDataOfFieldPositionsAreAvailable));
    public static MyEvent teamsSetuped = new MyEvent(nameof(teamsSetuped));
    public static MyEvent matchLoaded = new MyEvent(nameof(matchLoaded));
    public static MyEvent requestedFieldPositionWasAccepted = new MyEvent(nameof(requestedFieldPositionWasAccepted));
    public static MyEvent requestedFieldPositionWasDenied = new MyEvent(nameof(requestedFieldPositionWasDenied));
    public static MyEvent<string> otherPlayerLeftRoom = new MyEvent<string>(nameof(otherPlayerLeftRoom));
    public static MyEvent<GameObject> setMainBall = new MyEvent<GameObject>(nameof(setMainBall));
    public static MyEvent ballPhysicsMaterialLoaded = new MyEvent(nameof(ballPhysicsMaterialLoaded));
    public static MyEvent typeMatchSetuped = new MyEvent(nameof(typeMatchSetuped));
    public static MyEvent corner = new MyEvent(nameof(corner));
    public static MyEvent waitingWarmUp = new MyEvent(nameof(waitingWarmUp));
    public static MyEvent warmUp = new MyEvent(nameof(warmUp));
    public static MyEvent endPart = new MyEvent(nameof(endPart));
    public static MyEvent stopMatch = new MyEvent(nameof(stopMatch));
    public static MyEvent startMatch = new MyEvent(nameof(startMatch));
    public static MyEvent continueMatch = new MyEvent(nameof(continueMatch));
    public static MyEvent endMatch = new MyEvent(nameof(endMatch));
    public static MyEvent<GoalData> goal = new MyEvent<GoalData>(nameof(goal));
    public static MyEvent footballFieldLoaded = new MyEvent(nameof(footballFieldLoaded));
    public static MyEvent sizeFootballFieldChanged = new MyEvent(nameof(sizeFootballFieldChanged));
    public static MyEvent matchDataIsLoaded = new MyEvent(nameof(matchDataIsLoaded));
    //public static Event<ChaserData> addedChaserDataEvent = new Event<ChaserData>(nameof(addedChaserDataEvent));
    //public static Event<ChaserData> removedChaserDataEvent = new Event<ChaserData>(nameof(removedChaserDataEvent));
    public static MyEvent<PublicPlayerData> addedPublicPlayerDataToList = new MyEvent<PublicPlayerData>(nameof(addedPublicPlayerDataToList));
    public static MyEvent myPlayerIDLoaded = new MyEvent(nameof(myPlayerIDLoaded));
    public static MyEvent<KickEventArgs> kick = new MyEvent<KickEventArgs>(nameof(kick));
    public static MyEvent losePossession = new MyEvent(nameof(losePossession));
    public static MyEvent getPossession = new MyEvent(nameof(getPossession));
    public static MyEvent cameraIsInThirtPersonPosition = new MyEvent(nameof(cameraIsInThirtPersonPosition));
    public static MyEvent endStartingScreen = new MyEvent(nameof(endStartingScreen));
    public static MyEvent enableMenu = new MyEvent(nameof(enableMenu));
    public void Clear()
    {
        

        fieldPositionsChanged = new MyEvent<FieldPositionEventArgs>(nameof(fieldPositionsChanged));
        publicPlayerDataOfFieldPositionsAreAvailable = new MyEvent(nameof(publicPlayerDataOfFieldPositionsAreAvailable));
        publicPlayerDataOfAddedPlayerToTeamIsAvailable = new MyEvent<PlayerAddedToTeamEventArgs>(nameof(publicPlayerDataOfAddedPlayerToTeamIsAvailable));
        matchLoaded = new MyEvent(nameof(matchLoaded));
        requestedFieldPositionWasAccepted = new MyEvent(nameof(requestedFieldPositionWasAccepted));
        requestedFieldPositionWasDenied = new MyEvent(nameof(requestedFieldPositionWasDenied));
        otherPlayerLeftRoom = new MyEvent<string>(nameof(otherPlayerLeftRoom));
        playerAddedToTeam = new MyEvent<PlayerAddedToTeamEventArgs>(nameof(playerAddedToTeam));
        setMainBall = new MyEvent<GameObject>(nameof(setMainBall));
        ballPhysicsMaterialLoaded = new MyEvent<GameObject>(nameof(ballPhysicsMaterialLoaded));
        typeMatchSetuped = new MyEvent(nameof(typeMatchSetuped));
        corner = new MyEvent(nameof(corner));
        waitingWarmUp = new MyEvent(nameof(waitingWarmUp));
        warmUp = new MyEvent(nameof(warmUp));
        endPart = new MyEvent(nameof(endPart));
        startMatch = new MyEvent(nameof(startMatch));
        continueMatch = new MyEvent(nameof(continueMatch));
        endMatch = new MyEvent(nameof(endMatch));
        goal = new MyEvent<GoalData>(nameof(goal));
        footballFieldLoaded = new MyEvent(nameof(footballFieldLoaded));
        sizeFootballFieldChanged = new MyEvent(nameof(sizeFootballFieldChanged));
        matchDataIsLoaded = new MyEvent(nameof(matchDataIsLoaded));
        //addedChaserDataEvent = new Event<ChaserData>(nameof(addedChaserDataEvent));
        addedPublicPlayerDataToList = new MyEvent<PublicPlayerData>(nameof(addedPublicPlayerDataToList));
        //removedChaserDataEvent = new Event<ChaserData>(nameof(removedChaserDataEvent));
        myPlayerIDLoaded = new MyEvent(nameof(myPlayerIDLoaded));
        kick = new MyEvent<KickEventArgs>(nameof(kick));
        losePossession = new MyEvent(nameof(losePossession));
        getPossession = new MyEvent(nameof(getPossession));
        teamsSetuped = new MyEvent(nameof(teamsSetuped));
        cameraIsInThirtPersonPosition = new MyEvent(nameof(cameraIsInThirtPersonPosition));
        stopMatch = new MyEvent(nameof(stopMatch));
        endStartingScreen = new MyEvent(nameof(endStartingScreen));
        enableMenu = new MyEvent(nameof(enableMenu));
    }
}
