using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GenericEventSC<T> : ScriptableObject
{
    private List<GenericEventListener<T>> listeners =
        new List<GenericEventListener<T>>();

    public void Raise(T value)
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
            listeners[i].OnEventRaised(value);
    }

    public void RegisterListener(GenericEventListener<T> listener)
    { listeners.Add(listener); }

    public void UnregisterListener(GenericEventListener<T> listener)
    { listeners.Remove(listener); }
}