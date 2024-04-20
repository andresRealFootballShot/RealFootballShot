using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookingBallMoviment : Moviment
{
    public Sprint sprint;
    void Start()
    {
    }
    
    void Update()
    {
        Animator anim = componentsPlayer.animatorPlayer;

        Transform trace = componentsPlayer.transModelo;
        Transform transModelo = componentsPlayer.transBody;

        Transform transCamera = componentsPlayer.transCamera;
        Vector3 ballPos = componentsPlayer.componentsBall.transBall.position;
        Vector3 bodyPos = transModelo.position;
        Vector3 ballPosY0 = MyFunctions.setYToVector3(ballPos, 0);
        /*verticalAxes = movimentValues.velocityCurveSpeed.Evaluate(resistanceController.Value) * Input.GetAxis("Vertical");
        horizontalAxes = movimentValues.velocityCurveSpeed.Evaluate(resistanceController.Value) * Input.GetAxis("Horizontal");
        sprintAxes = movimentValues.velocityCurveSprint.Evaluate(resistanceController.Value) * Input.GetAxis("Fire1");*/
        float ballRadio = componentsPlayer.componentsBall.sphereCollider.radius * componentsPlayer.componentsBall.transBall.localScale.x;
        float bodyRadio = componentsPlayer.colliderPlayer.radius * componentsPlayer.transBody.localScale.x;
        float radio = ballRadio + bodyRadio;
        verticalAxes = Input.GetAxis("Vertical");
        horizontalAxes = Input.GetAxis("Horizontal");

        sprintAxes = resistanceController.resistanceVar.Value * Input.GetAxis("Fire1");

        Rigidbody rigBall = componentsPlayer.componentsBall.rigBall;
        Vector3 ballVelocity = rigBall.velocity;

        angle = FindAngle(Vector3.forward, new Vector3(horizontalAxes, 0, verticalAxes));
        vertical = Mathf.Clamp01(new Vector3(horizontalAxes, 0, verticalAxes).magnitude);
        
        Vector3 right = Vector3.Cross(Vector3.up, MyFunctions.setY0ToVector3(ballPosY0 - bodyPos));
        right.Normalize();
        TargetPosition = ballPosY0 - right * 0.3f;
        //float distance = Vector3.Distance(ballPos, bodyPos + transModelo.right * 0.3f) / controllerDistance.maxDistance;
        float offset = radio + 0.3f;
        float d = Vector3.Distance(MyFunctions.setY0ToVector3(ballPosY0 - right * 0.3f), MyFunctions.setY0ToVector3(bodyPos));
        float distance = Mathf.Clamp01(d - offset) / (controllerDistance.maxDistance);
        distance = distance < 0.001f ? 0 : distance;

        if (Input.GetKeyDown(ComponentsKeys.defensivePosition))
        {
            movementValues.isDefensivePosition = !movementValues.isDefensivePosition;
           
        }
        if (movementValues.isDefensivePosition)
        {
            DesiredLookDirection = BodyTargetDirection;
            Vector3 dirRotated = Quaternion.AngleAxis(angle, Vector3.up) * BodyTargetDirection;
            ForwardDesiredDirection = dirRotated;
        }
        else if (vertical != 0)
        {
            //speedRotation = Mathf.Lerp(movimentValues.minRotAcceleration, movimentValues.maxRotAcceleration, 1 - (verticalRig / (movimentValues.MaxForwardRunSpeed + movimentValues.MaxForwardSprintSpeed)));
            //speedRotation = Mathf.Lerp(10, 20, 1 - (verticalRig / (movimentValues.MaxForwardRunSpeed + movimentValues.MaxForwardSprintSpeed)));
            Vector3 dirRotated = Quaternion.AngleAxis(angle, Vector3.up) * BodyTargetDirection;
            //Quaternion transModeloRotation = Quaternion.Lerp(transModelo.rotation, Quaternion.LookRotation(dirRotated), Time.deltaTime *speedRotation);
            //transModelo.rotation = transModeloRotation;
            DesiredLookDirection = dirRotated;
            ForwardDesiredDirection = dirRotated;
            
        }

        float magnitudeBallVelocity = Mathf.Clamp01(ballVelocity.magnitude / movimentValues.maxVelocityBall);
        targetVelocityBall = Mathf.Lerp(targetVelocityBall, magnitudeBallVelocity, Time.deltaTime * movimentValues.speedChangeVelocityBall);
        curveVelocityEvaluated = movimentValues.curveVelocity.Evaluate(targetVelocityBall);
        
        //float distance = movimentValues.distanceCurve.Evaluate(distance);

        speedRotation2 = movimentValues.speedRotationCurve.Evaluate((1 - getSpeedRotation1(verticalAxes, horizontalAxes) / 180));
        //verticalRig = speedRotation2 * vertical *Mathf.Lerp(curveVelocityEvaluated * movimentValues.maxVelocityBall, movimentValues.MaxForwardRunSpeed + movimentValues.MaxForwardSprintSpeed * sprintAxes, distance+Mathf.Abs(angle/180));
        verticalRig = vertical * (playerComponents.getRunSpeed() + playerComponents.getSprintSpeed() * sprintAxes);

        
        ForwardDesiredSpeed = verticalRig;

        playerComponents.movementCtrl.rotation(Time.deltaTime);
        playerComponents.movementCtrl.getAdjustedForwardVelocitySpeed(Time.deltaTime);
        playerComponents.movementCtrl.animator(Time.deltaTime);
        


        /*
        forwardAnim = Mathf.Lerp(forwardAnim, (componentsPlayer.playerComponents.playerData.Speed) /movimentValues.MaxForwardRunSpeed,Time.deltaTime*10);
        float horizontalSpeed = getHorizontalSpeed(componentsPlayer.transBody.forward * verticalRig);
        float verticalSpeed = getVerticalSpeed(componentsPlayer.transBody.forward * verticalRig);
        sprintAnim = Mathf.Lerp(sprintAnim,(componentsPlayer.playerComponents.playerData.Speed - movimentValues.MaxForwardRunSpeed) / (movimentValues.MaxForwardSprintSpeed),Time.deltaTime*10);

        anim.SetFloat("vertical", verticalSpeed, 0.1f,Time.deltaTime * GeneralPlayerParameters.speedAnim);
        anim.SetFloat("horizontal", horizontalSpeed, 0.1f, Time.deltaTime * GeneralPlayerParameters.speedAnim);*/


    }
    
    private void FixedUpdate()
    {
        if (!componentsPlayer.scriptsPlayer.raycastWall.isHitting)
        {
            Vector3 trace = componentsPlayer.transModelo.forward;
            
            //componentsPlayer.rigBody.MovePosition(componentsPlayer.rigBody.position + componentsPlayer.transBody.forward * verticalRig * Time.fixedDeltaTime);
        }
        playerComponents.movementCtrl.movement(Time.fixedDeltaTime);
    }
    
}

