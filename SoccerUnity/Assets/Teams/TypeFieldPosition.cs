using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeFieldPosition : MonoVariable<TypeFieldPosition.Type>
{
    public enum Type
    {
        None,
        CentreMidfield,
        LeftCentreMidfield,
        RightCentreMidfield,
        CentreLeftBack,
        CentreRightBack,
        CentreBack,
        GoalKeeper
    }
    public static Dictionary<Type, string> UIString = new Dictionary<Type, string>()
    {
    { Type.CentreMidfield, "CM" },
    { Type.LeftCentreMidfield, "CM" },
    { Type.RightCentreMidfield, "CM" }
    };
    public string getUIString()
    {
        return UIString[Value];
    }
}
