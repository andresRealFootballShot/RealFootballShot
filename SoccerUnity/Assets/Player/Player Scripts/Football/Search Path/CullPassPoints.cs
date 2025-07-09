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
using UnityEditor.Experimental.GraphView;
using static UnityEditor.PlayerSettings;


public class CullPassPoints : MonoBehaviour
{
    [System.Serializable]
    public class CullPassPointsParams
    {
        
        public int entitySize = 10;
        public int entitySizePerNode = 10;
        public int entityPointSize = 10;
        public int differentCalculationsSize = 10;
        public int nodeCalculationPerFrame = 10;
        public int cullJobLonelyPointMaxSize = 100;
        public int repetitionPerFrame = 1;
        public int maxPosibleLonelyPointsSize=100;
    }
    public bool enableCullPassPointsSystem;
    public CullPassPointsParams cullPassPointsParams;
    public SearchLonelyPointsManager SearchLonelyPointsManager;
    public string teamName_Defense = "Red";
    public string teamName_Attacker = "Blue";
    public List<Transform> testLonelyPoints;
    public bool debug,debugPointResults, _debugNode;
    public bool _debugAllLonelyPointsOfNode;
    public bool _debugLonelyPointIndex,debugReachableLonelyPoints;
    public int debugNode = 0;
    public int debugLonelyPointIndex = 0;
    public bool debugPlayerIndex;
    public List<LonelyPointElement2> debugWeightLonelyPooints = new List<LonelyPointElement2>();
    public bool debugText;
    public int lonelyPointIndexPassTest;
    public int searchNodeDebug;
    public List<int> searchNodeDebugList;
    public List<Entity> entities = new List<Entity>();
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
    CullPassPointsSystem cullPassPointsSystem;
    int teamA_size, teamB_size,teamAttack_start,teamDefense_start,teamAttack_size,teamDefense_size;
    bool teamA_isAttacker;
    
    public int maxNodes { get; set; } = 0;
    public int maxNodes2 { get; set; } = 0;
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        cullPassPointsSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CullPassPointsSystem>();
        cullPassPointsSystem.CullPassPoints = this;
        cullPassPointsSystem.SearchLonelyPointsManager = SearchLonelyPointsManager;
        int previous=1;
        for (int i = 0; i < sortLonelyPointsSize.Count; i++)
        {
            previous = previous*sortLonelyPointsSize[i];
            maxNodes += previous;
        }
        previous = 1;
        for (int i = 0; i < sortLonelyPointsSize.Count-1; i++)
        {
            previous = previous * sortLonelyPointsSize[i];
            maxNodes2 += previous;
        }
        //cullPassPointsSystem.Snodes = new List<int>(new int[cullPassPointsParams.nodeCalculationPerFrame]);
        //cullPassPointsSystem.Fnodes = new List<int>(new int[cullPassPointsParams.nodeCalculationPerFrame]);
        int posibleLonelyPointsSize = sortLonelyPointsSize[0];
        searchPlayData.Load(cullPassPointsParams.nodeCalculationPerFrame);
        for (int i = 0; i < searchPlayData.searchPlayNodes.Count; i++)
        {
            searchPlayData.SetPlayerPositions(i, new NativeArray<float2>(searchPlayData.playerPosSize, Allocator.Persistent));
            searchPlayData.SetTriangulator(i,new Triangulator(Allocator.Persistent, searchPlayData.GetLonelyPointParameters));
        }
        triangulatorJob.searchPlayData = searchPlayData;
        createEntities();
        //searchPlayData.SetCullEntities(cullPassPointsParams.entitySizePerNode);
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
                teamA_size = CullPassPointsComponent.teamASize;
                
            }
            else
            {
                int index = isGoalkeeper ? CullPassPointsComponent.teamASize : CullPassPointsComponent.teamASize + CullPassPointsComponent.teamBSize;
                PlayerPositionElements.Insert(index, new PlayerPositionElement(Vector2.zero, Vector2.zero, Vector2.zero, 0));
                if (!aux)
                    players.Insert(index, playerAddedToTeamEventArgs.publicPlayerData);
                CullPassPointsComponent.teamBSize++;
                teamB_size = CullPassPointsComponent.teamBSize;
                
            }
            entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
            aux = true;
        }
        SetTeamAttacker(teamName_Attacker);
    }
    void SetTeamAttacker(string teamName)
    {
        foreach (var entity in entities)
        {
            CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
            CullPassPointsComponent.teamA_IsAttacker = teamName.Equals("Red");
            entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
        }
        teamA_isAttacker = teamName.Equals("Red");
        /*teamAttack_start = teamA_isAttacker ? teamA_size : 0;
        teamDefense_start = teamA_isAttacker ? teamA_size + teamB_size : teamA_size;
        teamAttack_size = teamA_isAttacker ? teamA_size : teamB_size;
        teamDefense_size = teamA_isAttacker ? teamB_size : teamA_size;*/
        teamAttack_start = teamA_isAttacker ? 0 : teamA_size;
        teamDefense_start = teamA_isAttacker ? teamA_size : 0;
        teamAttack_size = teamA_isAttacker ? teamA_size : teamB_size;
        teamDefense_size = teamA_isAttacker ? teamB_size : teamA_size;
        for (int k = 0; k < searchPlayData.searchPlayNodes.Count; k++)
        {
            int Snode = k;
            for (int i = teamAttack_start, j = 0; i < teamAttack_start + teamAttack_size; i++, j++)
            {

                Vector3 position = Vector3.one*i;
                Vector3 forward = Vector3.one * i;
                //Vector3 normalizedVelocity = teamPlayers.publicPlayerDatas[i].velocity;
                //normalizedVelocity.Normalize();
                float speed = 0;
                searchPlayData.SetPlayerPosition(Snode, i, position, speed, forward);
            }
            for (int i = teamDefense_start, j = 0; i < teamDefense_start + teamDefense_size; i++, j++)
            {

                Vector3 position = Vector3.one * i;
                Vector3 forward = Vector3.one * i;
                //Vector3 normalizedVelocity = teamPlayers.publicPlayerDatas[i].velocity;
                //normalizedVelocity.Normalize();
                float speed = 0;
                searchPlayData.SetPlayerPosition(Snode, i, position, speed, forward);
            }
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
            BallParamsComponent.BallPosition = MatchComponents.ballRigidbody.position;
            //searchPlayData.getSortedNodes(ref cullPassPointsSystem.Snodes, 0);
            searchPlayData.SetBallPosition(0, MatchComponents.ballRigidbody.position);
            entityManager.SetComponentData<BallParamsComponent>(entity, BallParamsComponent);
        }
    }
    public void SetBallPosition(List<int> nodes, int sizeNode)
    {
        for (int i = 0; i < sizeNode; i++)
        {

            int node = nodes[i];
            int cullCount = searchPlayData.getCullEntityCount(node);
            searchPlayData.SetBallPosition(node, MatchComponents.ballRigidbody.position);
            for (int j = 0; j < cullCount; j++)
            {
                int entityIndex = searchPlayData.getCullEntity(node, j);
                Entity entity = entities[entityIndex];

                BallParamsComponent BallParamsComponent = entityManager.GetComponentData<BallParamsComponent>(entity);
                BallParamsComponent.BallPosition = MatchComponents.ballRigidbody.position;
                entityManager.SetComponentData<BallParamsComponent>(entity, BallParamsComponent);
                
            }
        }
    }
    public void SetBallPosition2(List<int> Snodes, int size)
    {
        for (int i = 0; i < size; i++)
        {
            int node = Snodes[i];
            Vector3 ballPosition = searchPlayData.GetBallPosition(node);
            int cullCount = searchPlayData.getCullEntityCount(node);
            for (int j = 0; j < cullCount; j++)
            {
                int entityIndex = searchPlayData.getCullEntity(node, j);
                Entity entity = entities[entityIndex];

                BallParamsComponent BallParamsComponent = entityManager.GetComponentData<BallParamsComponent>(entity);
                BallParamsComponent.BallPosition = ballPosition;
                entityManager.SetComponentData<BallParamsComponent>(entity, BallParamsComponent);
            }
            
        }
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
    public void UpdateInstantPlayerPositions(Team defenseTeam,Team attackTeam,List<int> Snodes)
    {
        for (int k = 0; k < searchPlayData.searchPlayNodes.Count; k++)
        {
            int Snode = k;
            for (int i = teamAttack_start, j = 0; i < teamAttack_start + teamAttack_size; i++, j++)
            {

                Vector3 position = attackTeam.publicPlayerDatas[j].position;
                Vector3 forward = attackTeam.publicPlayerDatas[j].bodyTransform.forward;
                //Vector3 normalizedVelocity = teamPlayers.publicPlayerDatas[i].velocity;
                //normalizedVelocity.Normalize();
                float speed = attackTeam.publicPlayerDatas[j].speed;
                searchPlayData.SetPlayerPosition(Snode, i, position, speed, forward);
            }
            for (int i = teamDefense_start, j = 0; i < teamDefense_start + teamDefense_size; i++, j++)
            {

                Vector3 position = defenseTeam.publicPlayerDatas[j].position;
                Vector3 forward = defenseTeam.publicPlayerDatas[j].bodyTransform.forward;
                //Vector3 normalizedVelocity = teamPlayers.publicPlayerDatas[i].velocity;
                //normalizedVelocity.Normalize();
                float speed = defenseTeam.publicPlayerDatas[j].speed;
                searchPlayData.SetPlayerPosition(Snode, i, position, speed, forward);
            }
        }
            
        
        
    }
   
    public void UpdatePlayerPositions(List<int> nodes,int size,int startNode)
    {
        for (int i = 0; i < size; i++)
        {
            int node = nodes[i];
            
            int cullCount = searchPlayData.getCullEntityCount(node);
            for (int k = 0; k < cullCount; k++)
            {
                int SentityIndex = searchPlayData.getCullEntity(node, k);
                Entity Sentity = entities[SentityIndex];
                DynamicBuffer<PlayerPositionElement> PlayerPositionElements = entityManager.GetBuffer<PlayerPositionElement>(Sentity);
                //int playerCount = searchPlayData.GetPlayerCount(node);
                int playerCount = teamAttack_size+teamDefense_size;
                for (int j = 0; j < playerCount; j++)
                {
                    Vector2 playerPos = searchPlayData.GetPlayerPosition(node, j);
                    float speed = searchPlayData.GetPlayerSpeed(node, j);
                    Vector3 forward = searchPlayData.GetPlayerDirection(node, j);
                    Vector3 normalizedVelocity = forward * speed;
                    normalizedVelocity.Normalize();
                    PlayerPositionElement playerPositionElement = PlayerPositionElements[j];
                    playerPositionElement.position = playerPos;
                    playerPositionElement.bodyForward = new Vector2(forward.x, forward.z);
                    playerPositionElement.normalizedVelocity = new Vector2(normalizedVelocity.x, normalizedVelocity.z);
                    playerPositionElement.currentSpeed = speed;
                    PlayerPositionElements[j] = playerPositionElement;
                }
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
        int entityIndex = nodeIndex;
        int lonelyPointCount = 0;
        Entity entity = entities[entityIndex];
        CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
        DynamicBuffer<LonelyPointElement2> lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entity);
        CullPassPointsComponent.node = nodeIndex;
        for (int i = 0; i < bufferSizeComponent.lonelyPointsResultSize; i++)
        {

            LonelyPointElement2 lonelyPointElement2 = new LonelyPointElement2(lonelyPointElements[i]);
            
            lonelyPointElements2[lonelyPointCount] = lonelyPointElement2;
            lonelyPointCount++;
            if (lonelyPointCount>=cullPassPointsParams.entityPointSize)
            {
                searchPlayData.SetCullEntity(nodeIndex,entityIndex);
                CullPassPointsComponent.sizeLonelyPoints = lonelyPointCount;
                entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
                entityIndex++;
                entity = entities[entityIndex];
                lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entity);
                lonelyPointCount = 0;
                
            }
            
        }
        if (lonelyPointCount > 0)
        {
            searchPlayData.SetCullEntity(nodeIndex, entityIndex);
            entity = entities[entityIndex];
            CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
            CullPassPointsComponent.sizeLonelyPoints = lonelyPointCount;
            CullPassPointsComponent.node = nodeIndex;
        }
        entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
        entityManager.SetEnabled(entity, lonelyPointCount > 0);
    }
    public void PlacePoints2(List<int> nodes,int sizeNode,int startNode)
    {
        //searchPlayData.ResetNextCullEntity();
        for (int i = 0; i < sizeNode; i++)
        {
            int node = nodes[i];
            NativeArray<Point> points = searchPlayData.GetLonelyPoints(node);
            int lonelyCount = searchPlayData.GetLonelyPointsCount(node);
            int lonelyPointCount = 0;
            int entityIndex = searchPlayData.getNextCullEntity();
            Entity entity = entities[entityIndex];
            
            CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
            DynamicBuffer<LonelyPointElement2> lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entity);
            CullPassPointsComponent.node = node;


            for (int k = 0; k < lonelyCount; k++)
            {
                LonelyPointElement2 lonelyPointElement2 = new LonelyPointElement2(points[k], k);

                lonelyPointElements2[lonelyPointCount] = lonelyPointElement2;
                lonelyPointCount++;
                if (lonelyPointCount >= cullPassPointsParams.entityPointSize)
                {
                    searchPlayData.SetCullEntity(node, entityIndex);
                    CullPassPointsComponent.sizeLonelyPoints = lonelyPointCount;
                    entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
                    entityIndex++;
                    entity = entities[entityIndex];
                    lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entity);
                    lonelyPointCount = 0;
                }
            }
            if (lonelyPointCount > 0)
            {
                searchPlayData.SetCullEntity(node, entityIndex);
                entity = entities[entityIndex];
                CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
                CullPassPointsComponent.sizeLonelyPoints = lonelyPointCount;
                CullPassPointsComponent.node = node;
                entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
            }  
            entityManager.SetEnabled(entity, lonelyPointCount>0);
        }  
    }
    public void getDebugWeightPoints(List<int> Snodes)
    {
        if (Snodes.Contains(debugNode))
        {
            debugWeightLonelyPooints.Clear();
            int node = debugNode;
            int entityCount = searchPlayData.getCullEntityCount(node);
            for (int i = 0; i < entityCount; i++)
            {
                int entityIndex = searchPlayData.getCullEntity(node, i);
                Entity entity = entities[entityIndex];
                CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
                DynamicBuffer<LonelyPointElement2> lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entity);
                 for (int j = 0; j < CullPassPointsComponent.sizeLonelyPoints; j++)
                {
                    LonelyPointElement2 lonelyPointElement2 = lonelyPointElements2[j];
                    debugWeightLonelyPooints.Add(lonelyPointElement2);
                }
            }
        }
    }
    public void SetAllLonelyPointsCalculateNextPositionParameters(FieldPositionsData.HorizontalPositionType horizontalPositionType, Team team, List<int> Snodes, List<int> Fnodes, int nodeSizeTotal,int nodeSizePerNode,out int newNodesCount,int startNode,int nodeCalculationPerFrame, int totalNodeSize,int size2)
    {
        
        newNodesCount = 0;
        int size = Snodes.Count;
        for (int k = 0; k < size; k++)
        {
            int Snode = Snodes[0];
            Snodes.RemoveAt(0);
            int order = 0;
            bool block = false;
            int cullEntityCount = searchPlayData.getCullEntityCount(Snode);
            for (int l = 0; l < cullEntityCount; l++)
            {
                //if (order >= nodeSizePerNode) break;
                int entityIndex = searchPlayData.getCullEntity(Snode, l);
                Entity entity = entities[entityIndex];
                DynamicBuffer<LonelyPointElement2> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement2>(entity);
                CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
                for (int i = 0; i < CullPassPointsComponent.sizeLonelyPoints; i++)
                {
                    order = 0;
                    bool exit=false;
                    if (lonelyPointElements[i].weight == Mathf.Infinity && !block) continue;
                    if (searchPlayData.posibleNodes.Count >= cullPassPointsParams.maxPosibleLonelyPointsSize) return;
                    float minWeight = lonelyPointElements[i].weight;
                    for (int z = 0; z < cullEntityCount; z++)
                    {
                        int entityIndex2 = searchPlayData.getCullEntity(Snode, z);
                        Entity entity2 = entities[entityIndex2];
                        DynamicBuffer<LonelyPointElement2> lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entity2);
                        CullPassPointsComponent CullPassPointsComponent2 = entityManager.GetComponentData<CullPassPointsComponent>(entity2);
                        for (int j = 0; j < CullPassPointsComponent2.sizeLonelyPoints; j++)
                        {
                            if (order >= nodeSizePerNode)
                            {
                                exit = true;
                                break;
                            }
                            if ((z == l && i == j) || lonelyPointElements2[j].weight == Mathf.Infinity && !block) continue;
                            if (minWeight > lonelyPointElements2[j].weight)
                            {
                                //order = lonelyPointElements2[j].order;
                                order++;
                            }
                            else
                            {
                                //LonelyPointElement2 lonelyPointElement = lonelyPointElements2[j];
                                //order = lonelyPointElement.order;
                                //lonelyPointElement.order++;
                                //lonelyPointElements2[j] = lonelyPointElement;

                            }
                        }
                        if (exit) break;
                    }
                  
                    if (order < nodeSizePerNode)
                    {

                        LonelyPointElement2 lonelyPointElement = lonelyPointElements[i];
                        lonelyPointElement.order = order;
                        lonelyPointElements[i] = lonelyPointElement;
                        int FNode = searchPlayData.getNextFreeNode();
                        //posibleLonelyPoints[k][order] = lonelyPointElement;
                        SetCalculateNextPositionParameters(FNode, ref lonelyPointElement, horizontalPositionType, team);
                        //posibleLonelyPoints[calculationIndex][order] = lonelyPointElement;
                        searchPlayData.SetPosibleSortLonelyPoint(FNode, lonelyPointElement);
                        searchPlayData.AddPosibleNode(FNode);
                        searchPlayData.SetPreviousNode(FNode, Snode);
                        searchPlayData.AddNextNode(Snode, FNode);
                        order++;
                        newNodesCount++;
                        //posibleLonelyPointsSize[calculationIndex] = order;
                        if (order >= totalNodeSize)
                        {
                            //return;
                        }
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
    public void UpdateNextPlayerPoints(int nodeSize,FieldPositionsData.HorizontalPositionType horizontalPositionType, Team team,int nextPlayerPositionSize)
    {

        bool teamIsAttacker = teamName_Attacker.Equals(team.TeamName);
        for (int i = 0; i < nodeSize; i++)
        {
            int node = searchPlayData.posibleNodes[i];
            CalculateNextPositionComponents2 CalculateNextPositionComponents = searchPlayData.GetCalculateNextPositionComponents(node);
            NextPositionData2 nextPositionData = CalculateNextPositionComponents.normalNextPosition;
            LonelyPointElement2 lonelyPoint = searchPlayData.GetPosibleSortLonelyPoint(node);
            
            int k = teamIsAttacker ? teamAttack_start : teamDefense_start;
            //clearAuxNextPositionPublicPlayerDatas();
            
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
                if (nextPosition != Vector3.positiveInfinity)
                {
                    SetLonelyPosition2(node, k, nextPosition, endSpeed1, endDirection1);
                    k++;
                }
                if (nextPosition2 != Vector3.positiveInfinity)
                {
                    SetLonelyPosition2(node, k, nextPosition2, endSpeed2, endDirection2);
                    k++;
                }
            }
            PublicPlayerData goalkeeperPublicPlayerData = team.getGoalkeeperPublicPlayerData();
            if (goalkeeperPublicPlayerData != null)
            {
                Vector3 goalkeeperPosition = goalkeeperPublicPlayerData.bodyTransform.position;
                //Vector3 nextPosition2 = getCloseNextPosition(team, ref lonelyPoint, goalkeeperPosition, calculationIndex);
                SetLonelyPosition2(node, k, goalkeeperPosition,0, goalkeeperPublicPlayerData.bodyTransform.forward);
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
        if (publicPlayerData == null){
            endSpeed = 0;
            endDirection = Vector3.zero;
            return Vector3.positiveInfinity;
        }
        Transform playerTransform = publicPlayerData.bodyTransform;
        MovimentValues movimentValues = publicPlayerData.movimentValues;
        Vector3 ballPosition = new Vector3(lonelyPoint.position.x,0, lonelyPoint.position.y);
        
        Vector3 nextPosition = GetTimeToReachPointDOTS.accelerationGetPosition(playerTransform.position, publicPlayerData.speed, playerTransform.forward, movimentValues.rotationSpeed, publicPlayerData.movimentValues.minSpeedForRotateBody, movimentValues.forwardAcceleration, movimentValues.forwardDeceleration, movimentValues.maxAngleForRun, publicPlayerData.playerComponents.scope, optimalDefensePosition, publicPlayerData.maxSpeed, lonelyPoint.ballReachTime, out endSpeed,out  endDirection);
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
            Vector3 nextPosition = GetTimeToReachPointDOTS.accelerationGetPosition(playerTransform.position, publicPlayerData.speed, playerTransform.forward, movimentValues.rotationSpeed, publicPlayerData.movimentValues.minSpeedForRotateBody, movimentValues.forwardAcceleration, movimentValues.forwardDeceleration, movimentValues.maxAngleForRun, publicPlayerData.playerComponents.scope, optimalDefensePosition, publicPlayerData.maxSpeed, lonelyPoint.ballReachTime, out float endSpeed, out Vector3 endDirection);

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
    void SetCalculateNextPositionParameters(int node,ref LonelyPointElement2 lonelyPointElement, FieldPositionsData.HorizontalPositionType horizontalPositionType, Team team)
    {
        PressureFieldPositionDatas PressureFieldPositionDatas;
        if (!FootballPositionCtrl.getCurrentPressureFieldPositions(out PressureFieldPositionDatas)) return;

        Vector3 ballPosition = new Vector3(lonelyPointElement.position.x, 0, lonelyPointElement.position.y);
        Vector2 normalBallPosition = FootballPositionCtrl.getNormalizedPosition(horizontalPositionType, ballPosition, team.SideOfField);
        float offsideWeight;
        float offsideLineValueY = FootballPositionCtrl.GetOffsideLineGetValue(PressureFieldPositionDatas, normalBallPosition, out offsideWeight);
        //calculateNextPositionShedule.SetCalculateNextPositionParameters(index, normalBallPosition, offsideLineValueY, offsideWeight);
        searchPlayData.SetCalculateNextPositionParameters(node,normalBallPosition, offsideLineValueY, offsideWeight);

    }
    void SetLonelyPosition(ref DynamicBuffer<PointElement> points,int index,Vector3 position)
    {
        PointElement pointElement = points[index];
        pointElement.index = index;
        pointElement.position.x = position.x;
        pointElement.position.y = position.z;
        points[index] = pointElement;
    }
    void SetLonelyPosition2(int node,int index, Vector3 position,float endSpeed,Vector3 direction)
    {
        searchPlayData.SetPlayerPosition(node, index, position,endSpeed,direction);
    }
    public int GetPosibleLonelyPoints(int calculationIndex)
    {
        return posibleLonelyPointsSize[calculationIndex];
    }
    public void CompleteTriangulatorJob(int size)
    {
        searchPlayData.UpdatePoints(size);
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && debug)
        {
            if (debugPointResults)
            {

                if (searchPlayData.searchPlayNodes.Count > 0)
                {

                    int node = 0;
                    List<int> nodes = new List<int>();
                    nodes.Add(node);
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        int nextNode = nodes[i];
                        nodes.AddRange(searchPlayData.GetNextNodes(nextNode));
                        LonelyPointElement2 lonelyPointElement = searchPlayData.GetPosibleSortLonelyPoint(nextNode);
                        int previousNode = searchPlayData.GetPreviousNode(nextNode);
                        LonelyPointElement2 previousLonelyPoint = searchPlayData.GetPosibleSortLonelyPoint(previousNode);
                        DrawLonelyPoint(lonelyPointElement, nextNode, 0,"",Color.white);
                        Vector3 pos3 = new Vector3(lonelyPointElement.position.x, 1, lonelyPointElement.position.y);
                        Vector3 pos4 = new Vector3(previousLonelyPoint.position.x, 1, previousLonelyPoint.position.y);
                        DrawArrow.ForDebug(pos4, pos3 - pos4, 0.5f);
                    }
                }
            }
            if (_debugNode)
            {
                int node = 0;
                List<int> nodes = new List<int>();
                nodes.Add(node);
                for (int i = 0; i < nodes.Count; i++)
                {
                    int nextNode = nodes[i];
                    nodes.AddRange(searchPlayData.GetNextNodes(nextNode));

                    
                    
                    if (nextNode== debugNode)
                    {
                        LonelyPointElement2 lonelyPointElement = searchPlayData.GetPosibleSortLonelyPoint(nextNode);
                        DrawLonelyPoint(lonelyPointElement, nextNode, 0, "Node", new Color(0.5f,0.75f,0.25f));
                    }
                }
            }
            if (_debugAllLonelyPointsOfNode)
            {

                for (int i = 0; i < debugWeightLonelyPooints.Count; i++)
                {
                    if(!debugReachableLonelyPoints)
                    DrawLonelyPoint(debugWeightLonelyPooints[i], debugNode, i, "", Color.white);
                    else if(debugWeightLonelyPooints[i].weight!=Mathf.Infinity)
                        DrawLonelyPoint(debugWeightLonelyPooints[i], debugNode, i, "", Color.white);
                }
            }
            if (_debugLonelyPointIndex)
            {
                for (int i = 0; i < debugWeightLonelyPooints.Count; i++)
                {
                    if(debugLonelyPointIndex== debugWeightLonelyPooints[i].index)
                    DrawLonelyPoint(debugWeightLonelyPooints[i], debugNode, i, "Lonely Point", new Color(0.6f, 0.9f, 0.75f));
                }
            }
            if (debugPlayerIndex)
            {

                Team defenseTeam = Teams.getTeamByName(teamName_Defense);
                Team attackTeam = Teams.getTeamByName(teamName_Attacker);
                DebugPlayerIndex(defenseTeam, attackTeam);
            }
        }
    }
    public void DebugPlayerIndex(Team defenseTeam, Team attackTeam)
    {
        for (int i = teamAttack_start, j = 0; i < teamAttack_start + teamAttack_size; i++, j++)
        {

            Vector3 position = attackTeam.publicPlayerDatas[j].position;
            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.normal.textColor = Color.cyan;
            Handles.Label(position + Vector3.up * 1.25f, "player index=" + i, style);

        }
        for (int i = teamDefense_start, j = 0; i < teamDefense_start + teamDefense_size; i++, j++)
        {

            Vector3 position = defenseTeam.publicPlayerDatas[j].position;
            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.normal.textColor = Color.white;
            Handles.Label(position + Vector3.up * 1.25f, "player index=" + i, style);
        }

    }
    void DrawLonelyPoint(LonelyPointElement2 lonelyPointElement, int node, int index,string info,Color infoColor)
    {
        Vector3 pos = new Vector3(lonelyPointElement.position.x, 0, lonelyPointElement.position.y);
        Color color;
        if (lonelyPointElement.order == 0)
        {
            color = Color.cyan;
        }
        else if (lonelyPointElement.straightReachBall && lonelyPointElement.parabolicReachBall)
        {
            color = Color.green;
        }
        else if (lonelyPointElement.straightReachBall && !lonelyPointElement.parabolicReachBall)
        {
            color = Color.blue;
        }
        else if (!lonelyPointElement.straightReachBall && lonelyPointElement.parabolicReachBall)
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
        style.fontSize = 16;
        style.normal.textColor = infoColor;
        Handles.Label(pos + Vector3.up * 1.7f, info, style);
        style.fontSize = 14;
        style.normal.textColor = color;
        //string text = "ballReachPosTime=" + TestResultComponent.ballReachTargetPositionTime + " defenseIndex=" + TestResultComponent.defenseLonelyPointReachIndex + " defenseReachLonelyPosTime=" + TestResultComponent.defenseLonelyPointReachTime + " closestDistanceDefenseBall=" + TestResultComponent.closestDistanceDefenseBall;
        //string text = "straightReachBall=" + lonelyPointElement.straightReachBall + " parabolicReachBall=" + lonelyPointElement.parabolicReachBall + " i="+lonelyPointElement.index;
        string text = "i=" + lonelyPointElement.index;
        //string text = "ballReachPosTime=" + TestResultComponent.ballReachTargetPositionTime + " maximumControlSpeedReached=" + TestResultComponent.GetV0DOTSResult1.maximumControlSpeedReached + " maxKickForceReached=" + TestResultComponent.GetV0DOTSResult1.maxKickForceReached + " parabolicReachBall=" + TestResultComponent.parabolicReachBall + " straightReachBall=" + TestResultComponent.straightReachBall;
     

        Handles.Label(pos + Vector3.up * 0.5f, text, style);
        Color c = Color.Lerp(Color.green, Color.red, lonelyPointElement.weight);
        style.normal.textColor = c;
        float value = lonelyPointElement.weight * 100;
        text = "weight=" + value.ToString("f2") + " order=" + lonelyPointElement.order + " node=" + node + " index=" + lonelyPointElement.index + " Pos=" + lonelyPointElement.position.ToString("f2");
        if (debugText)
            Handles.Label(pos + Vector3.up * 1.25f, text, style);

    }
#endif
}
