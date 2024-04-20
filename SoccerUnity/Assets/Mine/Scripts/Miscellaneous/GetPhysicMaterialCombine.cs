using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPhysicMaterialCombine
{
    public static PhysicMaterialCombine getPhysicMaterialCombine(PhysicMaterialCombine a, PhysicMaterialCombine b)
    {
        if (a == PhysicMaterialCombine.Maximum || b == PhysicMaterialCombine.Maximum)
        {
            return PhysicMaterialCombine.Maximum;
        }
        else if (a == PhysicMaterialCombine.Multiply || b == PhysicMaterialCombine.Multiply)
        {
            return PhysicMaterialCombine.Multiply;
        }
        else if (a == PhysicMaterialCombine.Minimum || b == PhysicMaterialCombine.Minimum)
        {
            return PhysicMaterialCombine.Minimum;
        }
        else
        {
            return PhysicMaterialCombine.Average;
        }
    }
    public static float getCombination(float a, float b, PhysicMaterialCombine physicMaterialCombine)
    {
        switch (physicMaterialCombine)
        {
            case PhysicMaterialCombine.Average:
                return (a + b) / 2;
            case PhysicMaterialCombine.Maximum:
                return a > b ? a : b;
            case PhysicMaterialCombine.Minimum:
                return a > b ? b : a;
            case PhysicMaterialCombine.Multiply:
                return a * b;
            default:
                return a;
        }
    }
}
