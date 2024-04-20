using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

public class PlayerMessages : MonoBehaviourPunCallbacks,IOnEventCallback
{
    public static PlayerMessages myPlayerMessages;
    Animation animStartMatch;
    MatchDataObsolete matchData;
    TimerObsolete timer;
    public AudioSource pitidoEndFirstPart,endMatchPitido;
    Teams teams;
    private void Start()
    {

        matchData = GameObject.FindGameObjectWithTag("MatchData").GetComponent<MatchDataObsolete>();
        animStartMatch = GameObject.Find("StartMatchAnimation").GetComponent<Animation>();
        timer = GameObject.FindGameObjectWithTag("TimerMatch").GetComponent<TimerObsolete>();
        tag = "MyPlayerNet";
        myPlayerMessages = this;
        //photonView.RPC(nameof(RequestTeamsAvailables), RpcTarget.MasterClient);
        timer.EndEvent += EndPart;
        StartPartAnimation startMatchAnimation = GameObject.Find("StartMatchAnimation").GetComponent<StartPartAnimation>();
        startMatchAnimation.endAnimation += EndStartMatchAnimation;
        teams = GameObject.FindGameObjectWithTag("Teams").GetComponent<Teams>();
    }
    public void EndStartMatchAnimation()
    {
        
        timer.StartTimer();
    }
    [PunRPC]
    void RequestTeamsAvailables(PhotonMessageInfo info)
    {
        bool red, blue;
        Teams teams = GameObject.FindGameObjectWithTag("Teams").GetComponent<Teams>();
        Teams.TeamsAvailables(out red, out blue);
        photonView.RPC(nameof(SendTeamsAvailables), RpcTarget.All,red,blue);
    }
    [PunRPC]
    void SendTeamsAvailables(bool red, bool blue)
    {
        ChooseTeamMenu chooseTeam = GameObject.FindGameObjectWithTag("ChooseTeam").GetComponent<ChooseTeamMenu>();
        //chooseTeam.SetInteractableButtons(red,blue);
    }
    
    [PunRPC]
    void RequestJoin(int actor,string nameTeam)
    {
        teams = GameObject.FindGameObjectWithTag("Teams").GetComponent<Teams>();
        switch (nameTeam)
        {
            case "Red":
                if (teams.RedAvailable())
                {
                    byte evCode = CodeEventsNet.JoinPlayer; 
                    object[] content = new object[] { actor,"Red"};
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All,CachingOption=EventCaching.AddToRoomCache }; // You would have to set the Receivers to All in order to receive this event on the local client as well
                    SendOptions sendOptions = new SendOptions { Reliability = true };
                    PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
                    
                }
                break;
            case "Blue":
                if (teams.BlueAvailable())
                {
                    byte evCode = CodeEventsNet.JoinPlayer;
                    object[] content = new object[] { actor, "Blue" };
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCache }; // You would have to set the Receivers to All in order to receive this event on the local client as well
                    SendOptions sendOptions = new SendOptions { Reliability = true };
                    PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
                }
                break;
            default:
                photonView.RPC("RefusedJoin", matchData.players[actor].player);
                break;
        }
       

    }
    
    [PunRPC]
    void RefusedJoin( PhotonMessageInfo info)
    {
        SetStateButtons();
    }
    void IOnEventCallback.OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        
        if (eventCode == CodeEventsNet.JoinPlayer)
        {
            teams = GameObject.FindGameObjectWithTag("Teams").GetComponent<Teams>();
            object[] data = (object[])photonEvent.CustomData;
            int actor = (int)data[0];
            string teamName = (string)data[1];
            switch (teamName)
            {
                case "Red":
                        //teams.AddPlayerToRed(actor);
                    break;
                case "Blue":
                        //teams.AddPlayerToBlue(actor);
                    break;
            }
            if (PhotonNetwork.LocalPlayer.ActorNumber == actor)
            {
                SpawnBody();
                //teams.SetPosStartMatch(actor, teamName);
                SetStateButtons();
            }
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
    void SetStateButtons()
    {
        teams = GameObject.FindGameObjectWithTag("Teams").GetComponent<Teams>();
        bool red, blue;
        Teams.TeamsAvailables(out red, out blue);
        ChooseTeamMenu chooseTeam = GameObject.FindGameObjectWithTag("ChooseTeam").GetComponent<ChooseTeamMenu>();
        //chooseTeam.SetInteractableButtons(red, blue);
    }
    public void CheckStartMatch()
    {
        if (Teams.teamsAreFull())
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.FetchServerTimestamp();
                photonView.RPC(nameof(SendStartMatch), RpcTarget.All, PhotonNetwork.Time);
            }
        }
    }
    public void SpawnBody()
    {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameObject gameObject = GameObject.FindWithTag("Menu");
        gameObject.GetComponent<MenuNet>().enabled = true;
        photonView.RPC(nameof(RequestSpawn), RpcTarget.MasterClient,PhotonNetwork.LocalPlayer.ActorNumber);

    }
    [PunRPC]
    void RequestSpawn(int actor,PhotonMessageInfo info)
    {
        Teams teams = GameObject.FindGameObjectWithTag("Teams").GetComponent<Teams>();
        int spawn = teams.SetRandomSpawn(actor);
        photonView.RPC(nameof(SendSpawn), RpcTarget.All,spawn ,actor);
    }
    [PunRPC]
    void SendSpawn(int spawn,int actor)
    {
        
        teams = GameObject.FindGameObjectWithTag("Teams").GetComponent<Teams>();
        teams.SetSpawn(actor, spawn);
        InstantiatePlayer spawnBody = GameObject.FindGameObjectWithTag("MatchSequence").GetComponent<InstantiatePlayer>();
        if (PhotonNetwork.LocalPlayer.ActorNumber == actor)
        {
            //spawnBody.Spawn(teams.getTeamFromActor(actor).name);
        }
    }
    [PunRPC]
    void SendStartMatch(double timeStamp,PhotonMessageInfo info)
    {
        GameObject[] goalsTrigger = GameObject.FindGameObjectsWithTag("Goal");
        foreach(GameObject goal in goalsTrigger)
        {
            goal.GetComponent<GoalKickRandom>().enabled = false;
        }
        matchData.matchStarted = true;

        double time = PhotonNetwork.Time - timeStamp;
        Invoke(nameof(StartPart),Mathf.Clamp(matchData.startMatch - (float)time,0,Mathf.Infinity));
    }
    [PunRPC]
    public void SendSetInitialPos(string teamStartMatch,double timeStamp)
    {
        GameObject[] goalsTrigger = GameObject.FindGameObjectsWithTag("Goal");
        foreach (GameObject goal in goalsTrigger)
        {
            goal.GetComponent<GoalKickRandom>().enabled = false;
        }
        matchData.matchStarted = true;
        double time = PhotonNetwork.Time - timeStamp;
        StartCoroutine(SetInitialPos(teamStartMatch, Mathf.Clamp(matchData.startMatch - (float)time, 0, Mathf.Infinity)));
    }
    public void SetInitialPosBall()
    {
        GameObject behaviour = GameObject.FindGameObjectWithTag("ComponentsPlayer");
        Transform spawnBall = GameObject.FindGameObjectWithTag("SpawnBall").transform;
        ComponentsPlayer componentsPlayer = behaviour.GetComponent<ComponentsPlayer>();
        componentsPlayer.componentsBall.transBall.position = spawnBall.position;
        componentsPlayer.componentsBall.transBall.rotation = spawnBall.rotation;
        componentsPlayer.componentsBall.rigBall.velocity = Vector3.zero;
        componentsPlayer.componentsBall.rigBall.angularVelocity = Vector3.zero;
    }
    public IEnumerator SetInitialPos(string teamStart,float time)
    {
        yield return new WaitForSeconds(time);
        GameObject behaviour = GameObject.FindGameObjectWithTag("ComponentsPlayer");
        Transform spawnBall = GameObject.FindGameObjectWithTag("SpawnBall").transform;
        ComponentsPlayer componentsPlayer = behaviour.GetComponent<ComponentsPlayer>();
        componentsPlayer.EnableOnlyCamera();
        if (teamStart== "Blue")
        {
            
            Team team = teams.getTeamFromActor(PhotonNetwork.LocalPlayer.ActorNumber);
            Transform spawn;
            switch (team.TeamName)
            {
                case "Red":
                    spawn = team.spawnsTransform[team.spawns[PhotonNetwork.LocalPlayer.ActorNumber]];
                    componentsPlayer.transBody.position = spawn.position;
                    componentsPlayer.transBody.rotation = spawn.rotation;
                    componentsPlayer.transModelo.rotation = spawn.rotation;
                    break;
                case "Blue":
                    if (PhotonNetwork.LocalPlayer.ActorNumber == team.actorPosStartMatch)
                    {
                        spawn = team.posStartBall;
                    }
                    else
                    {
                        spawn = team.spawnsTransform[team.spawns[PhotonNetwork.LocalPlayer.ActorNumber]];
                    }
                    componentsPlayer.transBody.position = spawn.position;
                    componentsPlayer.transBody.rotation = spawn.rotation;
                    componentsPlayer.transModelo.rotation = spawn.rotation;
                    break;
            }
        }
        else if (teamStart == "Red")
        {
            
            Team team = teams.getTeamFromActor(PhotonNetwork.LocalPlayer.ActorNumber);
            Transform spawn;
            switch (team.TeamName)
            {
                case "Red":
                    if (PhotonNetwork.LocalPlayer.ActorNumber == team.actorPosStartMatch)
                    {
                        spawn = team.posStartBall;
                    }
                    else
                    {
                        spawn = team.spawnsTransform[team.spawns[PhotonNetwork.LocalPlayer.ActorNumber]];
                    }
                    componentsPlayer.transBody.position = spawn.position;
                    componentsPlayer.transBody.rotation = spawn.rotation;
                    componentsPlayer.transModelo.rotation = spawn.rotation;
                    break;
                case "Blue":
                    spawn = team.spawnsTransform[team.spawns[PhotonNetwork.LocalPlayer.ActorNumber]];
                    componentsPlayer.transBody.position = spawn.position;
                    componentsPlayer.transBody.rotation = spawn.rotation;
                    componentsPlayer.transModelo.rotation = spawn.rotation;
                    break;
            }
        }
        SetInitialPosBall();
        StartCoroutine(PlayStartAnimationMatch(teamStart));
    }
    public IEnumerator PlayStartAnimationMatch(string team)
    {
        yield return new WaitForSeconds(1);
        animStartMatch.Play();
        StartPartAnimation startMatchAnimation = GameObject.Find("StartMatchAnimation").GetComponent<StartPartAnimation>();
        GameObject myPlayerNet = GameObject.FindGameObjectWithTag("MyPlayerNet");
        myPlayerNet.GetComponent<GoalMessages>().activateGoal = true;
        matchData.enableGoals = true;
    }
    
    void StartPart()
    {
            matchData.part++;
            if (matchData.part == 1)
            {
                timer.Init();
                StartCoroutine(SetInitialPos("Blue", 0));
            }
            else if (matchData.part == 2)
            {
                timer.Init();
                StartCoroutine(SetInitialPos("Red", 0));
            }
    }
    public void EndPart()
    {
        if (matchData.part == 1)
        {
            matchData.enableGoals = false;
            pitidoEndFirstPart.Play();
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.FetchServerTimestamp();
                
                photonView.RPC("SendStartMatch", RpcTarget.All, PhotonNetwork.Time);
            }
        }
        else if(matchData.part ==2)
        {
            endMatchPitido.Play();
            matchData.endMatch = true;
            matchData.enableGoals = false;
            EndMatchObsolete endMatch = GameObject.FindGameObjectWithTag("MatchSequence").GetComponent<EndMatchObsolete>();
            endMatch.End();
        }
    }
    [PunRPC]
    public void EnableKicks()
    {
        ComponentsPlayer componentsPlayer = GameObject.FindGameObjectWithTag("ComponentsPlayer").GetComponent<ComponentsPlayer>();
        componentsPlayer.kickGObjt.SetActive(true);
    }
    [PunRPC]
    public void ActorWithPosession(int actor)
    {
        matchData.actorWithPosession = actor;
    }
    [PunRPC]
    public void StateSaqueCornerBanda(bool corner,bool banda)
    {
        matchData.saqueCorner = corner;
        matchData.saqueDeBanda = banda;
        ComponentsPlayer componentsPlayer = GameObject.FindGameObjectWithTag("ComponentsPlayer").GetComponent<ComponentsPlayer>();
        componentsPlayer.kickGObjt.SetActive(true);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if(PhotonNetwork.InRoom)
        if (matchData.matchStarted || matchData.endMatch)
        {
            //PhotonNetwork.LeaveRoom();
        }
    }
    public override void OnLeftRoom()
    {
        if(!Application.isEditor)
            SceneManager.LoadScene("Start");
    }

    
}
