using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ExitMatch : MonoBehaviour
{
    void Start()
    {
        
    }
    public void Exit_Click()
    {
        LeaveRoom.leaveRoom();
    }
}
