using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class InstantiateOnlineGoalkeepers : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public GameObject goalkeeperPref;
    List<EventTrigger> triggers = new List<EventTrigger>();
    List<EventTrigger<EventData>> onlineTriggers = new List<EventTrigger<EventData>>();
    void Start()
    {
        OnlineEvents.joinedRoom.AddListenerConsiderInvoked(joined);
        HideModelGoalkeepers();
    }

    void HideModelGoalkeepers()
    {
        GameObject[] goalkeeperGObjs = GameObject.FindGameObjectsWithTag(Tags.GoalKeeper);
        foreach (var goalkeeperGObj in goalkeeperGObjs)
        {
            PublicPlayerData publicPlayerData = MyFunctions.GetComponentInChilds<PublicPlayerData>(goalkeeperGObj, true);
            //publicPlayerData.bodyTransform.gameObject.SetActive(false);
            Destroy(publicPlayerData.bodyTransform.gameObject);
        }
    }
    void joined()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (var team in Teams.teamsList)
            {
                addTrigger(team);
            }
        }
    }
    void addTrigger(Team team)
    {
        EventTrigger<Team> trigger = new EventTrigger<Team>();
        trigger.addTrigger(team.lineupChanged, false, 1, true);
        trigger.addTrigger(team.sideOfFieldChanged, false, 1, true);
        trigger.addTrigger(MatchEvents.footballFieldLoaded, false, 1, true);
        trigger.addFunction(Instantiate, team);
        trigger.endLoadTrigger();
        triggers.Add(trigger);
    }
    private void Instantiate(Team team)
    {
        
        SideOfField sideOfField;
        if(!SideOfFieldCtrl.getSideOfFieldOfTeam(team.TeamName,out sideOfField))
        {
            return;
        }
        GoalComponents goalComponents = sideOfField.goalComponents;
        if (!goalComponents.goalkeeper.activeInHierarchy)
        {
            return;
        }
        //Vector3 position= goalComponents.centerMatchStoppedState.position;
        //Quaternion rotation = goalComponents.centerMatchStoppedState.rotation;
        GameObject goalkeeper = goalComponents.goalkeeper;
        //Vector3 position = goalkeeper.transform.TransformPoint(goalkeeperPref.transform.position);
        Vector3 position = goalkeeper.transform.TransformPoint(goalkeeperPref.transform.position);
        Quaternion rotation = goalkeeper.transform.rotation;
        GameObject instantiatedGoalkeeper = Instantiate(goalkeeperPref, position, rotation, goalComponents.goalkeeper.transform);
        setupGoalkeeper(goalComponents.goalkeeper, instantiatedGoalkeeper);
        addToTeam(team.TeamName, goalComponents.goalkeeper);
        PhotonView photonView = instantiatedGoalkeeper.GetComponent<PhotonView>();

        PlayerIDMonoBehaviour playerIDMonoBehaviour = goalComponents.goalkeeper.GetComponent<PlayerIDMonoBehaviour>();
            if (PhotonNetwork.AllocateViewID(photonView))
            {
                object[] data = new object[]
                {
                  position, rotation, photonView.ViewID,playerIDMonoBehaviour.getStringID(),team.TeamName
                };

                RaiseEventOptions raiseEventOptions = new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.Others,
                    CachingOption = EventCaching.AddToRoomCacheGlobal
                };

                SendOptions sendOptions = new SendOptions
                {
                    Reliability = true
                };
                DebugsList.testing.print("InstantiatePlayer Instantiate A");

                PhotonNetwork.RaiseEvent(CodeEventsNet.Goalkeeper, data, raiseEventOptions, sendOptions);

                return;
            }
            else
            {
                Debug.LogError("Failed to allocate a ViewId.");

                Destroy(instantiatedGoalkeeper);
                return;
            }
    }
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == CodeEventsNet.Goalkeeper)
        {
            DebugsList.testing.print("InstantiatePlayer Goalkeeper");
            object[] data = (object[])photonEvent.CustomData;
            string teamName = (string)data[4];
            Team team = Teams.getTeamByName(teamName);
            EventTrigger<EventData> trigger = new EventTrigger<EventData>();
            trigger.addTrigger(team.lineupChanged, false, 1, true);
            trigger.addTrigger(team.sideOfFieldChanged, false, 1, true);
            trigger.addTrigger(MatchEvents.footballFieldLoaded, false, 1, true);
            trigger.addFunction(onlineSetup, photonEvent);
            trigger.endLoadTrigger();
            onlineTriggers.Add(trigger);
        }
    }
    void onlineSetup(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;
        Vector3 position = (Vector3)data[0];
        Quaternion rotation = (Quaternion)data[1];
        string teamName = (string)data[4];
        SideOfField sideOfField;
        if (!SideOfFieldCtrl.getSideOfFieldOfTeam(teamName, out sideOfField))
        {
            return;
        }
        GameObject goalkeeper = sideOfField.goalComponents.goalkeeper;
        GameObject body = Instantiate(goalkeeperPref, position, rotation, goalkeeper.transform);
        PhotonView photonView = body.GetComponent<PhotonView>();
        photonView.ViewID = (int)data[2];
        PlayerIDMonoBehaviour playerIDMonoBehaviour = goalkeeper.GetComponent<PlayerIDMonoBehaviour>();
        string playerID = (string)data[3];
        playerIDMonoBehaviour.RemoteLoad(playerID);
        setupGoalkeeper(sideOfField.goalComponents.goalkeeper, body);
        addToTeam(teamName, goalkeeper);
    }
    void addToTeam(string teamName,GameObject goalkeeper)
    {
        GoalkeeperComponents goalkeeperComponents = goalkeeper.GetComponent<GoalkeeperComponents>();
        goalkeeperComponents.addPlayerToTeam.AddToTeam(teamName, TypeFieldPosition.Type.GoalKeeper);
    }
    void setupGoalkeeper(GameObject goalkeeper,GameObject goalkeeperBody)
    {
        GoalkeeperCtrl goalkeeperCtrl = goalkeeper.GetComponent<GoalkeeperCtrl>();
        //goalkeeperBody.transform.parent = goalkeeper.transform;
        goalkeeperCtrl.goalKeeperTransform = goalkeeperBody.transform;
        Transform wirst1 = MyFunctions.FindChildContainsName(goalkeeperBody, "Wirst1", true).transform;
        goalkeeperCtrl.wirst1 = wirst1;
        Transform armature = MyFunctions.FindChildContainsName(goalkeeperBody, "Armature", true).transform;
        
        setupPublicPlayerData(goalkeeper, goalkeeperBody);
        
        SetupGoalkeeper setupGoalkeeper = goalkeeper.GetComponent<SetupGoalkeeper>();
        
        setupGoalkeeper.StartSetup(SetupGoalkeeper.TypeSetupGoalkeeper.TeamSetup);
        if (!PhotonNetwork.IsMasterClient)
        {
            goalkeeperCtrl.Lock();
        }
    }

    void setupPublicPlayerData(GameObject goalkeeper, GameObject goalkeeperBody)
    {
        PublicPlayerData publicPlayerData = goalkeeper.GetComponent<PublicPlayerData>();
        publicPlayerData.bodyTransform = goalkeeperBody.transform;
        publicPlayerData.setupModel = goalkeeperBody.GetComponent<SetupModel>();
    }
}
