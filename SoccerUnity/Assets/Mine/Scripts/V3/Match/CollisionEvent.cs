using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class CollisionEvent : MonoBehaviourPunCallbacks
{
    public delegate void CollisionDelegate(Collision collision);
    public event CollisionDelegate Event;
    

    // Update is called once per frame
    void OnCollisionEnter(Collision collision)
    {
        Event?.Invoke(collision);
        
    }
    
}
