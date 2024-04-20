using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpLimit : MonoBehaviour
{
    public AnimationCurve velocity;
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "ObjectGoal")
        {
            other.attachedRigidbody.velocity *= velocity.Evaluate(other.attachedRigidbody.velocity.magnitude/100);
        }
    }
}
