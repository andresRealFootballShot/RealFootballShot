using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericEventListener<T> : MonoBehaviour
{
    public string info;
    public GenericEventSC<T> Event;
    public UnityEvent<T> Response;
    int eventsInvokatedCount = 0;
    private void OnEnable()
    {
       Event.RegisterListener(this);
    }
    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(T value)
    {
        Response.Invoke(value);
    }
}
