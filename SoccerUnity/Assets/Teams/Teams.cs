using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Teams : MonoBehaviour,ILoad,IClearBeforeLoadScene
{
    public MatchDataObsolete matchData;
    //public Team teamRed, teamBlue;
    public static Dictionary<string,Team> teamsDictionary = new Dictionary<string, Team>();
    public static List<Team> teamsList { get { return new List<Team>(teamsDictionary.Values); } }
    public static int staticLoadLevel = MatchEvents.staticLoadLevel+1;
    public int loadLevel { get => staticLoadLevel; set => staticLoadLevel = value; }
    public static MyEvent teamsAreLoadedEvent = new MyEvent(nameof(Teams)+" | "+nameof(teamsAreLoadedEvent));
    public static Color debugColor=new Color(0.5f,0.1f,0.9f);
    public static Team MyTeam { get => myTeam(); }
    public static List<PublicPlayerData> allPlayers = new List<PublicPlayerData>();
    public static List<ChaserData> firstChaserDatas = new List<ChaserData>();
    public void Load(int level)
    {
        if (loadLevel == level)
        {
            teamsDictionary.Clear();
            Team[] teams = transform.GetComponentsInChildren<Team>();
            foreach (var item in teams)
            {
                item.Load();
            }
            foreach (var item in teams)
            {
                teamsDictionary.Add(item.TeamName, item);
            }
            teamsAreLoadedEvent.Invoke();
            MatchEvents.publicPlayerDataOfAddedPlayerToTeamIsAvailable.AddListener(publicPlayerDataIsAdded);
        }
    }
    void publicPlayerDataIsAdded(PlayerAddedToTeamEventArgs playerAddedToTeamEventArgs)
    {
        Team team = getTeamByName(playerAddedToTeamEventArgs.TeamName);
        allPlayers.Add(playerAddedToTeamEventArgs.publicPlayerData);
        ChaserData chaserData;
        if(playerAddedToTeamEventArgs.publicPlayerData.getFirstChaserData(out chaserData))
        {
            firstChaserDatas.Add(chaserData);
            team.addFirstChaserData(chaserData);
        }
        team.addPublicPlayerData(playerAddedToTeamEventArgs.publicPlayerData);
    }
    static Team myTeam()
    {
        Team team;
        getTeamFromPlayer(ComponentsPlayer.myMonoPlayerID.playerIDStr,out team);
        return team;
    }
    public static bool getIndexOfTeam(string teamName, out int index)
    {
        if (teamsDictionary.ContainsKey(teamName))
        {
            index = teamsList.IndexOf(teamsDictionary[teamName]);
            return true;
        }
        else
        {
            index = -1;
            return false;
        }
    }
    public static bool getTeamByIndex(int index,out Team team)
    {
        if (teamsList.Count > index)
        {
            team = teamsList[index];
            return true;
        }
        else
        {
            team = null;
            return false;
        }
    }
    public static Team getRivalTeam(string teamName)
    {
       return teamsList.Find(x => x.TeamName != teamName);
    }
    public static Team getRivalTeamOfPlayer(string playerID)
    {
        Team team;
        getTeamFromPlayer(playerID, out team);
        return getRivalTeam(team.TeamName);
    }
    public void Clear()
    {
        teamsAreLoadedEvent = new MyEvent();
    }
    public static Team getAvailableRandomTeam()
    {
        List<Team> teamsNotFull = new List<Team>();
        foreach (var team in teamsList)
        {
            if (!team.fieldPositionsIsFull())
            {
                teamsNotFull.Add(team);
            }
        }
        int index = Random.Range(0, teamsNotFull.Count);
       return teamsNotFull[index];
    }
    public static string getRandomTeam()
    {
        int random = Random.Range(0, Teams.teamsDictionary.Count);
        List<string> names = Teams.getTeamNames();
        return names[random];
    }
    public static List<string> getTeamNames()
    {
        List<string> teamNames = new List<string>();
        foreach (var item in teamsList)
        {
            teamNames.Add(item.TeamName);
        }
        return teamNames;
    }
    public static Team getTeamByName(string teamName)
    {
      return teamsList.Find(item => item.TeamName == teamName);
    }
    public static void TeamsAvailables(out bool red, out bool blue)
    {
        red = !getTeamByName("Red").isFull();
        blue = !getTeamByName("Blue").isFull();

    }
    public static void printTeamsSize()
    {
        DebugsList.testing.print("printTeamsSize");
        foreach (var team in teamsList)
        {
            DebugsList.testing.print("Team " + team.TeamName+" | size="+team.teamMaxPlayers + " | playersCount=" + team.players.Count);
        }
    }
    public static void print()
    {
        DebugsList.RPCRequestFieldPosition.print("Teams.print()", debugColor);
        foreach (var team in teamsList)
        {
            DebugsList.RPCRequestFieldPosition.print("Team " + team.TeamName + " | size=" + team.teamMaxPlayers + " | playersCount=" + team.players.Count, debugColor);
            foreach (var fieldPosition in team.fieldPositionOfPlayers)
            {
                DebugsList.RPCRequestFieldPosition.print("FieldPosition " + fieldPosition.Key + " | playerID=" + fieldPosition.Value, debugColor);
            }
        }
    }
    public static bool teamsAreFull()
    {
        foreach (var item in teamsList)
        {
            if (!item.isFull())
            {
                return false;
            }
        }
        return true;
    }
    public static bool someTeamContainsPlayer(string playerID)
    {
        foreach (var item in teamsList)
        {
            if (item.ContainsPlayer(playerID))
            {
                return true;
            }
        }
        return false;
    }
    public static bool getTeamFromPlayer(string playerID,out Team team)
    {
        team = null;
        foreach (var item in teamsList)
        {
            if (item.ContainsPlayer(playerID))
            {
                team = item;
                return true;
            }
        }
        return false;
    }
    public static List<string> getPlayersOfAllTeams()
    {
        List<string> players = new List<string>();
        foreach (var item in teamsDictionary)
        {
            players.AddRange(item.Value.players);
        }
        return players;
    }
    public void RemovePlayer(string playerID)
    {
        foreach (var item in teamsList)
        {
           item.removePlayer(playerID);
        }
    }
    public static bool AddPlayerToTeam(string teamName, string playerID, string typeFieldPosition)
    {
        if (teamsDictionary.ContainsKey(teamName))
        {
            if (teamsDictionary[teamName].addPlayer(playerID))
            {
                teamsDictionary[teamName].assignFieldPositionToPlayer(playerID, typeFieldPosition);
                foreach (var team in teamsList)
                {
                    if (team.TeamName != teamName)
                    {
                        team.removePlayer(playerID);
                    }
                }
                return true;
            }
        }
        else
        {
            Debug.LogError("Team " + teamName + " not exist");
        }
        return false;
    }
    public static void AssignFieldPositionToPlayerToTeam(string teamName, string playerID, string typeFieldPosition)
    {
        if (teamsDictionary.ContainsKey(teamName))
        {
            teamsDictionary[teamName].assignFieldPositionToPlayer(playerID, typeFieldPosition);
        }
    }
    public static void AddPlayerToRandomFieldPositionAndTeam(string playerID)
    {
        List<Team> teamsNotFull = new List<Team>();
        foreach (var team in teamsList)
        {
            if (!team.isFull())
            {
                teamsNotFull.Add(team);
            }
        }
        int index = Random.Range(0, teamsNotFull.Count);
        teamsNotFull[index].addPlayerToRandomFieldPosition(playerID);
    }
    public static void setLineupToTeam(string teamName,string typeLineup)
    {
        teamsDictionary[teamName].setLineup(MyFunctions.parseEnum<Lineup.TypeLineup>(typeLineup));
    }
    public static void setLineupToAllTeams(Lineup.TypeLineup typeLineup)
    {
        foreach (var team in teamsList)
        {
            team.setLineup(typeLineup);
        }
    }
    public bool isPortero(int actor)
    {
        /*
        foreach (var item in teamsList)
        {
            if (item.actorPortero == actor)
            {
                return true;
            }
        }*/
        return false;
    }
    public Color GetColor(string teamName)
    {
        return getTeamByName(teamName).Color;
    }
    public void Goal(string team,int actor)
    {
        getTeamByName(team).addGoal(actor);
    }
    public int[] GetPlayers(string team)
    {
       return getTeamByName(team).GetPlayersToInt();
    }
    
    
    public bool BlueAvailable()
    {
        return !getTeamByName("Blue").isFull();
    }
    public bool RedAvailable()
    {
        return !getTeamByName("Red").isFull();
    }
    public int SetRandomSpawn(int actor)
    {
        /*
        foreach (var item in teamsList)
        {
            if (item.players.Contains(actor))
            {
               return item.setRandomSpawn(actor);
            }
        }*/
        return -1;
    }
    public void SetSpawn(int actor,int spawn)
    {
        /*
        foreach (var item in teamsList)
        {
            if (item.players.Contains(actor))
            {
               item.setSpawn(actor,spawn);
            }
        }*/
    }
    public Transform getSpawn(int actor)
    {
        /*
        foreach (var item in teamsList)
        {
            if (item.players.Contains(actor))
            {
               return item.getSpawn(actor);
            }
        }*/
        return null;
    }
    public Team getTeamFromActor(int actor)
    {
        /*
        foreach (var item in teamsList)
        {
            if (item.players.Contains(actor))
            {
                return item;
            }
        }*/
        return null;
    }
    public Team getTeamNotContainActor(int actor)
    {
        /*
        foreach (var item in teamsList)
        {
            if (!item.players.Contains(actor))
            {
                return item;
            }
        }*/
        return null;
    }
}
