using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalMatchSetup : MonoBehaviour, ILoad
{
    public static int staticLoadLevel = 0;
    public int loadLevel { get => staticLoadLevel; set => staticLoadLevel = value; }
    public bool debug = true;
    public Color debugColor = new Color(0, 0.4f, 1f);
    public Canvas matchHUDCanvas;
    public void Load(int level)
    {
        if (loadLevel == level)
        {
            Load();
        }
    }
    void Load()
    {
        MatchRulesSettings matchRulesSettings = (MatchRulesSettings) MyFunctions.GetSettingsWithSettingsValue(gameObject,SceneSetup.rulesSettingsType);
        MatchComponents.rulesComponents.settings = matchRulesSettings;
        //MatchComponents.kickOff = new OnlineKickOff();
        MatchComponents.matchHUDCanvas = matchHUDCanvas;
        MatchComponents.matchHUDCanvas.enabled = false;
        //SetupRules setupRules = GetComponent<SetupRules>();
        MatchComponents.timer = GetComponentInChildren<Timer>();
        MatchComponents.timer.Load();
        MatchComponents.timer.pauseProcess();
        CheckBallIsInFullArea.SetState(false);
        MatchData.addChangeStateListener(setupCheckBallIsInFullArea);
    }
    void setupCheckBallIsInFullArea()
    {
        if (!MatchData.isStarted)
        {
            CheckBallIsInFullArea.PointExitEvent.AddListener(invokeSetAllInitPosition);
            CheckBallIsInFullArea.SetState(true);
        }
        else
        {
            CheckBallIsInFullArea.PointExitEvent.RemoveListener(invokeSetAllInitPosition);
            CheckBallIsInFullArea.SetState(false);
        }
    }
    void invokeSetAllInitPosition()
    {
        Invoke(nameof(setAllInitPosition), 2);
    }
    void setAllInitPosition()
    {
        InitialPosition.SetAllInitPosition();
    }
}
