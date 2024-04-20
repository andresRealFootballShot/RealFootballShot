using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddPlayerToTeam : MonoBehaviour
{
    public PlayerComponents playerComponents;
    public bool addAwake = true;
    public string teamName;
    public TypeFieldPosition.Type fieldPositionType = TypeFieldPosition.Type.None;
    private void Start()
    {
        if (addAwake)
        {
            AddToTeam(teamName, fieldPositionType);
        }
    }
    void checkIamAddedToTeam(PlayerAddedToTeamEventArgs args)
    {
        if (args.PlayerID.Equals(playerComponents.publicPlayerData.playerID))
        {
            Team team = Teams.teamsDictionary[args.TeamName];
            playerComponents.playerEvents.addTeamEvent.Invoke(team);
        }
    }
    public void AddToTeam(Team team, TypeFieldPosition.Type typeFieldPosition)
    {
        if (team.addPlayer(playerComponents.publicPlayerData.playerID,typeFieldPosition.ToString()))
        {
            //print("addGoalkeeperToTeam " + team.TeamName + " " + playerComponents.publicPlayerData.playerID);
            playerComponents.playerEvents.addTeamEvent.Invoke(team);
        }
    }
    public void AddToTeam(string teamName, TypeFieldPosition.Type typeFieldPosition)
    {
        Team team = Teams.getTeamByName(teamName);
        if (team != null)
        {
            if (team.addPlayer(playerComponents.publicPlayerData.playerID, typeFieldPosition.ToString()))
            {
                //print("addGoalkeeperToTeam " + team.TeamName + " " + playerComponents.publicPlayerData.playerID);
                playerComponents.playerEvents.addTeamEvent.Invoke(team);
            }
        }
    }
}
