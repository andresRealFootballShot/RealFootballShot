using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataFieldPositionCtrl : MonoBehaviour,ILoad
{
    public GameObject lineupUIList;
    public EmptyEvent publicPlayerDatasAreAvailableEvent;
    public MyDebug myDebug;
    Dictionary<string, Dictionary<string, UITextFieldPositionCtrl>> dictionary = new Dictionary<string, Dictionary<string, UITextFieldPositionCtrl>>();
    int maxAttempts=10;
    float attemptsPeriod = 0.5f;
    EventTrigger updateDataTrigger;
    public static int staticLoadLevel = Teams.staticLoadLevel+1;
    public int loadLevel { get => staticLoadLevel; set => staticLoadLevel = value; }
    public void Load(int level)
    {
        if (level == loadLevel)
        {
            load();
        }
    }
    void load()
    {
        teamsAreLoaded();
        /*teamsAreLoadedTrigger = new EventTrigger();
        teamsAreLoadedTrigger.addTrigger(MatchEvents.teamsLoaded,false,1,true);
        teamsAreLoadedTrigger.addFunction(teamsAreLoaded);
        */
        updateDataTrigger = new EventTrigger();
        //updateDataTrigger.addTrigger(MatchEvents.teamsLoaded, false, 1, true);
        updateDataTrigger.addTrigger(MatchEvents.fieldPositionsChanged, true, 1, true);
        updateDataTrigger.addFunction(updateData);
        updateDataTrigger.endLoadTrigger();
        MatchEvents.otherPlayerLeftRoom.AddListener(removePlayer);
        //DebugsList.testing.print("DataFieldPositionCtrl.load()", Color.red);
    }
    public void teamsAreLoaded()
    {
        foreach (Team team in Teams.teamsList)
        {
            dictionary.Add(team.TeamName, new Dictionary<string, UITextFieldPositionCtrl>());
        }
    }
    //Called in getTeamsDataCtrl.endProcess
    public void updateData()
    {
        StartCoroutine(waitUntilPublicPlayerDatasAreAvailable());
    }
    IEnumerator waitUntilPublicPlayerDatasAreAvailable()
    {
        foreach (Team team in Teams.teamsList)
        {
            foreach (var playerID in team.getPlayersWithAssignedFieldPosition())
            {
                int countAttemp = 0;
                while (!PublicPlayerDataList.all.ContainsKey(playerID) && countAttemp<maxAttempts)
                {
                    DebugsList.RPCRequestFieldPosition.print("Wait for public player data of "+PlayerID.getIDToPrint(playerID) +" from team "+team.TeamName, Color.green);
                    yield return new WaitForSeconds(attemptsPeriod);
                    countAttemp++;
                }
                if (countAttemp == maxAttempts)
                {
                    OnlineErrorHandler.OnlineError("Data field positions");
                }
                else
                {
                    TypeFieldPosition.Type typeFieldPositionOfPlayer;
                    team.getTypeFieldPositionOfPlayer(playerID,out typeFieldPositionOfPlayer);
                    GameObject typeFieldPositionGObj = FieldPositionsCtrl.getUITypeFieldPosition(team.choosedLineup.typeLineup, typeFieldPositionOfPlayer);
                    if (typeFieldPositionGObj != null)
                    {
                        UITextFieldPositionCtrl text = typeFieldPositionGObj.GetComponent<UITextFieldPositionCtrl>();
                        PublicPlayerData publicPlayerData = PublicPlayerDataList.all[playerID];
                        if (!dictionary[team.TeamName].ContainsKey(playerID))
                        {
                            dictionary[team.TeamName].Add(playerID, text);
                            DebugsList.RPCRequestFieldPosition.print("Public player data of " + PlayerID.getIDToPrint(playerID) + " from team " + team.TeamName + " is available", MyColor.Red);
                            if (ChooseTeamCtrl.teamSelected != null)
                            {
                                //Necesario porque a veces se puede ejecutar ChooseFieldPositionCtrl.updateData antes que añadir dictionary[team.TeamName].Add(publicPlayerData, text);
                                clearFieldPositionDataUIText(ChooseTeamCtrl.teamSelected);
                                showDataFieldPositionsOfTeam(ChooseTeamCtrl.teamSelected);
                            }
                        }
                    }
                }
            }
        }
        //publicPlayerDatasAreAvailableEvent.Raise();
        MatchEvents.publicPlayerDataOfFieldPositionsAreAvailable.Invoke();
    }
    public void clearFieldPositionDataUIText(Team team)
    {
        
        GameObject lineupUIGObj = FieldPositionsCtrl.getUILineup(team.choosedLineup.typeLineup);
        UITextFieldPositionCtrl[] textFieldPositionCtrls = lineupUIGObj.transform.GetComponentsInChildren<UITextFieldPositionCtrl>();
        
        foreach (var item in textFieldPositionCtrls)
        {
            item.clearText();
            item.removeNameStringVar();
        }
    }
    public void showDataFieldPositionsOfTeam(Team team)
    {
         DebugsList.RPCRequestFieldPosition.print("DataFieldPositionCtrl.showDataFieldPositionsOfTeam()", Color.red);
        foreach (var item in dictionary[team.TeamName])
        {
            //DebugsList.testing.print("showDataFieldPositionsOfTeam 2", Color.red);
            if (PublicPlayerDataList.all.ContainsKey(item.Key))
            {
                item.Value.setNameStringVar(PublicPlayerDataList.all[item.Key].playerNameVar);
            }
            item.Value.updateText();
        }
    }
    void removePlayer(string playerID)
    {
        DebugsList.RPCRequestFieldPosition.print("DataFieldPositionCtrl.removePlayer()", Color.red);
        foreach (var item in dictionary)
        {
            if (item.Value.ContainsKey(playerID))
            {
                item.Value.Remove(playerID);
            }
        }
        clearFieldPositionDataUIText(ChooseTeamCtrl.teamSelected);
        showDataFieldPositionsOfTeam(ChooseTeamCtrl.teamSelected);
    }
}
