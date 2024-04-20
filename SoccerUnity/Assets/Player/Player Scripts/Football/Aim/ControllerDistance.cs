using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerDistance : MonoBehaviour
{
    //public float maxDistance,minDistance,heightBody=1.75f;
    //public float maxDistance { get => componentsPlayer.playerComponents.scope; }
    public float maxDistance { get => 1f; }
    public float heightBody { get => componentsPlayer.playerComponents.playerData.height; }
    public ComponentsPlayer componentsPlayer;
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    public bool isClose()
    {
        
        Vector3 bodyPos = componentsPlayer.transBody.position;
        Vector3 ballPos = componentsPlayer.componentsBall.transCenterBall.position;
        float d = Vector3.Distance(ballPos, new Vector3(bodyPos.x, Mathf.Clamp(ballPos.y, bodyPos.y, bodyPos.y + heightBody), bodyPos.z));
        if ( d<= maxDistance)
            return true;
        else
            return false;
    }
    public float GetDistance()
    {
        Vector3 bodyPos = componentsPlayer.transBody.position;
        Vector3 ballPos = componentsPlayer.componentsBall.transCenterBall.position;
        return Vector3.Distance(ballPos, new Vector3(bodyPos.x, Mathf.Clamp(ballPos.y, bodyPos.y, bodyPos.y + heightBody), bodyPos.z));
    }
    public float GetDistanceNormalized()
    {
        Vector3 bodyPos = componentsPlayer.transBody.position;
        Vector3 ballPos = componentsPlayer.componentsBall.transCenterBall.position;
        return Vector3.Distance(ballPos, new Vector3(bodyPos.x, Mathf.Clamp(ballPos.y, bodyPos.y, bodyPos.y + heightBody), bodyPos.z)) / maxDistance;
    }
}
