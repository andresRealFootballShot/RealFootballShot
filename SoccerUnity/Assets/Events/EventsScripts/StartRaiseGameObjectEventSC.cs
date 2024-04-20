using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartRaiseGameObjectEventSC : MonoBehaviour
{
    public GameObjectEventSC Event;
    public GameObject argument;
    void Start()
    {
        Event.Raise(argument);
    }
}
