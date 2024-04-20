using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateLocalGoalkeepers : MonoBehaviour
{
    static List<EventTrigger> setupGoalkeeperTriggers = new List<EventTrigger>();
    static EventTrigger setupGoalkeepers = new EventTrigger();
    static bool enable=true;
    /*void Start()
    {
        StartProcess();
    }*/
    public static void StartProcess()
    {
        if (enable)
        {
            setupGoalkeeperTriggers = new List<EventTrigger>();
            setupGoalkeepers = new EventTrigger();
            setupGoalkeepers.addTrigger(Teams.teamsAreLoadedEvent, false, 1, true);
            setupGoalkeepers.addTrigger(MatchEvents.myPlayerIDLoaded, false, 1, true);
            setupGoalkeepers.addFunction(SetupGoalkeepers);
            setupGoalkeepers.endLoadTrigger();
        }
    }
    public static void SetupGoalkeepers()
    {
        foreach (var team in Teams.teamsList)
        {
            addTrigger(team);
        }
    }
    static void addTrigger(Team team)
    {
        EventTrigger<Team> trigger = new EventTrigger<Team>();
        trigger.addTrigger(team.lineupChanged, false, 1, true);
        trigger.addTrigger(team.sideOfFieldChanged, false, 1, true);
        trigger.addTrigger(MatchEvents.footballFieldLoaded, false, 1, true);
        trigger.addFunction(setupGoalkeeper, team);
        trigger.endLoadTrigger();
        setupGoalkeeperTriggers.Add(trigger);
    }
    static void setupGoalkeeper(Team team)
    {
        SideOfField sideOfField;
        if (!SideOfFieldCtrl.getSideOfFieldOfTeam(team.TeamName, out sideOfField))
        {
            return;
        }
        GameObject goalkeeper = sideOfField.goalComponents.goalkeeper;
        GoalkeeperComponents goalkeeperComponents = goalkeeper.GetComponent<GoalkeeperComponents>();

        SetupGoalkeeper setupGoalkeeper = goalkeeperComponents.setupGoalkeeper;
        setupGoalkeeper.StartSetup(SetupGoalkeeper.TypeSetupGoalkeeper.TeamSetup);
        if (goalkeeperComponents.publicPlayerData.playerIDIsLoaded)
        {
            
            goalkeeperComponents.addPlayerToTeam.AddToTeam(team.TeamName, TypeFieldPosition.Type.GoalKeeper);
        }

    }
}
