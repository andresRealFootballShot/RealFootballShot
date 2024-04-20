using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetVariables : MonoBehaviour
{
    public FloatNet maxSpeedNet, velocityNet,resistanceNet, maximumJumpForceNet;
    public StringNet playerName;

    public void Awake()
    {
        /*
        PublicPlayerData publicPlayerData = GetComponent<PublicPlayerData>();
        maxSpeedNet.variable = publicPlayerData.maxSpeedVar;
        velocityNet.variable = publicPlayerData.velocityVar;
        playerName.variable = publicPlayerData.namePlayerVar;*/
    }
}
