using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct KickEventArgs
{
    public Vector3 kickVelocity;
    public Vector3 pointKick;
    public Vector3 previousVelocity;
    public Vector3 previousAngularVelocity;
    public bool setPreviousVelocities;
    public string playerID;
    public PublicPlayerData kickerPublicPlayerData;
    public Team kickerTeam;
    public KickEventArgs(Vector3 kickDirection, Vector3 previousVelocity, Vector3 previousAngularVelocity,Vector3 pointKick, string playerID)
    {
        this.kickVelocity = kickDirection;
        this.previousVelocity = previousVelocity;
        this.previousAngularVelocity = previousAngularVelocity;
        this.playerID = playerID;
        this.pointKick = pointKick;
        setPreviousVelocities = false;
        if(PublicPlayerDataList.all.ContainsKey(playerID))
            kickerPublicPlayerData = PublicPlayerDataList.all[playerID];
        else
            kickerPublicPlayerData=null;
        kickerTeam = Teams.getTeamFromPlayer(playerID);
    }
    public KickEventArgs(Vector3 kickDirection, Vector3 previousVelocity, Vector3 previousAngularVelocity, int onlineActor,int localActor)
    {
        this.kickVelocity = kickDirection;
        this.previousVelocity = previousVelocity;
        this.previousAngularVelocity = previousAngularVelocity;
        string playerID;
        PublicPlayerDataList.getPlayerID(onlineActor, localActor, out playerID);
        this.playerID = playerID;
        setPreviousVelocities = false;
        pointKick = Vector3.zero;
        if (PublicPlayerDataList.all.ContainsKey(playerID))
            kickerPublicPlayerData = PublicPlayerDataList.all[playerID];
        else
            kickerPublicPlayerData = null;
        kickerTeam = Teams.getTeamFromPlayer(playerID);
    }
}
