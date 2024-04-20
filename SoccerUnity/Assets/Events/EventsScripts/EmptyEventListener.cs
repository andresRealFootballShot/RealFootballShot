using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyEventListener : MonoBehaviour
{
    List<emptyDelegate> functions;
    List<EmptyEventV2> list = new List<EmptyEventV2>();
    public void addFunction(emptyDelegate function)
    {
        functions.Add(function);
    }
    public void removeFunction(emptyDelegate function)
    {
        functions.Remove(function);
    }
    public void addEvent(EmptyEventV2 emptyEvent)
    {
        list.Add(emptyEvent);
        emptyEvent.Event += callback;
    }
    public void removeEvent(EmptyEventV2 emptyEvent)
    {
        list.Remove(emptyEvent);
        emptyEvent.Event -= callback;
    }
    void callback(EmptyEventV2 emptyEvent)
    {
        if (list.Contains(emptyEvent))
        {
            list.Remove(emptyEvent);
            if (list.Count == 0)
            {
                foreach (var item in functions)
                {
                    item?.Invoke();
                }
            }
        }
        else
        {
            Debug.LogError("Este evento no está almacenado");
        }
    }
}
