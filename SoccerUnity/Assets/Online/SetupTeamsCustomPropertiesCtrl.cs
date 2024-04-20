using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupTeamsCustomPropertiesCtrl : MonoBehaviourPunCallbacks, ISetupTeams
{
    public void SetupTeams()
    {
        //DebugsList.testing.print("setupTeamsCustomProperties");
        if (PhotonNetwork.IsMasterClient)
        {
            
            DebugsList.RPCRequestFieldPosition.print("SetupTeamsCustomPropertiesCtrl.SetupTeams() IsMasterClient");
            ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            //ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
            ExitGames.Client.Photon.Hashtable teamsProperties = new ExitGames.Client.Photon.Hashtable();
            ExitGames.Client.Photon.Hashtable fieldPositions = new ExitGames.Client.Photon.Hashtable();
            List<SideOfFieldID> sideOfFieldIDs = SideOfFieldCtrl.GetAvailableSideOfFields();
            SideOfFieldCtrl.SetRandomSideOfField(Teams.getTeamNames());
            foreach (var team in Teams.teamsList)
            {
                ExitGames.Client.Photon.Hashtable teamProperties = new ExitGames.Client.Photon.Hashtable();
                ChooseLineupCtrl.chooseLineup(team, Lineup.TypeLineup.Default);
                teamProperties.Add(nameof(Lineup.TypeLineup), Lineup.TypeLineup.Default.ToString());
                /*SideOfFieldID sideOfFieldID = sideOfFieldIDs[UnityEngine.Random.Range(0, sideOfFieldIDs.Count)];
                sideOfFieldIDs.Remove(sideOfFieldID);
                teamProperties.Add(nameof(SideOfFieldID), sideOfFieldID.ToString());*/
                teamProperties.Add(nameof(SideOfFieldID), SideOfFieldCtrl.sideOfFieldOfTeam(team.TeamName).ToString());
                foreach (var fieldPosition in FieldPositionsCtrl.getFieldPositions(team.choosedLineup.typeLineup))
                {
                    //fieldPositions.Add(fieldPosition.ToString(), "None");
                    string key =RoomCustomPropertiesCtrl.getFieldPositionKey(team.TeamName, fieldPosition.ToString());
                    customProperties.Add(key, "None");
                }
                //teamProperties.Add("FieldPositions", fieldPositions);
                teamsProperties.Add(team.TeamName, teamProperties);
                //DebugsList.testing.print(team.TeamName + " " + SideOfFieldCtrl.sideOfFieldOfTeam(team.TeamName) + " " + team.choosedLineup.typeLineup.ToString());
            }
            customProperties.Add("Teams", teamsProperties);
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
            //No quitar, en el build es necesario
            //MatchEvents.publicPlayerDataOfFieldPositionsAreAvailable.Invoke();
        }
        else
        {
            setTeamsData();
        }
    }
    void setTeamsData()
    {
        
        DebugsList.RPCRequestFieldPosition.print("SetupTeamsCustomPropertiesCtrl.setTeamsData()");
        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        ExitGames.Client.Photon.Hashtable teamsCustomProperties = customProperties["Teams"] as ExitGames.Client.Photon.Hashtable;
        bool fieldPositionsChanged = false;
        foreach (var item in teamsCustomProperties)
        {
            ExitGames.Client.Photon.Hashtable teamProperties = item.Value as ExitGames.Client.Photon.Hashtable;
            Team team = Teams.getTeamByName(item.Key as string);
            string lineupStr = teamProperties[nameof(Lineup.TypeLineup)] as string;
            Lineup.TypeLineup lineup = MyFunctions.parseEnum<Lineup.TypeLineup>(lineupStr);
            ChooseLineupCtrl.chooseLineup(Teams.getTeamByName(item.Key as string), lineup);

            string sideOfField = teamProperties[nameof(SideOfFieldID)] as string;
            SideOfFieldID sideOfFieldID = MyFunctions.parseEnum<SideOfFieldID>(sideOfField);
            //Teams.teamsDictionary[team.TeamName].setSideOfField(sideOfFieldID);
            SideOfFieldCtrl.setTeamSide(team.TeamName, sideOfFieldID);
            //DebugsList.testing.print(team.TeamName + " " + sideOfFieldID + " " + lineupStr);
        }
        foreach (var item in customProperties)
        {
            if (RoomCustomPropertiesCtrl.isFieldPosition(item))
            {
                    string teamName, typeFieldPosition;
                    RoomCustomPropertiesCtrl.parseKeyFieldPosition(item.Key as string, out teamName, out typeFieldPosition);
                DebugsList.RPCRequestFieldPosition.print("Online assignFieldPositionToPlayer Team=" + teamName + " playerID=" + item.Value + " typeFieldPosition=" + typeFieldPosition, new Color(0,0.6f,0.3f));
                Teams.AddPlayerToTeam(teamName, item.Value as string, typeFieldPosition);
                    fieldPositionsChanged = true;
            }
        }

        if (fieldPositionsChanged)
        {
            MatchEvents.fieldPositionsChanged.Invoke();
        }
        else
        {
            MatchEvents.publicPlayerDataOfFieldPositionsAreAvailable.Invoke();
        }
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("Teams"))
        {
            setTeamsData();
        }
    }
 }
