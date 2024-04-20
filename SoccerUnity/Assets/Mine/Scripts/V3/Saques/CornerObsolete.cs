using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornerObsolete : MonoBehaviour
{
    public delegate void CornerDelegate(CornerObsolete corner);
    public event CornerDelegate CornerEvent;
    public Transform transPosCornerBall;
    public string teamName;
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "ObjectGoal")
        {
            CornerEvent?.Invoke(this);
        }
    }
}
