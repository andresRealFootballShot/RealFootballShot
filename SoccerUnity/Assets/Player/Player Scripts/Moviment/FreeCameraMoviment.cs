using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCameraMoviment : Moviment
{
    public Sprint sprint;
    

    void Start()
    {
    }
    void Update()
    {
        
        getInputAxes();
        Moviment(Time.deltaTime);
        //Animator(Time.deltaTime);
    }
    // Update is called once per frame

    void Moviment(float deltaTime)
    {
        
        Transform trace = componentsPlayer.transModelo;
        Transform transModelo = componentsPlayer.transBody;
        Transform transCamera = componentsPlayer.transCamera;
       
        

        Rigidbody rigBall = componentsPlayer.componentsBall.rigBall;
        Vector3 ballVelocity = rigBall.velocity;
        Vector3 axesDirection = new Vector3(horizontalAxes, 0, verticalAxes);
        angle = FindAngle(Vector3.forward, axesDirection);
        vertical = Mathf.Clamp01(new Vector3(horizontalAxes, 0, verticalAxes).magnitude);

        Vector3 directionResult = transModelo.forward;


        TargetPosition = bodyPosition + MyFunctions.setY0ToVector3(transCamera.forward)*100;

        if (Input.GetKeyDown(ComponentsKeys.defensivePosition))
        {
           movementValues.isDefensivePosition = !movementValues.isDefensivePosition;

        }
        if (movementValues.isDefensivePosition)
        {
            Vector3 dir = MyFunctions.setY0ToVector3(transCamera.forward);
            DesiredLookDirection = dir;
            directionResult = getDirection(dir, axesDirection);
            ForwardDesiredDirection = directionResult;
        }else if (vertical != 0)
        {
            //speedRotation = Mathf.Lerp(movimentValues.minRotAcceleration, movimentValues.maxRotAcceleration, 1 - (verticalRig / (movimentValues.MaxForwardRunSpeed + movimentValues.MaxForwardSprintSpeed)));
            speedRotation = Mathf.Lerp(10, 20, 1 - (verticalRig / (movimentValues.MaxForwardRunSpeed + movimentValues.MaxForwardSprintSpeed)));
            Vector3 dir = MyFunctions.setY0ToVector3(transCamera.forward);
            Vector3 dirRotated = Quaternion.AngleAxis(angle, Vector3.up) * dir;
            directionResult = getDirection(dir, axesDirection);
            //Quaternion transModeloRotation = Quaternion.Lerp(transModelo.rotation, Quaternion.LookRotation(directionResult), deltaTime * speedRotation);
            //Quaternion transModeloRotation = Quaternion.Lerp(transModelo.rotation, Quaternion.LookRotation(dirRotated), deltaTime * speedRotation);
            //transModelo.rotation = transModeloRotation;
            //transModelo.rotation = transModeloRotation;
            DesiredLookDirection = directionResult;
            ForwardDesiredDirection = directionResult;
        }
        float magnitudeBallVelocity = Mathf.Clamp01(ballVelocity.magnitude / movimentValues.maxVelocityBall);
        targetVelocityBall = Mathf.Lerp(targetVelocityBall, magnitudeBallVelocity, deltaTime * movimentValues.speedChangeVelocityBall);
        curveVelocityEvaluated = movimentValues.curveVelocity.Evaluate(targetVelocityBall);
        float distance = Mathf.Clamp01((controllerDistance.GetDistance() - controllerDistance.maxDistance * movimentValues.adjustMaxDistance) / movimentValues.maxDistance);
        float angleWithDirection = Vector3.Angle(transModelo.forward, directionResult);
        //speedRotation2 = movimentValues.speedRotationCurve.Evaluate((1 - getSpeedRotation1(verticalAxes, horizontalAxes) / 180));
        speedRotation2 = movimentValues.speedRotationCurve.Evaluate((1 - angleWithDirection / 180));
        //verticalRig = speedRotation2*vertical * (movimentValues.MaxForwardRunSpeed + movimentValues.MaxForwardSprintSpeed * sprintAxes);
        verticalRig = vertical * (playerComponents.getRunSpeed() + sprintAxes* playerComponents.getSprintSpeed());

        ForwardDesiredSpeed = verticalRig;
        TargetPosition = ballPosition;
        playerComponents.movementCtrl.rotation(deltaTime);
        playerComponents.movementCtrl.getAdjustedForwardVelocitySpeed(deltaTime);
        playerComponents.movementCtrl.animator(deltaTime);
        
    }
    Vector3 getDirection(Vector3 forwardDirection, Vector3 axes)
    {
        Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection);
        rightDirection.Normalize();
        rightDirection *= axes.x;
        forwardDirection.Normalize();
        forwardDirection *= axes.z;
        Vector3 dir = Vector3.Scale(forwardDirection, axes);
        return forwardDirection + rightDirection;
    }
    void getInputAxes()
    {

        verticalAxes = Input.GetAxis("Vertical");
        horizontalAxes = Input.GetAxis("Horizontal");
        sprintAxes = resistanceController.resistanceVar.Value * Input.GetAxis("Fire1");
    }
    private void Animator(float deltaTime)
    {
        //print(forwardAnim + " | " + sprintAnim);
        Animator anim = componentsPlayer.animatorPlayer;
        float horizontalSpeed = getHorizontalSpeed(componentsPlayer.transBody.forward * verticalRig);
        float verticalSpeed = getVerticalSpeed(componentsPlayer.transBody.forward * verticalRig);
        anim.SetFloat("vertical", verticalSpeed, 0.1f, deltaTime * GeneralPlayerParameters.speedAnim);
        anim.SetFloat("horizontal", horizontalSpeed, 0.1f, deltaTime * GeneralPlayerParameters.speedAnim);
    }
    private void FixedUpdate()
    {
        if (!componentsPlayer.scriptsPlayer.raycastWall.isHitting)
        {
            Vector3 trace = componentsPlayer.transModelo.forward;
            //componentsPlayer.rigBody.MovePosition(componentsPlayer.rigBody.position + componentsPlayer.transBody.forward * verticalRig * Time.fixedDeltaTime);
        }
        //Importante que se ejecuten en este orden y este lugar para evitar temblores en el movimiento de la camara
        //componentsPlayer.scriptsPlayer.cameraRotation.FreeLook(Time.fixedDeltaTime);
        playerComponents.movementCtrl.movement(Time.fixedDeltaTime);
    }
}
