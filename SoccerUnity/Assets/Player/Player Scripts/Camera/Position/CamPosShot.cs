using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPosShot : MonoBehaviour
{
    public ComponentsPlayer componentsPlayer;
    public AnimationCurve heightCurve, forwardCurve;
    public float minY, maxY, maxDistanceHeight, maxDistanceForward, speed, distance, posY;
    public float angleOffset;
    void Start()
    {
        
    }

    // Update is called once per frame
    public void LateUpdate()
    {
        Vector3 ballPos = componentsPlayer.componentsBall.transCenterBall.position;
        Vector3 bodyPos = componentsPlayer.transBody.position;
        Vector3 camPos = componentsPlayer.transCamera.position;
        Vector3 dirBallBody = new Vector3(bodyPos.x, ballPos.y, bodyPos.z) - ballPos;
        dirBallBody = Quaternion.AngleAxis(angleOffset, Vector3.up) * dirBallBody;
        dirBallBody = (dirBallBody.normalized) * distance;
        float distanceHeight = Vector3.Distance(componentsPlayer.transBody.position, ballPos) / maxDistanceHeight;
        float distanceForward = Vector3.Distance(componentsPlayer.transBody.position, ballPos) / maxDistanceForward;
        float heightEvaluated = heightCurve.Evaluate(distanceHeight);
        float forwardEvaluated = forwardCurve.Evaluate(distanceForward);
        float posOffsetY = Mathf.Lerp(Mathf.Clamp(ballPos.y, minY + bodyPos.y, maxY + bodyPos.y), posY + bodyPos.y, heightEvaluated);
        float posZ = dirBallBody.z;
        float posX = dirBallBody.x;
        Vector3 posOffsetCamera = new Vector3(posX, posOffsetY, posZ) + bodyPos;
        componentsPlayer.transCamera.position = Vector3.Lerp(camPos, posOffsetCamera, Time.deltaTime * speed);
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
