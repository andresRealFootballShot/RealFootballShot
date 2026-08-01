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
        switch (MatchDataObsolete2.matchState)
        {
            case MatchStateObsolete.WaitingForWarmUp:
                WaitingForWarmUpSetup();
                break;
            case MatchStateObsolete.WarmUp:
                WarmUpSetup();
                break;
            case MatchStateObsolete.Running:
                RunningSetup();
                break;
        }
    }
    void refereeWasAssigned()
    {
        
        if (MatchDataObsolete2.ImReferee && MatchDataObsolete2.matchState == MatchStateObsolete.Running)
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
        if (MatchDataObsolete2.currentPart == 0)
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
