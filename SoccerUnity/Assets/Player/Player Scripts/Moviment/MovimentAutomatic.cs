using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class MovimentAutomatic : MonoBehaviour
{
    public NavMeshAgent nav;
    public Transform transformDestiny;
    public Animator anim;
    public Transform transformBody;
    float forward, angle;
    public float speedAnimation = 1, maxDistanceDestiny = 50;
    public float speedAngular = 1;
    bool offsetBool;
    public float stopOffset;
    
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        Vector3 destination = new Vector3(transformDestiny.position.x, transformBody.position.y, transformDestiny.position.z);

        if (nav.remainingDistance <= nav.stoppingDistance)
        {
            if (!offsetBool)
            {
                nav.stoppingDistance += stopOffset;
                offsetBool = true;
            }

        }
        else
        {
            if (offsetBool)
            {
                nav.stoppingDistance -= stopOffset;
                offsetBool = false;
            }
        }
        angle = FindAngle(transformBody.forward, destination - transformBody.position);
        nav.SetDestination(destination);
        forward = Vector3.Project(nav.desiredVelocity, transformBody.forward).magnitude;
        transformBody.LookAt(destination);
        anim.SetFloat("vertical", forward, 0.1f, Time.deltaTime * speedAnimation);
    }
    float FindAngle(Vector3 fromVector, Vector3 toVector)
    {
        if (toVector == Vector3.zero)
            return 0;

        float angle = Vector3.Angle(fromVector, toVector);
        Vector3 signo = Vector3.Cross(fromVector, toVector);
        angle *= Mathf.Sign(signo.y);
        angle *= Mathf.Deg2Rad;
        return angle;
    }
    public void SetDestiny(Transform trans)
    {
        transformDestiny = trans;
    }
    void OnAnimatorMove()
    {
        nav.velocity = anim.deltaPosition / Time.deltaTime;
        transformBody.rotation = anim.rootRotation;
    }
}
