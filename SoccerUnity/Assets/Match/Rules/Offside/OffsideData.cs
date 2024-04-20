using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsideData
{
    public Vector3 point;
    public string playerID;
    public string teamName;
    public Team team;

    public OffsideData(Vector3 point, string playerID, string teamName)
    {
        this.point = point;
        this.playerID = playerID;
        this.teamName = teamName;
        team = Teams.getTeamByName(teamName);
    }
    public OffsideData(object[] data)
    {
        int index = 0;
        point = (Vector3)data[index++];
        byte[] playerIDData = (byte[])data[index++];
        PublicPlayerDataList.getPlayerID(playerIDData[0], playerIDData[1], out playerID);
        teamName = (string)data[index++];
        team = Teams.getTeamByName(teamName);
    }
    public object[] getData()
    {
        List<object> data = new List<object>();
        data.Add(point);
        PlayerID _playerID = new PlayerID(playerID);
        byte[] playerIDData = new byte[2] { (byte)_playerID.onlineActor, (byte)_playerID.localActor };
        data.Add(playerIDData);
        data.Add(teamName);
        return data.ToArray();
    }
}
