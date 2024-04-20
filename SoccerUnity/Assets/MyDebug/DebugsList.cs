using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugsList : MonoBehaviour
{
    public static MyDebug testing;
    public MyDebug _testing;
    public static MyDebug rules;
    public MyDebug _rules;
    public static MyDebug RPCRequestFieldPosition;
    public MyDebug _RPCRequestFieldPosition;
    public static MyDebug errors;
    public MyDebug _errors;
    public void Setup()
    {
        testing = _testing;
        rules = _rules;
        RPCRequestFieldPosition = _RPCRequestFieldPosition;
        errors = _errors;
    }
}
