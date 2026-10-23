using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PossessionBallMoviment : Moviment
{
    public float adjustDistanceWithPossession;
    public AnimationCurve speedRotationCurve,distanceCurve;
    float currentSpeed;
    public float acceleration = 5f;
    public Vector3 posOffset=new Vector3(-0.5f,0,0);

   
    void Start()
    {
    }

    private void Update()
    {
        Move();
    }
    void Move()
    {
        

        Animator anim = componentsPlayer.animatorPlayer;
        Transform trace = componentsPlayer.transModelo;
        Transform transModelo = componentsPlayer.transBody;
        Transform transCamera = componentsPlayer.transCamera;
        Vector3 ballPos = ballPosition;
        Vector3 ballPosY0 = MyFunctions.setYToVector3(ballPos, 0);
        Vector3 bodyPos = transModelo.position;
        verticalAxes = 1;
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
        if (vertical != 0 && distance>=0)
        {
            float normalizedVelocity = componentsPlayer.playerComponents.Speed / componentsPlayer.scriptsPlayer.movimentValues.maxSpeed.Value;
            speedRotation = movimentValues.rotationSpeed;
            Vector3 dirRotated = Quaternion.AngleAxis(angle, Vector3.up) * dirBodyBall;
        }

        float magnitudeBallVelocity = Mathf.Clamp01(ballVelocity.magnitude / movimentValues.maxVelocityBall);
        targetVelocityBall = Mathf.Lerp(targetVelocityBall, magnitudeBallVelocity, Time.deltaTime * movimentValues.speedChangeVelocityBall);
        curveVelocityEvaluated = movimentValues.curveVelocity.Evaluate(targetVelocityBall);

        speedRotation2 = speedRotationCurve.Evaluate((1 - getSpeedRotation2(verticalAxes, horizontalAxes) / 180));

        


        verticalRig = speedRotation2 * vertical * distance * (movimentValues.MaxForwardRunSpeed + movimentValues.MaxForwardSprintSpeed * sprintAxes);
        if(verticalRig > currentSpeed)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, verticalRig, Time.deltaTime * acceleration);
        }
        else
        {
            currentSpeed = verticalRig;
        }
        Vector3 forward = ballPos - bodyPos;
        forward.y = 0;
        forward.Normalize();
        if (playerData.shotKick || playerData.passKick)
        {

            TargetPosition = ballPos + forward * posOffset.z + right * posOffset.x;
        }
        else
        {
            TargetPosition = ballPos + transCamera.TransformDirection(posOffset);
        }
        //ForwardDesiredSpeed = currentSpeed;
        float speed = movimentValues.defaultMaxForwardSpeed;
        if (BodyTargetXZDistance < ballScope+0.2f)
        {
            ForwardDesiredSpeed = Mathf.Min(MatchComponents.ballXZSpeed, speed);
            movimentValues.maxSpeedForReachBall = Mathf.Min(MatchComponents.ballXZSpeed, speed);
        }
        else
        {
            ForwardDesiredSpeed = Mathf.Lerp(Mathf.Min(MatchComponents.ballXZSpeed, speed), movimentValues.defaultMaxForwardSpeed, (BodyBallXZDistance)/2f);
            movimentValues.maxSpeedForReachBall =0;
        }
        
        //movimentValues.forwardAcceleration = 50;
        //movimentValues.forwardDeceleration = 50;
        LookTarget = false;
        
        
        Vector3 lookDirection = ballPos - bodyPosition;
        lookDirection.y = 0;
        DesiredLookDirection = lookDirection;
        //float horizontalSpeed = getHorizontalSpeed(componentsPlayer.transBody.forward * verticalRig);
        //float verticalSpeed = getVerticalSpeed(componentsPlayer.transBody.forward * verticalRig);
        //anim.SetFloat("vertical", verticalSpeed, 0.1f, Time.deltaTime * GeneralPlayerParameters.speedAnim);
        //anim.SetFloat("horizontal", horizontalSpeed, 0.1f, Time.deltaTime * GeneralPlayerParameters.speedAnim);
        playerComponents.movementCtrl.rotation(Time.deltaTime);

        playerComponents.movementCtrl.getAdjustedForwardVelocitySpeed(Time.deltaTime);
        playerComponents.movementCtrl.animator(Time.deltaTime);
        //movementValues.ForwardDecelerationDistance = 1;
        //movementValues.ForwardAccelerationDistance = 3;
        //movementValues.calculateAccelerations();
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
