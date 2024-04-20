using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCoroutine
{
    public event emptyDelegate end;
    List<emptyDelegate> functions = new List<emptyDelegate>();
    List<conditionDelegate> conditions = new List<conditionDelegate>();
    IEnumerator coroutine;
    public void addFunction(emptyDelegate function)
    {
        functions.Add(function);
    }
    public void addCondition(conditionDelegate condition)
    {
        conditions.Add(condition);
    }
    public IEnumerator Coroutine(float period)
    {
        while (!checkAllConditions())
        {
            foreach (var item in functions)
            {
                item();
            }
            yield return new WaitForSeconds(period);
        }
        end?.Invoke();
    }
    bool checkAllConditions()
    {
        foreach (var item in conditions)
        {
            if (!item())
            {
                return false;
            }
        }
        return true;
    }
}
