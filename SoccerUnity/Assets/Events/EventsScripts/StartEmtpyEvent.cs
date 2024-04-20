using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartEmtpyEvent : MonoBehaviour
{
    public EmptyEvent emptyEvent;
    void Start()
    {
        emptyEvent.Raise();
    }
}
