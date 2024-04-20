using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalStartMatchNotifier : MonoBehaviour
{

    public bool debug = true;
    Color debugColor = new Color(0, 0.4f, 1f);
    void Start()
    {
        RulesEvents.nextPart.AddListenerConsiderInvoked(Enable);
    }
    public void Enable()
    {
        RulesEvents.notifyStartPart.AddListenerConsiderInvoked(notify);
    }

    public void Disable()
    {
        MatchEvents.publicPlayerDataOfAddedPlayerToTeamIsAvailable.RemoveListener(notify);
    }
    public void notify()
    {
        DebugsList.testing.print("LocalStartMatch notify", debugColor, debug);
        executeStartMatchExecutor();
    }
    void executeStartMatchExecutor()
    {
        DebugsList.testing.print("LocalStartMatch executeStartMatchExecutor", debugColor, debug);
        RulesEvents.startPart.Invoke();
    }
}
