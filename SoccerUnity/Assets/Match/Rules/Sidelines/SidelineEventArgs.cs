using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SidelineEventArgs
{
    public Vector3 point;
    public Vector3 velocity;
    public string causedTeamName;
    Team team;
    public Team Team { get => team; }
    

    public SidelineEventArgs(Vector3 point, Vector3 velocity, string causedTeamName)
    {
        this.point = point;
        this.velocity = velocity;
        this.causedTeamName = causedTeamName;
        team = Teams.getTeamByName(causedTeamName);
    }
    public SidelineEventArgs(object[] data)
    {
        int index = 0;
        point = (Vector3) data[index++];
        velocity = (Vector3) data[index++];
        causedTeamName = (string)data[index++];
        team = Teams.getTeamByName(causedTeamName);
        //byte[] playerIDData = (byte[])data[index++];
        //PublicPlayerDataList.getPlayerID(playerIDData[0], playerIDData[1], out lastKickOfPlayerID);
    }
    public object[] getData()
    {
        List<object> data = new List<object>();
        data.Add(point);
        data.Add(velocity);
        //PlayerID playerID = new PlayerID(lastKickOfPlayerID);
        //byte[] playerIDData = new byte[2] { (byte)playerID.onlineActor, (byte)playerID.localActor };
        data.Add(causedTeamName);
        return data.ToArray();
    }
}
