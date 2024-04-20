using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
public class InstantiateBody : MonoBehaviourPunCallbacks
{
    public GameObject PlayerPref;

    public Transform parent;
    
    
    public void SpawnBody(Vector3 position,Quaternion rotation)
    {
        GameObject player = Instantiate(PlayerPref);
        PhotonView pVBody = player.GetComponent<PhotonView>();
        PhotonView pVModelo = player.transform.Find("Modelo").GetComponent<PhotonView>();
        if (PhotonNetwork.AllocateViewID(pVBody) && PhotonNetwork.AllocateViewID(pVModelo))
        {
            object[] data = new object[]
            {
            position, rotation, pVBody.ViewID,pVModelo.ViewID
            };

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = EventCaching.AddToRoomCache
            };

            SendOptions sendOptions = new SendOptions
            {
                Reliability = true
            };

            PhotonNetwork.RaiseEvent(CodeEventsNet.Body, data, raiseEventOptions, sendOptions);
            setupComponents(player);
        }
        else
        {
            Debug.LogError("Failed to allocate a ViewId.");

            Destroy(player);
        }
    }
    
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == CodeEventsNet.Body)
        {
            object[] data = (object[])photonEvent.CustomData;

            GameObject player = (GameObject)Instantiate(PlayerPref, (Vector3)data[0], (Quaternion)data[1]);
            PhotonView photonView = player.GetComponent<PhotonView>();
            photonView.ViewID = (int)data[2];
        }
    }
    void setupComponents(GameObject gameObject)
    {
        /*
        componentsPlayer.transBody = gameObject.transform;
        componentsPlayer.transModelo = gameObject.transform.Find("Modelo");
        componentsPlayer.rigBody = gameObject.GetComponent<Rigidbody>();
        componentsPlayer.animatorPlayer = componentsPlayer.transModelo.GetComponent<Animator>();
        componentsPlayer.colliderPlayer = gameObject.GetComponent<Collider>();
        componentsPlayer.velocityClass = componentsPlayer.transModelo.GetComponent<Velocity>();
        componentsPlayer.photonView = gameObject.GetComponent<PhotonView>();
        gameObject.transform.parent = parent;*/
    }
}
