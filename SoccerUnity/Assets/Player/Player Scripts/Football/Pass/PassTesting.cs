using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassTesting : MonoBehaviour
{
    public Rigidbody ballRididbody;
    public Transform dirTransform;
    public float force = 20;
    public float timeScale = 1;
    bool nextFrame;
    void Start()
    {
        
    }

#if UNITY_EDITOR
    void Update()
    {
        if (nextFrame)
        {
            Time.timeScale = 0;
            nextFrame = false;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ballRididbody.velocity = Vector3.zero;
            ballRididbody.angularVelocity = Vector3.zero;
            ballRididbody.position = dirTransform.position + Vector3.up * MatchComponents.ballRadio;
            ballRididbody.AddForce(dirTransform.forward * force, ForceMode.VelocityChange);
            if (timeScale == 0)
            {
                nextFrame = true;
            }
            else
            {
                Time.timeScale = timeScale;
            }
        }
    }
#endif
}
