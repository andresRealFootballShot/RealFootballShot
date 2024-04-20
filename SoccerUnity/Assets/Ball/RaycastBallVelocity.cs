using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//el rayo va desde el balón con direción igual a la de su velocidad.
public class RaycastBallVelocity : MyRaycastHit
{
    public BallComponents componentsBall;
    public float maxRayLenght=100;
    public MonoVariable<Vector3> hitPosition;
    public MonoVariable<Collider> colliderVariable;
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit hit;
        Vector3 origin,direction;
        origin = componentsBall.rigBall.position;
        direction = componentsBall.rigBall.velocity.normalized;
        Ray ray = new Ray(origin, direction);
        if (Physics.Raycast(ray, out hit, maxRayLenght, layerMask))
        {
            isHitting = true;
            hitPosition.Value = hit.point;
            colliderVariable.Value = hit.collider;
        }
        else
        {
            isHitting = false;
        }
    }
}
