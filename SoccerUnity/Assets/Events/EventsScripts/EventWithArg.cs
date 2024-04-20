using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventWithArg<T> : MonoBehaviour
{
    public string info;
    private List<EventWithArgListener<T>> listeners =
        new List<EventWithArgListener<T>>();

    public void Raise(T arg)
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
            listeners[i].OnEventRaised(arg);
    }

    public void RegisterListener(EventWithArgListener<T> listener)
    { listeners.Add(listener); }

    public void UnregisterListener(EventWithArgListener<T> listener)
    { listeners.Remove(listener); }
}
