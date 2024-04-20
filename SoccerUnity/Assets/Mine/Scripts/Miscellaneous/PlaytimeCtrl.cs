using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaytimeCtrl : MonoBehaviour
{
    ComponentsPlayer componentsPlayer;
    //ComponentsBall componentsBall;
    FloatTransition transition = new FloatTransition();
    MyCoroutine cameraLookingBall = new MyCoroutine();
    Vector3 initCameraPosition;
    Quaternion initCameraRotation;
    float durationLerp = 0.5f;
    public AnimationCurve lerpCameraMoviment;
    public PossessionCtrl losePossessionEvent;
    public CheckBallIsInFullArea fullFieldCheck;
    EventTrigger matchLoadedTrigger = new EventTrigger();
    EventTrigger introTrigger = new EventTrigger();
    EventTrigger teamsAreLoadedTrigger = new EventTrigger();
    public FindRandomMatch findRandomMatch;
    public static bool enableFindRandomMatch;
    public bool playIntro=true;
    public void StartProcess()
    {
        componentsPlayer = FindObjectOfType<ComponentsPlayer>();
        //componentsBall = FindObjectOfType<ComponentsBall>();
        //setupBall();
        setupLocalPlayersID();
        setupComponentsPlayer();
        setupKicks();
        setupGoalkeeperTrigger();
        startingScreen();
        setupIntro();
        setupCamera();
        setupPublicPlayerData();
        //Intro();
        setupTypeMatch();
        setupMatchLoadedEvent();
        setupFieldPositionMenu();
        CheckBallIsInFullArea.SetState(false);
        MatchEvents.footballFieldLoaded.AddListenerConsiderInvoked(()=>CheckBallIsInFullArea.SetState(true));
        //CheckBallIsInFullArea.PointExitEvent.AddListenerConsiderInvoked(() => Invoke(nameof(Intro), 1));
        MatchData.myActor = 0;
        MatchData.setReferee("0","0");
        setResponsibles();
        
        if (enableFindRandomMatch)
        {
            OnlineEvents.connected.AddListenerConsiderInvoked(findRandomMatch.find);
        }
        MatchComponents.kickNotifier = new OfflineKickNotifier();
    }
    void startingScreen()
    {
        StartingScreen startingScreen = GetComponentInChildren<StartingScreen>();
        startingScreen.StartProcess();
    }
    void setResponsibles()
    {
            MatchData.setResponsibleForTheBall(MatchData.myActor);
            MatchData.setResponsibleForTheBall(MatchData.myActor);
    }
    void setupFieldPositionMenu()
    {
        FieldPositionsCtrl.UIFieldPositions.GetComponent<Canvas>().enabled = false;
    }
    void setTeamSide()
    {
        Teams.teamsList[0].setLineup(Lineup.TypeLineup.Default);
        Teams.teamsList[1].setLineup(Lineup.TypeLineup.Default);
        SideOfFieldCtrl.setTeamSide(Teams.teamsList[0].TeamName, SideOfFieldID.One);
        //Teams.teamsList[0].setSideOfField(SideOfFieldID.One);
        SideOfFieldCtrl.setTeamSide(Teams.teamsList[1].TeamName, SideOfFieldID.Two);
        //Teams.teamsList[1].setSideOfField(SideOfFieldID.Two);
    }
    void setupGoalkeeperTrigger()
    {
        teamsAreLoadedTrigger.addTrigger(Teams.teamsAreLoadedEvent, false, 1, true);
        teamsAreLoadedTrigger.addTrigger(MatchEvents.typeMatchSetuped, false, 1, true);
        teamsAreLoadedTrigger.addFunction(setTeamSide);
        teamsAreLoadedTrigger.addFunction(setupGoalkeepersTeam);
        //teamsAreLoadedTrigger.addFunction(setupBotsTeam);
        //teamsAreLoadedTrigger.addFunction(setupPuppetTeam);
        teamsAreLoadedTrigger.endLoadTrigger();
    }
    void setupBotsTeam()
    {
        List<GameObject> bots = PlayerType.FindGameObjects(PlayerTypeID.Bot);
        foreach (var bot in bots)
        {
            PlayerComponents botComponents = bot.GetComponent<PlayerComponents>();
            botComponents.addPlayerToTeam.AddToTeam(Teams.teamsList[1],TypeFieldPosition.Type.None);
        }
    }
    void setupPuppetTeam()
    {
        GameObject puppet = PlayerType.FindGameObject(PlayerTypeID.Puppet);
        if (puppet != null)
        {
            PlayerComponents goalkeeperComponents = puppet.GetComponent<PlayerComponents>();
            goalkeeperComponents.addPlayerToTeam.AddToTeam(Teams.teamsList[1], TypeFieldPosition.Type.None);
        }
    }
    void setupGoalkeepersTeam()
    {
        GameObject[] goalkeeperGObjs = GameObject.FindGameObjectsWithTag(Tags.GoalKeeper);
        int i = 0;
        foreach (var goalkeeperGObj in goalkeeperGObjs)
        {

            Team team = Teams.teamsList[i];
            SetupGoalkeeper setupGoalkeeper = goalkeeperGObj.GetComponent<SetupGoalkeeper>();
            setupGoalkeeper.StartSetup(SetupGoalkeeper.TypeSetupGoalkeeper.Init);
            GoalkeeperComponents goalkeeperComponents = goalkeeperGObj.GetComponent<GoalkeeperComponents>();
            GoalkeeperCtrl goalkeeper = goalkeeperComponents.goalkeeperCtrl;
            PublicPlayerData goalkeeperPublicPlayerData = goalkeeperComponents.publicPlayerData;
            //goalkeeperComponents.addPlayerToTeam.AddToTeam(team,TypeFieldPosition.Type.GoalKeeper);
            i++;
        }
    }
    void setupIntro()
    {
        //MatchEvents.stopMatch.AddListener(()=>Invoke(nameof(Intro),1));
        introTrigger.addTrigger(MatchEvents.setMainBall, false, 1,true);
        introTrigger.addFunction(Intro);
        introTrigger.endLoadTrigger();
    }
    void Intro()
    {
        Transform posBall = transform.Find("Intro/PosBall");
        Transform posPlayer = transform.Find("Intro/PosPlayer");
        if (playIntro)
        {
            MatchComponents.ballComponents.transBall.position = posBall.position;
        }
        if (componentsPlayer == null)
        {
            return;
        }
        componentsPlayer.scriptsPlayer.velocity.previousPos = posPlayer.position;
        componentsPlayer.transBody.position = posPlayer.position;

        componentsPlayer.transModelo.localEulerAngles = Vector3.zero;
        componentsPlayer.transBody.rotation = posPlayer.rotation;

        MatchComponents.ballComponents.rigBall.velocity = Vector3.zero;
        MatchComponents.ballComponents.rigBall.angularVelocity = Vector3.zero;
        Transform posCamera = MyFunctions.FindChildContainsName(gameObject, "PosCamera", true).transform;
        componentsPlayer.transCamera.position = posCamera.position;
        componentsPlayer.transCamera.rotation = posCamera.rotation;
        initCameraPosition = posCamera.position;
        initCameraRotation = posCamera.rotation;
        //losePossessionEvent.Invoke();
        componentsPlayer.DisableAll();
        MatchEvents.endStartingScreen.AddListenerConsiderInvoked(startPlayerIntro);
    }
    void startPlayerIntro()
    {
        StopAllCoroutines();
        StartCoroutine(cameraLookingBall.Coroutine(0));
        StartCoroutine(transition.Coroutine(durationLerp, 0));
        transition.endTransition += enableMenu;
        MatchEvents.continueMatch.Invoke();
    }
    void enableMenu()
    {
        MatchEvents.enableMenu.Invoke();
    }
    bool distanceFromCameraToTargetPositionLookingBall()
    {
        return Vector3.Distance(componentsPlayer.scriptsPlayer.cameraPosition.targetPositionLookingBall, componentsPlayer.transCamera.position) < 0.5f;
    }

    void setCameraRotationPosition(float deltaTime)
    {
        componentsPlayer.scriptsPlayer.cameraPosition.rotationPivot1();
        componentsPlayer.scriptsPlayer.cameraPosition.setPositionPivot1();
        componentsPlayer.transCamera.position = Vector3.Lerp(initCameraPosition, componentsPlayer.scriptsPlayer.cameraPosition.getTargetLookAtBallPosition(), lerpCameraMoviment.Evaluate(deltaTime / durationLerp));
        componentsPlayer.transCamera.rotation = Quaternion.Lerp(initCameraRotation, componentsPlayer.scriptsPlayer.cameraRotation.getTargetRotationLookAtBall(), lerpCameraMoviment.Evaluate(deltaTime / durationLerp));

        /*componentsPlayer.scriptsPlayer.cameraRotation.currentSpeedCamera = Mathf.Lerp(1, componentsPlayer.scriptsPlayer.cameraRotation.speedCameraLookAtBall, lerpCameraMoviment.Evaluate(deltaTime / durationLerp));
        componentsPlayer.scriptsPlayer.cameraPosition.currentSpeedCamera = Mathf.Lerp(1, componentsPlayer.scriptsPlayer.cameraPosition.speedCameraLookingBall, lerpCameraMoviment.Evaluate(deltaTime / durationLerp));*/
    }
    void cameraIsInPosition()
    {
        componentsPlayer.EnableAll();
        componentsPlayer.scriptsPlayer.hudCtrl.ShowHUD();
        componentsPlayer.scriptsPlayer.hudCtrl.HideGunSight();
    }
    void setupCamera()
    {
        cameraLookingBall.end += cameraIsInPosition;
        cameraLookingBall.addCondition(distanceFromCameraToTargetPositionLookingBall);

        transition.addFunction(setCameraRotationPosition);
    }
    void setupPublicPlayerData()
    {
        return;
        PublicPlayerData[] goalkeeperGObjs = GetComponentsInChildren<PublicPlayerData>();
        foreach (var item in goalkeeperGObjs)
        {
            //PublicPlayerDataList.addPublicPlayerData(item);
        }
        ComponentsPlayer.publicPlayerData = GameObject.FindGameObjectWithTag(Tags.MyPlayer).GetComponent<PublicFieldPlayerData>();
        ComponentsPlayer componentsPlayer = FindObjectOfType<ComponentsPlayer>();
        if (ComponentsPlayer.publicPlayerData != null)
        {
            ComponentsPlayer.publicPlayerData.maxSpeedVar = componentsPlayer.scriptsPlayer.movimentValues.maxSpeed;
            ComponentsPlayer.publicPlayerData.velocityVar = componentsPlayer.scriptsPlayer.movimentValues.velocityObsolete;
            ComponentsPlayer.publicPlayerData.resistanceVar = componentsPlayer.scriptsPlayer.resistanceController.resistanceVar;
            ComponentsPlayer.publicPlayerData.maximumJumpForce = componentsPlayer.scriptsPlayer.movimentValues.MaximumJumpForce;
        }
    }
    void setupLocalPlayersID()
    {
        PlayerIDMonoBehaviour[] playerIDMonoBehaviours = FindObjectsOfType<PlayerIDMonoBehaviour>();
        int count = 0;
        foreach (var item in playerIDMonoBehaviours)
        {
            item.LocalLoad(0);
            count++;
        }

    }
    void setupComponentsPlayer()
    {
        if (componentsPlayer != null)
        {
            componentsPlayer.EnableAll();
        }
    }
    void setupKicks()
    {
        foreach (Kick script in FindObjectsOfType<Kick>())
        {
            if(script.GetType() == typeof(TouchWithDirect))
            {
                script.setAddForceOffline();
                script.setBallControlOffline();
            }
            else
            {
                script.setAddForceAtPositionOffline();
            }
        }
    }
    void setupBall()
    {
        BallComponents componentsBall = GetComponentInChildren<BallComponents>();
        if (componentsBall != null)
        {
            MatchComponents.ballComponents = componentsBall;
            MatchEvents.setMainBall.Invoke(MatchComponents.ballComponents.gameObject);
        }
    }
    public static void setupTypeMatch()
    {
        TypeMatch.SizeFootballField = SizeFootballFieldID.ElevenVSEleven;
        TypeMatch.typeMatch = TypeMatchID.Playtime;
        TypeMatch.typeNormalMatch = TypeNormalMatch.TenVSTen;
        TypeMatch.maxPlayers = 20;
        MatchEvents.typeMatchSetuped.Invoke();
    }
    void setupMatchLoadedEvent()
    {
        matchLoadedTrigger.addFunction(() => MatchEvents.matchLoaded.Invoke());
        MatchEvents.matchLoaded.AddListener(()=>MatchEvents.warmUp.Invoke());
        matchLoadedTrigger.addTrigger(MatchEvents.setMainBall, true, 1, true);
        matchLoadedTrigger.addTrigger(MatchEvents.typeMatchSetuped, true, 1, true);
        matchLoadedTrigger.addTrigger(MatchEvents.footballFieldLoaded, true, 1, true);
        matchLoadedTrigger.endLoadTrigger();
    }
    void fullFieldArea()
    {
        
    }
}
