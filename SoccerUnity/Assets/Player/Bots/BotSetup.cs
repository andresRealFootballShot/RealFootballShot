using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotSetup : MonoBehaviour
{
    Behaviour behaviour;
    
    public bool ballDriving = true;
    public bool followBall = true;
    public bool ballControl = true;
    public string typePlayerSkills;
    public List<PlayerSkills> playerSkillsList;
    void Start()
    {
        setupPlayerSkills();
        setup();
    }
    void setupPlayerSkills()
    {
        foreach (var item in playerSkillsList)
        {
            if (item.typePlayerSkills.Equals(typePlayerSkills))
            {
                PlayerComponents playerComponents = MyFunctions.GetComponentInChilds<PlayerComponents>(gameObject, true);
                playerComponents.playerSkills = Instantiate(item);
                break;
            }
        }
    }
    void setup()
    {
        behaviour = GetComponent<Behaviour>();
        FloatTransition normalStateTransition = new FloatTransition(1, 0.1f,true);
        //normalStateTransition.addFunction((x) => print("normalStateTransition " + x));
        FloatTransition defaultStateTransition = new FloatTransition(1, 0.1f,true);
        //defaultStateTransition.addFunction((x) => print("defaultStateTransition " + x));
        State normalState = new State("normal State");
        //normalState.addEntryFunction(() => print("normalState entry functions"));
        //normalState.addExitFunction(() => print("normalState exit functions"));
        
        MovementCtrl movementCtrl = MyFunctions.GetComponentInChilds<MovementCtrl>(gameObject, true);
        if (followBall)
        {
            FollowBall followBall = MyFunctions.GetComponentInChilds<FollowBall>(gameObject, true);
            normalState.addUpdateFunction(followBall.runOptimalPointToReachBall);
        }
        normalState.addUpdateFunction(movementCtrl.rotation);
        normalState.addUpdateFunction(movementCtrl.getAdjustedForwardVelocitySpeed);
        normalState.addUpdateFunction(movementCtrl.animator);
        
        if (ballDriving)
        {
            /*BallDriving ballDriving = MyFunctions.GetComponentInChilds<BallDriving>(gameObject, true);
            normalState.addEntryFunction(ballDriving.StartProcess);
            normalState.addExitFunction(ballDriving.StopProcess);*/
            IndividualPlay individualPlay = MyFunctions.GetComponentInChilds<IndividualPlay>(gameObject, true);
            normalState.addEntryFunction(individualPlay.StartProcess);
            normalState.addExitFunction(individualPlay.StopProcess);
        }
        if (ballControl)
        {
            BallControl ballControl = MyFunctions.GetComponentInChilds<BallControl>(gameObject, true);
            normalState.addEntryFunction(ballControl.StartProcess);
            normalState.addExitFunction(ballControl.StopProcess);
        }
        //normalState.addFixedUpdateFunctions(movementCtrl.movement);
        normalState.addUpdateFunction(movementCtrl.movement);
        //State defaultState = new State("default State");
        //defaultState.addEntryFunction(() => print("defaultState entry functions"));
        //defaultState.addExitFunction(() => print("defaultState exit functions"));
        //defaultState.addUpdateFunction((float a) => print(defaultState.ToString()));
        //StateCondition normalStateCondition = new StateCondition(normalCondition, normalStateTransition, behaviour, normalState, false);
        //StateCondition defaultStateCondition = new StateCondition(defaultCondition, defaultStateTransition, behaviour, defaultState, false);
        //normalState.addUpdateCondition(defaultStateCondition);
        //defaultState.addUpdateCondition(normalStateCondition);
        behaviour.setCurrentState(normalState);
    }
}
