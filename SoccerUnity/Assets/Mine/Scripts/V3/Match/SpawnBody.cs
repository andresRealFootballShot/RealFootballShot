using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBody : MonoBehaviourPunCallbacks,IOnEventCallback
{
    public Animation animationEndChooseTeam;
    public GameObject bodyRedPref, bodyBluePref;
    public Transform parent;
    public Transform meParent;
    public MatchDataObsolete matchData;
    public Teams teams;
    void Start()
    {
        
    }
    
    // Update is called once per frame
   public GameObject Spawn(string team)
    {
        animationEndChooseTeam.Play();
        Teams teams = GameObject.FindGameObjectWithTag("Teams").GetComponent<Teams>();
        Transform transSpawn = teams.getSpawn(PhotonNetwork.LocalPlayer.ActorNumber);
        GameObject body=null;
        switch (team)
        {
            case "Red":
                body = Instantiate(bodyRedPref, transSpawn.position, transSpawn.rotation, meParent);
                break;
            case "Blue":
                body = Instantiate(bodyBluePref, transSpawn.position, transSpawn.rotation, meParent);
                break;
        }

        body.transform.parent = meParent;
        setupComponents(body);
        PhotonView pVBody = body.GetComponent<PhotonView>();
        PhotonView pVModelo = body.transform.Find("Modelo").GetComponent<PhotonView>();
        FueraDeJuego fueraDeJuego = GameObject.FindGameObjectWithTag("FueraDeJuego").GetComponent<FueraDeJuego>();
        fueraDeJuego.AddPlayer(PhotonNetwork.LocalPlayer.ActorNumber, body.transform);

        ComponentsPlayer componentsPlayer = GameObject.FindGameObjectWithTag("ComponentsPlayer").GetComponent<ComponentsPlayer>();
        componentsPlayer.SetParentPivot(body);

        if (PhotonNetwork.AllocateViewID(pVBody) && PhotonNetwork.AllocateViewID(pVModelo))
        {
            
            object[] data = new object[]
            {
            transSpawn.position, transSpawn.rotation, pVBody.ViewID,pVModelo.ViewID,team
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
            return body;
        }
        else
        {
            Debug.LogError("Failed to allocate a ViewId.");

            Destroy(body);
            return body;
        }
    }
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == CodeEventsNet.Body)
        {
            object[] data = (object[])photonEvent.CustomData;

            GameObject body = null;
            switch ((string)data[4])
            {
                case "Red":
                    body = Instantiate(bodyRedPref, (Vector3)data[0], (Quaternion)data[1], meParent);
                    break;
                case "Blue":
                    body = Instantiate(bodyBluePref, (Vector3)data[0], (Quaternion)data[1], meParent);
                    break;
            }
            body.transform.parent = parent;
            PhotonView pVBody = body.GetComponent<PhotonView>();
            PhotonView pVModelo = body.transform.Find("Modelo").GetComponent<PhotonView>();
            pVBody.ViewID = (int)data[2];
            pVModelo.ViewID = (int)data[3];
            if (Teams.teamsAreFull())
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.FetchServerTimestamp();
                    PhotonView pVPlayer = GameObject.FindGameObjectWithTag("MyPlayerNet").GetComponent<PhotonView>();
                    pVPlayer.RPC("SendStartMatch", RpcTarget.All, PhotonNetwork.Time);
                }
            }
            FueraDeJuego fueraDeJuego = GameObject.FindGameObjectWithTag("FueraDeJuego").GetComponent<FueraDeJuego>();
            fueraDeJuego.AddPlayer(pVBody.Owner.ActorNumber, body.transform);
        }
    }
    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    void setupComponents(GameObject gameObject)
    {
        GameObject behaviour = GameObject.FindGameObjectWithTag("Player").transform.Find("Behaviour").gameObject;
        ComponentsPlayer componentsPlayer = behaviour.GetComponent<ComponentsPlayer>();
        componentsPlayer.transBody = gameObject.transform;
        componentsPlayer.transModelo = gameObject.transform.Find("Modelo");
        componentsPlayer.rigBody = gameObject.GetComponent<Rigidbody>();
        componentsPlayer.animatorPlayer = componentsPlayer.transModelo.GetComponent<Animator>();
        componentsPlayer.colliderPlayer = gameObject.GetComponent<CapsuleCollider>();
        //componentsPlayer.photonView = gameObject.GetComponent<PhotonView>();
        gameObject.transform.parent = parent;
        //componentsPlayer.velocityClass = gameObject.transform.Find("Modelo").GetComponent<Velocity>();
        //componentsPlayer.EnableAll();
        componentsPlayer.scriptsPlayer.hudCtrl.ShowHUD();
        SaqueBandaCtrl saqueBandaCtrl = GameObject.FindGameObjectWithTag("SaqueDeBandaCtrl").GetComponent<SaqueBandaCtrl>();
        saqueBandaCtrl.enabled = true;
        Velocity velocity = gameObject.transform.Find("Modelo").GetComponent<Velocity>();
        //velocity.stringMy = componentsPlayer.transform.Find("Moviment").GetComponent<StringMy>();
    }
}
