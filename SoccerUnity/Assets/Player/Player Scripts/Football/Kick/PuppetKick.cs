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
    [System.Serializable]
    public class ManagedKickParams
    {
        public float min, max;
        public float speed = 1;
        public float managedKickInputSensibility = 10;
        public float managedKickTimeSensibility = 1;
    }
    public bool noButtonForPass;
    public KickParams conductionKickParams, passKickParams, shotKickParams;
    public ManagedKickParams conductionManagedKickParams, passManagedKickParams, shotManagedParams;
    public ControllerDistance controllerDistance;
    public ControllerAim controllerAim;
    public MyRaycastHit raycastAim;
    
    KickParams currentKickParams;
    ManagedKickParams currentManagedKickParams;
    public PlayerInput playerInput;
    public Slash slashHUD;
    bool conductionKickPressed,shotKickPressed;
    float _loadKick;
    float loadKick { get{return _loadKick; } set{ SetLoadKick(value); } }
    
    public ComponentsPlayer componentsPlayer;
    float kickForce;
    Vector2 ManagedKickDir;
    float managedKickTime;
    float distanceManagedKickMove;
    private bool ignoreMouseInput;
    public float timeScalePass=0.5f;
    bool inSlowTime{ get => playerData.onSlowTime; set => playerData.onSlowTime = value; }
    bool onLookAround{ get => playerData.onLookAround; set => playerData.onLookAround = value; }

    private bool inputReady = true;
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
        bool lookAroundPressed = playerInput.actions["LookAround"].IsPressed();
        bool lookAroundUp = playerInput.actions["LookAround"].WasReleasedThisFrame();
        if (conductionKickDown)
        {
            currentKickParams = conductionKickParams;
            currentManagedKickParams = conductionManagedKickParams;
        }
        if (shotKickDown)
        {
            currentKickParams = shotKickParams;
            currentManagedKickParams = shotManagedParams;
            SlowDownTime();
            playerData.shotKick = true;
            playerData.passKick = false;
            playerData.conductionKick = false;
            //playerData.managedKick = false;
            loadKick = 0;
        }
        if (passKickDown)
        {
            currentKickParams = passKickParams;
            currentManagedKickParams = passManagedKickParams;
            SlowDownTime();
            playerData.passKick = true;
            playerData.shotKick = false;
            playerData.conductionKick = false;
            //playerData.managedKick = false;
            loadKick = 0;

        }
        
        if (lookAroundPressed)
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
            if (!playerData.passKick && !playerData.shotKick&&!playerData.managedKick)
            {
                currentKickParams = conductionKickParams;
                currentManagedKickParams = conductionManagedKickParams;
                playerData.conductionKick = true;
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
        else if(playerData.onLookAround)
        {
            controllerAim.CenterAim();
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
        
        if (!playerData.managedKick && playerData.playerMode == PlayerState.WithPossession)
        {
            onLookAround = true;
        }
        else
        {
            onLookAround = false;
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
            managedKickTime = 0;
            playerData.managedKick = true;
            playerData.shotKick = false;
            playerData.passKick = false;
            playerData.executeKick = false;
            
        }
       
        if (playerData.managedKick)
        {
            
            ManagedKickDir += managedKickMoveDelta * Time.unscaledDeltaTime;
            distanceManagedKickMove += managedKickMoveDelta.magnitude*Time.unscaledDeltaTime;
            managedKickTime += Time.unscaledDeltaTime ;
        }
        else if (!managedKickStartUp)
        {
            ManagedKickDir = Vector2.zero;
            distanceManagedKickMove = 0;
            managedKickTime = 0;
        }
        if (managedKickStartUp&& playerData.managedKick)
        {
            RestartScaleTime();
            if (controllerDistance.isClose2())
            {
                
                Vector3 normalizedManagedKickDir3D = componentsPlayer.transCamera.TransformDirection(new Vector3(ManagedKickDir.x, 0, ManagedKickDir.y));
                normalizedManagedKickDir3D.y = 0;
                normalizedManagedKickDir3D.Normalize();
                
                float angle = Vector3.Angle(ballRigidbody.velocity, normalizedManagedKickDir3D);
                float timeSensibility= managedKickTime * currentManagedKickParams.managedKickTimeSensibility;
                float lerp = ((distanceManagedKickMove * currentManagedKickParams.managedKickInputSensibility) + timeSensibility)/2;
                lerp /= 1;
                float min = distanceManagedKickMove > 0 ? currentManagedKickParams.min : 0;
                float forceMagnitude = Mathf.Lerp(min, currentManagedKickParams.max, lerp);
                Vector3 force = normalizedManagedKickDir3D * forceMagnitude;
                //print(forceMagnitude+" | "+lerp + " | "+ timeSensibility+" | "+(distanceManagedKickMove * currentManagedKickParams.managedKickInputSensibility));
                Vector3 endForce = Vector3.Lerp(ballRigidbody.velocity.magnitude*0.5f * normalizedManagedKickDir3D + force, force*0.5f, angle / 180);
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
        if (controllerDistance.isClose2())
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
    private void OnDisable()
    {
        playerData.onLookAround = false;
        playerData.onSlowTime = false;
        playerData.executeKick = false;
        playerData.managedKick = false;
        playerData.loadKick = false;
        playerData.passKick = false;
        playerData.shotKick = false;
    }
}
