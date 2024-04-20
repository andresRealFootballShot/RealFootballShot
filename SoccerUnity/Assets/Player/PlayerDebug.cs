using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerDebug : PlayerComponent
{
    public bool disableAll;
    public bool playerSpeed;
    public bool playerBallDistance;
    public bool ballSpeedDebug;
    public bool stopOffset;
    public bool targetDistance;
    public bool ballBodyDirection;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && !disableAll)
        {
            float i = 0.1f;
            float i2 = 0.5f;

            if (playerSpeed)
            {
                GUIStyle style = new GUIStyle();
                style.fontSize = 14;
                style.normal.textColor = Color.yellow;

                string text = "Player Speed=" + Speed.ToString("f2") + " DesiredSpeed=" + ForwardDesiredSpeed;
                Handles.Label(headPosition + Vector3.up * i, text, style);
            }
            if (playerBallDistance)
            {
                i += i2;
                playerComponents.ballControl.debug(i);
            }
            if (ballSpeedDebug)
            {
                GUIStyle style = new GUIStyle();
                style.fontSize = 14;
                style.normal.textColor = Color.yellow;
                i += i2;
                Handles.Label(headPosition + Vector3.up * i, "Ball Speed=" + ballSpeed.ToString("f2"), style);
            }
            if (stopOffset)
            {
                GUIStyle style = new GUIStyle();
                style.fontSize = 14;
                style.normal.textColor = Color.yellow;
                i += i2;
                Handles.Label(headPosition + Vector3.up * i, "StopOffset=" + base.stopOffset.ToString("f2"), style);
            }
            if (targetDistance)
            {
                GUIStyle style = new GUIStyle();
                style.fontSize = 14;
                style.normal.textColor = Color.yellow;
                i += i2;
                string text = "TargetDistance=" + BodyTargetXZDistance.ToString("f2") + " distanceStopMoveBallPlayer=" + movementValues.distanceStopMoveBallPlayerOffset;
                Handles.Label(headPosition + Vector3.up * i, text, style);
            }
            


        }
    }
#endif
}
