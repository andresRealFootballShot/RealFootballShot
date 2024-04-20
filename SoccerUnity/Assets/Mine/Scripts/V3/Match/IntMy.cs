using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntMy : MonoBehaviour
{
    public delegate void IntDelegate(int value);
    public string info;
    public int initialValue;
    int value;
    public virtual int Value
    {
        get { return value; }
        set { this.value = value; Event?.Invoke(value); }
    }
    public event IntDelegate Event;
}
