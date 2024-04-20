using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3Event : MonoBehaviour
{
    public string info;
    public enum TypeEvent { Kick };
    public TypeEvent typeEvent;
    public delegate void Vector3Delegate(Vector3 value);
    public event Vector3Delegate Event;
    public void Invoke(Vector3 value) => Event?.Invoke(value);
}
