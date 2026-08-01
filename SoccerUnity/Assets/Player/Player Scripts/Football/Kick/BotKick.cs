using CullPositionPoint;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BotKick : PlayerComponent
{
    public float kickPeriod = 0.25f;
    public bool kickAvailable{ get => Time.time - startKickTime >= kickPeriod; }
    public float startKickTime{ get; set; }
    public float controlTime { get; set; }
    public bool controlTimeAvailable { get => Time.time - startControlTime >= controlTime; }
    public float startControlTime { get; set; } = -10;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public bool CheckKick(PassData passData)
    {
        if (ReachBall())
        {

            //EditorApplication.isPaused = true;
            

            Kick(passData.passVelocity, MatchComponents.ballRigidbody.angularVelocity);


           
            return true;
        }
        else
        {
            return false;
        }

        
    }
    public void Kick(Vector3 velocity,Vector3 angularVelocity)
    {
        KickEventArgs kickEventArgs = new KickEventArgs(velocity, MatchComponents.ballRigidbody.velocity, MatchComponents.ballRigidbody.angularVelocity, MatchComponents.ballRigidbody.position, publicPlayerData.playerID);
        MatchComponents.ballRigidbody.velocity = velocity;
        MatchComponents.ballRigidbody.angularVelocity = angularVelocity;
        startKickTime = Time.time;
        MatchEvents.kick.Invoke(kickEventArgs);
    }
    public void Kick(Vector3 velocity)
    {
        KickEventArgs kickEventArgs = new KickEventArgs(velocity, MatchComponents.ballRigidbody.velocity, MatchComponents.ballRigidbody.angularVelocity, MatchComponents.ballRigidbody.position, publicPlayerData.playerID);
        MatchComponents.ballRigidbody.velocity = velocity;
        startKickTime = Time.time;
        MatchEvents.kick.Invoke(kickEventArgs);
    }
    public bool ReachBall()
    {
        //return ballBodyAngle < 80 && BodyBallXZDistance < scope && ballPosition.y <= bodyHeight + ballRadio && kickAvailable;
        return BodyBallXZDistance < ballScope && ballPosition.y <= bodyPosition.y+ bodyHeight + ballRadio && kickAvailable;
    }
}
