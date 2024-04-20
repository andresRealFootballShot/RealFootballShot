using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Kick : MonoBehaviour
{
    public KickValues kickValues;
    public ComponentsPlayer componentsPlayer;
    public ControllerAim controllerAim;
    public ControllerDistance controllerDistance;
    public float sensibilityShot = 10, speedChangeShot = 5, powShot = 2;
    protected float x;
    public float min = 50, max = 200;
    public float maxDistanceCorrectErr = 50;
    protected float timeLoadKick;
    public delegate void FloatDelegate(float value);
    public event FloatDelegate CurrentValueEvent;
    public Vector3Event kickEvent;
    public float adjustCorrect = 0.5f, maxCorrectAngle = 90;
    public float currentValue { get; set; }
    protected void InvokeCurrentEvent(float value) => CurrentValueEvent?.Invoke(value);

    public Slash slashHUD;
    protected delegate void AddForceDelegate(Vector3 dir);
    protected delegate void AddForceAtPositionDelegate(Vector3 dir, Vector3 point);
    protected AddForceDelegate addForce;
    protected AddForceAtPositionDelegate addForceAtPosition;
    protected emptyDelegate ballControll;
    public static bool ballLocked;
    static ForceMode forceMode = ForceMode.VelocityChange;
    protected void Start()
    {
        CurrentValueEvent += slashHUD.ValueChange;
        //min *= componentsPlayer.componentsBall.rigBall.mass;
        //max *= componentsPlayer.componentsBall.rigBall.mass;
    }
    
    protected void InitCurrentForce()
    {
        currentValue = min;
        x = 0;
        timeLoadKick = 0;
        InvokeCurrentEvent(0);
    }
    
    void BallControlOffline()
    {
        if (!ballLocked)
        {
            Rigidbody rigBall = componentsPlayer.componentsBall.rigBall;
            rigBall.isKinematic = false;
            Vector3 velocity = Vector3.zero;
            KickEventArgs args = new KickEventArgs(velocity, Vector3.zero, Vector3.zero, rigBall.position, ComponentsPlayer.myMonoPlayerID.playerIDStr);
            AddForce(ForceMode.VelocityChange, args);
            MatchEvents.kick.Invoke(args);
        }
    }
    void BallControlOnline()
    {
        if (!ballLocked)
        {
            Rigidbody rigBall = componentsPlayer.componentsBall.rigBall;
            PlayerID myPlayerID = ComponentsPlayer.myMonoPlayerID.playerID;
            rigBall.isKinematic = false;
            Vector3 kickDirection = Vector3.zero;
            componentsPlayer.componentsBall.photonViewBall.RPC(nameof(OnlineBallCtrl.AddForceRPC), RpcTarget.All, componentsPlayer.componentsBall.transBall.position, componentsPlayer.componentsBall.transBall.eulerAngles, kickDirection, Vector3.zero, Vector3.zero, myPlayerID.onlineActor, myPlayerID.localActor);
        }
        //kickEvent.Invoke(Vector3.zero);
    }
    void AddForceAtPositionOffline(Vector3 dir, Vector3 point)
    {
        if (!ballLocked)
        {
            Rigidbody rigidbody = componentsPlayer.componentsBall.rigBall;
            KickEventArgs args = new KickEventArgs(dir, rigidbody.velocity, rigidbody.angularVelocity, point,ComponentsPlayer.myMonoPlayerID.playerIDStr);
            AddForceAtPosition(ForceMode.VelocityChange, args);
        }
    }
    void AddForceAtPositionOnline(Vector3 dir, Vector3 point)
    {
        if (!ballLocked)
        {
            Rigidbody rigBall = componentsPlayer.componentsBall.rigBall;
            PlayerID myPlayerID = ComponentsPlayer.myMonoPlayerID.playerID;
            rigBall.isKinematic = false;
            componentsPlayer.componentsBall.photonViewBall.RPC(nameof(OnlineBallCtrl.AddForceAtPositionRPC), RpcTarget.All, componentsPlayer.componentsBall.transBall.position, componentsPlayer.componentsBall.transBall.eulerAngles, point, dir, rigBall.velocity, rigBall.angularVelocity, myPlayerID.onlineActor, myPlayerID.localActor);
        }
        //kickEvent.Invoke(dir);
    }
    void AddForceOffline(Vector3 dir)
    {
        if (!ballLocked)
        {
            Rigidbody rigidbody = componentsPlayer.componentsBall.rigBall;
            KickEventArgs args = new KickEventArgs(dir, rigidbody.velocity, rigidbody.angularVelocity, rigidbody.position,ComponentsPlayer.myMonoPlayerID.playerIDStr);
            AddForce(ForceMode.VelocityChange, args);
        }
    }
    public static void AddForce(ForceMode _forceMode, KickEventArgs args)
    {
        if (Vector3.Angle(args.previousVelocity, args.kickDirection) > 90)
        {
            MatchComponents.ballRigidbody.velocity = Vector3.zero;
            MatchComponents.ballRigidbody.angularVelocity = Vector3.zero;
        }
        else
        {
            MatchComponents.ballRigidbody.velocity = args.previousVelocity;
            MatchComponents.ballRigidbody.angularVelocity = args.previousAngularVelocity;
        }
        MatchComponents.ballRigidbody.AddForce(args.kickDirection, forceMode);
        MatchEvents.kick.Invoke(args);
    }
    public static void AddForceAtPosition(ForceMode _forceMode, KickEventArgs args)
    {
        if (Vector3.Angle(args.previousVelocity, args.kickDirection) > 90)
        {
            MatchComponents.ballRigidbody.velocity = Vector3.zero;
            MatchComponents.ballRigidbody.angularVelocity = Vector3.zero;
        }
        else
        {
            MatchComponents.ballRigidbody.velocity = args.previousVelocity;
            MatchComponents.ballRigidbody.angularVelocity = args.previousAngularVelocity;
        }
        MatchComponents.ballRigidbody.AddForceAtPosition(args.kickDirection, args.pointKick, forceMode);
        //MatchComponents.ballComponents.controllerKickSound.ApplySoundKick(args.vo);
        MatchEvents.kick.Invoke(args);
    }
    void AddForceOnline(Vector3 dir)
    {
        if (!ballLocked)
        {
            Rigidbody rigBall = componentsPlayer.componentsBall.rigBall;
            PlayerID myPlayerID = ComponentsPlayer.myMonoPlayerID.playerID;
            rigBall.isKinematic = false;
            componentsPlayer.componentsBall.photonViewBall.RPC(nameof(OnlineBallCtrl.AddForceRPC), RpcTarget.All, componentsPlayer.componentsBall.transBall.position, componentsPlayer.componentsBall.transBall.eulerAngles, dir, rigBall.velocity, rigBall.angularVelocity,myPlayerID.onlineActor, myPlayerID.localActor);
        }
        //kickEvent.Invoke(dir);
    }
    protected virtual void getCurrentShot()
    {
        timeLoadKick += Time.deltaTime;
        x += Time.deltaTime * speedChangeShot;
        float increase = Mathf.Pow(x, powShot);
        currentValue = Mathf.Clamp(increase * sensibilityShot + min, min, max);
        InvokeCurrentEvent((currentValue - min) / (max - min));
    }
    public void setBallControlOffline()
    {
        ballControll = BallControlOffline;
    }
    public void setBallControlOnline()
    {
        ballControll = BallControlOnline;
    }
    public void setAddForceOffline()
    {
        addForce = AddForceOffline;
    }
    public void setAddForceOnline()
    {
        addForce = AddForceOnline;
    }
    public void setAddForceAtPositionOffline()
    {
        addForceAtPosition = AddForceAtPositionOffline;
    }
    public void setAddForceAtPositionOnline()
    {
        addForceAtPosition = AddForceAtPositionOnline;
    }
}
