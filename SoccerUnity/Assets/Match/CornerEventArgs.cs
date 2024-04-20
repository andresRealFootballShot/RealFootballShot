using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornerEventArgs
{
    public SideOfFieldID sideOfFieldID;
    public CornerID cornerID;
    public SideOfField sideOfField;
    public CornerComponents corner;
    public Vector3 cornerVelocity;
    public Vector3 goalKickVelocity;
    public string lastKickOfPlayerID;
    public string teamName;
    public CornerEventArgs(SideOfFieldID sideOfFieldID, CornerID cornerID, string lastKickOfPlayerID)
    {
        this.sideOfFieldID = sideOfFieldID;
        this.cornerID = cornerID;
        this.lastKickOfPlayerID = lastKickOfPlayerID;
        Team team;
        Teams.getTeamFromPlayer(lastKickOfPlayerID, out team);
        teamName = team.TeamName;
    }
    public CornerEventArgs(SideOfField sideOfField, CornerComponents corner, string lastKickOfPlayerID,Vector3 cornerVelocity,Vector3 goalKickVelocity)
    {
        this.sideOfFieldID = sideOfField.Value;
        this.cornerID = corner.cornerID;
        this.sideOfField = sideOfField;
        this.corner = corner;
        this.lastKickOfPlayerID = lastKickOfPlayerID;
        Team team;
        Teams.getTeamFromPlayer(lastKickOfPlayerID, out team);
        teamName = team.TeamName;
        this.cornerVelocity = cornerVelocity;
        this.goalKickVelocity = goalKickVelocity;
    }
    public CornerEventArgs(SideOfField sideOfField, CornerComponents corner, string lastKickOfPlayerID, string teamName,Vector3 velocity)
    {
        this.sideOfFieldID = sideOfField.Value;
        this.cornerID = corner.cornerID;
        this.sideOfField = sideOfField;
        this.corner = corner;
        this.lastKickOfPlayerID = lastKickOfPlayerID;
        this.teamName = teamName;
        this.cornerVelocity = velocity;
    }
    public CornerEventArgs(object[] data)
    {
        int index = 0;
        sideOfFieldID = MyFunctions.parseEnum<SideOfFieldID>((string)data[index++]);
        cornerID = MyFunctions.parseEnum<CornerID>((string)data[index++]);
        byte[] playerIDData = (byte[])data[index++];
        PublicPlayerDataList.getPlayerID(playerIDData[0], playerIDData[1], out lastKickOfPlayerID);
        teamName= (string)data[index++];

        SideOfFieldCtrl.getSideOfField(sideOfFieldID, out sideOfField);
        corner = sideOfField.corners.Find(x => x.cornerID == cornerID);
        cornerVelocity = (Vector3) data[index++];
        goalKickVelocity = (Vector3)data[index++];
    }
    public object[] getData()
    {
        List<object> data = new List<object>();
        data.Add(sideOfFieldID.ToString());
        data.Add(cornerID.ToString());
        PlayerID playerID = new PlayerID(lastKickOfPlayerID);
        byte[] playerIDData = new byte[2] { (byte)playerID.onlineActor, (byte)playerID.localActor };
        data.Add(playerIDData);
        data.Add(teamName);
        data.Add(cornerVelocity);
        data.Add(goalKickVelocity);
        return data.ToArray();
    }
}
