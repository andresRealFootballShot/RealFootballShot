using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class InstantiateTeamCtrl : MonoBehaviour
{
    public GameObject teamCtrlPref;
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.InstantiateSceneObject(teamCtrlPref.name, Vector3.zero, Quaternion.identity, 0, null);
        } 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
