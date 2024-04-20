using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PruebasResetBallPosition : MonoBehaviour
{
    public Rigidbody ball;
    public Transform initPosition;
    public bool nextToThePlayer;
    public bool useTimeScale = true;
    public Transform player;
    public Transform centerGoal;
    public float maxAngle=30,minDistance=15,maxDistance=35;
  
#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            
            MatchEvents.continueMatch.Invoke();
            MatchComponents.ballComponents.rigBall.velocity = Vector3.zero;
            MatchComponents.ballComponents.rigBall.angularVelocity = Vector3.zero;
            if (nextToThePlayer)
            {
                test2();
                if (useTimeScale)
                    Time.timeScale = 1;
            }
            else
            {

                MatchComponents.ballComponents.rigBall.position = initPosition.position;
            }
            
        }
        if (useTimeScale)
        {
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                Time.timeScale = 0.25f;
            }
        }
    }
    void test2()
    {
        MatchComponents.ballComponents.rigBall.position = player.position + player.forward;
    }
    void test1()
    {
        float angle = Random.Range(-maxAngle, maxAngle);
        float distance = Random.Range(minDistance, maxDistance);
        Vector3 dir = Quaternion.AngleAxis(angle,Vector3.up)* centerGoal.forward.normalized;
        dir *= distance;
        player.position = MyFunctions.setY0ToVector3(centerGoal.position) + dir;
        player.rotation = Quaternion.LookRotation(-dir.normalized, Vector3.up);
        MatchComponents.ballComponents.rigBall.position = player.position + player.forward;
    }
#endif
}
