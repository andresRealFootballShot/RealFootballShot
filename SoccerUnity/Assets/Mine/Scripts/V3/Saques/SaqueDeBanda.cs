using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class SaqueDeBanda : MonoBehaviour
{
    public SaqueBandaCtrl saqueBandaCtrl;
    void Start()
    {

    }
    private void Update()
    {
        
    }
    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ObjectGoal")
        {
            saqueBandaCtrl.BallOut(transform);
        }
    }

}
