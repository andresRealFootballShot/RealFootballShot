using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastWall : MyRaycastHit
{
    public ComponentsPlayer componentsPlayer;
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Transform trace = componentsPlayer.transModelo;
        Transform transModelo = componentsPlayer.transBody;
        Vector3 origin = transModelo.position + transModelo.up * 0.1f;
        Vector3 direction = transModelo.forward;
        float maxDistance = componentsPlayer.colliderPlayer.radius + 0.01f;
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, maxDistance, layerMask))
        {
            isHitting = true;
        }
        else
        {
            isHitting = false;
        }
        this.hit = hit;
    }
}
