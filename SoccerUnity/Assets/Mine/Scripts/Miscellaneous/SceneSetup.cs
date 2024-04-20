using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SettingsType
{
    Pruebas,
    Normal
}
public class SceneSetup : MonoBehaviour
{
    public GameObject sceneOnline, sceneOffline;
    public bool automaticConnect;
    public bool automaticChooseFieldPosition;
    public TypeMatchID typeMatch;
    public SceneModeID sceneMode;
    public SettingsType _rulesSettingsType;
    public TypeNormalMatch typeMatchName;
    public bool publicMatch;
    public static bool staticSetup;
    public static TypeMatchID staticTypeMatch;
    public static SceneModeID staticSceneMode;
    public static bool staticAutomaticConnect;
    public static bool staticAutomaticChooseFieldPosition;
    public static SettingsType rulesSettingsType;
    public bool automaticFindRandomMatch;
    public bool _showStartingScreen;
    public static bool showStartingScreen;
    public GameSounds gameSounds;

    void Awake()
    {
        AutomaticConnect.typeMatchName = typeMatchName;
        AutomaticConnect.publicMatch = publicMatch;
        FindRandomMatch.typeNormalMatch = typeMatchName;
        showStartingScreen = _showStartingScreen;
        PlayerIDMonoBehaviour.localActorCount = 0;
        MatchComponents.gameSounds = gameSounds;
        setupSettingsTypes();
        checkStaticSetup();
        PlaytimeCtrl.enableFindRandomMatch = automaticFindRandomMatch;
        SetParent[] setParents = transform.GetComponentsInChildren<SetParent>();
        foreach (var item in setParents)
        {
            item.Set();
        }
        DebugsList debugsList = FindObjectOfType<DebugsList>();
        if(debugsList!=null)
            debugsList.Setup();
        MyFunctions.SetStateGameObjectWithValue(typeMatch);
        MyFunctions.SetStateGameObjectWithValue(sceneMode);
        GameObject playerGObj = GameObject.FindGameObjectWithTag(Tags.MyPlayer);
        if (playerGObj != null)
        {
            PlayerIDMonoBehaviour myPlayerID = playerGObj.GetComponent<PlayerIDMonoBehaviour>();
            ComponentsPlayer.myMonoPlayerID = myPlayerID;
        }
        ComponentsPlayer.currentComponentsPlayer = FindObjectOfType<ComponentsPlayer>();
        //myPlayerID.awake();
        setupBots();
        executeLoads();
        generalCameraSetup();
        //findGetTheFirstChaserToReachTheTarget();
        switch (typeMatch)
        {
            case TypeMatchID.Playtime:
                playTimeSetup();
                break;
            case TypeMatchID.NormalMatch:
                normalMatchSetup();
                break;
        }
        handMoviment();
        //setupGoals();
        lookBallSetup();
        setupScripts();
        SetupGeneral setupGeneral = FindObjectOfType<SetupGeneral>();
        setupGeneral.StartProcess();
        CursorCtrl.clear();
    }
    void setupSettingsTypes()
    {
        rulesSettingsType = _rulesSettingsType;
    }
    void checkStaticSetup()
    {
        if (staticSetup)
        {
            sceneMode = staticSceneMode;
            typeMatch = staticTypeMatch;
            automaticChooseFieldPosition = staticAutomaticChooseFieldPosition;
            automaticConnect = staticAutomaticConnect;
        }
        else
        {
            staticTypeMatch = typeMatch;
        }
    }
    public static void setStaticSetup(TypeMatchID _staticSceneType, SceneModeID _staticSceneMode, bool _staticAutomaticConnect, bool _staticAutomaticChooseFieldPosition)
    {
        staticTypeMatch = _staticSceneType;
        staticSceneMode = _staticSceneMode;
        staticAutomaticConnect = _staticAutomaticConnect;
        staticAutomaticChooseFieldPosition = _staticAutomaticChooseFieldPosition;
        staticSetup = true;
        TypeMatch.sceneMode = _staticSceneMode;
    }
    void findGetTheFirstChaserToReachTheTarget()
    {
        GetChaserToReachTheTargetOrder getChaserToReachTheTargetOrder = FindObjectOfType<GetChaserToReachTheTargetOrder>();
        if (getChaserToReachTheTargetOrder != null)
        {
            //MatchComponents.chaserList = getChaserToReachTheTargetOrder.ChaserList;
        }
    }
    void setupBots()
    {
        return;
        GameObject[] bots = GameObject.FindGameObjectsWithTag(Tags.Bot);
        foreach (var bot in bots)
        {
            if (bot.activeInHierarchy)
            {
                PublicFieldPlayerData publicPlayerData = bot.GetComponent<PublicFieldPlayerData>();
                publicPlayerData.maxSpeedVar = new Variable<float>(10.5f);
                publicPlayerData.resistanceVar = new Variable<float>(3);
                publicPlayerData.maximumJumpForceVar = new Variable<float>();
                publicPlayerData.maximumJumpForce = 4;
                publicPlayerData.rigidbody = bot.GetComponentInChildren<Rigidbody>();
            }
        }
    }
    void executeLoads()
    {
        GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        List<ILoad> loads = new List<ILoad>();
        foreach (var item in rootGameObjects)
        {
            List<ILoad> list = MyFunctions.GetComponentsInChilds<ILoad>(item, true, true);
            loads.AddRange(list);
        }
        int maxLevelLoad = 0;
        foreach (var load in loads)
        {
            if (load.loadLevel > maxLevelLoad)
            {
                maxLevelLoad = load.loadLevel;
            }
        }
        for (int i = 0; i < maxLevelLoad+1; i++)
        {
            foreach (var load in loads)
            {
                load.Load(i);
            }
        }
    }
    void FindMatchComponents()
    {
        //MatchComponents.setupTeams = transform.GetComponentInChildren<ISetupTeams>();
        //MatchComponents.requestFieldPosition = transform.GetComponentInChildren<IRequestFieldPosition>();
    }
    void setupScripts()
    {
        ComponentsPlayer componentsPlayer = FindObjectOfType<ComponentsPlayer>();
        if (componentsPlayer != null)
        {
            componentsPlayer.scriptsPlayer.menu = componentsPlayer.GetComponentInChildren<PlayTimeMenu>();
            MenuOptions menuOptions = FindObjectOfType<MenuOptions>();
            if (menuOptions != null)
                componentsPlayer.scriptsPlayer.menuOptions = menuOptions;
        }
        
        //componentsPlayer.scriptsPlayer.menuOptions = componentsPlayer.GetComponentInChildren<MenuOptions>();
    }
    void playTimeSetup()
    {
        //MyFunctions.SetStateGameObjectWithValue(gameObject, sceneType);
        PlaytimeCtrl setupInitMenu = FindObjectOfType<PlaytimeCtrl>();
        if (setupInitMenu == null)
        {
            setupInitMenu = gameObject.AddComponent<PlaytimeCtrl>();
        }
        setupInitMenu.StartProcess();

    }
    void normalMatchSetup()
    {
       
        //MyFunctions.SetStateGameObjectWithValue(sceneType);
        //MyFunctions.SetStateGameObjectWithValue(sceneMode);
        ComponentsPlayer componentsPlayer = FindObjectOfType<ComponentsPlayer>();
        if (componentsPlayer != null)
        {
            componentsPlayer.DisableAll();
        }
        FindMatchComponents();
        //PublicPlayerDataList.Load();
        if (automaticChooseFieldPosition)
        {

            AutomaticChooseFieldPosition automaticChooseFieldPosition = FindObjectOfType<AutomaticChooseFieldPosition>();
            automaticChooseFieldPosition.StartProcess();
        }
        else
        {
            ManualChooseFieldPositionCtrl loadNormalMatchCtrl = FindObjectOfType<ManualChooseFieldPositionCtrl>();
            if(loadNormalMatchCtrl!=null)
                loadNormalMatchCtrl.StartProcess();
        }
        switch (sceneMode)
        {
            case SceneModeID.Online:
                OnlineNormalMatch();
                break;
            case SceneModeID.Offline:
                break;
        }
        if (automaticConnect)
        {
            //Importante que este despues de que se carge onlineNormalMatch y automaticChooseFieldPosition
            AutomaticConnect automaticLoadMatch = FindObjectOfType<AutomaticConnect>();
            automaticLoadMatch.StartProcess();
        }
        CornerGoalKickCtrl cornerCtrl = FindObjectOfType<CornerGoalKickCtrl>();
        normalMatchCameraSetup();
    }
   
    void OnlineNormalMatch()
    {
        LoadOnlineMatchCtrl loadOnlineMatchCtrl = transform.GetComponentInChildren<LoadOnlineMatchCtrl>();
        if(loadOnlineMatchCtrl!=null)
            loadOnlineMatchCtrl.start();
    }
    void generalCameraSetup()
    {
        GameObject camera = Camera.main.gameObject;
        ComponentsPlayer componentsPlayer = FindObjectOfType<ComponentsPlayer>();

        if (componentsPlayer != null)
        {
            componentsPlayer.camera = Camera.main;
            componentsPlayer.transCamera = camera.transform;
            componentsPlayer.camera.fieldOfView = 33;
        }
    }
    void normalMatchCameraSetup()
    {
        GameObject initPosCamera = GameObject.FindGameObjectWithTag(Tags.PosInitCamera);
        GameObject camera = Camera.main.gameObject;
        camera.transform.position = initPosCamera.transform.position;
        camera.transform.rotation = initPosCamera.transform.rotation;
    }
    void lookBallSetup()
    {

        ComponentsPlayer componentsPlayer = FindObjectOfType<ComponentsPlayer>();
        if (componentsPlayer != null)
        {
            componentsPlayer.scriptsPlayer.cameraPosition.currentSpeedCamera = componentsPlayer.scriptsPlayer.cameraPosition.speedCameraLookingBall;
            componentsPlayer.scriptsPlayer.cameraRotation.currentSpeedCamera = componentsPlayer.scriptsPlayer.cameraRotation.speedCameraLookAtBall;
        }
    }
    void setupGoals()
    {
        //Para que desde un inicio se ejecute el goalEvent
        foreach (GoalChecker script in FindObjectsOfType<GoalChecker>())
        {
            script.enableGoal();
        }
        //Para que cuando el balón salga de la portería se permita ejecutar el goalEvent
        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag(Tags.Goal))
        {
            /*
            EmptyEventListListener emptyEventListener = gameObject.transform.GetComponentInChildren<EmptyEventListListener>();
            if(emptyEventListener!=null && emptyEventListener.Event.typeEvent==TypeEvent.BallIsOutside)
            {
                GoalCtrl goalCtrl = gameObject.transform.GetComponentInChildren<GoalCtrl>();
                emptyEventListener.Response.AddListener(goalCtrl.enableGoal);
            }*/
        }

    }
    void handMoviment()
    {
        foreach (GoalkeeperCtrl script in FindObjectsOfType<GoalkeeperCtrl>())
        {
            script.setGoTo();
        }
    }
}
