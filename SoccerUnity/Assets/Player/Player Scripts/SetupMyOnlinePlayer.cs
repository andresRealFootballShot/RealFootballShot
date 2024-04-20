using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupMyOnlinePlayer : MonoBehaviour
{
    public AnimationCurve lerpCameraMoviment;
    public float durationLerp;
    ComponentsPlayer componentsPlayer;
    FloatTransition transition = new FloatTransition();
    MyCoroutine cameraLookingBall = new MyCoroutine();
    Vector3 initCameraPosition;
    Quaternion initCameraRotation;
    private void Awake()
    {
        
        
    }
    public void setup(GameObject body, GameObject model)
    {
        componentsPlayer = FindObjectOfType<ComponentsPlayer>();
        setupComponents(body, model);
        setupScripts();
        SetupKicks();
    }
    void setupScripts()
    {
        componentsPlayer.scriptsPlayer.cameraRotation.currentSpeedCamera = componentsPlayer.scriptsPlayer.cameraRotation.speedCameraLookAtBall;
        componentsPlayer.scriptsPlayer.cameraPosition.currentSpeedCamera = componentsPlayer.scriptsPlayer.cameraPosition.speedCameraLookingBall;
    }
    void setupComponents(GameObject body, GameObject model)
    {
        componentsPlayer.transBody = body.transform;
        componentsPlayer.transModelo = model.transform;
        componentsPlayer.rigBody = body.GetComponent<Rigidbody>();
        componentsPlayer.animatorPlayer = componentsPlayer.transModelo.GetComponent<Animator>();
        componentsPlayer.colliderPlayer = body.GetComponent<CapsuleCollider>();
        setupCamera();
        //componentsPlayer.EnableAll();
        //componentsPlayer.hudGObj.GetComponent<Canvas>().enabled = true;
        /*SaqueBandaCtrl saqueBandaCtrl = GameObject.FindGameObjectWithTag("SaqueDeBandaCtrl").GetComponent<SaqueBandaCtrl>();
        saqueBandaCtrl.enabled = true;*/
        //Velocity velocity = body.GetComponent<Velocity>();
        //velocity.stringMy = componentsPlayer.transform.Find("Moviment").GetComponent<StringMy>();
        //SetupModel
        //componentsPlayer.SetParentPivot(body);
    }
    void SetupKicks()
    {
        componentsPlayer.scriptsPlayer.touch.setAddForceAtPositionOnline();
        componentsPlayer.scriptsPlayer.shot.setAddForceAtPositionOnline();
        componentsPlayer.scriptsPlayer.touchWithDirect.setAddForceOnline();
        componentsPlayer.scriptsPlayer.touchWithDirect.setBallControlOnline();
    }
    void setupCamera()
    {
        initCameraPosition = componentsPlayer.transCamera.position;
        initCameraRotation = componentsPlayer.transCamera.rotation;
        cameraLookingBall.end += cameraIsInPosition;
        cameraLookingBall.addCondition(distanceFromCameraToTargetPositionLookingBall);
        StartCoroutine(cameraLookingBall.Coroutine(0));
        transition.addFunction(setCameraRotationPosition);
        StartCoroutine(transition.Coroutine(durationLerp,0));
    }
    bool distanceFromCameraToTargetPositionLookingBall()
    {
        return Vector3.Distance(componentsPlayer.scriptsPlayer.cameraPosition.targetPositionLookingBall, componentsPlayer.transCamera.position) < 0.5f;
    }
    
    void setCameraRotationPosition(float deltaTime)
    {
        componentsPlayer.scriptsPlayer.cameraPosition.rotationPivot1();
        componentsPlayer.scriptsPlayer.cameraPosition.setPositionPivot1();
        componentsPlayer.transCamera.position = Vector3.Lerp(initCameraPosition, componentsPlayer.scriptsPlayer.cameraPosition.getTargetLookAtBallPosition(), lerpCameraMoviment.Evaluate(deltaTime / durationLerp));
        componentsPlayer.transCamera.rotation = Quaternion.Lerp(initCameraRotation, componentsPlayer.scriptsPlayer.cameraRotation.getTargetRotationLookAtBall(), lerpCameraMoviment.Evaluate(deltaTime / durationLerp));

        /*componentsPlayer.scriptsPlayer.cameraRotation.currentSpeedCamera = Mathf.Lerp(1, componentsPlayer.scriptsPlayer.cameraRotation.speedCameraLookAtBall, lerpCameraMoviment.Evaluate(deltaTime / durationLerp));
        componentsPlayer.scriptsPlayer.cameraPosition.currentSpeedCamera = Mathf.Lerp(1, componentsPlayer.scriptsPlayer.cameraPosition.speedCameraLookingBall, lerpCameraMoviment.Evaluate(deltaTime / durationLerp));*/
    }
    void cameraIsInPosition()
    {
        componentsPlayer.EnableAll();
        componentsPlayer.scriptsPlayer.hudCtrl.ShowHUD();
        componentsPlayer.scriptsPlayer.hudCtrl.HideGunSight();
        MatchEvents.cameraIsInThirtPersonPosition.Invoke();
        MatchEvents.enableMenu.Invoke();
    }
}
