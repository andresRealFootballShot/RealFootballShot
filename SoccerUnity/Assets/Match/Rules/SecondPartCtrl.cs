using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondPartCtrl : MonoBehaviour
{
    public bool debug = true;
    public Color debugColor = new Color(0, 0.4f, 1f);
    public StartPartAnimation startPartAnimation;
    public EndMatchAnimation endMatchAnimation;
    public PartCtrl partCtrl;
    void Awake()
    {
        RulesEvents.nextPart.AddListenerConsiderInvoked(Enable);
        
    }
    public void Enable()
    {
        if (MatchData.currentPart == 1)
        {
            RulesEvents.startPart.AddListener(execute);
            MatchComponents.timer.endCountdown.AddListener(endCountDown);
            notifyStartPart();
        }
        else if(MatchData.currentPart > 1)
        {
            RulesEvents.startPart.RemoveListener(execute);
            MatchComponents.timer.endCountdown.RemoveListener(endCountDown);
            startPartAnimation.endAnimation -= endAnimation;
        }
    }

    public void DisableCheck()
    {
        
    }
    public void DisableExecute()
    {
        
    }
    void notifyStartPart()
    {
        SideOfFieldCtrl.alternateSideOfFieldsOfTeams();
        InitialPosition.SetAllInitPosition();
        ComponentsPlayer.currentComponentsPlayer.EnableOnlyCamera();
        DebugsList.rules.print("StartSecondPartCheker endCountDown", debugColor, debug);
        foreach (var publicPlayerData in PublicPlayerDataList.fieldPlayers.Values)
        {
            Collider collider = publicPlayerData.bodyTransform.GetComponent<Collider>();
            collider.isTrigger = true;
            publicPlayerData.rigidbody.useGravity = false;
            publicPlayerData.rigidbody.isKinematic = true;
        }
        RulesEvents.notifyStartPart.Invoke();
    }
    public void execute()
    {
        DebugsList.rules.print("StartSecondPartExecutor", debugColor, debug);
        foreach (var publicPlayerData in PublicPlayerDataList.fieldPlayers.Values)
        {
            Collider collider = publicPlayerData.bodyTransform.GetComponent<Collider>();
            collider.isTrigger = false;
            publicPlayerData.rigidbody.useGravity = true;
            publicPlayerData.rigidbody.isKinematic = false;
        }
        MatchEvents.continueMatch.Invoke();
        startPartAnimation.endAnimation += endAnimation;
        MatchData.teamNameOfServe = Teams.teamsList.Find(x=>x.TeamName!=MatchData.teamNameOfServe).TeamName;
        partCtrl.execute();
    }
    void endAnimation()
    {
    }
    void endCountDown()
    {
        MatchComponents.timer.endCountdown.RemoveListener(endCountDown);
        endMatchAnimation.play();
        MatchEvents.endPart.Invoke();
        MatchEvents.endMatch.Invoke();
    }
}
