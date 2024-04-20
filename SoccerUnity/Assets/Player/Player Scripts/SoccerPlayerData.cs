using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerPlayerData : MonoBehaviour
{
    public float maxKickForce = 33;
    public Vector3 ballTargetPosition { get; set; }
    public bool ballIsOrientedControlled { get; set; }
    public float scopeOffset=0.25f;
}
