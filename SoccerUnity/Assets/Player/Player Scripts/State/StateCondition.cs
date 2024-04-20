using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateCondition
{
    boolDelegate condition;
    ITransition transition;
    State nextState;
    public event emptyDelegate nextStateEvent;
    IBehaviour behaviour;
    EventTrigger trigger;
    List<MyEvent> events = new List<MyEvent>();
    bool _eventsWereCalled;
    bool eventsWereCalled { get => _eventsWereCalled || events.Count == 0; set => _eventsWereCalled = value; }
    bool waitEndTransition;
    
    public StateCondition(boolDelegate condition, ITransition transition, IBehaviour behaviour, State nextState, bool waitEndTransition)
    {
        this.condition = condition;
        this.transition = transition;
        this.behaviour = behaviour;
        this.waitEndTransition = waitEndTransition;
        this.nextState = nextState;
    }
    public StateCondition(ITransition transition, IBehaviour behaviour, State nextState, bool waitEndTransition)
    {
        this.transition = transition;
        this.behaviour = behaviour;
        this.waitEndTransition = waitEndTransition;
        this.nextState = nextState;
    }
    public void addEvent(MyEvent myEvent){
        events.Add(myEvent);
    }
    public void setupTrigger()
    {
        eventsWereCalled = false;
        trigger = new EventTrigger();
        foreach (var item in events)
        {
            trigger.addTrigger(item,false,1,false);
        }
        trigger.addFunction(nofityEventsTriggered);
        trigger.endLoadTrigger();
    }
    void nofityEventsTriggered()
    {
        eventsWereCalled = true;
        if (condition?.GetInvocationList().Length == 0)
        {
            executeTransition();
        }
    }
    void executeTransition()
    {
        foreach (var item in events)
        {
            trigger.removeTrigger(item);
        }
        if (waitEndTransition)
        {
            transition.endTransition += notifyNextState;
        }
        else
        {
            notifyNextState();
        }
        behaviour.stopCoroutines();
        behaviour.startTransition(transition);
    }
    public void CheckCondition()
    {
        if (eventsWereCalled && condition())
        {
            executeTransition();
        }
    }
    public void notifyNextState()
    {
        MyFunctions.RemoveListener(transition.endTransition, notifyNextState);
        nextStateEvent?.Invoke();
        behaviour.setCurrentState(nextState);
        
    }
}
