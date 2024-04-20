using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moviment : PlayerComponent
{
    public ComponentsPlayer componentsPlayer;
    public ControllerDistance controllerDistance;
    public ResistanceController resistanceController;
    public MovimentValues movimentValues;
    protected float targetVelocityBall;
    [HideInInspector] public float speedRotation,speedRotation2,angle, verticalAxes, horizontalAxes, sprintAxes, forwardAnim, sprintAnim, curveVelocityEvaluated, verticalRig, vertical;
    
    void Start()
    {

    }
    protected float getVerticalSpeed(Vector3 velocity)
    {
        float verticalSpeed = Vector3.Dot(componentsPlayer.transBody.forward, velocity);
        return verticalSpeed;
    }
    protected float getHorizontalSpeed(Vector3 velocity)
    {
        float horizontalSpeed = Vector3.Dot(componentsPlayer.transBody.right, velocity);
        return horizontalSpeed;
    }

    public float getSpeedRotation1(float verticalAxes,float horizontalAxes)
    {
        
        Vector3 axes = new Vector3(horizontalAxes,0,verticalAxes);
        Vector3 dirAxesRespectCamera = componentsPlayer.transCamera.TransformDirection(axes);
        dirAxesRespectCamera.y = 0;
        Vector3 trace = componentsPlayer.transModelo.forward;
        Vector3 dirModelo = componentsPlayer.transBody.forward;
        float angle = Vector3.Angle(dirModelo, dirAxesRespectCamera);
        if (angle < 10)
            return 0;
        else return angle;
    }
    public float getSpeedRotation2(float verticalAxes, float horizontalAxes)
    {

        Vector3 axes = new Vector3(horizontalAxes, 0, verticalAxes);
        Vector3 trace = componentsPlayer.transModelo.TransformDirection(axes);
        Vector3 dirAxesRespectCamera = componentsPlayer.transBody.TransformDirection(axes);
        dirAxesRespectCamera.y = 0;
        Vector3 trace2 = componentsPlayer.componentsBall.transBall.position - componentsPlayer.transModelo.position - componentsPlayer.transCamera.right * 0.3f;
        Vector3 dirModeloBall = componentsPlayer.componentsBall.transBall.position - componentsPlayer.transBody.position - componentsPlayer.transCamera.right * 0.3f;
        float angle = Vector3.Angle(dirModeloBall, dirAxesRespectCamera);
        if (angle < 10)
            return 0;
        else return angle;
    }
    protected float FindAngle(Vector3 fromVector, Vector3 toVector)
    {
        if (toVector == Vector3.zero)
            return 0;

        float angle = Vector3.Angle(fromVector, toVector);
        Vector3 signo = Vector3.Cross(fromVector, toVector);
        angle *= Mathf.Sign(signo.y);
        return angle;
    }
    public void EnableScript()
    {
        enabled = true;
    }
    public void DisableScript()
    {
        enabled = false;
    }
    private void OnDisable()
    {
        return;
        if (componentsPlayer.animatorPlayer != null)
        {
            componentsPlayer.animatorPlayer.SetFloat("vertical", 0);
            componentsPlayer.animatorPlayer.SetFloat("sprint", 0);
        }
    }
}
