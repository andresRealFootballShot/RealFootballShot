using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartCtrl : MonoBehaviour
{
    public bool debug = true;
    public StartPartAnimation startMatchAnimation;

    public Color debugColor = new Color(0, 0.4f, 1f);
    void Awake()
    {
        //RulesEvents.enableStartMach.AddListenerConsiderInvoked(Enable);
        Enable();
        
    }
    public void Enable()
    {
        //RulesEvents.startPart.AddListenerConsiderInvoked(execute);
        startMatchAnimation.endAnimation += endAnimation;
    }

    public void Disable()
    {
        RulesEvents.startPart.RemoveListener(execute);
        startMatchAnimation.endAnimation -= endAnimation;
    }
    public void execute()
    {
        DebugsList.rules.print("StartPart Executor", debugColor, debug);

        //InitialPosition.SetAllInitPosition();
        startMatchAnimation.play();
        MatchComponents.matchHUDCanvas.enabled = true;
        MatchComponents.timer.resetValues();
        ComponentsPlayer.currentComponentsPlayer.EnableOnlyCamera();
        
    }
    void endAnimation()
    {
        ComponentsPlayer.currentComponentsPlayer.EnableAll();
        MatchComponents.timer.startProcess();
        MatchComponents.kickOff.startProcess(MatchData.teamNameOfServe);
        MatchComponents.kickOff.notifyTeamServe(MatchData.teamNameOfServe);
    }
}
