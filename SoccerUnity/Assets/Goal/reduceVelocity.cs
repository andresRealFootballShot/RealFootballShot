using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class reduceVelocity : MonoBehaviour
{
    public float velocityReduced = 5;
    public float threshold = 20;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "ObjectGoal")
        {
            if(other.attachedRigidbody.velocity.magnitude > threshold)
            {
                other.attachedRigidbody.velocity = other.attachedRigidbody.velocity.normalized * velocityReduced;
            }
        }
    }
}
