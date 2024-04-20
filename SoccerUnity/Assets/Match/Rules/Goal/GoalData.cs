using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalData : MonoBehaviour
{
    public SideOfFieldID sideOfFieldID;
    public string playerID;
    public string teamName;
    public Team team;
    public GoalData(SideOfFieldID sideOfFieldID, string playerID, string teamName)
    {
        this.sideOfFieldID = sideOfFieldID;
        this.playerID = playerID;
        this.teamName = teamName;
        team = Teams.getTeamByName(teamName);
    }
    public GoalData(object[] data)
    {
        int index = 0;
        sideOfFieldID = MyFunctions.parseEnum<SideOfFieldID>((string)data[index++]);
        byte[] playerIDData = (byte[])data[index++];
        PublicPlayerDataList.getPlayerID(playerIDData[0], playerIDData[1], out playerID);
        teamName = (string)data[index++];
        team = Teams.getTeamByName(teamName);
    }
    public object[] getData()
    {
        List<object> data = new List<object>();
        data.Add(sideOfFieldID.ToString());
        PlayerID _playerID = new PlayerID(playerID);
        byte[] playerIDData = new byte[2] { (byte)_playerID.onlineActor, (byte)_playerID.localActor };
        data.Add(playerIDData);
        data.Add(teamName);
        return data.ToArray();
    }
}
