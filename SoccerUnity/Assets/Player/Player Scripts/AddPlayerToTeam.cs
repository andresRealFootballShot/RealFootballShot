using DOTS_ChaserDataCalculation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddPlayerToTeam : PlayerComponent
{
    public PlayerComponents playerComponents;
    public bool addAwake = true;
    public bool addSideOfField;
    public SideOfFieldID sideOfFieldID;
    public string teamName;
    public TypeFieldPosition.Type fieldPositionType = TypeFieldPosition.Type.None;
    private void Start()
    {
        if (addAwake)
        {
            if (!addSideOfField)
            {

                AddToTeam(teamName, fieldPositionType);
            }
            else
            {
               
               Team team = Teams.teamsList.Find(x=>x.SideOfField.Value == sideOfFieldID);
               AddToTeam(team, fieldPositionType);
            }
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
            publicPlayerData.team = team;
            SoccerPlayerComponent.myTeam = team;
            SoccerPlayerComponent.rivalTeam = Teams.getRivalTeam(team.TeamName);
            publicPlayerData.rivalTeam = Teams.getRivalTeam(team.TeamName);
            team.StartPressurePosition(publicPlayerData);
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
                SoccerPlayerComponent.myTeam = team;
                SoccerPlayerComponent.rivalTeam = Teams.getRivalTeam(team.TeamName);
                publicPlayerData.team = team;
                publicPlayerData.rivalTeam = Teams.getRivalTeam(team.TeamName);
                team.StartPressurePosition(publicPlayerData);
            }
        }
    }
    public void AddToTeam()
    {
        Team team = Teams.getTeamByName(teamName);
        if (team != null)
        {
            if (team.addPlayer(playerComponents.publicPlayerData.playerID, fieldPositionType.ToString()))
            {
                //print("addGoalkeeperToTeam " + team.TeamName + " " + playerComponents.publicPlayerData.playerID);
                playerComponents.playerEvents.addTeamEvent.Invoke(team);
                if (SoccerPlayerComponent != null)
                {
                    SoccerPlayerComponent.myTeam = team;
                    SoccerPlayerComponent.rivalTeam = Teams.getRivalTeam(team.TeamName);
                }
                team.StartPressurePosition(publicPlayerData);
            }
        }
    }
}
