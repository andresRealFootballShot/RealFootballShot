using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RulesEvents : MonoBehaviour, IClearBeforeLoadScene
{
    public static MyEvent notifyStartPart = new MyEvent(nameof(notifyStartPart));
    public static MyEvent<CornerEventArgs> notifyCorner = new MyEvent<CornerEventArgs>(nameof(notifyCorner));
    public static MyEvent<SidelineEventArgs> notifySideline = new MyEvent<SidelineEventArgs>(nameof(notifySideline));
    public static MyEvent<GoalData> notifyGoal = new MyEvent<GoalData>(nameof(notifyGoal));
    public static MyEvent<OffsideData> notifyOffside = new MyEvent<OffsideData>(nameof(notifyOffside));
    public static MyEvent startPart = new MyEvent(nameof(startPart));
    public static MyEvent nextPart = new MyEvent(nameof(nextPart));
    public static MyEvent refereeIsAssigned = new MyEvent(nameof(refereeIsAssigned));
    public void Clear()
    {
        notifyStartPart = new MyEvent(nameof(notifyStartPart));
        startPart = new MyEvent(nameof(startPart));
        nextPart = new MyEvent(nameof(nextPart));
        refereeIsAssigned = new MyEvent(nameof(refereeIsAssigned));
        notifyCorner = new MyEvent<CornerEventArgs>(nameof(notifyCorner));
        notifyGoal = new MyEvent<GoalData>(nameof(notifyGoal));
        notifySideline = new MyEvent<SidelineEventArgs>(nameof(notifySideline));
        notifyOffside = new MyEvent<OffsideData>(nameof(notifyOffside));
    }
}
