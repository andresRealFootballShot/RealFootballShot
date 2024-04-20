using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Collections.Generic.Dictionary<object, object>;

public class RoomCustomPropertiesCtrl : MonoBehaviour
{
    public static string fieldPositionsSeparator="&&";
   
    public static string getFieldPositionKey(string teamName, string typeFieldPosition)
    {
        return "FieldPosition"+fieldPositionsSeparator+ teamName + fieldPositionsSeparator + typeFieldPosition;
    }
    public static void parseKeyFieldPosition(string key,out string teamName,out string typeFieldPosition)
    {
        string[] fields = key.Split(new string[] { fieldPositionsSeparator }, 3, StringSplitOptions.None);
        if (fields.Length == 3)
        {
            teamName = fields[1];
            typeFieldPosition = fields[2];
        }
        else
        {
            teamName = "";
            typeFieldPosition = "";
        }
    }
    public static bool isFieldPosition(DictionaryEntry dictionaryEntry)
    {
        string key = dictionaryEntry.Key as string;
        string[] fields = key.Split(new string[] { fieldPositionsSeparator }, 3, StringSplitOptions.None);
        if(fields.Length>0 && fields[0].Equals("FieldPosition"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool isFieldPosition(ExitGames.Client.Photon.Hashtable fieldPosition)
    {
        foreach (var item in fieldPosition)
        {
            string key = item.Key as string;
            string[] fields = key.Split(new string[] { fieldPositionsSeparator }, 3, StringSplitOptions.None);
            if (fields.Length > 0 && fields[0].Equals("FieldPosition"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    public static ExitGames.Client.Photon.Hashtable copyTeams(ExitGames.Client.Photon.Hashtable teams)
    {
        ExitGames.Client.Photon.Hashtable newTeamsProperties = new ExitGames.Client.Photon.Hashtable();
        foreach (var team in teams)
        {
            ExitGames.Client.Photon.Hashtable newTeamProperties = new ExitGames.Client.Photon.Hashtable();
            ExitGames.Client.Photon.Hashtable teamProperties = team.Value as ExitGames.Client.Photon.Hashtable;
            newTeamProperties.Add(nameof(Lineup.TypeLineup), teamProperties[nameof(Lineup.TypeLineup)] as string);
            newTeamProperties.Add(nameof(SideOfFieldID), teamProperties[nameof(SideOfFieldID)] as string);
            ExitGames.Client.Photon.Hashtable newFieldPositions = new ExitGames.Client.Photon.Hashtable();
            /*
            ExitGames.Client.Photon.Hashtable fieldPositions = teamProperties["FieldPositions"] as ExitGames.Client.Photon.Hashtable;
            foreach (var fieldPosition in fieldPositions)
            {
                newFieldPositions.Add(fieldPosition.Key as string, fieldPosition.Value as string);
            }
            newTeamProperties.Add("FieldPositions", newFieldPositions);*/
            newTeamsProperties.Add(team.Key as string, newTeamProperties);
        }
        ExitGames.Client.Photon.Hashtable newTeams = new ExitGames.Client.Photon.Hashtable();
        newTeams.Add("Teams", newTeamsProperties);
        return newTeams;
    }
    public static ExitGames.Client.Photon.Hashtable assignFieldPositionToPlayer(ExitGames.Client.Photon.Hashtable fieldPosition,string teamName,string typeFieldPosition,string playerID)
    {
        string key = getFieldPositionKey(teamName,typeFieldPosition);
        fieldPosition[key] = playerID;
        return fieldPosition;
    }
    public static ExitGames.Client.Photon.Hashtable createFieldPosition(string teamName, string typeFieldPosition, string playerID)
    {
        ExitGames.Client.Photon.Hashtable newFieldPosition = new ExitGames.Client.Photon.Hashtable();
        string key = getFieldPositionKey(teamName, typeFieldPosition);
        newFieldPosition.Add(key, playerID);
        return newFieldPosition;
    }
    public static string getFieldPositionOwner(string teamName, string typeFieldPosition)
    {
        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        string key = getFieldPositionKey(teamName,typeFieldPosition);
        return customProperties[key] as string;
    }
    public static void printCustomProperties(ExitGames.Client.Photon.Hashtable customProperties)
    {
        DebugsList.testing.print("Count customProperties "+customProperties.Count, MyColor.Yellow);
        if (customProperties.ContainsKey("Teams"))
        {
            ExitGames.Client.Photon.Hashtable teams = customProperties["Teams"] as ExitGames.Client.Photon.Hashtable;
            DebugsList.testing.print("Count teams " + teams.Count, MyColor.Yellow);
            foreach (var team in teams)
            {
                DebugsList.testing.print((string)team.Key, MyColor.Blue);
                ExitGames.Client.Photon.Hashtable teamProperties = team.Value as ExitGames.Client.Photon.Hashtable;
                DebugsList.testing.print("Count teamProperties " + team.Key +" "+ teamProperties.Count, MyColor.Yellow);
                DebugsList.testing.print("Lineup" + " | " + (string)teamProperties[nameof(Lineup.TypeLineup)], MyColor.Green);
                DebugsList.testing.print("Side of field" + " | " + (string)teamProperties[nameof(SideOfFieldID)], MyColor.Green);
                /*ExitGames.Client.Photon.Hashtable fieldPositions = teamProperties["FieldPositions"] as ExitGames.Client.Photon.Hashtable;
                DebugsList.testing.print("Count field positions " + team.Key + " " + fieldPositions.Count, MyColor.Yellow);*/

                foreach (var fieldPosition in FieldPositionsCtrl.getFieldPositions(Teams.getTeamByName((string)team.Key).choosedLineup.typeLineup))
                {
                    string key = getFieldPositionKey((string)team.Key,fieldPosition.ToString());
                    DebugsList.testing.print(key + " | " + (string)customProperties[key], MyColor.Green);
                }

            }
        }
    }
    public static void printFieldPosition(ExitGames.Client.Photon.Hashtable fieldPosition,string info)
    {
        foreach (var item in fieldPosition)
        {
            if (isFieldPosition(fieldPosition))
            {
                DebugsList.testing.print(info+" printFieldPosition " + (string)item.Key + " " + (string)item.Value, MyColor.Blue);
            }
        } 
    }
}
