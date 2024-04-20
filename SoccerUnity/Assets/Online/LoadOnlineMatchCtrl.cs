using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadOnlineMatchCtrl : MonoBehaviourPunCallbacks
{
    public ManualChooseFieldPositionCtrl loadNormalMatchCtrl;
    public DataFieldPositionCtrl dataFieldPositionCtrl;
    public GameObject getTeamsInfoCtrlsGObj;
    public InstantiatePhotonGameObject masterClientRPCsInstantiator;
    public ChooseFieldPositionCtrl chooseFieldPositionCtrl;
    public InstantiatePlayer instantiatePlayer;
    public LoadingMenu loadingMenu;
    public InstantiateBall instantiateBall;
    EventTrigger setupTeamsTrigger,onlineMatchIsLoadedTrigger;
    List<PlayerIDMonoBehaviour> myPlayerIDs;
    bool setupedOnline;
    public void start()
    {
        setupTeamsTrigger = new EventTrigger();
        setupTeamsTrigger.addTrigger(MatchEvents.typeMatchSetuped,false,1, true);
        setupTeamsTrigger.addTrigger(OnlineEvents.joinedRoom,false, 1, true);
        //setupTeamsTrigger.addFunction(MatchComponents.setupTeams.SetupTeams);
        setupTeamsTrigger.endLoadTrigger();

        onlineMatchIsLoadedTrigger = new EventTrigger();
        onlineMatchIsLoadedTrigger.addTrigger(OnlineEvents.masterClientRPCsInstantiated, false, 1, true);
        onlineMatchIsLoadedTrigger.addTrigger(MatchEvents.setMainBall, false, 1, true);
        onlineMatchIsLoadedTrigger.addTrigger(MatchEvents.publicPlayerDataOfFieldPositionsAreAvailable, false, 1, true);
        onlineMatchIsLoadedTrigger.addTrigger(MatchEvents.footballFieldLoaded, false, 1, true);
        onlineMatchIsLoadedTrigger.addTrigger(MatchEvents.teamsSetuped, false, 1, true);
        onlineMatchIsLoadedTrigger.addFunction(onlineMatchIsLoaded);
        onlineMatchIsLoadedTrigger.endLoadTrigger();
        loadingMenu.ShowMenu();
        OnlineBallCtrl.getRoutineData = true;
        myPlayerIDs = new List<PlayerIDMonoBehaviour>();
        foreach (var item in FindObjectsOfType<PlayerIDMonoBehaviour>())
        {
            myPlayerIDs.Add(item);
        }
        MatchComponents.kickNotifier = new OnlineKickNotifier();
    }
    //Funcion de Photon
    public override void OnJoinedRoom()
    {
        foreach (var item in myPlayerIDs)
        {
            item.LocalLoad(PhotonNetwork.LocalPlayer.ActorNumber);
        }
        MatchData.myActor = PhotonNetwork.LocalPlayer.ActorNumber;
        MatchEvents.myPlayerIDLoaded.Invoke();
        instantiateBall.Instantiate();
        instantiateMasterClientRPCs();
        setupReferee();
        DebugsList.testing.print("Joined on Room");
        MatchEvents.typeMatchSetuped.Invoke();
        if (PhotonNetwork.LocalPlayer.NickName.Equals(""))
        {
            string randomNickName = "Player " + Random.Range(0, 100);
            PhotonNetwork.LocalPlayer.NickName = randomNickName;
        }
        setResponsibles();
        OnlineEvents.joinedRoom.Invoke();
    }
    void setResponsibles()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            MatchData.setResponsibleForTheBall(PhotonNetwork.LocalPlayer.ActorNumber);
            MatchData.setResponsibleForTheGoalkeepers(PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }
    void setupReferee()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            MatchData.setReferee(PhotonNetwork.LocalPlayer.ActorNumber.ToString(), PhotonNetwork.LocalPlayer.ActorNumber.ToString());
        }
        else
        {
            MatchData.setReferee("",PhotonNetwork.LocalPlayer.ActorNumber.ToString());
        }
    }
    public void onlineMatchIsLoaded()
    {
        if (!setupedOnline)
        {
            setupedOnline = true;
            DebugsList.RPCRequestFieldPosition.print("onlineMatchIsLoaded");
            Teams.print();
            loadingMenu.HideMenu();
            MatchEvents.matchLoaded.Invoke();
            
        }
    }
    public void instantiateMasterClientRPCs()
    {
        masterClientRPCsInstantiator.gameObjectIsInstantiatedEvent += masterClientRPCsInstantiated;
        masterClientRPCsInstantiator.errorEvent += instantiationError;
        masterClientRPCsInstantiator.setup(CodeEventsNet.MasterClientRPCsInstantiate,Vector3.zero,Vector3.zero);
        if (PhotonNetwork.IsMasterClient)
        {
            masterClientRPCsInstantiator.Instantiate();
        }
    }

    public void masterClientRPCsInstantiated(GameObject gObj)
    {
        /*
        RPCRequestFieldPosition RPCRequestFieldPosition = gObj.GetComponent<RPCRequestFieldPosition>();
        okChooseFieldPosition.selectedEvent.AddListener(RPCRequestFieldPosition.StartRequestFieldPosition);
        okChooseFieldPosition.selectedEvent.AddListener(okChooseFieldPosition.Disable);
        RPCRequestFieldPosition.acceptRequest += loadNormalMatchCtrl.acceptRequestFieldPosition;
        RPCRequestFieldPosition.denyRequest += loadNormalMatchCtrl.dennyRequestFieldPosition;*/
        OnlineEvents.masterClientRPCsInstantiated.Invoke();
    }

    public void instantiationError()
    {
        OnlineErrorHandler.OnlineError("Master client instantiation");
    }
}