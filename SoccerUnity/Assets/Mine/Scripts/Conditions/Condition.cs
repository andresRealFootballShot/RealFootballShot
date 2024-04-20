using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Chain of Responsibility
public class Condition : MonoBehaviour
{
    public virtual bool isTrue()
    {
        return true;
    }
}
