using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateNetScene : MonoBehaviour
{
    public string info;
    public GameObject pref;
    void Start()
    {
        
    }

    // Update is called once per frame
    void InstantiateNet(Vector3 position,Quaternion rotation,byte group,object[]data)
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.InstantiateSceneObject(pref.name, Vector3.zero, rotation, group, data);
        }
    }
}
