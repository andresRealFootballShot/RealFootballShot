using NextMove_Algorithm;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;
public class PuppetKick : PlayerComponent
{
    [System.Serializable]
    public class KickParams
    {
        public float min, max;
        public float speed = 1;
    }
    public bool noButtonForPass;
    public KickParams conductionKickParams,passKickParams,shotKickParams,managedKickParams;

    public ControllerDistance controllerDistance;
    public ControllerAim controllerAim;
    public MyRaycastHit raycastAim;
    
    KickParams currentKickParams;
    public PlayerInput playerInput;
    public Slash slashHUD;
    bool conductionKickPressed,shotKickPressed;
    float _loadKick;
    float loadKick { get{return _loadKick; } set{ SetLoadKick(value); } }
    public float managedKickInputSensibility=10;
    public ComponentsPlayer componentsPlayer;
    float kickForce;
    Vector2 ManagedKickDir;
    float distanceManagedKickMove;
    private bool ignoreMouseInput;
    public float timeScalePass=0.5f;
    bool inSlowTime{ get => playerData.onSlowTime; set => playerData.onSlowTime = value; }
    bool onLookAround{ get => playerData.onLookAround; set => playerData.onLookAround = value; }

    private bool inputReady = false;
    bool managedKick;

    private IEnumerator Start()
    {
        currentKickParams = conductionKickParams;
        loadKick = 0;
        yield return new WaitForSeconds(1);
        
        inputReady = true;
        
    }

    // Update is called once per frame
    void Update()
    {
        bool shotKickDown = playerInput.actions["ShotKick"].WasPressedThisFrame();
        bool shotKickPressed = playerInput.actions["ShotKick"].IsPressed();
        bool shotKickUp= playerInput.actions["ShotKick"].WasReleasedThisFrame();
        bool conductionKickDown = playerInput.actions["ConductionKick"].WasPressedThisFrame();
        bool conductionKickPressed = playerInput.actions["ConductionKick"].IsPressed();
        bool passKickDown = playerInput.actions["PassKick"].WasPressedThisFrame();
        bool passKickPressed = playerInput.actions["PassKick"].IsPressed();
        bool passKickUp = playerInput.actions["PassKick"].WasReleasedThisFrame();
        bool lookAroundDown = playerInput.actions["LookAround"].WasPressedThisFrame();
        bool lookAroundUp = playerInput.actions["LookAround"].WasReleasedThisFrame();
        if (conductionKickDown)
        {
            currentKickParams = conductionKickParams;
        }
        if (shotKickDown)
        {
            currentKickParams = shotKickParams;
            SlowDownTime();
            playerData.shotKick = true;
            playerData.passKick = false;
            playerData.managedKick = false;
            loadKick = 0;
        }
        if (passKickDown)
        {
            currentKickParams = passKickParams;
            SlowDownTime();
            playerData.passKick = true;
            playerData.shotKick = false;
            playerData.managedKick = false;
            loadKick = 0;

        }
        if (lookAroundDown)
        {
            LookAround();
        }
        if(lookAroundUp)
        {
            StopLookAround();
        }
        if (passKickUp)
        {
            playerData.passKick = false;
            if (!shotKickPressed) loadKick = 0;
        }
        if (shotKickUp)
        {
            playerData.shotKick = false;
            if (!passKickPressed) loadKick = 0;
        }
        if (passKickUp|| shotKickUp)
        {
            RestartScaleTime();
            
        }
        if (noButtonForPass)
        {
            if (!playerData.passKick && !playerData.shotKick)
            {
                currentKickParams = conductionKickParams;
            }
            if (playerData.shotKick && !playerData.passKick)
            {
                currentKickParams = shotKickParams;
                
            }
            if (!playerData.shotKick && playerData.passKick)
            {
                currentKickParams = passKickParams;
                
            }
        }
        if (passKickUp)
        {
            if (!shotKickPressed) loadKick = 0;
        }
        if (shotKickUp)
        {
            if (!passKickPressed) loadKick = 0;
        }
        bool kickPressed = playerInput.actions["Kick"].IsPressed();
        bool kickUp = playerInput.actions["Kick"].WasReleasedThisFrame();
        bool kickDown = playerInput.actions["Kick"].WasPressedThisFrame();
        bool LoadKickPressed = playerInput.actions["LoadKick"].IsPressed();
        bool LoadKickUp = playerInput.actions["LoadKick"].WasReleasedThisFrame();
        bool LoadKickDown = playerInput.actions["LoadKick"].WasPressedThisFrame();
        if (kickDown)
        {
            playerData.executeKick = true;
            playerData.managedKick = false;
        }
        if (LoadKickDown)
        {
            playerData.loadKick = true;
            loadKick = 0;
        }
        if ((playerData.passKick||playerData.shotKick)&& playerData.loadKick&& LoadKickPressed && !playerData.managedKick)
        {
            IncreaseLoadKick();
        }else if (kickPressed&&!playerData.passKick && !playerData.shotKick && !playerData.managedKick)
        {
            IncreaseLoadKick();
        }else if (!kickUp && !playerData.passKick && !playerData.shotKick)
        {
            loadKick = 0;
        }
        if (kickUp&& playerData.executeKick && !playerData.managedKick)
        {
            ExecuteKick();
            RestartScaleTime();
            playerData.executeKick = false;
            loadKick = 0;
            playerData.loadKick = false;
        }
        ManagedKick();
        if(playerData.playerMode != PlayerState.WithPossession)
        {
            RestartScaleTime();
        }
        if (!playerData.managedKick&&!playerData.onLookAround ||playerData.executeKick)
        {
            controllerAim.MoveAim();
        }
        else 
        {
            //controllerAim.CenterAim();
        }
    }
    void SlowDownTime()
    {
        if (!inSlowTime && playerData.playerMode==PlayerState.WithPossession)
        {
            Time.timeScale = timeScalePass;
            inSlowTime = true;
            playerData.onSlowTime = true;
        }
    }
    void LookAround()
    {
        if (!onLookAround && playerData.playerMode == PlayerState.WithPossession)
        {
            onLookAround = true;
        }
    }
    void StopLookAround()
    {
        onLookAround = false;
    }
    void RestartScaleTime()
    {
        if (inSlowTime)
        {
            inSlowTime = false;
            Time.timeScale = 1f;
            playerData.onSlowTime = false;
        }
    }
    void ManagedKick()
    {
        if (!inputReady)
            return;
        bool managedKickStartDown = playerInput.actions["ManagedKickStart"].WasPressedThisFrame();
        bool managedKickStartUp = playerInput.actions["ManagedKickStart"].WasReleasedThisFrame();
        bool managedKickStartPressed = playerInput.actions["ManagedKickStart"].IsPressed();

        Vector2 managedKickMoveDelta = playerInput.actions["ManagedKickMove"].ReadValue<Vector2>();
        
        if (managedKickStartDown)
        {
            ManagedKickDir = Vector2.zero;
            distanceManagedKickMove = 0;
            playerData.managedKick = true;
            playerData.shotKick = false;
            playerData.passKick = false;
            playerData.executeKick = false;
            RestartScaleTime();
        }
       
        if (playerData.managedKick)
        {
            
            ManagedKickDir += managedKickMoveDelta * Time.deltaTime;
            distanceManagedKickMove += managedKickMoveDelta.magnitude*Time.deltaTime;
        }
        else if (!managedKickStartUp)
        {
            ManagedKickDir = Vector2.zero;
            distanceManagedKickMove = 0;
        }
        if (managedKickStartUp&& playerData.managedKick)
        {
            if (controllerDistance.isClose())
            {
                
                Vector3 normalizedManagedKickDir3D = componentsPlayer.transCamera.TransformDirection(new Vector3(ManagedKickDir.x, 0, ManagedKickDir.y));
                normalizedManagedKickDir3D.y = 0;
                normalizedManagedKickDir3D.Normalize();
                float angle = Vector3.Angle(ballRigidbody.velocity, normalizedManagedKickDir3D);
                float forceMagnitude = Mathf.Clamp(distanceManagedKickMove * managedKickInputSensibility,distanceManagedKickMove > 0? managedKickParams.min : 0, managedKickParams.max);
                Vector3 force = normalizedManagedKickDir3D * forceMagnitude;
                Vector3 endForce = Vector3.Lerp(ballRigidbody.velocity.magnitude*1.1f * normalizedManagedKickDir3D + force, force, angle / 180);
                endForce.y = ballRigidbody.velocity.y;
                KickEventArgs args = new KickEventArgs(force, ballRigidbody.velocity, ballRigidbody.angularVelocity, ballPosition, ComponentsPlayer.myMonoPlayerID.playerID.ToString());
                ballRigidbody.velocity = endForce;
                MatchEvents.kick.Invoke(args);
            }
            playerData.managedKick = false;
        }
    }
    void IncreaseLoadKick()
    {
        loadKick = loadKick + Time.unscaledDeltaTime;
        
    }
    void SetLoadKick(float value)
    {
        _loadKick = value;
        kickForce = Mathf.Clamp(currentKickParams.min+ loadKick * currentKickParams.speed, currentKickParams.min, currentKickParams.max);
        ChangeSlashHUD();
    }
    void ChangeSlashHUD()
    {
        float slashValue = kickForce / shotKickParams.max;
        slashHUD.ValueChange(slashValue);
    }
    void ExecuteKick()
    {
        if (controllerDistance.isClose())
        {
            if (raycastAim.isHitting)
            {
                Rigidbody ballRigidbody = MatchComponents.ballRigidbody;

                float angle = Vector3.Angle(ballRigidbody.velocity, -raycastAim.hit.normal);
                Vector3 force = -raycastAim.hit.normal * kickForce;
                Vector3 endForce = Vector3.Lerp(ballRigidbody.velocity + force, force, angle / 180);
                
                KickEventArgs args = new KickEventArgs(-raycastAim.hit.normal, ballRigidbody.velocity, ballRigidbody.angularVelocity, raycastAim.hit.point, ComponentsPlayer.myMonoPlayerID.playerID.ToString());
                ballRigidbody.velocity = endForce;
                MatchEvents.kick.Invoke(args);
            }
        }
    }
}
