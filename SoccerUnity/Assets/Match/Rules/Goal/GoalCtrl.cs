using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalCtrl : MonoBehaviour
{
    public GoalAnimation goalAnimation;
    void Start()
    {
        MatchComponents.rulesComponents.goalCtrl = this;
        enabled = false;
        MatchEvents.footballFieldLoaded.AddListenerConsiderInvoked(footballFieldIsLoadedEvent);
        goalAnimation.EndEvent.AddListener(endGoalAnimation);
        MatchEvents.goal.AddListenerConsiderInvoked(execute);
        //MatchEvents.endPart.AddListenerConsiderInvoked(endPart);
    }
    void footballFieldIsLoadedEvent()
    {
        enabled = true;
    }
    private void FixedUpdate()
    {
        if (MatchComponents.enabledRules)
        {
            check();
        }
    }
    void check()
    {
        foreach (var sideOfField in MatchComponents.footballField.sideOfFields)
        {
            if (sideOfField.goalComponents.goalChecker.check())
            {
                
               
                GoalData args = new GoalData(sideOfField.Value,MatchComponents.MatchData.posssessionPlayer.playerID, MatchComponents.MatchData.possessionTeam.TeamName);
                MatchComponents.MatchCtrl.Goal(args);
            }
        }
    }
    public void execute(GoalData args)
    {
        //DebugsList.rules.print("GoalCtrl.execute() player "+args.playerID+ " team "+args.teamName);
        
        string victimTeamName;
        SideOfFieldCtrl.getTeamOfSideOfField(args.sideOfFieldID, out victimTeamName);
        MatchComponents.MatchData.startAttackTeamName = victimTeamName;
        Teams.getRivalTeam(victimTeamName).addGoal(args);
        //MatchComponents.kickOff.teamName = victimTeamName;
        goalAnimation.Play(args);
    }
    void endGoalAnimation()
    {
        //ComponentsPlayer.currentComponentsPlayer.EnableOnlyCamera();
        Invoke(nameof(continueMatch), 1);
    }
    void continueMatch()
    {
        
        MatchComponents.MatchCtrl.StartContinueMatch();
    }
}
