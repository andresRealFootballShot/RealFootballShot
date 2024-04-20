using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupRules : MonoBehaviour
{
    public MessageWithCountDown MessageWithCountDown;
    public bool debug = true;
    public Color debugColor = new Color(0, 0.4f, 1f);
    
    void Start()
    {
        MatchComponents.rulesComponents.MessageWithCountDown = MessageWithCountDown;
        MatchComponents.rulesComponents.MessageWithCountDown.Hide();
        RulesCtrl.Disable();
        MatchEvents.matchDataIsLoaded.AddListenerConsiderInvoked(matchDataIsLoaded);
        RulesEvents.refereeIsAssigned.AddListenerConsiderInvoked(refereeWasAssigned);
        MatchEvents.kick.AddListener(unlockBall);
    }
    void unlockBall()
    {
        if (Kick.ballLocked)
        {
            Kick.ballLocked = false;
            MatchComponents.rulesComponents.invisibleCircularWall.Disable();
        }
    }
    void matchDataIsLoaded()
    {
        RulesEvents.nextPart.Invoke();
        switch (MatchData.matchState)
        {
            case MatchState.WaitingForWarmUp:
                WaitingForWarmUpSetup();
                break;
            case MatchState.WarmUp:
                WarmUpSetup();
                break;
            case MatchState.Running:
                RunningSetup();
                break;
        }
    }
    void refereeWasAssigned()
    {
        
        if (MatchData.ImReferee && MatchData.matchState == MatchState.Running)
        {
            DebugsList.rules.print("SetupRules.refereeWasAssigned() ImReferee", debugColor, debug);
            RulesCtrl.Enable();
        }
        else
        {
            DebugsList.rules.print("SetupRules.refereeWasAssigned() Im not Referee", debugColor, debug);
            RulesCtrl.Disable();
        }
    }
    void RunningSetup()
    {
        DebugsList.rules.print("RunningSetup", debugColor, debug);
        if (MatchData.currentPart == 0)
        {
        }
    }
    void WaitingForWarmUpSetup()
    {
        DebugsList.rules.print("WaitingForWarmUpSetup", debugColor, debug);
        DebugsList.rules.print("Invoke warmUp", debugColor, debug);
        MatchEvents.warmUp.Invoke();
    }
    void WarmUpSetup()
    {
        DebugsList.rules.print("WarmUpSetup", debugColor, debug);
        MatchEvents.warmUp.Invoke();
    }
}
