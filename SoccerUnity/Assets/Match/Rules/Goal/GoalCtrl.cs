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
        //MatchEvents.endPart.AddListenerConsiderInvoked(endPart);
    }
    void footballFieldIsLoadedEvent()
    {
        enabled = true;
    }
    private void FixedUpdate()
    {
        if (RulesCtrl.checkersEnabled)
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
                MatchEvents.stopMatch.Invoke();
                GoalData args = new GoalData(sideOfField.Value,MatchData.lastPlayerIDPossession,MatchData.lastTeamPossession);
                RulesEvents.notifyGoal.Invoke(args);
            }
        }
    }
    public void execute(GoalData args)
    {
        //DebugsList.rules.print("GoalCtrl.execute() player "+args.playerID+ " team "+args.teamName);
        MatchEvents.stopMatch.Invoke();
        string victimTeamName;
        SideOfFieldCtrl.getTeamOfSideOfField(args.sideOfFieldID, out victimTeamName);
        
        Teams.getRivalTeam(victimTeamName).addGoal(args);
        MatchComponents.kickOff.teamName = victimTeamName;
        goalAnimation.Play(args);
    }
    void endGoalAnimation()
    {
        InitialPosition.SetAllInitPosition();
        ComponentsPlayer.currentComponentsPlayer.EnableOnlyCamera();
        Invoke(nameof(continueMatch), 1);
    }
    void continueMatch()
    {
        MatchComponents.rulesComponents.whistleAnimation.Play();
        MatchComponents.kickOff.startProcess();
        ComponentsPlayer.currentComponentsPlayer.EnableAll();
        MatchEvents.continueMatch.Invoke();
    }
}
