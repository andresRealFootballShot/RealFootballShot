using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldTriangleSpace;
using UnityEditor;

public class DefensiveMovementCtrl : SoccerPlayerComponent
{
    public bool debug;
    void Start()
    {
        
    }
    private void Update()
    {
        ForwardDesiredDirection = bodyBallY0Direction;
        DesiredLookDirection = bodyBallY0Direction;
        ForwardDesiredSpeed = 10;
        TargetPosition = ballPosition;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying&&enabled&&debug)
        {
            
        }
    }
#endif
    
}
