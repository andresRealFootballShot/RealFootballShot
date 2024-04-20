using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum TypeOfCommunication
{
    RPC,
    CustomProperties
}
public class OnlineSettings : MonoBehaviour, ILoad
{
    public static int staticLoadLevel = 0;
    public int loadLevel { get => staticLoadLevel; set => staticLoadLevel = value; }
    public TypeOfCommunication _positionRequestCommunicationType;
    public static TypeOfCommunication positionRequestCommunicationType;
    public void Load(int level)
    {
        if (loadLevel == level)
        {
            Load();
        }
    }
    void Load()
    {
        positionRequestCommunicationType = _positionRequestCommunicationType;
    }
}
