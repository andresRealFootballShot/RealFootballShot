using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void EmptyDelegateV2(EmptyEventV2 emptyEvent);
public class EmptyEventV2
{
    public event EmptyDelegateV2 Event;
    public void Invoke()
    {
        Event?.Invoke(this);
    }
}
