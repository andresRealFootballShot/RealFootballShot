using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    floatDelegate updateFunctions;
    floatDelegate fixedUpdateFunctions;
    emptyDelegate entryFunctions,exitFunctions;
    List<StateCondition> updateConditions = new List<StateCondition>();
    List<StateCondition> fixedUpdateConditions = new List<StateCondition>();
    string stateName;
    bool isRunning;
    public State(string stateName)
    {
        this.stateName = stateName;
    }
    public void addUpdateFunction(floatDelegate function)
    {
        updateFunctions += function;
    }
    public void addEntryFunction(emptyDelegate function)
    {
        entryFunctions += function;
    }
    public void addExitFunction(emptyDelegate function)
    {
        exitFunctions += function;
    }
    public void addFixedUpdateFunctions(floatDelegate function)
    {
        fixedUpdateFunctions += function;
    }
    public void executeEntryFunctions()
    {
        isRunning = true;
        foreach (var item in updateConditions)
        {
            item.nextStateEvent += executeExitFunctions;
            item.setupTrigger();
        }
        foreach (var item in fixedUpdateConditions)
        {
            item.nextStateEvent += executeExitFunctions;
            item.setupTrigger();
        }
        entryFunctions?.Invoke();
        

    }
    public void executeExitFunctions()
    {
        isRunning = false;
        foreach (var item in updateConditions)
        {
            item.nextStateEvent -= executeExitFunctions;
        }
        foreach (var item in fixedUpdateConditions)
        {
            item.nextStateEvent -= executeExitFunctions;
        }
        exitFunctions?.Invoke();

    }
    public void addUpdateCondition(StateCondition condition)
    {
        updateConditions.Add(condition);
    }
    public void addFixedUpdateCondition(StateCondition condition)
    {
        fixedUpdateConditions.Add(condition);
    }
    public void checkUpdateConditions(float deltaTime)
    {
        foreach (var item in updateConditions)
        {
            item.CheckCondition();
        }
    }
    public void checkFixedUpdateConditions(float deltaTime)
    {
        foreach (var item in fixedUpdateConditions)
        {
            item.CheckCondition();
        }
    }
    public void executeUpdateFunctions(float deltaTime)
    {
        updateFunctions?.Invoke(deltaTime);
    }
    public void executeFixedUpdateFunctions(float deltaTime)
    {
        fixedUpdateFunctions?.Invoke(deltaTime);
    }
    public override string ToString()
    {
        return stateName +" isRunning="+isRunning;
    }
}
