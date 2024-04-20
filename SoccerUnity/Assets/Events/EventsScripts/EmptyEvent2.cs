using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyEvent2 : MonoBehaviour
{
    public string info;
    public enum TypeEvent { Kick,LosePossession};
    public TypeEvent typeEvent;
    public delegate void EmptyDelegate();
    public event EmptyDelegate Event;
    public void Invoke() => Event?.Invoke();
}
