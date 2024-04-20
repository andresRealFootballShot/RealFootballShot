using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BallControl : SoccerPlayerComponent
{
    float offsetRadio = 0.25f;
    public float minSpeedControl = 3;
    public float maxSpeedControl = 6;
    Coroutine coroutine;
    public float minTimeHitAgain = 0.1f;
    public float maxTimeHitAgain = 0.5f;
    public float timeHitAgain { get=> Mathf.Lerp(minTimeHitAgain, maxTimeHitAgain, 1 - playerSkills.ballTimeControl);}
    float minBallBody_BallTargetAngle = 75;
    float minBallTarget_BallVelocityAngle=75;
    float minBodyForward_BallTargetAngle = 75;
    bool addKickPositions;
    List<Vector3> kickPositions = new List<Vector3>();
    List<Vector3> kickDirections = new List<Vector3>();
    public float speedOrientedControl { get=>Mathf.Lerp(minSpeedControl, maxSpeedControl, playerSkills.ballForceControl);}
    public float minBallSpeedControl { get => Mathf.Lerp(2, 17, playerSkills.ballControl); }
    public float ballControl { get => Mathf.Lerp(7, 25, playerSkills.ballControl); }
    public float maxBallControlYAngle { get => Mathf.Lerp(45, 0, playerSkills.ballControl); }
    public AnimationCurve ballControlYAngleCurve { get => playerParameters.ballControlYAngleCurve; }
    float reboundForce { get => Mathf.Lerp(3,0, playerSkills.ballControl); }
    public void StartProcess()
    {
        coroutine = StartCoroutine(controlBallChecker());
    }
    public void StopProcess()
    {
        StopCoroutine(coroutine);
    }
    IEnumerator controlBallChecker()
    {
        while (true)
        {
            float ballYPosition = ballPosition.y;
            
            if (((Mathf.Abs(ballVelocity.y)>0.1f && ballYPosition>ballRadio+0.3f) || ballBody_BallTargetAngle> minBallBody_BallTargetAngle || (ballTarget_BallVelocityAngle > minBallTarget_BallVelocityAngle && ballSpeed> minBallSpeedControl)) && reachBall())
            {
                ballIsOrientedControlled = false;

                Vector3 randomControlDir = randomControl();

                Vector3 axis = Vector3.Cross(bodyBallY0Direction, bodyBallTargetY0Direction);
                Vector3 dir = Quaternion.AngleAxis(Mathf.Clamp(bodyForward_BallTargetAngle, -90, 90), axis) * bodyY0Forward * (bodyBallRadio + offsetRadio);
                //DrawArrow.ForDebug(bodyPosition + Vector3.up * 0.1f, dir.normalized * Vector3.Distance(bodyPosition, ballTargetPosition), Color.blue);
                
                Vector3 force = bodyPosition + dir - ballPosition + Vector3.up*ballRadio + randomControlDir;
                force *= speedOrientedControl;
                ballRigidbody.velocity = force;

                if (addKickPositions)
                {
                    kickPositions.Add(ballRigidbody.position);
                    kickDirections.Add(force);
                }
                float t = 0;
                while(t< timeHitAgain && reachBall())
                {
                    //bodyTransform.gameObject.layer = LayerMask.NameToLayer("BallTransparent");
                    if (ballTargetDistance < 0.5f)
                    {
                        ballRigidbody.velocity = Vector3.Lerp(ballRigidbody.velocity,Vector3.zero,Time.deltaTime*10);
                        ballRigidbody.angularVelocity = Vector3.Lerp(ballRigidbody.angularVelocity, Vector3.zero, Time.deltaTime * 10);
                        if (addKickPositions)
                        {
                            kickPositions.Add(ballRigidbody.position);
                            kickDirections.Add(force);
                        }
                    }
                    if (ballPosition.y <= ballRadio+0.05f && Mathf.Abs(ballRigidbody.velocity.y)>0.1f)
                    {
                        ballRigidbody.velocity = MyFunctions.setYToVector3(ballRigidbody.velocity,0);
                        if (addKickPositions)
                        {
                            kickPositions.Add(ballRigidbody.position);
                            kickDirections.Add(force);
                        }
                        //ballRigidbody.angularVelocity = Vector3.zero;
                        //ballRigidbody.angularVelocity = Vector3.Lerp(ballRigidbody.angularVelocity,Vector3.zero,Time.deltaTime*1);
                    }
                    if (bodyForward_BallTargetAngle < 10)
                    {
                        //float x = Random.Range(-2, 2);
                        //float z = Random.Range(-1, 2);
                        float x = 0;
                        float z = 0;
                        //print(t +" "+vy + " "+ballRigidbody.velocity.y);
                        ballRigidbody.velocity = new Vector3(x, ballRigidbody.velocity.y, z);
                        if (addKickPositions)
                        {
                            kickPositions.Add(ballRigidbody.position);
                            kickDirections.Add(force);
                        }
                        //ballRigidbody.angularVelocity = Vector3.zero;
                        //ballRigidbody.angularVelocity = Vector3.Lerp(ballRigidbody.angularVelocity, Vector3.zero, Time.deltaTime * 5);
                    }
                    if (ballBody_BallTargetAngle < minBallBody_BallTargetAngle && bodyForward_BallTargetAngle < minBodyForward_BallTargetAngle && Mathf.Abs(ballVelocity.y) < 2f && ballYPosition <= ballRadio + 0.3f)
                    {
                        ballRigidbody.angularVelocity = Vector3.Lerp(ballRigidbody.angularVelocity,Vector3.zero,Time.deltaTime*10);
                        ballIsOrientedControlled = true;
                    }
                    yield return null;
                    t += Time.deltaTime;
                }
            }
            else if(Mathf.Abs(ballVelocity.y) < 0.1f && ballYPosition <= ballRadio + 0.3f && ballBodyAngle < 45 && BodyBallXZDistance < scope && ballBody_BallTargetAngle < minBallBody_BallTargetAngle && bodyForward_BallTargetAngle < minBodyForward_BallTargetAngle)
            {
                //ballRigidbody.angularVelocity = Vector3.Lerp(ballRigidbody.angularVelocity, Vector3.zero, Time.deltaTime * 10);
                ballIsOrientedControlled = true;
                if (ballTargetDistance < 0.5f)
                {
                    ballRigidbody.velocity = Vector3.Lerp(ballRigidbody.velocity, Vector3.zero, Time.deltaTime * 10);
                    ballRigidbody.angularVelocity = Vector3.Lerp(ballRigidbody.angularVelocity, Vector3.zero, Time.deltaTime * 10);
                }
                yield return null;
            }
            else
            {
                ballIsOrientedControlled = false;
                yield return null;
            }
        }
    }
    bool reachBall()
    {
        return ballBodyAngle < 80 && BodyBallXZDistance < scope && ballPosition.y <= bodyHeight + ballRadio;
    }
    Vector3 randomControl()
    {
        if (ballBody_BallVelocityAngle < 45)
        {
            float control = Mathf.Clamp01((ballSpeed - minBallSpeedControl) / (ballControl));
            
            control *= reboundForce;
            Vector3 direction = -ballVelocity;
            float random = Random.Range(-1.0f, 1.0f);
            float absRandom = Mathf.Abs(random);
            float fx = ballControlYAngleCurve.Evaluate(absRandom);
            fx *= Mathf.Sign(random);
            float angle = Mathf.Lerp(-maxBallControlYAngle, maxBallControlYAngle, (fx + 1) / 2);
            direction = Quaternion.AngleAxis(angle, Vector3.up) * direction.normalized;
            direction *= control;
            //print("control="+control + " ballSpeed=" + ballSpeed + " minBallSpeedControl=" + minBallSpeedControl + " ballControl=" + ballControl + " maxBallControlYAngle=" + maxBallControlYAngle+ " reboundForce="+ reboundForce);
            //print(control + " " + fx+ " "+ angle + " "+direction);
            return direction;
        }
        else
        {
            return Vector3.zero;
        }
    }
    public void debug(float posY)
    {
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        if (ballIsOrientedControlled || true)
        {
            style.normal.textColor = Color.green;
        }
        else
        {
            style.normal.textColor = Color.red;
        }
        //Handles.Label(headPosition + Vector3.up * posY, "ballIsOrientedControlled=" + ballIsOrientedControlled, style);
        Handles.Label(headPosition + Vector3.up * posY, "BallBodyDistance=" + BodyBallXZDistance, style);
#endif
    }
    private void Update()
    {
        for (int i = 0; i < kickPositions.Count; i++)
        {
            //DrawArrow.ForDebug(kickPositions[i], kickDirections[i]);
        }
    }
}
