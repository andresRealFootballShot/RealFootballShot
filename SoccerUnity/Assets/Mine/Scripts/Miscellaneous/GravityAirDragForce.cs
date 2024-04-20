using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityAirDragForce : MonoBehaviour
{
    new Rigidbody rigidbody;
    public float drag;
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.drag = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        applyForce();
    }
    void applyForce()
    {
        Vector3 velocityV2 = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
        float v = velocityV2.magnitude;
        float forceAmount = v * drag * rigidbody.mass;
        rigidbody.AddForce((-velocityV2.normalized * forceAmount) - (Vector3.up * 9.81f * rigidbody.mass));
    }
}
