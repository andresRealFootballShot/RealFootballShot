using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalChecker : MonoBehaviour, ILoad
{
    BallComponents componentsBall;
    public GoalComponents goalComponents;
    public GameObject inside, outside;
    //No cambiar por BoolEvent,enableGoal necesita EmptyEvent
    public EmptyEvent ballIsOutsideEvent, ballIsInsideEvent;
    bool computer,goalIsEnable;
    public static int _loadLevel = MatchEvents.staticLoadLevel + 1;
    public int loadLevel { get => _loadLevel; set => _loadLevel = value; }
    bool lastIsForward;
    
    public void Load(int level)
    {
        if (loadLevel == level)
        {
            Load();
        }
    }
    void Load()
    {
        MatchEvents.setMainBall.AddListenerConsiderInvoked(ballInstantiated);
        MatchEvents.continueMatch.AddListenerConsiderInvoked(() => goalIsEnable = true);
        setupIsOutside();
        componentsBall = FindObjectOfType<BallComponents>();
        if (componentsBall == null)
        {
            enabled = false;
        }
    }
    void ballInstantiated(GameObject ballGObj)
    {
        componentsBall = ballGObj.GetComponent<BallComponents>();
        Vector3 offset = new Vector3(0, 0, -componentsBall.radio);
        goalComponents.goalPlane.buildPlanes(offset);
        enabled = true;
    }
    void Update()
    {
        //checkIsInside();
        //checkIsInside();
    }
    public bool check()
    {
        bool isForward, isInside;
        //print(goalComponents.goalPlane+" "+ goalComponents.goalPlane.checkAreaLoaded());
        goalComponents.goalPlane.PointIsForward(componentsBall.rigBall.position, out isInside, out isForward);
        if (isInside && !isForward && lastIsForward)
        {
            if (!computer)
            {
                setupIsInside();
                ballIsInsideEvent.Raise();
                computer = !computer;
            }
            return true;
            /*if (goalIsEnable)
            {
                isGoal();
                goalIsEnable = !goalIsEnable;
                return true;
            }*/
        }
        else
        {
            if (computer)
            {
                setupIsOutside();
                ballIsOutsideEvent.Raise();
                computer = !computer;
            }
        }
        lastIsForward = isForward;
        return false;
    }
    
    public void enableGoal()
    {
        goalIsEnable = true;
    }
    void isGoal()
    {
        //DebugsList.rules.print("isGoal");
        //MatchEvents.goal.Invoke(new GoalEventArgs());
        //MatchEvents.stopMatch.Invoke();
    }
    void setupIsInside()
    {
        inside.SetActive(false);
        outside.SetActive(true);

    }
    void setupIsOutside()
    {
        inside.SetActive(true);
        outside.SetActive(false);
    }
}
