using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public Transform ballTransform;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ballTransform.position.y < 0)
        {
            ballTransform.position = new Vector3(ballTransform.position.x,ballTransform.localScale.y*0.5f, ballTransform.position.z);
        }
    }
}
