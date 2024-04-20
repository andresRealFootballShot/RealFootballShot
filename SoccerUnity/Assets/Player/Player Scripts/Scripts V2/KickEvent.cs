using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickEvent : MonoBehaviour
{
    public delegate void Vector3Delegate(Vector3 value);
    public event Vector3Delegate Event;
    public void Invoke(Vector3 value) => Event?.Invoke(value);
}
