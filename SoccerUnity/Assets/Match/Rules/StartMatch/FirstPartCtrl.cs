using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPartCtrl : MonoBehaviour
{
    public bool debug = true;
    public Color debugColor = new Color(0, 0.4f, 1f);
    public StartPartAnimation startPartAnimation;
    public EndFirstPartAnimation endPartAnimation;
    public PartCtrl partCtrl;
    void Awake()
    {
       MatchEvents.cameraIsInThirtPersonPosition.AddListenerConsiderInvoked(()=>RulesEvents.nextPart.AddListenerConsiderInvoked(Enable));
    }
    public void Enable()
    {
        if (MatchDataObsolete2.currentPart == 0)
        {
            MatchEvents.publicPlayerDataOfAddedPlayerToTeamIsAvailable.AddListenerConsiderInvoked(checkStartFirstPart);
            RulesEvents.startPart.AddListenerConsiderInvoked(execute);
            MatchComponents.timer.endCountdown.AddListener(endCountDown);
        }
        else if(MatchDataObsolete2.currentPart == 1)
        {
            RulesEvents.startPart.RemoveListener(execute);
            MatchComponents.timer.endCountdown.RemoveListener(endCountDown);
            startPartAnimation.endAnimation -= endAnimation;
            endPartAnimation.endEvent -= endAnimationEvent;
        }
    }

    public void checkStartFirstPart()
    {
        DebugsList.rules.print("StartMatchChecker check", debugColor, debug);
        if (PublicPlayerDataList.all.Count >= TypeMatch.getGlobalMaxPlayersWithGoalkeepers())
        {
            DebugsList.rules.print("StartMatchChecker teamsAreFull", debugColor, debug);
            RulesEvents.notifyStartPart.Invoke();
            MatchEvents.publicPlayerDataOfAddedPlayerToTeamIsAvailable.RemoveListener(checkStartFirstPart);
        }
    }
    void endCountDown()
    {
        MatchEvents.endPart.Invoke();
        endPartAnimation.endEvent += endAnimationEvent;
        endPartAnimation.play();
    }
    void endAnimationEvent()
    {
        MatchDataObsolete2.currentPart++;
        RulesEvents.nextPart.Invoke();
    }
    void DisableExecute()
    {
       
    }
    public void execute()
    {
        DebugsList.rules.print("StartMatch Executor", debugColor, debug);
        InitialPosition.SetAllInitPosition();
        startPartAnimation.endAnimation += endAnimation;
        MatchDataObsolete2.teamNameOfServe = Teams.getRandomTeam();
        partCtrl.execute();
    }
    void endAnimation()
    {
        MatchEvents.initialMatch.Invoke();
        DisableExecute();
        RulesCtrl.Enable();
    }
}
