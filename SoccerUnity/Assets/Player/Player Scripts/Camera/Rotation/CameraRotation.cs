using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    public float speedCameraFree=20f, speedCameraLookAtBall, speedCameraTransition, speedStartCameraTransition;
    public GameObject eventsGObj;
    public ComponentsPlayer componentsPlayer;
    public ControllerSpeedMouse controllerSpeed;
    public ComponentsKeys componentsKeys;
    public float distanceAround,angleMax,angleMin,maxAngle,angleMinPivotmangleMaxPivot,maxDistanceForward;
    float mouseX, mouseY;
    public PlayerState playerMode;
    public FloatTransition transition;
    IEnumerator transitionSpeedCamera;
    public float currentSpeedCamera { get; set; }
    public float lookingBallTransitionDuration;
    public Transform pivot1CameraTransform { get => componentsPlayer.transPivotCamera1; }
    public Transform pivot2CameraTransform { get => componentsPlayer.transPivotCamera2; }
    Transform cameraTrans { get => componentsPlayer.transCamera; }
    float height { get => componentsPlayer.playerComponents.playerData.height; }
    private void Awake()
    {
        //currentSpeedCamera = speedCameraLookAtBall;
        //MenuCtrl.showMenuEvent.AddListener(showMenu);
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
                }
                else
                {
                    playerMode = PlayerState.LookingBall;
                }
            }
        }
    }
    void Update()
    {
        if (!MenuCtrl.isEnable && (Input.GetKeyDown(ComponentsKeys.keyFreeCamera) || false && Input.GetKeyDown(ComponentsKeys.joyRotateAround)))
        {
            if (true || playerMode == PlayerState.LookingBall)
            {
                if(playerMode == PlayerState.Free)
                {
                    Vector3 point = MyFunctions.setYToVector3(componentsPlayer.componentsBall.transCenterBall.position,pivot2CameraTransform.position.y);
                    
                    StartCoroutine(freeCameraTransition(point, 10));
                    currentSpeedCamera = 100;
                }
                else
                {

                    float distance = Vector3.Distance(componentsPlayer.transPivotCamera1.position, componentsPlayer.transPivotCamera2.position);
                    Vector3 pointLookCamera = cameraTrans.position + cameraTrans.forward * 10 + componentsPlayer.transPivotCamera2.right * distance;
                    StartCoroutine(freeCameraTransition(pointLookCamera, 10));

                    startTransitionSpeedCameraCoroutine(15, 50, speedCameraFree);
                }
                playerMode = PlayerState.Free;
                //startTransitionSpeedCameraCoroutine(speedStartCameraTransition, speedCameraTransition, speedCameraFree);
            }
            else if (playerMode == PlayerState.WithPossession)
            {
                playerMode = PlayerState.Free;
                startTransitionSpeedCameraCoroutine(speedStartCameraTransition, speedCameraTransition, speedCameraFree);
            }
            else
            {
                playerMode = PlayerState.Free;
                startTransitionSpeedCameraCoroutine(speedStartCameraTransition, speedCameraTransition, speedCameraFree);
            }
        }
        if (!MenuCtrl.isEnable && (Input.GetKeyDown(ComponentsKeys.keyLookingBall) || false && Input.GetKeyDown(ComponentsKeys.joyRotateAround)))
        {
            if (playerMode == PlayerState.LookingBall)
            {
                //playerMode = PlayerState.LookingBall;
                //StopAllCoroutines();
                //StartCoroutine(lookingBallTransition());
                //startTransitionSpeedCameraCoroutine(speedStartCameraTransition, speedCameraTransition, speedCameraLookAtBall);
                /*
                //Vector3 dirBallPivot1 = componentsPlayer.componentsBall.transCenterBall.position - componentsPlayer.transPivotCamera1.position;
                Vector3 dirBallPivot1 = componentsPlayer.componentsBall.transCenterBall.position - pivot1CameraTransform.position;
                dirBallPivot1.y = 0;

                Quaternion targetRotation1 = Quaternion.LookRotation(dirBallPivot1);
                targetRotation1.x = 0;
                targetRotation1.z = 0;
                //componentsPlayer.transPivotCamera1.rotation = targetRotation1;
                pivot1CameraTransform.rotation = targetRotation1;
                
                startTransitionSpeedCameraCoroutine(speedStartCameraTransition, speedCameraTransition, speedCameraFree);*/
            }
            else if (playerMode == PlayerState.WithPossession)
            {
                playerMode = PlayerState.LookingBall;
                startTransitionSpeedCameraCoroutine(speedStartCameraTransition, speedCameraTransition, speedCameraLookAtBall);
                /*playerMode = PlayerState.Lock;
                componentsPlayer.scriptsPlayer.cameraPosition.playerMode = PlayerState.Lock;
                StopAllCoroutines();
                StartCoroutine(lookingBallTransition());*/
            }
            else
            {
                /*playerMode = PlayerState.Lock;
                componentsPlayer.scriptsPlayer.cameraPosition.playerMode = PlayerState.Lock;
                StopAllCoroutines();
                StartCoroutine(lookingBallTransition());*/
                playerMode = PlayerState.LookingBall;
                startTransitionSpeedCameraCoroutine(speedStartCameraTransition, speedCameraTransition, speedCameraLookAtBall);
            }
        }
    }
    IEnumerator freeCameraTransition(Vector3 pointLookCamera, float speed)
    {
        //Vector3 directionLookTarget = target - transform.position;
        Transform pivot1CameraTrans = componentsPlayer.transPivotCamera1;
        Vector3 directionLookTarget = pointLookCamera - pivot1CameraTrans.position;
        //directionLookTarget.y = 0;
        Vector3 posBall = MatchComponents.ballComponents.transCenterBall.position;
        Vector3 dir2 = posBall - pivot1CameraTrans.position;
        dir2.y = 0;
        pivot1CameraTrans.rotation = componentsPlayer.transCamera.rotation;
        Vector3 forward = pivot1CameraTrans.forward;
        
        float angle = Vector3.Angle(pivot1CameraTrans.forward, directionLookTarget);
        float angleY = Vector3.Angle(MyFunctions.setY0ToVector3(pivot1CameraTrans.forward), MyFunctions.setY0ToVector3(directionLookTarget));
        Vector3 axisY = Vector3.Cross(MyFunctions.setY0ToVector3(pivot1CameraTrans.forward), MyFunctions.setY0ToVector3(directionLookTarget));
        Vector3 vector = Quaternion.AngleAxis(angleY, axisY) * pivot1CameraTrans.forward;
        Vector3 axisX = Vector3.Cross(vector, directionLookTarget);
        float angleX = Vector3.Angle(vector, directionLookTarget);
        float t = 0;
        float duration = 1;
        while (t< duration)
        {
            float a = Mathf.Lerp(0, angleY, t/ duration);
            float b = Mathf.Lerp(0, angleX, t/ duration);
            Vector3 dir = Quaternion.AngleAxis(a, axisY) * forward;
            dir = Quaternion.AngleAxis(b, axisX) * dir;
            pivot1CameraTrans.rotation = Quaternion.LookRotation(dir);
            Debug.DrawRay(pivot1CameraTrans.position, pivot1CameraTrans.forward);
            yield return null;
            t += Time.deltaTime * speed;
        }
        pivot1CameraTrans.rotation = Quaternion.LookRotation(directionLookTarget);
    }
    public void FixedUpdate()
    {
        switch (playerMode)
        {
            case PlayerState.Free:
                //Importante que FreeLook() lo llame NormalMoviment
                FreeLook(Time.fixedDeltaTime);
                break;
            case PlayerState.LookingBall:
                LookAtBall();
                break;
            case PlayerState.WithPossession:
                checkDistance();
                LookAtBall();
                break;
        }
        //print("Rotation-"+currentSpeedCamera);
    }
    void checkDistance()
    {
        Vector3 ballPos = componentsPlayer.componentsBall.transBall.position;
        Vector3 bodyPos = componentsPlayer.transBody.position;
        float distance = Vector3.Distance(bodyPos, ballPos);
        if (distance > PuppetParameters.maxPossessionDistance)
        {
            playerMode = PlayerState.LookingBall;
        }
    }
    public void LookAtBall()
    {
        componentsPlayer.transCamera.rotation = Quaternion.Lerp(componentsPlayer.transCamera.rotation, getTargetRotationLookAtBall(), currentSpeedCamera * Time.deltaTime);
    }
    public Quaternion getTargetRotationLookAtBall()
    {
        Vector3 posCamera = componentsPlayer.transCamera.position;
        Vector3 posBall = componentsPlayer.componentsBall.transCenterBall.position;
        Vector3 targetLook;
        float radio = MatchComponents.ballRadio;
        float screenD = screenLimitYDistance();
        float percentEdgeMaxBallScreenY = 10;
        float d = screenD + componentsPlayer.transCamera.position.y - radio - screenD/ percentEdgeMaxBallScreenY;
        float x = Mathf.Clamp01((posBall.y - d)/1);
        float posY = Mathf.Lerp(posCamera.y, posBall.y,x);
        //posY = posCamera.y;
        targetLook = new Vector3(posBall.x, posY, posBall.z);
        return Quaternion.LookRotation(targetLook - posCamera);
    }
    public float a=10, b=10;
    float screenLimitYDistance()
    {
        Transform ballTransform = componentsPlayer.componentsBall.transBall;
        
        Transform cameraTransform = componentsPlayer.camera.transform;
        float distance = Vector3.Distance(ballTransform.position, MyFunctions.setYToVector3(cameraTransform.position, ballTransform.position.y));
        Camera camera = componentsPlayer.camera;
        float angle = camera.fieldOfView / 2;
        float maxY = Mathf.Tan(angle * Mathf.Deg2Rad) * distance;
        return maxY ;
    }
    Vector3 pos1;
    private void OnDrawGizmos()
    {
        Gizmos.color=Color.red;
        Gizmos.DrawSphere(pos1, 0.1f);
    }
    public void FreeLook(float deltaTime)
    {
        Transform pivot1CameraTransform = this.pivot1CameraTransform;
        Transform pivot2CameraTransform = this.pivot2CameraTransform;
        //mouseX = Input.GetAxis("Mouse X") * controllerSpeed.speedMouseX * Time.deltaTime + Input.GetAxis("RightStickX") * controllerSpeed.speedJoystickX * deltaTime;
        //mouseY = -Input.GetAxis("Mouse Y") * controllerSpeed.speedMouseY * Time.deltaTime + Input.GetAxis("RightStickY") * controllerSpeed.speedJoystickY * deltaTime;

        mouseX = Input.GetAxis("Mouse X") * controllerSpeed.speedMouseX * deltaTime ;
        mouseY = -Input.GetAxis("Mouse Y") * controllerSpeed.speedMouseY * deltaTime;
        
        Vector3 eulerCamera = pivot1CameraTransform.eulerAngles;
        float aux = mouseY + eulerCamera.x;
        if (aux > angleMin && aux < 180)
        {
            mouseY = 0;
        }
        else if (aux > 200 && aux < angleMax)
        {
            mouseY = 0;
        }
        pivot1CameraTransform.eulerAngles += new Vector3(mouseY, mouseX, 0);

        //Debido a temblores en la rotación hay que dejar esto
        //Vector3 targetPos = SetPositionPivot();
        Vector3 targetPosition = pivot2CameraTransform.TransformPoint(Vector3.forward * distanceAround);
        //print("Rotation " + currentSpeedCamera);
        componentsPlayer.transCamera.position = Vector3.Lerp(componentsPlayer.transCamera.position, targetPosition, deltaTime * currentSpeedCamera);

        Quaternion targetRotation = Quaternion.LookRotation(-pivot2CameraTransform.forward);
        componentsPlayer.transCamera.rotation = Quaternion.Lerp(componentsPlayer.transCamera.rotation, targetRotation, currentSpeedCamera * deltaTime);
        
    }
    
    public void startTransitionSpeedCameraCoroutine(float startSpeed,float speed, float target)
    {
        if (transitionSpeedCamera != null)
        {
            StopCoroutine(transitionSpeedCamera);
        }
        currentSpeedCamera = startSpeed;
        transitionSpeedCamera = transitionCoroutine(startSpeed,speed, (x) => currentSpeedCamera = currentSpeedCamera + x, target);
        StartCoroutine(transitionSpeedCamera);
    }
    IEnumerator transitionCoroutine(float startSpeed,float speed, System.Action<float> setVar, float targetValue)
    {
        float delta = 0, currentValue = startSpeed;
        float sign = Mathf.Sign(targetValue);
        while (sign * targetValue - sign * currentValue > 0)
        {

            setVar(delta);
            //delta son pequeños incrementos necesarios para llegar a targetValue
            delta = sign * speed * Time.deltaTime;
            currentValue += delta;

            yield return null;
        }
        setVar(delta);
    }
    public void InitLook()
    {
        componentsPlayer.transCamera.eulerAngles = componentsPlayer.transBody.eulerAngles;
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
/*
        if (!MenuCtrl.isEnable && (Input.GetKeyDown(ComponentsKeys.keyFreeCamera) || false&&Input.GetKeyDown(ComponentsKeys.joyRotateAround)))
        {
            if (playerMode == PlayerState.LookingBall)
            {

                //Vector3 dirBallPivot1 = componentsPlayer.componentsBall.transCenterBall.position - componentsPlayer.transPivotCamera1.position;
                Vector3 dirBallPivot1 = componentsPlayer.componentsBall.transCenterBall.position - pivot1CameraTransform.position;
                dirBallPivot1.y = 0;
              
                Quaternion targetRotation1 = Quaternion.LookRotation(dirBallPivot1);
                targetRotation1.x = 0;
                targetRotation1.z = 0;
                //componentsPlayer.transPivotCamera1.rotation = targetRotation1;
                pivot1CameraTransform.rotation = targetRotation1;
                playerMode = PlayerState.Free;
                startTransitionSpeedCameraCoroutine(speedStartCameraTransition, speedCameraTransition, speedCameraFree);
            }
            else if(playerMode == PlayerState.WithPossession)
            {
                playerMode = PlayerState.LookingBall;
                startTransitionSpeedCameraCoroutine(speedStartCameraTransition, speedCameraTransition, speedCameraLookAtBall);
            }
            else
            {
                playerMode = PlayerState.LookingBall;
                startTransitionSpeedCameraCoroutine(speedStartCameraTransition, speedCameraTransition, speedCameraLookAtBall);
            }
        }*/

/*
IEnumerator lookingBallTransition()
{
    Vector3 position = componentsPlayer.transCamera.position;
    Quaternion rotation = componentsPlayer.transCamera.rotation;
    float t = 0;
    //t < lookingBallTransitionDuration
    while (Vector3.Distance(componentsPlayer.transCamera.position, componentsPlayer.scriptsPlayer.cameraPosition.getTargetLookAtBallPosition())>0.1f)
    {
        yield return null;
        t += Time.deltaTime;
        componentsPlayer.scriptsPlayer.cameraPosition.rotationPivot1();
        componentsPlayer.scriptsPlayer.cameraPosition.setPositionPivot1();
        //componentsPlayer.transCamera.position = Vector3.Lerp(position, componentsPlayer.scriptsPlayer.cameraPosition.getTargetLookAtBallPosition(), t / lookingBallTransitionDuration);
        componentsPlayer.transCamera.position = Vector3.Lerp(componentsPlayer.transCamera.position, componentsPlayer.scriptsPlayer.cameraPosition.getTargetLookAtBallPosition(), Time.deltaTime * 10);
        float d = Vector3.Distance(componentsPlayer.transCamera.position, componentsPlayer.scriptsPlayer.cameraPosition.getTargetLookAtBallPosition());
        componentsPlayer.transCamera.rotation = Quaternion.Lerp(rotation, getTargetRotationLookAtBall(), 1 - d);

    }
    print("a");
    componentsPlayer.transCamera.position = componentsPlayer.scriptsPlayer.cameraPosition.getTargetLookAtBallPosition();
    componentsPlayer.transCamera.rotation = getTargetRotationLookAtBall();
    playerMode = PlayerState.LookingBall;
    componentsPlayer.scriptsPlayer.cameraPosition.playerMode = PlayerState.LookingBall;
    //transform.rotation = Quaternion.LookRotation(directionLookTarget);
}*/
