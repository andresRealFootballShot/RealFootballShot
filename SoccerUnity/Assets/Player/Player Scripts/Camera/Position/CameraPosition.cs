using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CameraPosition : MonoBehaviour
{
    public ComponentsPlayer componentsPlayer;
    public GameObject eventsGObj;
    public ComponentsKeys componentsKeys;
    public AnimationCurve heightCurve, forwardCurve;
    public float minY, maxY,lookBallInterpolation,distanceMin,distanceMax, posYMinLookBall, posYMaxLookBall;
    public float angleOffsetMin,angleOffsetMax,distanceAround, speedCameraWithPossession, distanceMaxPossession, distanceWithPossession;
    public PlayerState playerMode { get; set; }
    [HideInInspector]public float posOffsetY;
    float targetRotation;
    public float speedRotationKey,speedRotationJoy,speedCameraTransitionBallDir , speedCameraWithPossesion, speedCamera90Rotation, speedStartCameraTransition, speedCameraTransition, speedCameraFree, speedCameraLookingBall;
    public float speedPossesionMouseRotation = 600;
    public float instantRotationAngle = 45;
    public float offsetYPositionWithPossesion;

    bool kickWithForce;
    Vector3 dirBall,dirBallInterpolation;
    IEnumerator transitionRotate90Degrees,transitionSpeedCamera;
    public float currentSpeedCamera;
    [HideInInspector]
    public Vector3 targetPositionLookingBall;
    Vector3 pivot1LocalPosition = Vector3.up;
    string currentState;
    Transform headTrans;
    float height { get => componentsPlayer.playerComponents.playerData.height; }
    Vector3 cameraPosition { get => componentsPlayer.transCamera.position; }
    float scope { get => componentsPlayer.playerComponents.scope - scopeAdjust; }
    public float scopeAdjust;
    public ControllerAim controllerAim;
    float addAngleRotation;
    void Awake()
    {
        //currentSpeedCamera = speedCameraLookingBall;
        //MenuCtrl.showMenuEvent.AddListener(showMenu);
        headTrans = componentsPlayer.animatorPlayer.GetBoneTransform(HumanBodyBones.Head);
        //height = componentsPlayer.colliderPlayer.height * componentsPlayer.transBody.localScale.y;
    }
    void showMenu()
    {
        if (playerMode == PlayerState.WithPossession)
        {
            playerMode = PlayerState.LookingBall;
        }
    }
    void Start()
    {
        MatchEvents.kick.AddListener(Kick);
        playerMode = PlayerState.LookingBall;
        MatchEvents.losePossession.AddListener(LosePossession);
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
                {
                    playerMode = PlayerState.WithPossession;
                    startTransitionSpeedCameraCoroutine(30, speedCameraTransition, speedCameraWithPossesion);
                    //startTransitionSpeedCameraCoroutine(speedStartCameraTransition, speedCameraTransition, speedCameraWithPossesion);
                }
                else
                {
                    playerMode = PlayerState.LookingBall;
                }
            }
            if (kickDirection.magnitude == 0)
            {
                dirBall = componentsPlayer.componentsBall.rigBall.position - componentsPlayer.transCamera.position;
                dirBallInterpolation = -dirBall;
                kickWithForce = false;
            }
            else
            {
                //kickWithForce usado para el calculo continuo de la dirección del balón en positionWithPosesion.
                kickWithForce = true;
                dirBallInterpolation = -(componentsPlayer.componentsBall.rigBall.position - componentsPlayer.transCamera.position);
            }
            targetRotation = 0;
            speedCameraWithPossession = 10;
        }
    }
    private void Update()
    {
        if (MenuCtrl.isEnable) return;

        if ((Input.GetKeyDown(ComponentsKeys.keyFreeCamera) || false && Input.GetKeyDown(ComponentsKeys.joyRotateAround) ))
        {
            if (playerMode==PlayerState.LookingBall)
            {
                playerMode = PlayerState.Free;
                Vector3 posPivot1 = componentsPlayer.transPivotCamera1.position;
                //componentsPlayer.transPivotCamera1.position = new Vector3(posPivot1.x, posOffsetY, posPivot1.z);
                componentsPlayer.transPivotCamera1.position = new Vector3(posPivot1.x, posOffsetY, posPivot1.z);
                startTransitionSpeedCameraCoroutine(speedStartCameraTransition,speedCameraTransition, speedCameraFree);
            }
            else if(playerMode == PlayerState.WithPossession || false&&playerMode == PlayerState.Free)
            {
                playerMode = PlayerState.Free;
                startTransitionSpeedCameraCoroutine(speedStartCameraTransition, speedCameraTransition, speedCameraFree);
            }
        }
        if ((Input.GetKeyDown(ComponentsKeys.keyLookingBall) || false && Input.GetKeyDown(ComponentsKeys.joyRotateAround)))
        {
            if (playerMode == PlayerState.Free)
            {
                //playerMode = PlayerState.Lock;
                playerMode = PlayerState.LookingBall;
                startTransitionSpeedCameraCoroutine(speedStartCameraTransition, speedCameraTransition, speedCameraLookingBall);
                /*
                
                //Vector3 posPivot1 = componentsPlayer.transPivotCamera1.position;
                Vector3 posPivot1 = pivot1CameraTransform.position;
                //componentsPlayer.transPivotCamera1.position = new Vector3(posPivot1.x, posOffsetY, posPivot1.z);
                pivot1CameraTransform.position = new Vector3(posPivot1.x, posOffsetY, posPivot1.z);
                startTransitionSpeedCameraCoroutine(speedStartCameraTransition, speedCameraTransition, speedCameraFree);*/
            }
            else if (playerMode == PlayerState.WithPossession || false && playerMode == PlayerState.Free)
            {
                //playerMode = PlayerState.Lock;
                playerMode = PlayerState.LookingBall;
                startTransitionSpeedCameraCoroutine(speedStartCameraTransition, speedCameraTransition, speedCameraLookingBall);
            }
        }
        if (playerMode == PlayerState.WithPossession)
        {
            if (Input.GetKeyDown(ComponentsKeys.keyRotRight90))
            {
                myStartCoroutine(speedCamera90Rotation, -instantRotationAngle);
            }
            else if (Input.GetKeyDown(ComponentsKeys.keyRotLeft90))
            {
                myStartCoroutine(speedCamera90Rotation, instantRotationAngle);
            }
            
        }
    }
    void myStartCoroutine(float speed,float target)
    {
        if (transitionRotate90Degrees != null)
        {
            StopCoroutine(transitionRotate90Degrees);
        }
        transitionRotate90Degrees = transition(0,speed, (x) => targetRotation = targetRotation + x, target);
        StartCoroutine(transitionRotate90Degrees);
    }
    void startTransitionSpeedCameraCoroutine(float startSpeed,float speed, float target)
    {
        if (transitionSpeedCamera != null)
        {
            StopCoroutine(transitionSpeedCamera);
        }
        currentSpeedCamera = startSpeed;
        transitionSpeedCamera = transition(startSpeed,speed, (x) => currentSpeedCamera = currentSpeedCamera + x, target);
        StartCoroutine(transitionSpeedCamera);
    }
    
    //2º parametro necesario para pasar una variable por referencia a una coroutina
    //Transicion para la rotación de 90 grados. targetRotation debe interpolar
    IEnumerator transition(float startSpeed,float speed, System.Action<float> setVar,float targetValue)
    {
        float delta=0,currentValue= startSpeed;
        float sign = Mathf.Sign(targetValue);
        while (sign*targetValue - sign*currentValue > 0)
        {
            
            setVar(delta);
            //delta son pequeños incrementos necesarios para llegar a targetValue
            delta = sign * speed * Time.deltaTime;
            currentValue += delta;
            
            yield return null;
        }
        setVar(delta);
    }
    
    public void FixedUpdate()
    {
        //Dejar SetPositionPivot aquí para que al cambiar a FreeLook la camara esté mirando al balón

        switch (playerMode)
        {
            case PlayerState.Free:
                //Importante que CameraRotation.FreeLook() llame a SetPositionPivot();

                SetPositionPivot();
                currentState = "PlayerState.Free";
                break;
            case PlayerState.LookingBall:
                setPositionLookingBall(Time.fixedDeltaTime);
                currentState = "PlayerState.LookingBall";
                break;
            case PlayerState.WithPossession:
                positionWithPossession(Time.fixedDeltaTime);
                currentState = "PlayerState.WithPossession";
                break;
            case PlayerState.Lock:
                break;
        }
    }

    
    public void setPositionLookingBall(float deltaTime)
    {
        setPositionPivot1();
        rotationPivot1();
        Vector3 camPos = componentsPlayer.transCamera.position;
        /*rotationPivot1();
        setPositionPivot1();*/
        //Debug.DrawLine(componentsPlayer.transBody.position, getTargetLookAtBallPosition());
        componentsPlayer.transCamera.position = Vector3.Lerp(camPos, getTargetLookAtBallPosition(), deltaTime * currentSpeedCamera);
    }
    public Vector3 getTargetLookAtBallPosition()
    {
        Vector3 ballPos = componentsPlayer.componentsBall.transCenterBall.position;
        Vector3 bodyPos = componentsPlayer.transBody.position;
        Vector3 pivotCamera2Position = componentsPlayer.transPivotCamera2.TransformPoint(Vector3.zero);
        float bodyBallDistance = Vector3.Distance(bodyPos, MyFunctions.setYToVector3(ballPos, bodyPos.y));
        float pivotBallDistance = Vector3.Distance(pivotCamera2Position, MyFunctions.setYToVector3(ballPos, pivotCamera2Position.y));
        float interpolation = (bodyBallDistance - scope) / lookBallInterpolation;
        //float adjustedMinDistance = bodyBallDistance < scope ? distanceMin - pivotBallDistance : distanceMin;
        float adjustedMinDistance = Mathf.Clamp( distanceMin - pivotBallDistance,1,Mathf.Infinity);
        float cameraBallDistance = Vector3.Distance(cameraPosition, MyFunctions.setYToVector3(ballPos, cameraPosition.y));
        //print((adjustedMinDistance + ));
        //print("scope=" + scope + " adjustedMinDistance=" + adjustedMinDistance + " dPivotBall=" + pivotBallDistance+ " | interpolation="+ interpolation + " | dBallBody=" + bodyBallDistance);
        if (bodyBallDistance < scope)
        {
            //print("a dCameraBall"+cameraBallDistance + " " + distanceMin);
        }
        else
        {
            //print("b dCameraBall" + cameraBallDistance + " " + distanceMin);
        }
        //Debug.DrawRay(componentsPlayer.transPivotCamera2.position, componentsPlayer.transPivotCamera2.forward, Color.red);
        //print("adjustedMinDistance="+adjustedMinDistance + " | interpolation=" + interpolation + "  | d=" + d+ "  | distanceMin=" + distanceMin);
        
        
        float lerpDistance = Mathf.Lerp(adjustedMinDistance, distanceMax, interpolation);
        return targetPositionLookingBall = componentsPlayer.transPivotCamera2.TransformPoint(Vector3.forward * lerpDistance);
    }
    public void rotationPivot1()
    {
        Vector3 targetLookPivot1;
        Vector3 posBall = componentsPlayer.componentsBall.transCenterBall.position;
        Vector3 posPivot1 = componentsPlayer.transPivotCamera1.position;
        Vector3 cross = Vector3.Cross(Vector3.up, posBall - componentsPlayer.transPivotCamera1.position);
        float d =Vector3.Distance(componentsPlayer.transPivotCamera1.position,componentsPlayer.transPivotCamera2.position);
        
        targetLookPivot1 = new Vector3(posBall.x, posPivot1.y, posBall.z) - posPivot1;
        componentsPlayer.transPivotCamera1.LookAt(posPivot1 + targetLookPivot1 - cross.normalized*d);
    }
    public void setPositionPivot1()
    {
        Vector3 ballPos = componentsPlayer.componentsBall.transCenterBall.position;
        Vector3 bodyPos = componentsPlayer.transBody.position;
        float x = 0;
        //float distanceHeight = (MyFunctions.DistancePointAndFiniteLine(ballPos, bodyPos, headTrans.position + Vector3.up * 0.13f) -x) / (maxDistanceHeight-x);
        float ballBodyDistance = Vector3.Distance(MyFunctions.setY0ToVector3(ballPos), MyFunctions.setY0ToVector3(bodyPos));
        float distanceHeight = (ballBodyDistance - scope)/ lookBallInterpolation;
        posOffsetY = Mathf.Clamp(ballPos.y + offsetYPositionWithPossesion, 0, height);
        //float y = 
        posOffsetY = Mathf.Lerp(posOffsetY, posYMinLookBall + bodyPos.y, distanceHeight);
        Vector3 posPivot1 = componentsPlayer.localPositionPivot1;
        Vector3 posOffsetPivot1 = new Vector3(posPivot1.x, posOffsetY, posPivot1.z);
        //componentsPlayer.transPivotCamera1.localPosition = posOffsetPivot1;
        componentsPlayer.transPivotCamera1.position = MyFunctions.setYToVector3(bodyPos, posOffsetY);
    }
    void positionWithPossession(float delta)
    {
        Transform transCamera = componentsPlayer.transCamera;
        Vector3 ballPos = componentsPlayer.componentsBall.transBall.position;
        Vector3 bodyPos = componentsPlayer.transBody.position;
        Vector3 cameraPos = transCamera.position;
        //float distance = Vector3.Distance(bodyPos, ballPos);
        float distance = MyFunctions.DistancePointAndFiniteLine(ballPos,bodyPos, headTrans.position + Vector3.up*0.13f);
        if (distance > distanceMaxPossession)
        {
            if(playerMode!= PlayerState.LookingBall)
            {
                startTransitionSpeedCameraCoroutine(speedStartCameraTransition, speedCameraTransition, speedCameraLookingBall);
                playerMode = PlayerState.LookingBall;
            }
        }
        else
        {
            //float addAngleRotation = Input.GetAxis("HorizontalKey") * speedRotationKey;
            addAngleRotation = Input.GetAxis("HorizontalKey") * speedRotationKey;
            addAngleRotation += Input.GetAxis("HorizontalJoy") * speedRotationJoy;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                addAngleRotation += Input.GetAxis("Mouse X") * speedPossesionMouseRotation;
            }
            
            targetRotation += addAngleRotation * delta;
            if (kickWithForce)
            {
                dirBall = componentsPlayer.componentsBall.rigBall.velocity;
            }
            dirBall.y = 0;
            dirBallInterpolation = Vector3.Lerp(dirBallInterpolation, -dirBall, delta * speedCameraTransitionBallDir);
            
            //print(height);
            //posOffsetY = Mathf.Lerp(0.55f + bodyPos.y, offsetYPositionWithPossesion + bodyPos.y, ((ballPos.y - bodyPos.y) / 1.25f));
            posOffsetY = Mathf.Clamp(ballPos.y + offsetYPositionWithPossesion,0, height);
            float distanceHeight = Vector3.Distance(bodyPos, ballPos) / lookBallInterpolation;
            //posOffsetY = Mathf.Lerp(posYMinLookBall + bodyPos.y, posYMaxLookBall + bodyPos.y, ((ballPos.y - bodyPos.y) / posYMaxLookBall) + distanceHeight);
            //Vector3 targetPos = Quaternion.AngleAxis(targetRotation, Vector3.up) * dirBallInterpolation.normalized * distanceWithPossession + ballPos;
            
            Vector3 targetPos = Quaternion.AngleAxis(targetRotation, Vector3.up) * dirBallInterpolation.normalized * distanceWithPossession + ballPos;
            targetPos.y = posOffsetY;
            //Debido a un error,no se debe utilizar lerp; si no, la camara interpolara en linea recta y no circularmente alrededor del balón
            //transCamera.position = targetPos;
            //print(dirBallInterpolation.magnitude + " | " + dirBall + " " + transCamera.InverseTransformPoint(ballPos) + " " + transCamera.InverseTransformPoint(targetPos) + " " + Vector3.Distance(transCamera.position, ballPos)); ;
            //print("Position | "+ dirBallInterpolation + "| currentSpeedCamera=" + currentSpeedCamera + " | speedCameraTransitionBallDir=" + speedCameraTransitionBallDir+ " | addAngleRotation="+ addAngleRotation);
            //transCamera.position = Vector3.Lerp(transCamera.position, targetPos, Time.deltaTime * currentSpeedCamera);
            transCamera.position = Vector3.Lerp(transCamera.position,targetPos, delta * currentSpeedCamera);
        }
    }
    public void SetPositionPivot()
    {
        Transform pivotCamera1 = componentsPlayer.transPivotCamera1;
        Vector3 ballPos = componentsPlayer.componentsBall.transCenterBall.position;
        Vector3 bodyPos = componentsPlayer.transBody.position;
        posOffsetY = Mathf.Lerp(1f + bodyPos.y, 1f + bodyPos.y, (ballPos.y - bodyPos.y) / 1.25f);
        pivotCamera1.position = new Vector3(bodyPos.x, posOffsetY, bodyPos.z);
    }
    /*
    public void SetPositionPivot()
    {
        Vector3 ballPos = componentsPlayer.componentsBall.transCenterBall.position;
        Vector3 bodyPos = componentsPlayer.transBody.position;
        float distanceHeight = Vector3.Distance(bodyPos, ballPos) / maxDistanceHeight;
        float heightEvaluated = heightCurve.Evaluate(distanceHeight);
        posOffsetY = Mathf.Lerp(1f + bodyPos.y, 1f + bodyPos.y, (ballPos.y - bodyPos.y) / 1.25f);
        Vector3 posPivot1 = componentsPlayer.transPivotCamera1.position;
        componentsPlayer.transPivotCamera1.position = new Vector3(posPivot1.x, posOffsetY, posPivot1.z);
        
        Vector3 targetPosition = componentsPlayer.transPivotCamera2.TransformPoint(Vector3.forward * distanceAround);
        componentsPlayer.transCamera.position = Vector3.Lerp(componentsPlayer.transCamera.position, targetPosition, Time.deltaTime* currentSpeedCamera);
    }*/



    public void SetPositionBall()
    {
        Vector3 ballPos = componentsPlayer.componentsBall.transCenterBall.position;
        
        Vector3 bodyPos = componentsPlayer.transBody.position;
        Vector3 camPos = componentsPlayer.transCamera.position;
        Vector3 dirBallBody = new Vector3(bodyPos.x, ballPos.y, bodyPos.z) - ballPos;
        

        float distanceHeight = Vector3.Distance(bodyPos, ballPos) / lookBallInterpolation;
        float distanceForward = Vector3.Distance(bodyPos, ballPos) / lookBallInterpolation;
        float heightEvaluated = heightCurve.Evaluate(distanceHeight);
        float forwardEvaluated = forwardCurve.Evaluate(distanceForward);
        dirBallBody = Quaternion.AngleAxis(Mathf.Lerp(angleOffsetMin, angleOffsetMax, distanceForward), Vector3.up) * dirBallBody;
        dirBallBody = (dirBallBody.normalized) * Mathf.Lerp(distanceMin, distanceMax, distanceForward);
        posOffsetY = Mathf.Lerp(Mathf.Clamp(ballPos.y, minY + bodyPos.y, maxY + bodyPos.y), posYMinLookBall + bodyPos.y, heightEvaluated);
        float posZ = dirBallBody.z;
        float posX = dirBallBody.x;
        Vector3 posOffsetCamera = new Vector3(posX, posOffsetY, posZ) + bodyPos;
        componentsPlayer.transCamera.position = Vector3.Lerp(camPos, posOffsetCamera, Time.deltaTime * speedCameraLookingBall);
    }
    public void EnableScript()
    {
       enabled = true;
    }
    public void DisableScript()
    {
        enabled = false;
    }

}
