using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiatePlayer : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public GameObject bodyPref;
    public GameObject playerScripts;
    public GameObject modelsPref;
    public Transform parent;
    public SetupMyOnlinePlayer setupMyOnlinePlayer;
    List<EventTrigger<EventData>> onlineTriggers = new List<EventTrigger<EventData>>();
    ComponentsPlayer componentsPlayer;
    void Start()
    {
        componentsPlayer = FindObjectOfType<ComponentsPlayer>();
    }
    public GameObject Instantiate()
    {
        Vector3 initPosition; 
        Quaternion initRotation;
        if(!InitialPosition.getInitPositionAndRotation(ComponentsPlayer.myMonoPlayerID.getStringID(),out initPosition,out initRotation))
        {
            return null;
        }
        GameObject emptyGameObject = new GameObject("Me Net Player-"+PhotonNetwork.NickName+"-"+PhotonNetwork.LocalPlayer.ActorNumber);
        emptyGameObject.transform.parent = parent;
        GameObject body = Instantiate(bodyPref, initPosition, initRotation, emptyGameObject.transform);
        GameObject newModel= instantiateModel(ChooseModel.choosedModel,body);
        GameObject newPlayerScripts = Instantiate(playerScripts, Vector3.zero, Quaternion.identity, emptyGameObject.transform);
        setupPlayerScripts(newPlayerScripts, body, newModel, PhotonNetwork.LocalPlayer, ComponentsPlayer.myMonoPlayerID.getStringID());
        setupModel(newModel,newPlayerScripts, PhotonNetwork.LocalPlayer);
        setupMyOnlinePlayer.setup(body, newModel);
        PhotonView pVBody = body.GetComponent<PhotonView>();
        PhotonView pVModelo = newModel.GetComponent<PhotonView>();
        PhotonView pVPlayerScripts = newPlayerScripts.GetComponent<PhotonView>();
        FueraDeJuego fueraDeJuego = GameObject.FindGameObjectWithTag("FueraDeJuego").GetComponent<FueraDeJuego>();
        fueraDeJuego.AddPlayer(PhotonNetwork.LocalPlayer.ActorNumber, body.transform);
        if (PhotonNetwork.AllocateViewID(pVBody) && PhotonNetwork.AllocateViewID(pVModelo)&& PhotonNetwork.AllocateViewID(pVPlayerScripts))
        {
            object[] data = new object[]
            {
            initPosition, initRotation, pVBody.ViewID,pVModelo.ViewID,pVPlayerScripts.ViewID,ChooseModel.choosedModel.ToString(),ComponentsPlayer.myMonoPlayerID.getStringID()
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
            DebugsList.testing.print("Instantiate my Player Instantiate");
            PublicPlayerDataList.addPublicFieldPlayerData(newPlayerScripts);
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
            DebugsList.testing.print("Instantiate other Player OnEvent");
            EventTrigger<EventData> trigger = new EventTrigger<EventData>();
            trigger.addTrigger(MatchEvents.footballFieldLoaded, false, 1, true);
            trigger.addFunction(waitLoadedMatch, photonEvent);
            trigger.endLoadTrigger();
            onlineTriggers.Add(trigger);
        }
    }
    void waitLoadedMatch(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;
        string modelNameStr = (string)data[5];
        ModelName.Name modelName = (ModelName.Name)System.Enum.Parse(typeof(ModelName.Name), modelNameStr);
        Player sender;
        MyOnlineFunctions.getPlayer(photonEvent.Sender, out sender);
        GameObject emptyGameObject = new GameObject("Net Player-" + sender.NickName + "-" + photonEvent.Sender);
        emptyGameObject.transform.parent = parent;
        GameObject body = Instantiate(bodyPref, (Vector3)data[0], (Quaternion)data[1], emptyGameObject.transform);
        GameObject model = instantiateModel(modelName, body);
        GameObject newPlayerScripts = Instantiate(playerScripts, Vector3.zero, Quaternion.identity, emptyGameObject.transform);
        PhotonView pVBody = body.GetComponent<PhotonView>();
        PhotonView pVModelo = model.GetComponent<PhotonView>();
        PhotonView pVPlayerScripts = newPlayerScripts.GetComponent<PhotonView>();
        pVBody.ViewID = (int)data[2];
        pVModelo.ViewID = (int)data[3];
        pVPlayerScripts.ViewID = (int)data[4];
        string playerID = (string)data[6];
        setupPlayerScripts(newPlayerScripts, body, model, pVBody.Owner, playerID);
        setupModel(model, newPlayerScripts, sender);
        PublicPlayerDataList.addPublicFieldPlayerData(newPlayerScripts);
        //DebugsList.testing.print("End Instantiate other Player OnEvent");
        /*
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
        fueraDeJuego.AddPlayer(pVBody.Owner.ActorNumber, body.transform);*/
    }
    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    void setupPlayerScripts(GameObject playerScripts,GameObject body,GameObject model,Player player,string playerIDStr)
    {
        PublicFieldPlayerData publicPlayerData = playerScripts.GetComponent<PublicFieldPlayerData>();
        CollisionEvent collisionEvent = body.GetComponent<CollisionEvent>();
        NetVariables netVariables = playerScripts.GetComponent<NetVariables>();
        publicPlayerData.collisionEvent = collisionEvent;
        Variable<float> maxSpeed,velocity,resistance,maximumJumpForce;
        if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            maxSpeed = componentsPlayer.scriptsPlayer.movimentValues.maxSpeed;
            velocity = componentsPlayer.scriptsPlayer.movimentValues.velocityObsolete;
            resistance = componentsPlayer.scriptsPlayer.resistanceController.resistanceVar;
            maximumJumpForce = componentsPlayer.scriptsPlayer.movimentValues.maximumJumpForceVar;
            PlayerIDMonoBehaviour playerID = ComponentsPlayer.myMonoPlayerID;
            publicPlayerData.playerIDMono = playerID;
            PlayerIDMonoBehaviour playerIDOfPlayerScripts = playerScripts.GetComponent<PlayerIDMonoBehaviour>();
            Destroy(playerIDOfPlayerScripts);
        }
        else
        {
            maxSpeed = new Variable<float>();
            velocity = new Variable<float>();
            resistance = new Variable<float>();
            maximumJumpForce = new Variable<float>();
            PlayerIDMonoBehaviour playerID = playerScripts.GetComponent<PlayerIDMonoBehaviour>();
            playerID.RemoteLoad(playerIDStr);
            
        }
        netVariables.maxSpeedNet.variable = maxSpeed;
        netVariables.velocityNet.variable = velocity;
        netVariables.playerName.variable = publicPlayerData.playerNameVar;
        netVariables.resistanceNet.variable = resistance;
        netVariables.maximumJumpForceNet.variable = maximumJumpForce;
        publicPlayerData.animator = model.GetComponent<Animator>();
        publicPlayerData.setupModel = model.GetComponent<SetupModel>();
        publicPlayerData.rigidbody = body.GetComponent<Rigidbody>();
        publicPlayerData.bodyTransform = body.transform;
        publicPlayerData.maxSpeedVar = maxSpeed;
        publicPlayerData.velocityVar = velocity;
        publicPlayerData.resistanceVar = resistance;
        publicPlayerData.maximumJumpForceVar = maximumJumpForce;
        publicPlayerData.maximumJumpForce = maximumJumpForce.Value;
        InitialPosition.setInitPositionAndRotationInPublicPlayerData(publicPlayerData);
        NickNameHUDCtrl nickName = playerScripts.GetComponent<NickNameHUDCtrl>();
        nickName.text.SetVariable(publicPlayerData.playerNameVar);
        publicPlayerData.playerNameVar.Value = player.NickName;
        publicPlayerData.nickNameHUDCtrl = nickName;

        FieldPlayerSetup.setupChaserDataList(publicPlayerData);
    }
    void setupModel(GameObject model,GameObject playerScripts, Player player)
    {
        model.transform.localPosition = Vector3.zero;
        model.transform.localEulerAngles = Vector3.zero;
        SetupModel setupModel = model.GetComponent<SetupModel>();
        Steps steps = model.GetComponent<Steps>();
        steps.publicPlayerData = playerScripts.GetComponent<PublicPlayerData>();
        if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            componentsPlayer.scriptsPlayer.transparencyCtrl.setupModel = setupModel;
        }
    }
    GameObject instantiateModel(ModelName.Name modelName,GameObject body)
    {
        List<ModelName> modelNames = MyFunctions.GetComponentsInChilds<ModelName>(modelsPref, true, false);
        GameObject modelPref = modelNames.Find(item => item.Value == modelName).gameObject;
        GameObject newModel = Instantiate(modelPref, body.transform.position, body.transform.rotation, body.transform);
        return newModel;
    }
}
