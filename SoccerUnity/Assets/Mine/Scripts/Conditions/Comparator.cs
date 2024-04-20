using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Comparator<T> : Condition
{
    public MonoVariable<T> value1, value2;
    public override bool isTrue()
    {
        if(value1.Value != null && value2.Value != null)
        {
            return value1.Value.Equals(value2.Value);
        }
        else
        {
            return false;
        }
    }
}
