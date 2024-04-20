using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Variable
{
    public emptyDelegate waitingFunctions;
    public event emptyDelegate EmptyVariableChanged;
    public bool isNull = true;
    public MyEvent VariableChangedEmptyMyEvent;
    public Variable()
    {
        VariableChangedEmptyMyEvent = new MyEvent(GetHashCode().ToString());
    }
    public void addObserver(emptyDelegate observer)
    {
        EmptyVariableChanged += observer;
    }
    public void removeObserver(emptyDelegate observer)
    {
        EmptyVariableChanged -= observer;
    }
    public void addWaitingFunction(emptyDelegate function)
    {
        waitingFunctions += function;
    }
    public void removeWaitingFunction(emptyDelegate function)
    {
        waitingFunctions -= function;
    }
    public void addWaitingFunctionIfNotContains(emptyDelegate function)
    {
        if (!isNull)
        {
            function();
        }
        else
        {
            if (waitingFunctions != null)
            {
                foreach (var waitingFunction in waitingFunctions.GetInvocationList())
                {
                    if (function.Equals(waitingFunction))
                    {
                        return;
                    }
                }
                waitingFunctions += function;
            }
            else
            {
                waitingFunctions += function;
            }
        }
    }
    public void InvokeEmptyVariableChanged()=> EmptyVariableChanged?.Invoke();
}
public class Variable<T> : Variable
{
    public string info;
    [SerializeField]
    T value;
    public VariableDelegate waitingFunctionsT;
    public delegate void VariableDelegate(T value);
    public event VariableDelegate VariableChanged;
    public MyEvent<T> VariableChangedMyEvent;
    public virtual T Value { get { return value; } set { setValue(value); } }
    public bool Equal(MonoVariable<T> other) { return Value.Equals(other.Value); }
    public override string ToString()
    {
        return Value.ToString();
    }
    void setValue(T value)
    {
        this.value = value;
        checkWaitingFunctions();
        InvokeEmptyVariableChanged(); 
        VariableChanged?.Invoke(value);
        VariableChangedEmptyMyEvent.Invoke();
        VariableChangedMyEvent.Invoke(value);
    }
    void checkWaitingFunctions()
    {
        if (isNull)
        {
            isNull = false;
            waitingFunctions?.Invoke();
            waitingFunctionsT?.Invoke(value);

        }
    }
    public Variable(T value){

        VariableChangedMyEvent = new MyEvent<T>(GetHashCode().ToString());
        this.value = value;
    }

    public void addObserverAndExecuteIfValueNotIsNull(VariableDelegate observer)
    {
        VariableChanged += observer;
        if (!isNull)
        {
            observer(value);
        }
    }
    public void addObserverAndExecuteIfValueNotIsNull(emptyDelegate observer)
    {
        EmptyVariableChanged += observer;
        if (!isNull)
        {
            observer();
        }
    }
    public void addObserver(VariableDelegate observer)
    {
        VariableChanged += observer;
    }
    public void removeObserver(VariableDelegate observer)
    {
        VariableChanged -= observer;
    }
    public void addWaitingFunctionIfNotContains(VariableDelegate function)
    {
        if (!isNull)
        {
            function(value);
        }
        else
        {
            if (waitingFunctions != null)
            {
                foreach (var waitingFunction in waitingFunctionsT.GetInvocationList())
                {

                    if (function.Equals(waitingFunction))
                    {
                        return;
                    }
                }
                waitingFunctionsT += function;
            }
            else
            {

                waitingFunctionsT += function;
            }
        }
    }
    public void removeWaitingFunction(VariableDelegate function)
    {
        waitingFunctionsT -= function;
    }
    public Variable()
    {
        VariableChangedMyEvent = new MyEvent<T>(GetHashCode().ToString());
    }
}
