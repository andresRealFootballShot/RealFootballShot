using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class EventWithArgListener<T> : MonoBehaviour
{
    public EventWithArg<T> Event;
    public UnityEvent<T> Response;

    private void OnEnable()
    { Event.RegisterListener(this); }

    private void OnDisable()
    { Event.UnregisterListener(this); }

    public void OnEventRaised(T arg)
    { Response.Invoke(arg); }
}
