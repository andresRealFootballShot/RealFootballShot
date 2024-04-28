using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NextMove_Algorithm;
using Unity.Entities;
using FieldTriangleV2;
using Unity.Collections;
using CullPositionPoint;
using static Unity.Burst.Intrinsics.X86;
using DOTS_ChaserDataCalculation;
using Unity.Entities.UniversalDelegates;
using UnityEditor;
public class CullPassPoints : MonoBehaviour
{
    [System.Serializable]
    public class CullPassPointsParams
    {
        public int entitySize = 10;
        public int entityPointSize = 10;
    }
    public CullPassPointsParams cullPassPointsParams;
    public SearchLonelyPointsManager SearchLonelyPointsManager;
    public string teamName_Defense = "Red";
    public string teamName_Attacker = "Blue";
    public Transform testLonelyPoint;
    public bool test,debug;
    public List<Entity> entities = new List<Entity>();
    EntityManager entityManager;
    List<PublicPlayerData> players = new List<PublicPlayerData>();
    public float v0y = 5;
    public float y = 2;
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        CullPassPointsSystem cullPassPointsSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CullPassPointsSystem>();
        cullPassPointsSystem.CullPassPoints = this;

        createEntities();
        SetTeamAttacker(teamName_Attacker);
    }
    void test1()
    {
        float k = 0.1f;
        
        float pos0Y = 0;
        float tf = 10;
        float vf = 9.8f/k;
        float t,tt;
        float t1, t2;
        ParabolicWithDragDOTS.timeToReachHeightParabolicNoDrag(y, 9.8f, v0y, pos0Y, out t1, out t2);
        float maxY = ParabolicWithDragDOTS.getMaximumY(v0y, k, 9.8f);
        float maxYT = ParabolicWithDragDOTS.getMaximumYTime(v0y, k, 9.8f);
        float posY;
        print("maxY=" + maxY + " " + t1 + " " + t2);
        ParabolicWithDragDOTS.getTimeToReachY(true,y,t1,t2, maxYT, k,v0y,pos0Y,vf, 0.1f, out t);
        posY = ParabolicWithDragDOTS.ParabolicWithDragGetPosYAtTime(t, k, v0y, pos0Y, vf);
        print(t + " " + posY);
        ParabolicWithDragDOTS.getTimeToReachY(false, y, t1, t2, maxYT, k, v0y, pos0Y, vf, 0.1f, out t);
        posY = ParabolicWithDragDOTS.ParabolicWithDragGetPosYAtTime(t, k, v0y, pos0Y, vf);
        print(t + " " + posY);
    }
    void PlayerAddedToTeam(PlayerAddedToTeamEventArgs playerAddedToTeamEventArgs)
    {
        bool aux = false;
        bool isGoalkeeper = playerAddedToTeamEventArgs.publicPlayerData.IsGoalkeeper;
        foreach (var entity in entities)
        {
            CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
            DynamicBuffer<PlayerPositionElement> PlayerPositionElements = entityManager.GetBuffer<PlayerPositionElement>(entity);
            if (playerAddedToTeamEventArgs.TeamName.Equals("Red"))
            {
                int index = isGoalkeeper ? 0 : CullPassPointsComponent.teamASize;
                PlayerPositionElements.Insert(index, new PlayerPositionElement(Vector2.zero));
                if (!aux)
                    players.Insert(index, playerAddedToTeamEventArgs.publicPlayerData);
                CullPassPointsComponent.teamASize++;
            }
            else
            {
                int index = isGoalkeeper ? CullPassPointsComponent.teamASize : CullPassPointsComponent.teamASize + CullPassPointsComponent.teamBSize;
                PlayerPositionElements.Insert(index, new PlayerPositionElement(Vector2.zero));
                if (!aux)
                    players.Insert(index, playerAddedToTeamEventArgs.publicPlayerData);
                CullPassPointsComponent.teamBSize++;
            }
            entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
            aux = true;
        }
    }
    void SetTeamAttacker(string teamName)
    {
        foreach (var entity in entities)
        {
            CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
            CullPassPointsComponent.teamA_IsAttacker = teamName.Equals("Red");
            entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
        }
    }
    void createEntities()
    {
        for (int i = 0; i < cullPassPointsParams.entitySize; i++)
        {
            EntityArchetype entityArchetype = entityManager.CreateArchetype(typeof(LonelyPointElement), typeof(CullPassPointsComponent), typeof(PlayerPositionElement), typeof(BallParamsComponent),typeof(TestResultComponent));
            Entity entity = entityManager.CreateEntity(entityArchetype);
            DynamicBuffer<LonelyPointElement> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement>(entity);
            for (int j = 0; j < cullPassPointsParams.entityPointSize; j++)
            {
                lonelyPointElements.Add(new LonelyPointElement());
            }
            entities.Add(entity);
        }
        MatchEvents.publicPlayerDataOfAddedPlayerToTeamIsAvailable.AddListener(PlayerAddedToTeam);
        MatchEvents.ballPhysicsMaterialLoaded.AddListenerConsiderInvoked(() => SetBallParams());
    }
    void SetBallParams()
    {
        foreach (var entity in entities)
        {
            BallParamsComponent BallParamsComponent = new BallParamsComponent();
            BallParamsComponent.k = MatchComponents.ballRigidbody.drag;
            BallParamsComponent.g = Physics.gravity.magnitude;
            BallParamsComponent.ballRadio = MatchComponents.ballRadio;
            BallParamsComponent.friction = MatchComponents.ballComponents.friction;
            BallParamsComponent.dynamicFriction = MatchComponents.ballComponents.dynamicFriction;
            BallParamsComponent.mass = MatchComponents.ballComponents.mass;
            BallParamsComponent.groundY= MatchComponents.ballComponents.radio * MatchComponents.ballComponents.transBall.localScale.x;
            BallParamsComponent.bounciness = MatchComponents.ballComponents.bounciness;
            SetBallPosition(ref BallParamsComponent);
            entityManager.SetComponentData<BallParamsComponent>(entity, BallParamsComponent);
        }
    }
    public void SetBallPosition(ref BallParamsComponent BallParamsComponent)
    {
        BallParamsComponent.BallPosition = MatchComponents.ballRigidbody.position;
    }
    public void PlaceTestLonelyPoint()
    {
        foreach (var entity in entities)
        {
            DynamicBuffer<LonelyPointElement> lonelyPointElements= entityManager.GetBuffer<LonelyPointElement>(entity);
            CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
            Vector2 position = new Vector2(testLonelyPoint.position.x, testLonelyPoint.position.z);
            LonelyPointElement LonelyPointElement = new LonelyPointElement(position, 0, false);
            lonelyPointElements[0]= LonelyPointElement;
            CullPassPointsComponent.sizeLonelyPoints = 1;
            entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
        }
    }
    public void PlacePoints()
    {
        Entity searchLonelyPointsEntity = SearchLonelyPointsManager.searchLonelyPointsEntitys[teamName_Defense];
        /*DynamicBuffer<EdgeElement> edges = entityManager.GetBuffer<EdgeElement>(searchLonelyPointsEntity);
        DynamicBuffer<TriangleElement> triangles = entityManager.GetBuffer<TriangleElement>(searchLonelyPointsEntity);
        DynamicBuffer<PointElement> points = entityManager.GetBuffer<PointElement>(searchLonelyPointsEntity);*/
        BufferSizeComponent bufferSizeComponent = entityManager.GetComponentData<BufferSizeComponent>(searchLonelyPointsEntity);
        DynamicBuffer<LonelyPointElement> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement>(searchLonelyPointsEntity);
        int entityIndex = 0;
        //print(bufferSizeComponent.lonelyPointsResultSize);

        DynamicBuffer<LonelyPointElement> lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement>(entities[0]);
        for (int i = 0; i < bufferSizeComponent.lonelyPointsResultSize; i++)
        {

            lonelyPointElements2[i % cullPassPointsParams.entityPointSize] = lonelyPointElements[i];
            if (i % cullPassPointsParams.entityPointSize >= cullPassPointsParams.entityPointSize - 1)
            {
                entityIndex++;
                if (entityIndex >= cullPassPointsParams.entitySize) break;
                lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement>(entities[entityIndex]);
            }
        }
    }
    public void UpdatePlayerPositions()
    {
        
        foreach (var entity in entities)
        {
            //CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
            DynamicBuffer<PlayerPositionElement> PlayerPositionElements = entityManager.GetBuffer<PlayerPositionElement>(entity);
            for (int i = 0; i < players.Count; i++)
            {
                Vector3 position = players[i].position;
                PlayerPositionElement playerPositionElement = PlayerPositionElements[i];
                playerPositionElement.position = new Vector2(position.x, position.z);
                PlayerPositionElements[i] = playerPositionElement;
            }
        }
    }
    private void Update()
    {
        TestDebug();
    }
    void TestDebug()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Entity entity = entities[0];
            TestResultComponent TestResultComponent = entityManager.GetComponentData<TestResultComponent>(entity);
            MatchComponents.ballRigidbody.velocity = TestResultComponent.straightReachBall ? TestResultComponent.GetV0DOTSResult1.v0 : TestResultComponent.GetV0DOTSResult2.v0;

            StartCoroutine(TestCoroutine(TestResultComponent));
            StartCoroutine(TestCoroutine2(TestResultComponent));
        }
        
    }
    IEnumerator TestCoroutine(TestResultComponent TestResultComponent)
    {
        float t = 0;
        Vector3 attackPosition = players[TestResultComponent.attackLonelyPointReachIndex].bodyTransform.position;
        Vector3 attack_LonelyPositionDir = TestResultComponent.lonelyPosition - attackPosition;
        attack_LonelyPositionDir.Normalize();
        Transform attackTransform = players[TestResultComponent.attackLonelyPointReachIndex].bodyTransform;
        
        while (t< TestResultComponent.attackReachTime)
        {
            t += Time.deltaTime;
            attackTransform.position += attack_LonelyPositionDir * 10.5f*Time.deltaTime;

            yield return null;
        }
        yield return new WaitForSeconds(TestResultComponent.ballReachTargetPositionTime - TestResultComponent.attackReachTime);
        print(MatchComponents.ballRigidbody.velocity.magnitude);
    }
    IEnumerator TestCoroutine2(TestResultComponent TestResultComponent)
    {
        float t = 0;
        Vector3 defensePosition = players[TestResultComponent.defenseLonelyPointReachIndex].bodyTransform.position;
        Vector3 defense_LonelyPositionDir = TestResultComponent.closestPosition - defensePosition;
        defense_LonelyPositionDir.Normalize();
        Transform defenseTransform = players[TestResultComponent.defenseLonelyPointReachIndex].bodyTransform;
        print(TestResultComponent.closestDistanceDefenseBall);
        while (t < TestResultComponent.defenseClosestReachTime)
        {
            t += Time.deltaTime;
            defenseTransform.position += defense_LonelyPositionDir * 10.5f * Time.deltaTime;

            yield return null;
        }
        Vector3 defensePos = defenseTransform.position;
        defensePos.y = MatchComponents.ballRigidbody.position.y;
        print(Vector3.Distance(defensePos, MatchComponents.ballRigidbody.position));
        //yield return new WaitForSeconds(TestResultComponent.ballReachTargetPositionTime - TestResultComponent.attackReachTime);
        //print(MatchComponents.ballRigidbody.velocity.magnitude);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && debug)
        {
            Entity entity = entities[0];
            TestResultComponent TestResultComponent = entityManager.GetComponentData<TestResultComponent>(entity);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(TestResultComponent.closestPosition + Vector3.up * 0.5f, 0.1f);
            
#if UNITY_EDITOR
                GUIStyle style = new GUIStyle();
                style.fontSize = 10;
                style.normal.textColor = Color.yellow;
                Handles.color = Color.green;
            //string text = "ballReachPosTime=" + TestResultComponent.ballReachTargetPositionTime + " defenseIndex=" + TestResultComponent.defenseLonelyPointReachIndex + " defenseReachLonelyPosTime=" + TestResultComponent.defenseLonelyPointReachTime + " closestDistanceDefenseBall=" + TestResultComponent.closestDistanceDefenseBall;
            string text = "ballReachPosTime=" + TestResultComponent.ballReachTargetPositionTime +" defenseReachLonelyPosTime=" + TestResultComponent.defenseLonelyPointReachTime + " closestDistanceDefenseBall=" + TestResultComponent.closestDistanceDefenseBall + " parabolicReachBall=" + TestResultComponent.parabolicReachBall + " straightReachBall=" + TestResultComponent.straightReachBall;
            //string text = "ballReachPosTime=" + TestResultComponent.ballReachTargetPositionTime + " maximumControlSpeedReached=" + TestResultComponent.GetV0DOTSResult1.maximumControlSpeedReached + " maxKickForceReached=" + TestResultComponent.GetV0DOTSResult1.maxKickForceReached + " parabolicReachBall=" + TestResultComponent.parabolicReachBall + " straightReachBall=" + TestResultComponent.straightReachBall;
                Handles.Label(TestResultComponent.closestPosition + Vector3.up * 1.0f, text, style);
#endif
        }
    }
    public void PlacePoints2()
    {
        Entity searchLonelyPointsEntity = SearchLonelyPointsManager.searchLonelyPointsEntitys[teamName_Defense];
        /*DynamicBuffer<EdgeElement> edges = entityManager.GetBuffer<EdgeElement>(searchLonelyPointsEntity);
        DynamicBuffer<TriangleElement> triangles = entityManager.GetBuffer<TriangleElement>(searchLonelyPointsEntity);
        DynamicBuffer<PointElement> points = entityManager.GetBuffer<PointElement>(searchLonelyPointsEntity);*/
        BufferSizeComponent bufferSizeComponent = entityManager.GetComponentData<BufferSizeComponent>(searchLonelyPointsEntity);
        DynamicBuffer<LonelyPointElement> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement>(searchLonelyPointsEntity);
        int entityIndex = 0;
        //print(bufferSizeComponent.lonelyPointsResultSize);
        int lonelyPointCount = 0;
        Entity entity = entities[entityIndex];
        CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
        DynamicBuffer<LonelyPointElement> lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement>(entity);
        lonelyPointElements2.Clear();
        for (int i = 0; i < bufferSizeComponent.lonelyPointsResultSize; i++)
        {
            

            lonelyPointCount++;
            lonelyPointElements2.Add(lonelyPointElements[i]);
            if (lonelyPointCount>=cullPassPointsParams.entityPointSize)
            {
                entityIndex++;
                entity = entities[entityIndex];
                lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement>(entity);
                lonelyPointElements2.Clear();
                CullPassPointsComponent.sizeLonelyPoints = lonelyPointCount;
                entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
                lonelyPointCount = 0;
            }
            
        }
        entity = entities[entityIndex];
        CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
        CullPassPointsComponent.sizeLonelyPoints = lonelyPointCount;
        entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
    }
    private void TestPlayers()
    {
        print("eooo");
        Team teamRed = Teams.getTeamByName("Red");
        Team teamBlue = Teams.getTeamByName("Blue");
        foreach (var publicPlayerData in teamRed.publicPlayerDatas)
        {
            print(publicPlayerData.playerID);
        }
        foreach (var publicPlayerData in teamBlue.publicPlayerDatas)
        {
            print(publicPlayerData.playerID);
        }
        print("aaaaa");
        foreach (var publicPlayerData in players)
        {
            print(publicPlayerData.playerID);
        }
    }
}
