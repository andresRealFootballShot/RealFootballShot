using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallTargetPositionCtrl : SoccerPlayerComponent
{
    public Transform targetPosition;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ballTargetPosition = targetPosition.position;
    }
}
