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
using andywiecko.BurstTriangulator;
using Unity.Mathematics;

public class CullPassPoints : MonoBehaviour
{
    [System.Serializable]
    public class CullPassPointsParams
    {
        
        public int entitySize = 10;
        public int entityPointSize = 10;
        public int differentCalculationsSize = 10;
    }
    public CullPassPointsParams cullPassPointsParams;
    public SearchLonelyPointsManager SearchLonelyPointsManager;
    public string teamName_Defense = "Red";
    public string teamName_Attacker = "Blue";
    public List<Transform> testLonelyPoints;
    public bool test,debug,debugPointResults,debugPassTest,debugNextLonelyPoints;
    public bool debugOnlyOrderLonelyPoint;
    public int debugOrderLonelyPointIndex;
    public int lonelyPointIndexPassTest;
    public List<Entity> entities = new List<Entity>();
    public List<List<LonelyPointElement2>> posibleLonelyPoints = new List<List<LonelyPointElement2>>();
    public List<int> posibleLonelyPointsSize = new List<int>();
    public List<bool> AuxNextPositionPlayerBusiesList = new List<bool>();
    EntityManager entityManager;
    List<PublicPlayerData> players = new List<PublicPlayerData>();
    public float v0y = 5;
    public float y = 2;
    public int batchesPerChunk = 1;
    public FootballPositionCtrl FootballPositionCtrl;
    public CalculateNextPositionShedule calculateNextPositionShedule;
    public TriangulatorJob triangulatorJob;
    public List<int> sortLonelyPointsSize;
    public string lineupName="Default", pressureName = "Default";
    public PublicPlayerData publicPlayerData;
    public float testTime = 1;
    public SearchPlayData searchPlayData;
    float fieldOffset = 2;
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        CullPassPointsSystem cullPassPointsSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CullPassPointsSystem>();
        cullPassPointsSystem.CullPassPoints = this;
        cullPassPointsSystem.SearchLonelyPointsManager = SearchLonelyPointsManager;
        searchPlayData = new SearchPlayData();
        triangulatorJob.searchPlayData = searchPlayData;
        createEntities();
        SetTeamAttacker(teamName_Attacker);
        MatchEvents.footballFieldLoaded.AddListenerConsiderInvoked(footballFieldLoaded);
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
                PlayerPositionElements.Insert(index, new PlayerPositionElement(Vector2.zero,Vector2.zero, Vector2.zero, 0));
                if (!aux)
                    players.Insert(index, playerAddedToTeamEventArgs.publicPlayerData);
                CullPassPointsComponent.teamASize++;
            }
            else
            {
                int index = isGoalkeeper ? CullPassPointsComponent.teamASize : CullPassPointsComponent.teamASize + CullPassPointsComponent.teamBSize;
                PlayerPositionElements.Insert(index, new PlayerPositionElement(Vector2.zero, Vector2.zero, Vector2.zero, 0));
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
        int posibleLonelyPointsSize = sortLonelyPointsSize[0];
        for (int i = 0; i < cullPassPointsParams.entitySize; i++)
        {
            EntityArchetype entityArchetype = entityManager.CreateArchetype(typeof(LonelyPointElement2), typeof(CullPassPointsComponent), typeof(PlayerPositionElement), typeof(BallParamsComponent),typeof(TestResultComponent));
            Entity entity = entityManager.CreateEntity(entityArchetype);
            DynamicBuffer<LonelyPointElement2> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement2>(entity);
            for (int j = 0; j < cullPassPointsParams.entityPointSize; j++)
            {
                lonelyPointElements.Add(new LonelyPointElement2());
            }
            entities.Add(entity);
            
        }
        for (int i = 0; i < cullPassPointsParams.differentCalculationsSize; i++)
        {

            posibleLonelyPoints.Add(new List<LonelyPointElement2>());
            for (int j = 0; j < posibleLonelyPointsSize; j++)
            {
                posibleLonelyPoints[i].Add(new LonelyPointElement2());
            }
            this.posibleLonelyPointsSize.Add(0);
        }
        for (int j = 0; j < 11; j++)
        {
            AuxNextPositionPlayerBusiesList.Add(false);
        }
        MatchEvents.publicPlayerDataOfAddedPlayerToTeamIsAvailable.AddListener(PlayerAddedToTeam);
        MatchEvents.ballPhysicsMaterialLoaded.AddListenerConsiderInvoked(() => SetBallParams());
        MatchEvents.footballFieldLoaded.AddListenerConsiderInvoked(() => setFootballFieldParameters());
    }
    void setFootballFieldParameters()
    {
        foreach (var entity in entities)
        {
            CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
            SideOfField sideOfField;
            SideOfFieldCtrl.getSideOfFieldOfTeam(teamName_Defense, out sideOfField);
            Vector3 pos1 = sideOfField.goalComponents.left.position;
            Vector2 post1 = new Vector2(pos1.x, pos1.z);
            Vector3 pos2 = sideOfField.goalComponents.right.position;
            Vector2 post2 = new Vector2(pos2.x, pos2.z);
            CullPassPointsComponent.post1Position = post1;
            CullPassPointsComponent.post2Position = post2;
            CullPassPointsComponent.distanceWeightLerp = MatchComponents.footballField.fieldLenght;
            entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
        }
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
            DynamicBuffer<LonelyPointElement2> lonelyPointElements= entityManager.GetBuffer<LonelyPointElement2>(entity);
            CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
            for (int i = 0; i < testLonelyPoints.Count; i++)
            {
                Vector2 position = new Vector2(testLonelyPoints[i].position.x, testLonelyPoints[i].position.z);
                LonelyPointElement2 LonelyPointElement = new LonelyPointElement2(position, 0);
                lonelyPointElements[i] = LonelyPointElement;
            }
            CullPassPointsComponent.sizeLonelyPoints = testLonelyPoints.Count;
            entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
        }
    }
    public void PlacePoints()
    {
        Entity searchLonelyPointsEntity = SearchLonelyPointsManager.teamsSearchLonelyPointsEntitys[teamName_Defense];
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
    public void UpdatePlayerPositions2()
    {
        
        foreach (var entity in entities)
        {
            //CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
            DynamicBuffer<PlayerPositionElement> PlayerPositionElements = entityManager.GetBuffer<PlayerPositionElement>(entity);
            for (int i = 0; i < players.Count; i++)
            {
                Vector3 position = players[i].position;
                Vector3 forward = players[i].bodyTransform.forward;
                Vector3 normalizedVelocity = players[i].velocity;
                normalizedVelocity.Normalize();
                PlayerPositionElement playerPositionElement = PlayerPositionElements[i];
                playerPositionElement.position = new Vector2(position.x, position.z);
                playerPositionElement.bodyForward = new Vector2(forward.x, forward.z);
                playerPositionElement.normalizedVelocity = new Vector2(normalizedVelocity.x, normalizedVelocity.z);
                playerPositionElement.currentSpeed = players[i].speed;
                PlayerPositionElements[i] = playerPositionElement;
            }
        }
    }
    public void UpdatePlayerPositions(int size)
    {
        for (int i = 0; i < size; i++)
        {
            int Snode = searchPlayData.getSortedNode(i);
            int Fnode = searchPlayData.getFreeNode(i);
            int entityIndex = searchPlayData.getCullEntity(Fnode, i);
            Entity entity = entities[entityIndex];
            DynamicBuffer<PlayerPositionElement> PlayerPositionElements = entityManager.GetBuffer<PlayerPositionElement>(entity);
            for (int j = 0; j < players.Count; j++)
            {
                Vector3 position = players[j].position;
                Vector2 playerPos = searchPlayData.GetPlayerPosition(Snode,j);

                Vector3 forward = players[j].bodyTransform.forward;
                Vector3 normalizedVelocity = players[j].velocity;
                normalizedVelocity.Normalize();
                PlayerPositionElement playerPositionElement = PlayerPositionElements[j];
                playerPositionElement.position = new Vector2(position.x, position.z);
                playerPositionElement.bodyForward = new Vector2(forward.x, forward.z);
                playerPositionElement.normalizedVelocity = new Vector2(normalizedVelocity.x, normalizedVelocity.z);
                playerPositionElement.currentSpeed = players[j].speed;
                PlayerPositionElements[j] = playerPositionElement;
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
            foreach (var entity in entities)
            {
                TestResultComponent TestResultComponent = entityManager.GetComponentData<TestResultComponent>(entity);
                DynamicBuffer<LonelyPointElement2> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement2>(entity);
                foreach (var lonelyPointElement in lonelyPointElements)
                {
                    if (lonelyPointElement.index == lonelyPointIndexPassTest)
                    {
                        MatchComponents.ballRigidbody.velocity = TestResultComponent.straightReachBall ? TestResultComponent.GetV0DOTSResult1.v0 : TestResultComponent.GetV0DOTSResult2.v0;
                        //MatchComponents.ballRigidbody.velocity = TestResultComponent.GetV0DOTSResult1.v0;
                        GetV0DOTSResult GetV0DOTSResult = TestResultComponent.straightReachBall ? TestResultComponent.GetV0DOTSResult1 : TestResultComponent.GetV0DOTSResult2;
                        setAttackTargetPosition(TestResultComponent, GetV0DOTSResult);
                        setDefenseTargetPosition(TestResultComponent, GetV0DOTSResult);
                        //StartCoroutine(TestCoroutine(TestResultComponent, GetV0DOTSResult));
                        //StartCoroutine(TestCoroutineDefenseLonleyPosition(TestResultComponent));
                        return;
                    }
                }
            }
        }
        
    }
    void setAttackTargetPosition(TestResultComponent TestResultComponent, GetV0DOTSResult GetV0DOTSResult)
    {
        PublicPlayerData publicPlayerData = players[TestResultComponent.attackLonelyPointReachIndex];
        Transform attackTransform = publicPlayerData.bodyTransform;

        Vector3 attackPosition = attackTransform.position;
        Vector3 dir = TestResultComponent.lonelyPosition - attackPosition;
        publicPlayerData.playerComponents.TargetPosition = TestResultComponent.lonelyPosition;
        publicPlayerData.playerComponents.ForwardDesiredDirection =dir;
        publicPlayerData.playerComponents.ForwardDesiredSpeed = publicPlayerData.maxSpeed;
        publicPlayerData.playerComponents.DesiredLookDirection = dir;
    }
    void setDefenseTargetPosition(TestResultComponent TestResultComponent, GetV0DOTSResult GetV0DOTSResult)
    {
        PublicPlayerData publicPlayerData = players[TestResultComponent.defenseLonelyPointReachIndex];
        Transform defenseTransform = publicPlayerData.bodyTransform;

        Vector3 defensePosition = defenseTransform.position;
        Vector3 reachPosition = TestResultComponent.closestPosition;
        Vector3 dir = reachPosition - defensePosition;
        publicPlayerData.playerComponents.TargetPosition = reachPosition;
        publicPlayerData.playerComponents.ForwardDesiredDirection = dir;
        publicPlayerData.playerComponents.ForwardDesiredSpeed = publicPlayerData.maxSpeed;
        publicPlayerData.playerComponents.DesiredLookDirection = dir;
    }

    IEnumerator TestCoroutine(TestResultComponent TestResultComponent, GetV0DOTSResult GetV0DOTSResult)
    {
        float t = 0;
        Vector3 attackPosition = players[TestResultComponent.attackLonelyPointReachIndex].bodyTransform.position;
        Vector3 attack_LonelyPositionDir = TestResultComponent.lonelyPosition - attackPosition;
        attack_LonelyPositionDir.Normalize();
        PublicPlayerData publicPlayerData = players[TestResultComponent.attackLonelyPointReachIndex];
        Transform attackTransform = publicPlayerData.bodyTransform;
        float s1, s2;
        ParabolicWithDragDOTS.timeToReachHeightParabolicNoDrag(0, 9.8f, GetV0DOTSResult.v0.y, 0, out s1, out s2);
        print("v=" + GetV0DOTSResult.v0 +" "+ GetV0DOTSResult.v0Magnitude);
        while (t< TestResultComponent.attackReachTime)
        {
            t += Time.deltaTime;
            attackTransform.position += attack_LonelyPositionDir * publicPlayerData.maxSpeed* Time.deltaTime;

            yield return null;
        }
        yield return new WaitForSeconds(s2 - TestResultComponent.attackReachTime);
        Vector3 v = new Vector3(MatchComponents.ballRigidbody.velocity.x, 0, MatchComponents.ballRigidbody.velocity.z);
        print("velocity=" + MatchComponents.ballRigidbody.velocity.magnitude + " " + v.magnitude);
    }
    IEnumerator TestCoroutineDefenseClosestPosition(TestResultComponent TestResultComponent)
    {
        float t = 0;
        Vector3 defensePosition = players[TestResultComponent.defenseLonelyPointReachIndex].bodyTransform.position;
        Vector3 defense_LonelyPositionDir = TestResultComponent.closestPosition - defensePosition;
        defense_LonelyPositionDir.Normalize();
        Transform defenseTransform = players[TestResultComponent.defenseLonelyPointReachIndex].bodyTransform;
        print("closestDistanceDefenseBall=" + TestResultComponent.closestDistanceDefenseBall);
        while (t < TestResultComponent.defenseClosestReachTime)
        {
            t += Time.deltaTime;
            defenseTransform.position += defense_LonelyPositionDir * 10.5f * Time.deltaTime;

            yield return null;
        }
        Vector3 defensePos = defenseTransform.position;
        defensePos.y = MatchComponents.ballRigidbody.position.y;
        print("Distance(defense,ball)=" + Vector3.Distance(defensePos, MatchComponents.ballRigidbody.position));
        //yield return new WaitForSeconds(TestResultComponent.ballReachTargetPositionTime - TestResultComponent.attackReachTime);
        //print(MatchComponents.ballRigidbody.velocity.magnitude);
    }
    IEnumerator TestCoroutineDefenseLonleyPosition(TestResultComponent TestResultComponent)
    {
        float t = 0;
        Vector3 defensePosition = players[TestResultComponent.defenseLonelyPointReachIndex].bodyTransform.position;
        Vector3 defense_LonelyPositionDir = TestResultComponent.closestPosition - defensePosition;
        defense_LonelyPositionDir.Normalize();
        Transform defenseTransform = players[TestResultComponent.defenseLonelyPointReachIndex].bodyTransform;
        print("defenseLonelyPointReachTime="+TestResultComponent.defenseLonelyPointReachTime);
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
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && debug)
        {


            if (debugPassTest)
            {
                Entity entity = entities[0];
                TestResultComponent TestResultComponent = entityManager.GetComponentData<TestResultComponent>(entity);
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(TestResultComponent.closestPosition + Vector3.up * 0.5f, 0.1f);

                GUIStyle style = new GUIStyle();
                style.fontSize = 10;
                style.normal.textColor = Color.yellow;
                Handles.color = Color.green;
                //string text = "ballReachPosTime=" + TestResultComponent.ballReachTargetPositionTime + " defenseIndex=" + TestResultComponent.defenseLonelyPointReachIndex + " defenseReachLonelyPosTime=" + TestResultComponent.defenseLonelyPointReachTime + " closestDistanceDefenseBall=" + TestResultComponent.closestDistanceDefenseBall;
                string text = "defenseReachLonelyPosTime=" + TestResultComponent.defenseLonelyPointReachTime + " closestDistanceDefenseBall=" + TestResultComponent.closestDistanceDefenseBall + " parabolicReachBall=" + TestResultComponent.parabolicReachBall + " straightReachBall=" + TestResultComponent.straightReachBall;
                //string text = "ballReachPosTime=" + TestResultComponent.ballReachTargetPositionTime + " maximumControlSpeedReached=" + TestResultComponent.GetV0DOTSResult1.maximumControlSpeedReached + " maxKickForceReached=" + TestResultComponent.GetV0DOTSResult1.maxKickForceReached + " parabolicReachBall=" + TestResultComponent.parabolicReachBall + " straightReachBall=" + TestResultComponent.straightReachBall;
                Handles.Label(TestResultComponent.closestPosition + Vector3.up * 1.0f, text, style);
            }
            if (debugPointResults)
            {
                foreach (var entity2 in entities)
                {
                    CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity2);

                    DynamicBuffer<LonelyPointElement2> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement2>(entity2);
                    for (int i = 0; i < CullPassPointsComponent.sizeLonelyPoints; i++)
                    {
                        

                        LonelyPointElement2 lonelyPointElement = lonelyPointElements[i];
                        if (debugOnlyOrderLonelyPoint)
                        {
                            if (lonelyPointElement.order != debugOrderLonelyPointIndex)
                            {
                                continue;
                            }
                        }
                        Vector3 pos = new Vector3(lonelyPointElements[i].position.x, 0, lonelyPointElements[i].position.y);
                        Color color;
                        if (lonelyPointElement.order == 0)
                        {
                            color = Color.cyan;
                        }
                       else if (lonelyPointElement.straightReachBall && lonelyPointElement.parabolicReachBall)
                        {
                            color = Color.green;
                        }else if (lonelyPointElement.straightReachBall && !lonelyPointElement.parabolicReachBall)
                        {
                            color = Color.blue;
                        }
                        else if(!lonelyPointElement.straightReachBall && lonelyPointElement.parabolicReachBall)
                        {
                            color = Color.yellow;
                        }
                        else
                        {
                            color = Color.red;
                        }
                        Gizmos.color = color;
                        Gizmos.DrawSphere(pos + Vector3.up * 0.25f, 0.2f);
                        GUIStyle style = new GUIStyle();
                        style.fontSize = 12;
                        style.normal.textColor = color;
                        //string text = "ballReachPosTime=" + TestResultComponent.ballReachTargetPositionTime + " defenseIndex=" + TestResultComponent.defenseLonelyPointReachIndex + " defenseReachLonelyPosTime=" + TestResultComponent.defenseLonelyPointReachTime + " closestDistanceDefenseBall=" + TestResultComponent.closestDistanceDefenseBall;
                        //string text = "straightReachBall=" + lonelyPointElement.straightReachBall + " parabolicReachBall=" + lonelyPointElement.parabolicReachBall + " i="+lonelyPointElement.index;
                        string text = "i=" + lonelyPointElement.index;
                        //string text = "ballReachPosTime=" + TestResultComponent.ballReachTargetPositionTime + " maximumControlSpeedReached=" + TestResultComponent.GetV0DOTSResult1.maximumControlSpeedReached + " maxKickForceReached=" + TestResultComponent.GetV0DOTSResult1.maxKickForceReached + " parabolicReachBall=" + TestResultComponent.parabolicReachBall + " straightReachBall=" + TestResultComponent.straightReachBall;
                        Handles.Label(pos + Vector3.up * 0.5f, text, style);
                        Color c = Color.Lerp(Color.green, Color.red, lonelyPointElement.weight);
                        style.normal.textColor = c;
                        float value = lonelyPointElement.weight * 100;
                        text = "weight=" + value.ToString("f2") + " order="+ lonelyPointElement.order;
                        Handles.Label(pos + Vector3.up * 1.25f, text, style);
                        
                    }
                }
            }
            if (debugNextLonelyPoints)
            {
                DebugNextLonelyPoints();
            }

        }
    }
#endif
    void DebugNextLonelyPoints()
    {
        Team team;
        team = Teams.getTeamByName(teamName_Defense);
        Entity searchLonelyPoint =  SearchLonelyPointsManager.sharedSearchLonelyPointsEntitys[0];
        SearchLonelyPointsManager.DebugSearchLonelyPoints(searchLonelyPoint, SearchLonelyPointsManager.searchLonelyPointsDebug, team);
    }
    public void PlacePoints(int nodeIndex)
    {
        Entity searchLonelyPointsEntity = SearchLonelyPointsManager.teamsSearchLonelyPointsEntitys[teamName_Defense];
        BufferSizeComponent bufferSizeComponent = entityManager.GetComponentData<BufferSizeComponent>(searchLonelyPointsEntity);
        DynamicBuffer<LonelyPointElement> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement>(searchLonelyPointsEntity);
        int entityIndex = 0;
        int lonelyPointCount = 0;
        Entity entity = entities[entityIndex];
        CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
        DynamicBuffer<LonelyPointElement2> lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entity);
        for (int i = 0; i < bufferSizeComponent.lonelyPointsResultSize; i++)
        {

            LonelyPointElement2 lonelyPointElement2 = new LonelyPointElement2(lonelyPointElements[i]);
            
            lonelyPointElements2[lonelyPointCount] = lonelyPointElement2;
            lonelyPointCount++;
            if (lonelyPointCount>=cullPassPointsParams.entityPointSize)
            {

                CullPassPointsComponent.sizeLonelyPoints = lonelyPointCount;
                entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
                entityIndex++;
                entity = entities[entityIndex];
                lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entity);
                lonelyPointCount = 0;
            }
            
        }
        entity = entities[entityIndex];
        CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
        CullPassPointsComponent.sizeLonelyPoints = lonelyPointCount;
        entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
    }
    public void PlacePoints2(int nodeIndex)
    {
    //    Entity searchLonelyPointsEntity = SearchLonelyPointsManager.teamsSearchLonelyPointsEntitys[teamName_Defense];
    //    BufferSizeComponent bufferSizeComponent = entityManager.GetComponentData<BufferSizeComponent>(searchLonelyPointsEntity);
    //    DynamicBuffer<LonelyPointElement> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement>(searchLonelyPointsEntity);
        int entityIndex = 0;
        int lonelyPointCount = 0;
        Entity entity = entities[entityIndex];
        CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
        DynamicBuffer<LonelyPointElement2> lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entity);
        int lonelyPointSize = searchPlayData.GetLonelyPointSize(nodeIndex);
        for (int i = 0; i < lonelyPointSize; i++)
        {
            Point point;
            searchPlayData.GetLonelyPoint(nodeIndex, i,out point);
            LonelyPointElement2 lonelyPointElement2 = new LonelyPointElement2(point,i);

            lonelyPointElements2[lonelyPointCount] = lonelyPointElement2;
            lonelyPointCount++;
            if (lonelyPointCount >= cullPassPointsParams.entityPointSize)
            {

                CullPassPointsComponent.sizeLonelyPoints = lonelyPointCount;
                entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
                entityIndex++;
                entity = entities[entityIndex];
                lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entity);
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
   
    public void SetAllLonelyPointsCalculateNextPositionParameters(int calculationIndex,int lonelyPointSize, FieldPositionsData.HorizontalPositionType horizontalPositionType, Team team,int startSearchLonelyPointIndex)
    {
        bool block = false;
        int order = 0;
        for (int k = 0; k < entities.Count; k++)
        {
            DynamicBuffer<LonelyPointElement2> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement2>(entities[k]);
            CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entities[k]);
            
            for (int i = 0; i < CullPassPointsComponent.sizeLonelyPoints; i++)
            {
                if (lonelyPointElements[i].weight == Mathf.Infinity && !block) continue;
                float minWeight = lonelyPointElements[i].weight;
                

                for (int z = 0; z < entities.Count; z++)
                {
                    DynamicBuffer<LonelyPointElement2> lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entities[z]);
                    CullPassPointsComponent CullPassPointsComponent2 = entityManager.GetComponentData<CullPassPointsComponent>(entities[z]);
                    for (int j = 0; j < CullPassPointsComponent2.sizeLonelyPoints; j++)
                    {
                        if ((z==k && i == j) || lonelyPointElements2[j].weight == Mathf.Infinity && !block) continue;
                        if (minWeight > lonelyPointElements2[j].weight)
                        {
                            //order = lonelyPointElements2[j].order;
                            //order++;
                        }
                        else
                        {
                            LonelyPointElement2 lonelyPointElement = lonelyPointElements2[j];
                            //order = lonelyPointElement.order;
                            //lonelyPointElement.order++;
                            lonelyPointElements2[j] = lonelyPointElement;
                                
                        }
                    }
                }
                if (order < lonelyPointSize)
                {
                    
                    LonelyPointElement2 lonelyPointElement = lonelyPointElements[i];
                    lonelyPointElement.order = order;
                    lonelyPointElements[i] = lonelyPointElement;
                    //posibleLonelyPoints[i][order] = lonelyPointElement;
                    SetCalculateNextPositionParameters(order,ref lonelyPointElement, horizontalPositionType, team);
                    posibleLonelyPoints[calculationIndex][order] = lonelyPointElement;
                    order++;
                    posibleLonelyPointsSize[calculationIndex] = order;
                    if (order >= lonelyPointSize)
                    {
                        break;
                    }
                }
            }
            
        }
    }
    public void clearAuxNextPositionPublicPlayerDatas()
    {
        for (int i = 0; i < AuxNextPositionPlayerBusiesList.Count; i++)
        {
            AuxNextPositionPlayerBusiesList[i] = false;
        }
    }
    public void UpdateNextPlayerPoints(int calculationIndex,int searchLonelyPointEntitySize,int startSearchLonelyPointIndex, FieldPositionsData.HorizontalPositionType horizontalPositionType, Team team,int nextPlayerPositionSize)
    {
        for (int i = startSearchLonelyPointIndex; i < startSearchLonelyPointIndex+searchLonelyPointEntitySize; i++)
        {

            NextPositionData2 nextPositionData = calculateNextPositionShedule.normalNextPosition[i];
            LonelyPointElement2 lonelyPoint = posibleLonelyPoints[calculationIndex][i];
            int k = 0;
            clearAuxNextPositionPublicPlayerDatas();
            
            for (int j = 0; j < nextPlayerPositionSize; j++)
            {
                
                Vector2 normalNextPosition = nextPositionData.NextPositionData.Get(j), normalNextPosition2 = nextPositionData.symetricNextPositionData.Get(j);
                Vector3 nextPosition;
                nextPosition=FootballPositionCtrl.getGlobalPosition(horizontalPositionType, normalNextPosition, team.SideOfField);
                FieldPositionsData.HorizontalPositionType otherHorizontalPositionType = FootballPositionCtrl.getOtherHorizontalPositionType(horizontalPositionType);

                Vector3 nextPosition2 = FootballPositionCtrl.getGlobalPosition(otherHorizontalPositionType, normalNextPosition2, team.SideOfField);
                //nextPosition = getCloseNextPosition(team, ref lonelyPoint, nextPosition);
                //nextPosition2 = getCloseNextPosition(team, ref lonelyPoint, nextPosition2);
                nextPosition = getOrderNextPosition(team, ref lonelyPoint, nextPosition, j, 0,out float endSpeed1,out Vector3 endDirection1);
                nextPosition2 = getOrderNextPosition(team, ref lonelyPoint, nextPosition2, j, 1, out float endSpeed2, out Vector3 endDirection2);
                SetLonelyPosition2(i, k,nextPosition);
                k++;
                SetLonelyPosition2(i, k, nextPosition2);
                k++;
            }
            PublicPlayerData goalkeeperPublicPlayerData = team.getGoalkeeperPublicPlayerData();
            if (goalkeeperPublicPlayerData != null)
            {
                Vector3 goalkeeperPosition = goalkeeperPublicPlayerData.bodyTransform.position;
                //Vector3 nextPosition2 = getCloseNextPosition(team, ref lonelyPoint, goalkeeperPosition, calculationIndex);
                SetLonelyPosition2(i, k, goalkeeperPosition);
            }
        }
     }
    Vector3 getOrderNextPosition(Team team, ref LonelyPointElement2 lonelyPoint, Vector3 optimalDefensePosition,int indexFieldPosition,int sideFieldPosition,out float endSpeed,out Vector3 endDirection)
    {
        PlayerPositionType playerPositionType = calculateNextPositionShedule.playerPositionTypeOrder[indexFieldPosition];
        List<TypeFieldPosition.Type> typeFieldPositions = null;
        if (sideFieldPosition == 0)
        {
            typeFieldPositions = calculateNextPositionShedule.RightPlayerPosition_TypeFieldPosition[playerPositionType];
        }
        else
        {
            typeFieldPositions = calculateNextPositionShedule.LeftPlayerPosition_TypeFieldPosition[playerPositionType];
        }
        PublicPlayerData publicPlayerData;
        team.getPublicPlayerData(typeFieldPositions, out publicPlayerData);

        Transform playerTransform = publicPlayerData.bodyTransform;
        MovimentValues movimentValues = publicPlayerData.movimentValues;
        Vector3 ballPosition = new Vector3(lonelyPoint.position.x,0, lonelyPoint.position.y);
        
        Vector3 nextPosition = GetTimeToReachPointDOTS.accelerationGetPosition(playerTransform.position, publicPlayerData.speed, playerTransform.forward, movimentValues.rotationSpeed, publicPlayerData.movimentValues.minSpeedForRotateBody, movimentValues.forwardAcceleration, movimentValues.forwardDeceleration, movimentValues.maxAngleForRun, publicPlayerData.playerComponents.scope, optimalDefensePosition, publicPlayerData.maxSpeed, lonelyPoint.ballReachTime, ballPosition, out endSpeed,out  endDirection);
        return nextPosition;
    }
    Vector3 getCloseNextPosition(Team team,ref LonelyPointElement2 lonelyPoint,Vector3 optimalDefensePosition)
    {
        int i = 0;
        float minDistance = Mathf.Infinity;
        int playerIndex=0;
        Vector3 nextPositionResult = Vector3.zero;
        
        foreach (var publicPlayerData in team.publicPlayerDatas)
        {
            
            if (AuxNextPositionPlayerBusiesList[i] || publicPlayerData.IsGoalkeeper)
            {
                i++;
                continue;
            }
            Transform playerTransform = publicPlayerData.bodyTransform;
            MovimentValues movimentValues = publicPlayerData.movimentValues;
            Vector3 ballPosition = new Vector3(lonelyPoint.position.x, 0, lonelyPoint.position.y);
            Vector3 nextPosition = GetTimeToReachPointDOTS.accelerationGetPosition(playerTransform.position, publicPlayerData.speed, playerTransform.forward, movimentValues.rotationSpeed, publicPlayerData.movimentValues.minSpeedForRotateBody, movimentValues.forwardAcceleration, movimentValues.forwardDeceleration, movimentValues.maxAngleForRun, publicPlayerData.playerComponents.scope, optimalDefensePosition, publicPlayerData.maxSpeed, lonelyPoint.ballReachTime,ballPosition, out float endSpeed, out Vector3 endDirection);

            //float d = publicPlayerData.maxSpeed * lonelyPoint.ballReachTime;
            //d = Mathf.Clamp(d, 0, Vector3.Distance(publicPlayerData.bodyTransform.position, optimalDefensePosition));
            float d = Vector3.Distance(nextPosition, optimalDefensePosition);
            if (d < minDistance)
            {
                minDistance = d;
                playerIndex = i;
                Vector3 dir = optimalDefensePosition - publicPlayerData.bodyTransform.position;
                dir.y = 0;
                dir.Normalize();
                //Vector3 nextPosition = dir * d + publicPlayerData.bodyTransform.position;
                nextPositionResult = nextPosition;
            }
            i++;
        }
        AuxNextPositionPlayerBusiesList[playerIndex] = true;
        //if (test == 0 && test != -1) print(nextPositionResult);
        return nextPositionResult;

    }
    void SetCalculateNextPositionParameters(int index,ref LonelyPointElement2 lonelyPointElement, FieldPositionsData.HorizontalPositionType horizontalPositionType, Team team)
    {
        PressureFieldPositionDatas PressureFieldPositionDatas;
        if (!FootballPositionCtrl.getCurrentPressureFieldPositions(out PressureFieldPositionDatas)) return;

        Vector3 ballPosition = new Vector3(lonelyPointElement.position.x, 0, lonelyPointElement.position.y);
        Vector2 normalBallPosition = FootballPositionCtrl.getNormalizedPosition(horizontalPositionType, ballPosition, team.SideOfField);
        float offsideWeight;
        float offsideLineValueY = FootballPositionCtrl.GetOffsideLineGetValue(PressureFieldPositionDatas, normalBallPosition, out offsideWeight);
        calculateNextPositionShedule.SetCalculateNextPositionParameters(index, normalBallPosition, offsideLineValueY, offsideWeight);
            

    }
    void SetLonelyPosition(ref DynamicBuffer<PointElement> points,int index,Vector3 position)
    {
        PointElement pointElement = points[index];
        pointElement.index = index;
        pointElement.position.x = position.x;
        pointElement.position.y = position.z;
        points[index] = pointElement;
    }
    void SetLonelyPosition2(int jobIndex,int index, Vector3 position)
    {
        searchPlayData.SetPlayerPosition(jobIndex, index, position);
    }
    public int GetPosibleLonelyPoints(int calculationIndex)
    {
        return posibleLonelyPointsSize[calculationIndex];
    }
    public void CompleteTriangulatorJob(int startIndex,int endIndex)
    {
        searchPlayData.UpdatePoints(startIndex, endIndex);
    }
    void DrawPoint(Vector3 position,string info)
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawSphere(position, 0.25f);
        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.black;
        Handles.Label(position + Vector3.up * 1f, info, style);
    }
    private void footballFieldLoaded()
    {
        for (int i = 0; i < searchPlayData.maxSize; i++)
        {
            SearchPlayData.SearchPlayNode searchPlayNode = searchPlayData.searchPlayNodes[i];
            
            NativeArray<float2> array = searchPlayNode.playerPositions;
            for (int j = 0; j < MatchComponents.footballField.cornersComponents.Count; j++)
            {
                Transform cornerTransform = MatchComponents.footballField.cornersComponents[j].cornerPoint;
                Vector3 pos = cornerTransform.position + cornerTransform.TransformDirection(new Vector3(fieldOffset, 0, fieldOffset));
                array[j] = new Vector2(pos.x, pos.z);

            }
            searchPlayNode.playerPositions = array;
        }
    }
    private void OnDestroy()
    {
        searchPlayData.Dispose();
    }
}
