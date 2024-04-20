using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompareStrings : Condition
{
    public MonoVariable<string> stringVariable1, stringVariable2;
    private void Start()
    {
    }
    public override bool isTrue()
    {
        
        return stringVariable1.Value.Equals(stringVariable2.Value);
    }
}
