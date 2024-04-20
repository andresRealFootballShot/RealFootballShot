using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringNet : MonoBehaviourPunCallbacks, IPunObservable
{
    public string info;
    public Variable<string> variable;
    void Start()
    {

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.IsWriting)
        {
            stream.SendNext(variable.Value);
        }
        else
        {
            variable.Value = (string)stream.ReceiveNext();
        }
    }
}
