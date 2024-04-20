using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPosAdjust : MonoBehaviour
{
    public ComponentsPlayer componentsPlayer;
    public ComponentsKeys keys;
    public float maxDisHeight, maxDisForward;
    public AnimationCurve heightCurve, forwardCurve;
    public float minY, maxY, minZ, maxZ;
    public Vector3 normalPosition;
    public float speedCamera;
    public CamPosShot camPosShot;
    bool shotPressed;
    void Start()
    {
        
    }

    private void Update()
    {
        if(Input.GetKeyDown(ComponentsKeys.keyShot)|| Input.GetKeyDown(ComponentsKeys.joyShot))
        {
            shotPressed = true;
        }
        if (Input.GetKeyUp(ComponentsKeys.keyShot) || Input.GetKeyUp(ComponentsKeys.joyShot))
        {
            shotPressed = false;
        }
    }
    void LateUpdate()
    {
        if (!shotPressed)
        {
            Vector3 globalPositionBall = componentsPlayer.componentsBall.transCenterBall.position;
            Vector3 localPivotBall = componentsPlayer.transPivotCamera2.InverseTransformPoint(globalPositionBall);
            Vector3 localCameraBall = componentsPlayer.transCamera.InverseTransformPoint(globalPositionBall);
            float distanceHeight = Vector3.Distance(componentsPlayer.transBody.position, globalPositionBall) / maxDisHeight;
            float distanceForward = Vector3.Distance(componentsPlayer.transBody.position, globalPositionBall) / maxDisForward;
            float heightEvaluated = heightCurve.Evaluate(distanceHeight);
            float forwardEvaluated = forwardCurve.Evaluate(distanceForward);
            float posY = Mathf.Lerp(Mathf.Clamp(localPivotBall.y, minY, maxY), normalPosition.y, heightEvaluated);
            float posZ = Mathf.Lerp(minZ, normalPosition.z, forwardEvaluated);
            Vector3 posOffsetCamera = new Vector3(normalPosition.x, posY, posZ);
            componentsPlayer.transCamera.localPosition = Vector3.Lerp(componentsPlayer.transCamera.localPosition, posOffsetCamera, Time.deltaTime * speedCamera);
        }
        
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
