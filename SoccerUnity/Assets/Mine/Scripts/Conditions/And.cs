using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class And : Condition
{
    public Condition condition1, condition2;
    public override bool isTrue()
    {
        return condition1.isTrue() && condition2.isTrue();
    }
}
