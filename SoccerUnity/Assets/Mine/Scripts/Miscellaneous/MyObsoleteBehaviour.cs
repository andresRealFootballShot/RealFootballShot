using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyObsoleteBehaviour
{
    List<ObsoleteState> enabledStates = new List<ObsoleteState>();
    //Para saber el orden en que deben ejecutarse los estados. Los eventos quitan y añaden estados a la lista states
    Dictionary<string, int> statesOrder = new Dictionary<string, int>();
    List<Calculo> calculos = new List<Calculo>();
    ObsoleteState lastActiveState;
    emptyDelegate currentExecution;
    public bool enabled;
    public MyObsoleteBehaviour(bool _enabled)
    {
        enabled = _enabled;
        if (_enabled)
        {
            currentExecution = EnableExecution;
        }
        else
        {
            currentExecution = DisableExecution;
        }
    }
    public MyObsoleteBehaviour()
    {
        currentExecution = EnableExecution;
        enabled = true;
    }
    public void Enable()
    {
        enabled = true;
        currentExecution = EnableExecution;
    }
    public void Disable()
    {
        enabled = false;
        currentExecution = DisableExecution;
    }
    void DisableExecution() { 
        //Debug.Log("DisableExecution"); 
    }
    void EnableExecution()
    {
        
        executeCalculos();
        //Debug.Log("EnableExecution");
        foreach (var state in enabledStates)
        {
            //Debug.Log("EnableExecution "+ state.checkConditions());
            if (state.checkConditions())
            {
                if (lastActiveState != state)
                {
                    lastActiveState = state;
                    state.activeListener();
                    state.ExecuteEnterFunctions();
                    List<ObsoleteState> states2 = new List<ObsoleteState>();
                    states2.AddRange(enabledStates);
                    states2.Remove(state);
                    foreach (var state2 in states2)
                    {
                        state2.deactiveListener();
                        state.ExecuteExitFunctions();
                    }
                }
                state.Execute();
                return;
            }
        }
    }
    public void Execute()
    {
        currentExecution();
    }
    public void addState(ObsoleteState state,bool stateIsEnable)
    {
        state.disableEvent += disableState;
        state.enableEvent += enableState;
        statesOrder.Add(state.name, statesOrder.Count);
        if (stateIsEnable)
        {
            addState(state);
        }

    }
    void addState(ObsoleteState state)
    {
        int index = Mathf.Clamp(statesOrder[state.name], 0, enabledStates.Count);
        enabledStates.Insert(index, state);
    }
    void enableState(ObsoleteState state)
    {
        if (!enabledStates.Contains(state))
        {
            addState(state);
        }
    }
    void disableState(ObsoleteState state)
    {
        if (enabledStates.Contains(state))
        {
            enabledStates.Remove(state);
        }
    }
    public void addCalculo(Calculo calculo)
    {
        calculo.enableEvent += enableCalculo;
        calculo.disableEvent += disableCalculo;
    }
    void enableCalculo(Calculo calculo)
    {
        calculos.Add(calculo);
        executeCalculos();
    }
    void disableCalculo(Calculo calculo)
    {
        calculos.Remove(calculo);
    }
    void executeCalculos()
    {
        foreach (var calculo in calculos)
        {
            calculo.Execute();
        }
    }
}
public class Calculo
{
    emptyDelegate functions;
    Dictionary<string,bool> listeners = new Dictionary<string, bool>();
    public calculoDelegate enableEvent, disableEvent;
    int activeListenersCount = 0;
    bool active = false;
    public Calculo() { }
    public Calculo(emptyDelegate function)
    {
        functions+=function;
    }
    public void Execute()
    {
       functions();
    }
    public void activateListener(string state)
    {
        if (!listeners[state])
        {
            listeners[state] = true;
            activeListenersCount++;
            checkActive();
        }
    }
    public void deactivateListener(string state)
    {
        if (listeners[state])
        {
            listeners[state] = false;
            activeListenersCount--;
            checkNoActive();
        }
    }
    void checkActive()
    {
        
        if (activeListenersCount > 0 && !active)
        {
            enableEvent?.Invoke(this);
            active = true;
        }
    }
    void checkNoActive()
    {
        if (activeListenersCount == 0 && active)
        {
            disableEvent?.Invoke(this);
            active = false;
        }
    }
    public void addFunction(emptyDelegate function)
    {
        functions+=function;
    }

    public void addListener(ObsoleteState state)
    {
        listeners.Add(state.name, false);
        state.addCalculo(this);
    }
}
public class ObsoleteState
{
    static int index = 0;
    public string name;
    emptyDelegate functions,enterFunctions,exitFunctions;
    
    List<boolDelegate> conditions = new List<boolDelegate>();
    public stateDelegate enableEvent, disableEvent;
    List<Calculo> calculos = new List<Calculo>();
    public ObsoleteState(string _name)
    {
        name = _name + index.ToString();
        index++;
    }
    public ObsoleteState()
    {
        name = index.ToString();
        index++;
    }
    public void Execute()
    {
        functions?.Invoke();
    }
    public void ExecuteEnterFunctions()
    {
        enterFunctions?.Invoke();
    }
    public void ExecuteExitFunctions()
    {
        exitFunctions?.Invoke();
    }
    public void deactiveListener()
    {
        foreach (var calculo in calculos)
        {
            calculo.deactivateListener(name);
        }
    }
    public void activeListener()
    {
        foreach (var calculo in calculos)
        {
            calculo.activateListener(name);
        }
    }
    public void addCalculo(Calculo calculo)
    {
        calculos.Add(calculo);
    }
    public void addBehaviour(MyObsoleteBehaviour behaviour)
    {
        functions+=() => behaviour.Execute();
    }
    public void addCondition(boolDelegate condition)
    {
        conditions.Add(condition);
    }
    public void addFunction(emptyDelegate function)
    {
        functions+=function;
    }
    public void addEnterFunction(emptyDelegate function)
    {
        enterFunctions += function;
    }
    public void addExitFunction(emptyDelegate function)
    {
        exitFunctions += function;
    }
    public void addEnableTrigger(EventTrigger trigger)
    {
        trigger.addFunction(() => enableEvent.Invoke(this));
    }
    public void addDisableTrigger(EventTrigger trigger)
    {
        trigger.addFunction(() => disableEvent.Invoke(this));
    }
    public void addEnableTrigger(MyEvent _event)
    {
        _event.AddListener(()=>enableEvent.Invoke(this));
    }
    public void addDisableTrigger(MyEvent _event)
    {
        _event.AddListener(() => disableEvent.Invoke(this));
    }
    public bool checkConditions()
    {
        foreach (var condition in conditions)
        {
            if (!condition())
            {
                return false;
            }
        }
        return true;
    }
}
