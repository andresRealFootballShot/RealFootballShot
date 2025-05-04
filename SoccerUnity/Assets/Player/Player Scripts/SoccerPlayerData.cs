using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerPlayerData : SoccerPlayerComponent
{
    public new float maxKickForce = 33;
    public new Vector3 ballTargetPosition { get; set; }
    public new bool ballIsOrientedControlled { get; set; }
    public  float scopeOffset=0.25f;
}
