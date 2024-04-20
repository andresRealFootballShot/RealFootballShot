using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerAim : MonoBehaviour
{
    public string strAim;
    public string strTouchManaged, strJoyStickChangeAim, strJoyManaged;
    public ComponentsKeys keys;
    public ComponentsPlayer componentsPlayer;
    public ImageCtrl imagePosition,gunSightImage;
    delegate void Vector2Delegate(Vector2 value);
    event Vector2Delegate ValueChangeEvent;
    public Vector2 position;
    Vector2 positionMouse;
    bool computer,computerTouchManaged;
    public ControllerSpeedMouse controllerSpeedAim;
    public ControllerDistance controllerDistance;
    public ControllerZoneAim controllerZoneAim;
    public Color colorEnable, colorDisable;
    bool aimEnable,aimIsClose;
    public ColorImage colorImage;
    public GameObject eventsGObj;
    public EmptyEvent2 losePossession;
    MyRaycastHit myRaycast;
    PlayerState playerMode;
    void Start()
    {
        MatchEvents.kick.AddListener(Kick);
        playerMode = PlayerState.LookingBall;
        MatchEvents.losePossession.AddListener(LosePossession);
        myRaycast = componentsPlayer.scriptsPlayer.raycastAim;
    }
    void Awake()
    {
        position = new Vector2(Screen.width / 2, Screen.height / 2);
        ValueChangeEvent += imagePosition.ValueChanged;
    }
    public void LosePossession()
    {
        playerMode = PlayerState.LookingBall;
    }
    void Kick(KickEventArgs args)
    {
        if (args.playerID.Equals(ComponentsPlayer.myMonoPlayerID.playerIDStr))
        {
            Vector3 kickDirection = args.kickDirection;
            if (playerMode == PlayerState.LookingBall || playerMode == PlayerState.WithPossession)
            {
                if (kickDirection.magnitude < PuppetParameters.maxKickForceToExitWithPossessionState)
                    playerMode = PlayerState.WithPossession;
                else
                    playerMode = PlayerState.LookingBall;
            }
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(ComponentsKeys.keyFreeCamera) || Input.GetKeyDown(ComponentsKeys.joyRotateAround))
        {
            if (playerMode == PlayerState.LookingBall)
            {
                //resetPosition(new Vector2(Screen.width/2,Screen.height/2));
                playerMode = PlayerState.Free;
                

            }
            else
            {
                checkAimIsClose();
                playerMode = PlayerState.Free;
            }
            gunSightImage.hideImage();
        }
        if (Input.GetKeyDown(ComponentsKeys.keyLookingBall) || Input.GetKeyDown(ComponentsKeys.joyRotateAround))
        {
            if (playerMode == PlayerState.LookingBall)
            {
                //resetPosition(new Vector2(Screen.width/2,Screen.height/2));
                playerMode = PlayerState.LookingBall;
                //gunSightImage.hideImage();

            }
            else
            {
                checkAimIsClose();
                playerMode = PlayerState.LookingBall;
            }

        }
        if (Input.GetKeyDown(ComponentsKeys.keyTouchManaged) || Input.GetKeyDown(ComponentsKeys.joyTouchedManaged))
        {
            computerTouchManaged = true;
        }
        if (Input.GetKeyUp(ComponentsKeys.keyTouchManaged)|| Input.GetKeyUp(ComponentsKeys.joyTouchedManaged))
        {
            computerTouchManaged = false;
            //Vector3 ballPosScreen = componentsPlayer.camera.WorldToScreenPoint(componentsPlayer.componentsBall.transCenterBall.position);
            //resetPosition(new Vector2(ballPosScreen.x, ballPosScreen.y));
        }
        //Importante que lo siguiente esté en Update() y no en FixedUpdate() para evitar errores en la posicion de la mirilla
        if (!computerTouchManaged)
        {
            if ((playerMode == PlayerState.LookingBall || playerMode == PlayerState.WithPossession))
            {
                MoveAim();
            }
            else
            {
                MoveAim2();
            }

        }
    }
    void checkAimIsClose()
    {
        if (controllerDistance.isClose())
        {
            aimIsClose = true;
            if (playerMode != PlayerState.Free)
            {
                gunSightImage.showImage();
            }
        }
        else
        {
            aimIsClose = false;
            if (playerMode != PlayerState.Free)
            {
                gunSightImage.hideImage();
            }
        }
    }
    public void FixedUpdate()
    {
        checkAimIsClose();
        if (aimIsClose)
        {
            if (myRaycast.isHitting)
            {
                if (!aimEnable)
                {
                    aimEnable = true;
                    colorImage.SetColor(colorEnable);
                }
            }
            else 
            {
                if (aimEnable)
                {
                    aimEnable = false;
                    colorImage.SetColor(colorDisable);
                }
            }

        }
        else
        {
            if (aimEnable)
            {
                aimEnable = false;
                colorImage.SetColor(colorDisable);
            }
        }
       
        
        
    }
    void resetPosition(Vector2 pos)
    {
        position = pos;
        ValueChangeEvent?.Invoke(Vector2.zero);
        positionMouse = Vector2.zero;
    }
    void MoveAim()
    {
        Vector3 ballPosScreen = componentsPlayer.camera.WorldToScreenPoint(componentsPlayer.componentsBall.transBall.position);
        float scaleBall = componentsPlayer.componentsBall.transBall.localScale.x;
        Vector3 ballLimitPosScreen = componentsPlayer.camera.WorldToScreenPoint(componentsPlayer.componentsBall.transCenterBall.position+Vector3.up* componentsPlayer.componentsBall.radio);
        float limit = Vector2.Distance(new Vector2(ballPosScreen.x, ballPosScreen.y),new Vector2(ballLimitPosScreen.x,ballLimitPosScreen.y));
        positionMouse = controllerZoneAim.ControlZoneAim(positionMouse, limit,Time.deltaTime);
        Vector2 centerBall = new Vector2(ballPosScreen.x, ballPosScreen.y) - new Vector2(Screen.width / 2, Screen.height / 2);

        ValueChangeEvent?.Invoke(positionMouse + centerBall);
        position = positionMouse + new Vector2(ballPosScreen.x,ballPosScreen.y);
        gunSightImage.ValueChanged(centerBall);
        
    }
    void MoveAim2()
    {
        Vector3 ballPosScreen = componentsPlayer.camera.WorldToScreenPoint(componentsPlayer.componentsBall.transCenterBall.position);
        Vector2 centerBall = new Vector2(ballPosScreen.x, ballPosScreen.y) - new Vector2(Screen.width / 2, Screen.height / 2);
        ValueChangeEvent?.Invoke(centerBall);
        position = new Vector2(ballPosScreen.x, ballPosScreen.y);


    }

}
