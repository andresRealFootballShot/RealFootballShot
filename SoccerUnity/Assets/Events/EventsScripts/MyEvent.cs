using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MyEvent
{
    protected stringDelegate triggersEvent;
    protected emptyDelegate listeners;
    string nameEvent;
    public string NameEvent { get => nameEvent; set => nameEvent = value; }
    public int countInvoked = 0;
    public bool isInvoked { get => countInvoked > 0; }
    public MyEvent(string nameEvent)
    {
        this.nameEvent = nameEvent;
    }
    public MyEvent()
    {
    }
    public void AddTriggerListener(stringDelegate function)
    {
        triggersEvent+=function;
    }
    public void RemoveTriggerListener(stringDelegate function)
    {
        triggersEvent -= function;
    }
    public void AddListener(emptyDelegate function)
    {
        listeners +=function;
    }
    public void AddListenerConsiderInvoked(emptyDelegate function)
    {
        listeners += function;
        if (isInvoked)
        {
            function();
        }
    }
    public void RemoveListener(emptyDelegate newListener)
    {
        if (listeners != null)
        {
            foreach (var listener in listeners.GetInvocationList())
            {
                if (listener.Equals(newListener))
                {
                    listeners -= newListener;
                }
            }
        }
    }
    public virtual void Invoke()
    {
        countInvoked++;
        triggersEvent?.Invoke(GetHashCode().ToString());
        listeners?.Invoke();
    }
}
public class MyEvent<T> : MyEvent
{
    public delegate void EventDelegate(T arg);
    List<T> allArgs = new List<T>();
    List<T> allInvokedArgs = new List<T>();
    List<T> allNotInvokedArgs = new List<T>();
    EventDelegate listeners;
    public MyEvent(string nameEvent) : base(nameEvent)
    {
        NameEvent = nameEvent;
    }
    public MyEvent() : base()
    {
    }
    public void AddListener(EventDelegate newListener)
    {
        listeners += newListener;
    }
    public void AddListenerConsiderInvoked(EventDelegate newListener)
    {
        listeners += newListener;
        if (isInvoked)
        {
            if (allArgs.Count > 0)
            {
                newListener(allArgs[allArgs.Count - 1]);
            }
        }
    }
    public void RemoveListener(EventDelegate newListener)
    {
        if (listeners != null)
        {
            foreach (var listener in listeners.GetInvocationList())
            {
                if (listener.Equals(newListener))
                {
                    listeners -= newListener;
                }
            }
        }
    }
    public void addArg(T arg)
    {
        allArgs.Add(arg);
        allNotInvokedArgs.Add(arg);
    }
    public void Invoke(T arg)
    {
        allArgs.Add(arg);
        allInvokedArgs.Add(arg);
        base.Invoke();
        listeners?.Invoke(arg);
    }
    
    public override void Invoke()
    {
        base.Invoke();
        foreach (var arg in allNotInvokedArgs)
        {
            allInvokedArgs.Add(arg);
            listeners?.Invoke(arg);
        }
        allNotInvokedArgs.Clear();
    }
    public T getLastArg()
    {
        if (allArgs.Count > 0)
        {
            return allArgs[allArgs.Count - 1];
        }
        else{
            return default;
        }
    }
    public T getLastInvokedArg()
    {
        if (allArgs.Count > 0)
        {
            return allInvokedArgs[allInvokedArgs.Count - 1];
        }
        else
        {
            return default;
        }
    }
    public List<T> getAllArgs()
    {
        return allArgs;
    }
    public List<T> getAllInvokedArgs()
    {
        return allInvokedArgs;
    }
    public List<T> getAllNotInvokedArgs()
    {
        return allNotInvokedArgs;
    }

    internal void AddListener()
    {
        throw new System.NotImplementedException();
    }
}
/*
public class Listener
{
    public EventDelegate listeners;
    List<T> argsList;
    public Listener(List<T> argsList, EventDelegate listener)
    {
        this.argsList = argsList;
        listeners += listener;
    }
    public Listener(EventDelegate listener)
    {
        this.argsList = new List<T>();
        listeners += listener;
    }
    public Listener()
    {
        this.argsList = new List<T>();
    }
    public void addArg(T arg)
    {
        argsList.Add(arg);
    }
    public void addListener(EventDelegate listener)
    {
        listeners += listener;
    }
    public void removeListener(EventDelegate listener)
    {
        listeners -= listener;
    }
    public void Inovke(T value)
    {
        listeners?.Invoke(value);
    }
}*/
