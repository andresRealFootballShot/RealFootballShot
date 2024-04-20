using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lineup : MonoBehaviour
{
    public enum TypeLineup
    {
        Default
    }
    public string info;
    public TypeLineup typeLineup;
    public void setLineup(Lineup lineup)
    {
        typeLineup = lineup.typeLineup;
    }
    public void setLineup(TypeLineup typeLineup)
    {
        this.typeLineup = typeLineup;
    }
    public object[] getData()
    {
        return null;
    }
}
