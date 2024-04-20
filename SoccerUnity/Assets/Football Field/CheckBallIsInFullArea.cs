using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckBallIsInFullArea : MonoBehaviour,IClearBeforeLoadScene
{
    public static MyEvent PointExitEvent = new MyEvent(nameof(PointExitEvent));
    public static Variable<CheckBallIsInFullArea> checkBallIsInFullArea = new Variable<CheckBallIsInFullArea>();
    public Area area;
    public Transform pointTrans;
    bool wasInside = true;
    static bool state;
    void Awake()
    {
        checkBallIsInFullArea.Value = this;
    }
    public static void SetState(bool _state)
    {
        state = _state;
        
        if (!checkBallIsInFullArea.isNull)
        {
            DebugsList.testing.print("CheckBallIsInFullArea.State() 1 " + state, Color.cyan);
            checkBallIsInFullArea.Value.enabled = state;
        }
        else
        {
            DebugsList.testing.print("CheckBallIsInFullArea.State() 2 " + state, Color.cyan);
            checkBallIsInFullArea.addWaitingFunctionIfNotContains(()=> checkBallIsInFullArea.Value.enabled = state);
        }
    }
    void Update()
    {
        if (!MatchComponents.footballField.fullFieldArea.PointIsInside(MatchComponents.ballComponents.transBall.position))
        {
            if (wasInside)
            {
                PointExitEvent.Invoke();
                wasInside = false;
            }
        }
        else
        {
            wasInside = true;
        }
    }

    public void Clear()
    {
        PointExitEvent = new MyEvent(nameof(PointExitEvent));
        checkBallIsInFullArea = new Variable<CheckBallIsInFullArea>();
    }
}
