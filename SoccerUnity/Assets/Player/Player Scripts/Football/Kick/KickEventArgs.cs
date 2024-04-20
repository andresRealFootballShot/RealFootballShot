using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickEventArgs
{
    public Vector3 kickDirection;
    public Vector3 pointKick;
    public Vector3 previousVelocity;
    public Vector3 previousAngularVelocity;
    public bool setPreviousVelocities;
    public string playerID;
    public KickEventArgs(Vector3 kickDirection, Vector3 previousVelocity, Vector3 previousAngularVelocity,Vector3 pointKick, string playerID)
    {
        this.kickDirection = kickDirection;
        this.previousVelocity = previousVelocity;
        this.previousAngularVelocity = previousAngularVelocity;
        this.playerID = playerID;
        this.pointKick = pointKick;
    }
    public KickEventArgs(Vector3 kickDirection, Vector3 previousVelocity, Vector3 previousAngularVelocity, int onlineActor,int localActor)
    {
        this.kickDirection = kickDirection;
        this.previousVelocity = previousVelocity;
        this.previousAngularVelocity = previousAngularVelocity;
        string playerID;
        PublicPlayerDataList.getPlayerID(onlineActor, localActor, out playerID);
        this.playerID = playerID;
    }
}
