using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EmptyEventListListener : MonoBehaviour
{
    public string info;
    public List<EmptyEventSC> eventSCList;
    public List<EmptyEvent> eventList;
    Dictionary<EmptyEventSC,bool> eventSCIsInvokated;
    Dictionary<EmptyEvent, bool> eventIsInvokated;
    public UnityEvent Response;
    int eventsInvokatedCount = 0;
    private void OnEnable()
    {
        eventSCIsInvokated = new Dictionary<EmptyEventSC, bool>();
        foreach (EmptyEventSC Event in eventSCList)
        {
            Event.RegisterListener(this);
            eventSCIsInvokated.Add(Event,false);
        }
        eventIsInvokated = new Dictionary<EmptyEvent, bool>();
        foreach (EmptyEvent Event in eventList)
        {
            Event.RegisterListener(this);
            eventIsInvokated.Add(Event, false);
        }
    }

    private void OnDisable()
    {
        foreach (EmptyEventSC Event in eventSCList)
        {
            Event.UnregisterListener(this);
        }
        foreach (EmptyEvent Event in eventList)
        {
            Event.UnregisterListener(this);
        }
    }
    public void OnEventRaised(EmptyEventSC emptyEventSC)
    {
        if (!eventSCIsInvokated[emptyEventSC])
        {
            eventsInvokatedCount++;
            eventSCIsInvokated[emptyEventSC] = true;
        }
        if(eventsInvokatedCount== eventSCList.Count + eventList.Count)
        {
            Response.Invoke();
        }
    }
    public void OnEventRaised(EmptyEvent emptyEvent)
    {
        if (!eventIsInvokated[emptyEvent])
        {
            eventsInvokatedCount++;
            eventIsInvokated[emptyEvent] = true;
        }
        if (eventsInvokatedCount == eventSCList.Count + eventList.Count)
        {
            Response.Invoke();
        }
    }
}
