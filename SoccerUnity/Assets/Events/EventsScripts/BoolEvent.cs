using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoolEvent : MonoBehaviour
{
    public string info;
    public TypeEvent typeEvent;
    private List<BoolEventListener> listeners =
        new List<BoolEventListener>();

    public void Raise(bool value)
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
            listeners[i].OnEventRaised(value);
    }

    public void RegisterListener(BoolEventListener listener)
    { listeners.Add(listener); }

    public void UnregisterListener(BoolEventListener listener)
    { listeners.Remove(listener); }
}
