using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPosNormal : MonoBehaviour
{
    public ComponentsPlayer componentsPlayer;
    public Vector3 localPositionCamera;
    public float speedCamera;
    void Start()
    {
       
    }
   
    void LateUpdate()
    {
       Vector3 trace = componentsPlayer.transModelo.TransformPoint(localPositionCamera);
       componentsPlayer.transCamera.position = componentsPlayer.transBody.TransformPoint(localPositionCamera);
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
