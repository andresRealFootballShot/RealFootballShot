using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behaviour : MonoBehaviour,IBehaviour
{
    class CoroutineData
    {
        public bool interruptible;
        public Coroutine coroutine;

        public CoroutineData(bool interruptible, Coroutine coroutine)
        {
            this.interruptible = interruptible;
            this.coroutine = coroutine;
        }
    }
    State currentState;
    List<CoroutineData> transitions = new List<CoroutineData>();
    public void setCurrentState(State currentState)
    {
        this.currentState = currentState;
        currentState.executeEntryFunctions();
    }
    public void startTransition(ITransition transition)
    {
        CoroutineData coroutineData = new CoroutineData(transition.isInterruptible, StartCoroutine(transition.Coroutine()));
        transitions.Add(coroutineData);
    }
    public void stopCoroutines()
    {
        foreach (var transition in transitions)
        {
            if (transition.interruptible)
            {
                StopCoroutine(transition.coroutine);
            }
        }
    }
    public void UpdateRun()
    {
        currentState.checkUpdateConditions(Time.deltaTime);
        currentState.executeUpdateFunctions(Time.deltaTime);
    }
    public void FixedUpdateRun()
    {
        currentState.checkFixedUpdateConditions(Time.fixedDeltaTime);
        currentState.executeFixedUpdateFunctions(Time.fixedDeltaTime);
    }
    void Update()
    {
        UpdateRun();

    }
    private void FixedUpdate()
    {
        FixedUpdateRun();
    }
}
