using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class MyWaitUntil
{
    List<Func<bool>> conditions = new List<Func<bool>>();
    List<emptyDelegate> functions = new List<emptyDelegate>();
    stringDelegate trigger;
    public string name;
    public MyWaitUntil(string _name)
    {
        name = _name;
    }
    public void addCondition(Func<bool> condition)
    {
        conditions.Add(condition);
    }
    public void addFunction(emptyDelegate function)
    {
        functions.Add(function);
    }
    public void addTrigger(stringDelegate _trigger)
    {
        trigger += _trigger;
    }
    public IEnumerator coroutine()
    {
        foreach (var condition in conditions)
        {
            yield return new WaitUntil(condition);
        }
        foreach (var function in functions)
        {
            function();
        }
        trigger?.Invoke(name);
    }
}
