using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchRulesSettings : MonoBehaviour,IRulesSettingsType<SettingsType>
{
    public SettingsType _settingsType;
    public SettingsType settingsType { get => _settingsType; set => _settingsType = value; }
    public double timeWaitToStart = 3;
    public float timeWaitToStartNextPart = 3;
    public float timeWaitToShowEndMenu= 3;
    public float forceKickOff = 5;
    [Header("Timer")]
    public int partMinutes = 4;
    public float partSeconds = 0;
    public float speedTimer = 1;
    public bool lockTimerInStopMatch = false;
    [Space(10)]
    public float TimeWaitToSetBallData = 2;
    public float distanceCircularWall = 9;
    public float MessageKickWait = 10;
    public float inactivityWait = 10;
    public float goalAnimationDuration = 5;
    public float SidelineBallYPosition = 1.7f;
    public float offsideDistanceBallPlayer = 3;
}
