using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossessionBallMoviment : Moviment
{
    public float adjustDistanceWithPossession;
    public AnimationCurve speedRotationCurve,distanceCurve;
    float currentSpeed;
    public float acceleration = 5f;
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
        Vector3 ballPosY0 = MyFunctions.setYToVector3(ballPos, 0);
        Vector3 bodyPos = transModelo.position;
        verticalAxes = 1;
        //horizontalAxes = 1;
        sprintAxes = resistanceController.resistanceVar.Value;
        float ballRadio = componentsPlayer.componentsBall.sphereCollider.radius * componentsPlayer.componentsBall.transBall.localScale.x;
        float bodyRadio = componentsPlayer.colliderPlayer.radius * componentsPlayer.transBody.localScale.x;
        float radio = ballRadio + bodyRadio;
        Rigidbody rigBall = componentsPlayer.componentsBall.rigBall;
        Vector3 ballVelocity = rigBall.velocity;

        angle = FindAngle(Vector3.forward, new Vector3(horizontalAxes, 0, verticalAxes));
        vertical = Mathf.Clamp01(new Vector3(horizontalAxes, 0, verticalAxes).magnitude);
        Vector3 cameraPos = transCamera.position;
        Vector3 right = Vector3.Cross(Vector3.up, MyFunctions.setY0ToVector3(ballPosY0 - bodyPos));
        right.Normalize();
        Vector3 dirBodyBall = new Vector3(ballPos.x, bodyPos.y, ballPos.z) - bodyPos - right * 0.3f;
        float offset = radio + 0.3f;
        float d = Vector3.Distance(MyFunctions.setY0ToVector3(ballPosY0 - right * 0.3f), MyFunctions.setY0ToVector3(bodyPos));
        float distance = Mathf.Clamp01(d - offset) / (controllerDistance.maxDistance- offset);
        distance = distance < 0.001f ? 0 : distance;
        //Debug.DrawLine(bodyPos, MyFunctions.setYToVector3(ballPos - right * 0.3f, bodyPos.y), Color.red);
        //Debug.DrawRay(bodyPos, dirBodyBall, Color.green);
        if (vertical != 0 && distance>=0)
        {
            float normalizedVelocity = componentsPlayer.scriptsPlayer.movimentValues.velocityObsolete.Value / componentsPlayer.scriptsPlayer.movimentValues.maxSpeed.Value;
            speedRotation = movimentValues.rotationSpeed;
            Vector3 dirRotated = Quaternion.AngleAxis(angle, Vector3.up) * dirBodyBall;
            //Quaternion transModeloRotation = Quaternion.Lerp(transModelo.rotation, Quaternion.LookRotation(dirRotated), Time.deltaTime * speedRotation);
            //transModelo.rotation = transModeloRotation;

            DesiredLookDirection = dirRotated;

            ForwardDesiredDirection = dirRotated;
        }

        float magnitudeBallVelocity = Mathf.Clamp01(ballVelocity.magnitude / movimentValues.maxVelocityBall);
        targetVelocityBall = Mathf.Lerp(targetVelocityBall, magnitudeBallVelocity, Time.deltaTime * movimentValues.speedChangeVelocityBall);
        curveVelocityEvaluated = movimentValues.curveVelocity.Evaluate(targetVelocityBall);

        speedRotation2 = speedRotationCurve.Evaluate((1 - getSpeedRotation2(verticalAxes, horizontalAxes) / 180));
        //verticalRig = speedRotation2 * vertical*  distance * Mathf.Lerp(curveVelocityEvaluated * movimentValues.maxVelocityBall, movimentValues.speedRunVertical + movimentValues.speedSprint * sprintAxes, distance + Mathf.Abs(angle / 180));

        //verticalRig = speedRotation2 * vertical* Mathf.Lerp(curveVelocityEvaluated * movimentValues.maxVelocityBall, movimentValues.speedRunVertical + movimentValues.speedSprint, distance + Mathf.Abs(angle / 180));

        verticalRig = speedRotation2 * vertical * distance * (movimentValues.MaxForwardRunSpeed + movimentValues.MaxForwardSprintSpeed * sprintAxes);
        if(verticalRig > currentSpeed)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, verticalRig, Time.deltaTime * acceleration);
        }
        else
        {
            currentSpeed = verticalRig;
        }
        ForwardDesiredSpeed = currentSpeed;
        TargetPosition = ballPosition;
        //float horizontalSpeed = getHorizontalSpeed(componentsPlayer.transBody.forward * verticalRig);
        //float verticalSpeed = getVerticalSpeed(componentsPlayer.transBody.forward * verticalRig);
        //anim.SetFloat("vertical", verticalSpeed, 0.1f, Time.deltaTime * GeneralPlayerParameters.speedAnim);
        //anim.SetFloat("horizontal", horizontalSpeed, 0.1f, Time.deltaTime * GeneralPlayerParameters.speedAnim);
        playerComponents.movementCtrl.rotation(Time.deltaTime);
        playerComponents.movementCtrl.getAdjustedForwardVelocitySpeed(Time.deltaTime);
        playerComponents.movementCtrl.animator(Time.deltaTime);
        
    }
    
    private void FixedUpdate()
    {
        if (!componentsPlayer.scriptsPlayer.raycastWall.isHitting)
        {
            Vector3 trace = componentsPlayer.transModelo.forward;
            //print(currentSpeed);
            //componentsPlayer.rigBody.MovePosition(componentsPlayer.rigBody.position + componentsPlayer.transBody.forward * currentSpeed * Time.fixedDeltaTime);
        }

        playerComponents.movementCtrl.movement(Time.fixedDeltaTime);
    }
}
