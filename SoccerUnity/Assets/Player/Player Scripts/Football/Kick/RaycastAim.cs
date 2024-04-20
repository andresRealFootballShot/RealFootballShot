using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastAim : MyRaycastHit
{
    public ComponentsPlayer componentsPlayer;
    void FixedUpdate()
    {
        if (componentsPlayer.scriptsPlayer.controllerDistance.isClose())
        {

            Ray ray;
            RaycastHit hit;
            float distance = Vector3.Distance(componentsPlayer.transCamera.position, componentsPlayer.componentsBall.transCenterBall.position);
            ray = componentsPlayer.camera.ScreenPointToRay(componentsPlayer.scriptsPlayer.controllerAim.position);
            if (Physics.Raycast(ray, out hit, distance + 0.5f, layerMask))
            {
                if (tag.Equals("All") || hit.collider.tag == tag)
                {
                    isHitting = true;
                }
                else
                {
                    isHitting = false;
                }
                this.hit = hit;
            }
            else
            {
                isHitting = false;
            }
        }
        else
        {
            isHitting = false;
        }
    }
}
