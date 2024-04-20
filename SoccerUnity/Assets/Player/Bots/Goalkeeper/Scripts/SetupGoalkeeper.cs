using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupGoalkeeper : MonoBehaviour
{
    public enum TypeSetupGoalkeeper
    {
        Init,TeamSetup
    }
    GoalkeeperComponents goalkeeperComponents;
    public SideOfField initSideOfField;
    public MyEvent goalkeeperSetuped;
    public List<Transform> chaserTransformList;
    public Transform chaserTransformParent;
    void Start()
    {
        goalkeeperComponents = GetComponent<GoalkeeperComponents>();
        goalkeeperComponents.goalkeeperCtrl.checkEnable();
        goalkeeperComponents.goalkeeperCtrl.targetPosition = goalkeeperComponents.goalkeeperCtrl.goalKeeperTransform.position;
        
    }
    public void StartSetup(TypeSetupGoalkeeper typeSetupGoalkeeper)
    {
        goalkeeperSetuped = new MyEvent(GetHashCode().ToString());
        goalkeeperComponents = GetComponent<GoalkeeperComponents>();
        goalkeeperComponents.goalkeeperCtrl.enabled = false;
        switch (typeSetupGoalkeeper)
        {
            case TypeSetupGoalkeeper.TeamSetup:
                initFlexibleSetup();
                break;
            case TypeSetupGoalkeeper.Init:
                initSetup();
                break;
        }
    }
    void setupGetTimeToReachPoint()
    {
        PublicGoalkeeperData myPublicPlayerData = gameObject.GetComponent<PublicGoalkeeperData>();
        GetTimeToReachPoint getTimeToReachPoint = new GetTimeToReachPoint(myPublicPlayerData);
        getTimeToReachPoint.getTimeToReachPointDelegate = getTimeToReachPoint.linearGetTimeToReachPosition;
        myPublicPlayerData.playerComponents.GetTimeToReachPosition = getTimeToReachPoint;
        //myPublicPlayerData.GetTimeToReachPosition = getTimeToReachPoint;
    }
    void initSetup()
    {
        initLoad();
        initSideOfField.isLoadedEvent.AddListenerConsiderInvoked(initSetupPhase2);
        MatchEvents.setMainBall.AddListenerConsiderInvoked(changeBall);
        
    }
    void initSetupPhase2()
    {
        
        goalkeeperComponents.goalkeeperCtrl.setSideOfField(initSideOfField);
        GoalkeeperValues goalkeeperValues = GetComponent<GoalkeeperValues>();
        goalkeeperValues.setGoalComponents(initSideOfField.goalComponents);
        setupPublicGoalkeeperData();
    }
    void initFlexibleSetup()
    {
        initLoad();
        GoalkeeperCtrl goalkeeperCtrl = goalkeeperComponents.goalkeeperCtrl;
        goalkeeperComponents.goalkeeperEvents.addTeamEvent.AddListenerConsiderInvoked(addToTeamEvent);
        MatchEvents.setMainBall.AddListenerConsiderInvoked(changeBall);
    }
    void changeBall(GameObject ball)
    {
        goalkeeperComponents.goalkeeperCtrl.ComponentsBall = MyFunctions.GetComponentInChilds<BallComponents>(ball,true);
    }
    void addToTeamEvent(Team team)
    {
        team.sideOfFieldChanged.AddListenerConsiderInvoked(sideOfFieldOfTeamChanged);
    }
    void sideOfFieldOfTeamChanged(SideOfField sideOfField)
    {
        GoalkeeperCtrl goalkeeperCtrl = goalkeeperComponents.goalkeeperCtrl;
        goalkeeperCtrl.setSideOfField(sideOfField);
        GoalkeeperValues goalkeeperValues = goalkeeperComponents.goalkeeperValues;
        goalkeeperValues.setGoalComponents(sideOfField.goalComponents);
        goalkeeperValues.isLoadedEvent.AddListenerConsiderInvoked(setupPublicGoalkeeperData);
    }
    void initLoad()
    {
        PublicGoalkeeperData myPublicPlayerData = gameObject.GetComponent<PublicGoalkeeperData>();
        goalkeeperComponents.goalkeeperCtrl.myPublicPlayerData = myPublicPlayerData;
        goalkeeperComponents.goalkeeperCtrl.Load();
        MatchEvents.footballFieldLoaded.AddListenerConsiderInvoked(setupChaserDataList);
        setupColliders();
        setupGetTimeToReachPoint();
    }
    void setupPublicGoalkeeperData()
    {
        GoalkeeperValues goalkeeperValues = goalkeeperComponents.goalkeeperValues;
        PublicGoalkeeperData myPublicPlayerData = gameObject.GetComponent<PublicGoalkeeperData>();
        myPublicPlayerData.maximumJumpHeights.Clear();
        myPublicPlayerData.addMaximumJumpHeight(goalkeeperValues.maxHeightInArea, goalkeeperComponents.goalkeeperCtrl.Area);
        myPublicPlayerData.addMaximumJumpHeight(goalkeeperValues.maxHeightOutsideArea, null);
        myPublicPlayerData.maxSpeedVar = goalkeeperValues.maxSpeedVar;
        
        
        goalkeeperComponents.goalkeeperCtrl.checkEnable();
        
        GoalComponents goalComponents;
        if(SideOfFieldCtrl.getGoalComponentsOfPlayer(myPublicPlayerData.playerID, out goalComponents))
        {
            myPublicPlayerData.InitPosition = goalComponents.centerMatchStoppedState.position;
            myPublicPlayerData.InitRotation = goalComponents.centerMatchStoppedState.rotation;
        }
        PublicPlayerDataList.addPublicPlayerData(myPublicPlayerData);
        Team team;
        Teams.getTeamFromPlayer(myPublicPlayerData.playerID, out team);
        //team.sideOfFieldChanged.RemoveListener(sideOfFieldOfTeamChanged);
        goalkeeperSetuped.Invoke();
    }
    public static void changeSideOfField(PublicGoalkeeperData publicGoalkeeperData)
    {
        publicGoalkeeperData.maximumJumpHeights.Clear();
        publicGoalkeeperData.addMaximumJumpHeight(publicGoalkeeperData.values.maxHeightInArea, publicGoalkeeperData.components.goalkeeperCtrl.Area);
        publicGoalkeeperData.addMaximumJumpHeight(publicGoalkeeperData.values.maxHeightOutsideArea, null);
        GoalComponents goalComponents;
        if (SideOfFieldCtrl.getGoalComponentsOfPlayer(publicGoalkeeperData.playerID, out goalComponents))
        {
            publicGoalkeeperData.InitPosition = goalComponents.centerMatchStoppedState.position;
            publicGoalkeeperData.InitRotation = goalComponents.centerMatchStoppedState.rotation;
        }
    }
    void setupColliders()
    {
        SetHandColliders setHandColliders = gameObject.GetComponent<SetHandColliders>();
        PublicGoalkeeperData myPublicPlayerData = gameObject.GetComponent<PublicGoalkeeperData>();
        setHandColliders.StartProcess(myPublicPlayerData.bodyTransform);
    }
    public void setupChaserDataList(){
        PublicPlayerData myPublicPlayerData = gameObject.GetComponent<PublicPlayerData>();
        string[] nameList = new string[]{ "Armature", "Wirst1", "Corazon3" };
        Vector3[] offset = new Vector3[] { Vector3.zero,Vector3.up*0.5f, Vector3.zero };
        myPublicPlayerData.ChaserDataList.Clear();
        Transform wirst1 = MyFunctions.FindChildContainsName(gameObject, "Wirst1", true).transform;
        chaserTransformParent.position = wirst1.position;
        for (int i = 0; i< chaserTransformList.Count; i++)
        {
            Transform trans = MyFunctions.FindChildContainsName(gameObject, nameList[i], true).transform;
            //chaserTransformList[i].position = wirst1.position + chaserTransformList[i].localPosition;
            Vector3 position = myPublicPlayerData.bodyTransform.InverseTransformPoint(chaserTransformList[i].position);
            //Vector3 scale = trans.localScale;
            //trans.localScale =
            //Vector3 position = myPublicPlayerData.bodyTransform.InverseTransformPoint(trans.position + offset[i]);
            myPublicPlayerData.addChaserData(new ChaserData(position,myPublicPlayerData,MatchComponents.footballField.fullFieldArea,goalkeeperComponents.goalkeeperValues.chaserScope, chaserTransformList[i].name));
        }
        goalkeeperComponents.goalkeeperCtrl.checkEnable();
    }
}
