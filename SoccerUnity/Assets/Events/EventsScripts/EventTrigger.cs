using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class EventTrigger
{
    class TriggerData
    {
        enum TypeConsiderEventInvoked
        {
            LastInvoked,
            All,
            None
        }
        public string id;
        public string name;
        public bool infinite;
        public int count;
        public MyEvent _event;
        public bool considerEventCountInvoked;
        public TriggerData(string id,string name,bool infinite,int count)
        {
            this.id = id;
            this.name = name;
            this.infinite = infinite;
            this.count = count;
        }
        public TriggerData(string id,string name, bool infinite, int count, MyEvent _event, bool considerEventCountInvoked)
        {
            this.name = name;
            this.infinite = infinite;
            this.count = count;
            this._event = _event;
            this.considerEventCountInvoked = considerEventCountInvoked;
            this.id = id;
        }
        public override string ToString()
        {
            string unityEventNull = "";
            if (_event == null)
            {
                unityEventNull = "Null";
            }
            return name + " "+infinite+" "+ count + " "+ unityEventNull;
        }
        public int getCount()
        {
            if (considerEventCountInvoked && _event!=null)
            {
                return count - _event.countInvoked;
            }
            else
            {
                return count;
            }
        }
        public void substractCounter()
        {
            if (!considerEventCountInvoked)
            {
                count--;
            }
        }
    }
    List<MyEvent> waitingEvents = new List<MyEvent>();
    List<emptyDelegate> waitingFunctions = new List<emptyDelegate>();
    Dictionary<string,TriggerData> triggers = new Dictionary<string, TriggerData>();
    string triggerName;
    protected bool printRunTrigger;
    public EventTrigger(){}
    public EventTrigger(bool printRunTrigger) { this.printRunTrigger = printRunTrigger; }
    public EventTrigger(string _triggerName) { triggerName = _triggerName; }
    public EventTrigger(MyEvent triggerEvent, bool infinite, int count, bool considerEventCountInvoked)
    {
        addTrigger(triggerEvent,infinite,count,considerEventCountInvoked);
    }

    public void addEvent(MyEvent waitingEvent)
    {
        waitingEvents.Add(waitingEvent);
    }
    public void addFunction(emptyDelegate waitingFunction)
    {
        waitingFunctions.Add(waitingFunction);
    }
    public void removeEvent(MyEvent waitingEvent)
    {
        waitingEvents.Remove(waitingEvent);
    }
    public void removeFunction(emptyDelegate waitingFunction)
    {
        waitingFunctions.Remove(waitingFunction);
    }
    public void addTrigger(MyEvent triggerEvent,bool infinite,int count,bool considerEventCountInvoked)
    {
        TriggerData newTriggerData = new TriggerData(triggerEvent.GetHashCode().ToString(),triggerEvent.NameEvent, infinite, count, triggerEvent, considerEventCountInvoked);
        triggers.Add(triggerEvent.GetHashCode().ToString(), newTriggerData);
        triggerEvent.AddTriggerListener(runTrigger);
    }
    public void addTrigger(MyEvent triggerEvent, bool infinite, int count, bool considerEventCountInvoked,bool printRunTrigger)
    {
        TriggerData newTriggerData = new TriggerData(triggerEvent.GetHashCode().ToString(), triggerEvent.NameEvent, infinite, count, triggerEvent, considerEventCountInvoked);
        triggers.Add(triggerEvent.GetHashCode().ToString(), newTriggerData);
        triggerEvent.AddTriggerListener(runTrigger);
        this.printRunTrigger = printRunTrigger;
    }
    public void endLoadTrigger()
    {
        if (printRunTrigger)
        {
            foreach (var item in triggers)
            {
                //Debug.Log("runTrigger "+triggerName + " | "+this.triggerName);
                if (item.Value.getCount() <= 0)
                {
                    Debug.Log("Event " + item.Key + " triggered");
                }
            }
        }
        runTrigger();
    }
    public void addTrigger(emptyDelegate trigger,string name, bool infinite, int count)
    {
        triggers.Add(trigger.GetHashCode().ToString(),new TriggerData(trigger.GetHashCode().ToString(), name, infinite, count));
        
    }
    public void addTrigger(MyWaitUntil waitUntil, bool infinite, int count)
    {
        triggers.Add(waitUntil.GetHashCode().ToString(), new TriggerData(waitUntil.GetHashCode().ToString(), waitUntil.name, infinite, count));
        waitUntil.addTrigger(runTrigger);
    }
    public void removeTrigger(MyEvent triggerEvent)
    {
        triggers.Remove(triggerEvent.GetHashCode().ToString());
        triggerEvent.RemoveTriggerListener(runTrigger);
    }
    public void removeTrigger(string triggerName)
    {
        triggers.Remove(triggerName);
    }
    public void runTrigger(string triggerName)
    {
        
        if (printRunTrigger)
        {
            //Debug.Log("runTrigger "+triggerName + " | "+this.triggerName);
            Debug.Log("Event "+triggerName+" triggered");
        }
        if (triggers.ContainsKey(triggerName))
        {
            TriggerData triggerData = triggers[triggerName];
            //Debug.Log("triggerData before " + triggerData.ToString() + " | "+triggerName);
            if (triggerData.count>0)
            {
                triggerData.substractCounter();
                if (checkInvoke())
                {
                    //Debug.Log("checkInvoke()");
                    Invoke();
                }
                checkRemove(triggerData);
            }
            else if(triggerData.infinite)
            {
                //Debug.Log("triggerData.infinite");
                if (checkInvoke())
                {
                    //Debug.Log("checkInvoke()");
                    Invoke();
                }
            }
            else
            {
                //Debug.Log("else");
                checkRemove(triggerData);
            }
            //Debug.Log("triggerData after" + triggers[triggerName].ToString());
        }
        else
        {
            Debug.LogError("Triggers dont contains "+triggerName);
        }
    }
    public void runTrigger()
    {
        if (checkInvoke())
        {
            //Debug.Log("checkInvoke()");
            Invoke();
        }
        checkAllRemove();
    }
    bool checkInvoke()
    {
        foreach (var item in triggers)
        {
            //Debug.Log("checkInvoke" + " | " + triggerName + " | "+item.Value.name + " | " + item.Value.getCount());
            if (item.Value.getCount() > 0)
            {
                return false;
            }
        }
        return true;
    }
    protected virtual void Invoke()
    {
        //Debug.Log("Invoke" + " | " + triggerName + " | " + waitingFunctions.Count);
        if (printRunTrigger)
        {
            Debug.Log("Trigger "+triggerName+" is Invoked");
        }
        foreach (var item in waitingEvents)
        {
            item.Invoke();
        }
        foreach (var item in waitingFunctions)
        {
            item();
        }
    }
    void checkRemove(TriggerData triggerData)
    {
        if(!triggerData.infinite && triggerData.getCount() <= 0)
        {
            if (triggerData._event != null)
            {
                triggerData._event.RemoveTriggerListener(runTrigger);
            }
            triggers.Remove(triggerData.id);
            //Debug.Log("Remove "+ triggerData.name);
        }
    }
    void checkAllRemove()
    {
        List<string> listToDelete = new List<string>();
        foreach (var trigger in triggers)
        {
            if (!trigger.Value.infinite && trigger.Value.getCount() <= 0)
            {
                if (trigger.Value._event != null)
                {
                    trigger.Value._event.RemoveTriggerListener(runTrigger);
                }
                listToDelete.Add(trigger.Value.id);
                //Debug.Log("Remove "+ triggerData.name);
            }
        }
        foreach (var item in listToDelete)
        {
            triggers.Remove(item);
        }
    }
}
public class EventTrigger<T> : EventTrigger
{
    public delegate void EventDelegate(T arg);
    public EventDelegate waitingFunctions;
    T arg;
    public EventTrigger(bool printRunTrigger) { this.printRunTrigger = printRunTrigger; }
    public EventTrigger() {}

    protected override void Invoke()
    {
        base.Invoke();
        waitingFunctions?.Invoke(arg);
    }
    public void addFunction(EventDelegate function, T arg)
    {
        this.arg = arg;
        waitingFunctions += function;
    }
}


