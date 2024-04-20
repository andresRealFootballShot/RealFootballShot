using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class GoalKickRandom : MonoBehaviour
{
    public Vector3 velocityMax;
    public Vector3 velocityMin;
    public Vector3 position;
    public float timeGoalKickRandom;
    public ComponentsPorteria componentsPorteria;
    void Start()
    {
        
    }
    private void Update()
    {
        Debug.DrawRay(componentsPorteria.transPortero.position,componentsPorteria.transPortero.TransformDirection(Vector3.forward));
    }
    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        
        if (other.tag == "ObjectGoal"&&enabled)
        {
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.IsMasterClient)
                {

                    Invoke("KickRandom", timeGoalKickRandom);
                }
            }
            else
            {
                Invoke("KickRandom", timeGoalKickRandom);
            }
            
            
        }
    }
    void KickRandom()
    {

        Rigidbody rigBall = componentsPorteria.componentsBall.rigBall;
        componentsPorteria.componentsBall.transBall.position = componentsPorteria.transPortero.TransformPoint(position);
        Vector3 randomVelocity = new Vector3(Random.Range(velocityMin.x, velocityMax.x), Random.Range(velocityMin.y, velocityMax.y), Random.Range(velocityMin.z, velocityMax.z));
        rigBall.velocity = Vector3.zero;
        rigBall.velocity = componentsPorteria.transPortero.TransformDirection(randomVelocity);
        rigBall.angularVelocity = Vector3.zero;

    }
}
