using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringMy : MonoBehaviour
{
    public delegate void StringDelegate(string value);
    public event StringDelegate Event;
    public string info;
    protected string value;
    public virtual string Value
    {
        get { return value; }
        set { this.value = value; Event?.Invoke(value); }
    }
    public void InvokeValueChanged(string value) => Event?.Invoke(value);
}
