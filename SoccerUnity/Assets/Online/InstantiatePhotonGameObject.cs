using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Para utilizar PhotonNetwork.AllocateViewID hay que hacerlo con un prefab y no con un GameObject de la escena
public class InstantiatePhotonGameObject : MonoBehaviour, IOnEventCallback
{
    public enum AllocationType
    {
        Player, Scene
    }
    public ReceiverGroup receiverGroup;
    public EventCaching eventCaching;
    public GameObject prefab;
    public AllocationType allocationType;
    public Transform parent;
    //public Transform positionTrans, rotationTrans;
    public Vector3 position, rotation;
    /*public UnityEvent<GameObject> gameObjectIsInstantiatedEvent;
    public UnityEvent errorEvent;*/
    public event gameObjectDelegate gameObjectIsInstantiatedEvent;
    public event emptyDelegate errorEvent;
    byte code=0;
    bool isSetuped = false;
    public void setup(byte code,Vector3 position,Vector3 rotation)
    {
        this.code = code;
        this.position = position;
        this.rotation = rotation;
        isSetuped = true;
    }
    public void Instantiate()
    {
        GameObject newGameObject = Instantiate(prefab, position,Quaternion.Euler(rotation), parent);
        PhotonView photonView = newGameObject.GetComponent<PhotonView>();
        bool allocationResult = false;
        switch (allocationType)
        {
            case AllocationType.Player:
                allocationResult = PhotonNetwork.AllocateViewID(photonView);
                break;
            case AllocationType.Scene:
                allocationResult = PhotonNetwork.AllocateSceneViewID(photonView);
                break;
        }
        if (allocationResult)
        {
            object[] data = new object[]
            {
             photonView.ViewID
            };

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                Receivers = receiverGroup,
                CachingOption = eventCaching
            };
            SendOptions sendOptions = new SendOptions
            {
                Reliability = true
            };

            PhotonNetwork.RaiseEvent(code, data, raiseEventOptions, sendOptions);
            gameObjectIsInstantiatedEvent?.Invoke(newGameObject);
        }
        else
        {
            Debug.LogError("Failed to allocate a ViewId.");
            Destroy(newGameObject);
            errorEvent?.Invoke();
        }
    }
    public void OnEvent(EventData photonEvent)
    {
        StartCoroutine(waitUntilCodeIsSetuped(photonEvent));
    }
    IEnumerator waitUntilCodeIsSetuped(EventData photonEvent)
    {
        yield return new WaitUntil(() => isSetuped);
        if (photonEvent.Code == code)
        {
            object[] data = (object[])photonEvent.CustomData;
            GameObject newGameObject = Instantiate(prefab, position, Quaternion.Euler(rotation), parent);
            PhotonView photonView = newGameObject.GetComponent<PhotonView>();
            photonView.ViewID = (int)data[0];
            gameObjectIsInstantiatedEvent?.Invoke(newGameObject);
        }
    }
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
