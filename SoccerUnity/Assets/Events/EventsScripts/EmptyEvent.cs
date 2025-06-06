using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyEvent : MonoBehaviour
{
    public string info;
    public TypeEvent typeEvent;
    private List<EmptyEventListListener> listeners =
        new List<EmptyEventListListener>();

    public void Raise()
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
            listeners[i].OnEventRaised(this);
    }

    public void RegisterListener(EmptyEventListListener listener)
    { listeners.Add(listener); }

    public void UnregisterListener(EmptyEventListListener listener)
    { listeners.Remove(listener); }
}
