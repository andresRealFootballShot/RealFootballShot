using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCameraBody : MonoBehaviour
{
    public ComponentsPlayer componentsPlayer;
    public float speedCamera, speedBody;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        Quaternion targetRotation = Quaternion.LookRotation(componentsPlayer.componentsBall.transCenterBall.position - componentsPlayer.transCamera.position);
        componentsPlayer.transCamera.rotation = Quaternion.Lerp(componentsPlayer.transCamera.rotation, targetRotation, speedCamera * Time.deltaTime);
        Quaternion targetRotationBody = Quaternion.LookRotation(new Vector3(componentsPlayer.componentsBall.transCenterBall.position.x, componentsPlayer.transCamera.position.y, componentsPlayer.componentsBall.transCenterBall.position.z) - componentsPlayer.transCamera.position);
        componentsPlayer.transBody.transform.rotation = Quaternion.Lerp(componentsPlayer.transBody.transform.rotation, targetRotationBody, speedBody * Time.deltaTime);
    }
    public void InitLook()
    {
        componentsPlayer.transCamera.eulerAngles = componentsPlayer.transBody.eulerAngles;
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
