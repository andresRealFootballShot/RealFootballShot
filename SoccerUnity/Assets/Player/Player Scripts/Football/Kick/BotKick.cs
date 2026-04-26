using CullPositionPoint;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BotKick : PlayerComponent
{
    public float kickPeriod = 0.25f;
    public bool kickAvailable=true;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public bool Kick(PassData passData)
    {
        if (ReachBall())
        {
            EditorApplication.isPaused = true;
            KickEventArgs kickEventArgs = new KickEventArgs(passData.passVelocity, MatchComponents.ballRigidbody.velocity, MatchComponents.ballRigidbody.angularVelocity, MatchComponents.ballRigidbody.position, publicPlayerData.playerID);
            MatchComponents.ballRigidbody.velocity = passData.passVelocity;
            Invoke(nameof(enableKick), kickPeriod);
            kickAvailable = false;
            MatchEvents.kick.Invoke(kickEventArgs);
            return true;
        }
        else
        {
            return false;
        }

        
    }
    void enableKick()
    {
        kickAvailable = true;
    }
    bool ReachBall()
    {
        //return ballBodyAngle < 80 && BodyBallXZDistance < scope && ballPosition.y <= bodyHeight + ballRadio && kickAvailable;
        return BodyBallXZDistance < scope && ballPosition.y <= bodyHeight + ballRadio && kickAvailable;
    }
}
