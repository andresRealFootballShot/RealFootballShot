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
using static UnityEngine.Rendering.DebugUI;
using System.Xml.Linq;
using static Photon.Pun.UtilityScripts.PunTeams;
using System.Reflection;


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
    public bool startCullPassPointsSystem;
    public CullPassPointsParams cullPassPointsParams;
    public SearchLonelyPointsManager SearchLonelyPointsManager;
    public string teamName_Defense = "Red";
    public string teamName_Attacker = "Blue";
    public List<Transform> testLonelyPoints;
    public ShotResult bestShot;
    public Team defenseTeam{ get; set; }
    public Team attackTeam { get; set; }

    public List<int> searchNodeDebugList;
    public List<Entity> entities = new List<Entity>();
    public List<int> posibleLonelyPointsSize = new List<int>();
    public List<bool> AuxNextPositionPlayerBusiesList = new List<bool>();
    public EntityManager entityManager;
    [HideInInspector]
    public List<PublicPlayerData> players = new List<PublicPlayerData>();
    public float v0y = 5;
    public float y = 2;
    public int batchesPerChunk = 1;
    public FootballPositionCtrl FootballPositionCtrl;
    public CalculateNextPositionShedule calculateNextPositionShedule;
    public TriangulatorJob triangulatorJob;
    public List<int> sortLonelyPointsSize;
    public List<LonelyPointElement2> firstReachLonelyPoints = new List<LonelyPointElement2>();
    public string lineupName="Default", pressureName = "Default";
    public PublicPlayerData publicPlayerData;
    public float testTime = 1;
    public SearchPlayData searchPlayData;
    float fieldOffset = 2;
    CullPassPointsSystem cullPassPointsSystem;
    ShotSystem ShotSystem;
    [HideInInspector]
    public int teamA_size, teamB_size,teamAttack_start,teamDefense_start,teamAttack_size,teamDefense_size;
    bool teamA_isAttacker;
    
    public int maxNodes { get; set; } = 0;
    public int maxNodes2 { get; set; } = 0;
    public bool debugTestLonelyPoints { get => CullPassPointsDebug.debugTestLonelyPoints; }
    public BallInterceptionSystem ballInterceptionSystem;
    public CullPassPointsDebug CullPassPointsDebug;
    public Brains teamBrains;

    public Vector3 ballReachPosition { get; set; }
    public PublicPlayerData firstPublicPlayerData { get; set; }
    public float firstPlayerReachTime { get; set; }
    private void Awake()
    {
        MatchComponents.CullPassPoints = this;
    }
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        cullPassPointsSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CullPassPointsSystem>();
        ShotSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ShotSystem>();
        ShotSystem.CullPassPoints = this;
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
        defenseTeam = Teams.getTeamByName(teamName_Defense);
        attackTeam = Teams.getTeamByName(teamName_Attacker);
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
                PlayerPositionElements.Insert(index, new PlayerPositionElement(Vector2.zero,Vector2.zero, Vector2.zero, 0,0));
                if (!aux)
                    players.Insert(index, playerAddedToTeamEventArgs.publicPlayerData);
                CullPassPointsComponent.teamASize++;
                teamA_size = CullPassPointsComponent.teamASize;
                
            }
            else
            {
                int index = isGoalkeeper ? CullPassPointsComponent.teamASize : CullPassPointsComponent.teamASize + CullPassPointsComponent.teamBSize;
                PlayerPositionElements.Insert(index, new PlayerPositionElement(Vector2.zero, Vector2.zero, Vector2.zero, 0, 0));
                if (!aux)
                    players.Insert(index, playerAddedToTeamEventArgs.publicPlayerData);
                CullPassPointsComponent.teamBSize++;
                teamB_size = CullPassPointsComponent.teamBSize;
                
            }
            entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
            aux = true;
        }
        //SetTeamAttacker(teamName_Attacker);
    }
    void SetPasserPlayer(int passerIndex)
    {
        foreach (var entity in entities)
        {
            CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
            CullPassPointsComponent.passerIndex = passerIndex;
            entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
        }
        
    }
    void SetTeamAttacker(string attackTeamName)
    {
        teamName_Attacker = attackTeamName;
        teamName_Defense = Teams.getRivalTeam(attackTeamName).TeamName;
        attackTeam = Teams.getTeamByName(teamName_Attacker);
        defenseTeam = Teams.getTeamByName(teamName_Defense);
        foreach (var entity in entities)
        {
            CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
            CullPassPointsComponent.teamA_IsAttacker = attackTeamName.Equals("Red");
            entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
        }
        teamA_isAttacker = attackTeamName.Equals("Red");
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
        setFootballFieldParameters();
    }
    public PublicPlayerData GetPublicPlayerData(int index)
    {
        if(index< players.Count&&index>=0)return players[index];
        return null;
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
            
            Team defenseTeam = Teams.getTeamByName(teamName_Defense);
            Vector3 pos1 = defenseTeam.SideOfField.goalComponents.left.position;
            Vector2 post1 = new Vector2(pos1.x, pos1.z);
            Vector3 pos2 = defenseTeam.SideOfField.goalComponents.right.position;
            Vector2 post2 = new Vector2(pos2.x, pos2.z);
            CullPassPointsComponent.post1Position = post1;
            CullPassPointsComponent.post2Position = post2;
            CullPassPointsComponent.distanceWeightLerp = MatchComponents.footballField.fieldLenght;
            CullPassPointsComponent.midfield = new Vector2(MatchComponents.footballField.center.x, MatchComponents.footballField.center.z);
            Vector3 goalCenter = defenseTeam.SideOfField.goalComponents.centerOptimalPosition.position;
            CullPassPointsComponent.defenseGoalPosition = new Vector2(goalCenter.x,goalCenter.z);
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
            BallParamsComponent.ballVelocity= MatchComponents.ballRigidbody.velocity;
            //searchPlayData.getSortedNodes(ref cullPassPointsSystem.Snodes, 0);
            searchPlayData.SetBallPosition(0, MatchComponents.ballRigidbody.position);
            entityManager.SetComponentData<BallParamsComponent>(entity, BallParamsComponent);
        }
    }
    public void SetBallPosition(List<int> nodes, int sizeNode,Vector3 ballPosition,float t0)
    {
        for (int i = 0; i < sizeNode; i++)
        {

            int node = nodes[i];
            int cullCount = searchPlayData.getCullEntityCount(node);
            searchPlayData.SetBallPosition(node, ballPosition);
            for (int j = 0; j < cullCount; j++)
            {
                int entityIndex = searchPlayData.getCullEntity(node, j);
                Entity entity = entities[entityIndex];

                BallParamsComponent BallParamsComponent = entityManager.GetComponentData<BallParamsComponent>(entity);
                BallParamsComponent.BallPosition = ballPosition;
                BallParamsComponent.t0 = t0;
                BallParamsComponent.ballVelocity = MatchComponents.ballRigidbody.velocity;
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
                BallParamsComponent.ballVelocity = MatchComponents.ballRigidbody.velocity;
                entityManager.SetComponentData<BallParamsComponent>(entity, BallParamsComponent);
            }
            
        }
    }
    public void PlaceTestLonelyPoint()
    {
        Entity searchLonelyPointsEntity = SearchLonelyPointsManager.teamsSearchLonelyPointsEntitys[teamName_Defense];
        BufferSizeComponent bufferSizeComponent = entityManager.GetComponentData<BufferSizeComponent>(searchLonelyPointsEntity);
        DynamicBuffer<LonelyPointElement> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement>(searchLonelyPointsEntity);
        int nodeIndex = 0;
        int entityIndex = nodeIndex;
        int lonelyPointCount = 0;
        Entity entity = entities[entityIndex];
        CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
        DynamicBuffer<LonelyPointElement2> lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entity);
        CullPassPointsComponent.node = nodeIndex;
        for (int i = 0; i < testLonelyPoints.Count; i++)
        {
            Vector3 ballPosition = MatchComponents.ballPosition;
            Vector3 ballReachPosition2 = new Vector3(ballPosition.x, 0, ballPosition.z);
            float distance = Vector3.Distance(testLonelyPoints[i].position, ballReachPosition2);
            if (distance < 2) continue;
            LonelyPointElement2 lonelyPointElement2 = new LonelyPointElement2(testLonelyPoints[i].position, i);
            
            
            lonelyPointElements2[lonelyPointCount] = lonelyPointElement2;
            lonelyPointCount++;
            if (lonelyPointCount >= cullPassPointsParams.entityPointSize)
            {
                searchPlayData.SetCullEntity(nodeIndex, entityIndex);
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

                if (attackTeam.publicPlayerDatas[j].Equals(firstPublicPlayerData)) continue;
                Vector3 position = attackTeam.publicPlayerDatas[j].position;
                Vector3 forward = attackTeam.publicPlayerDatas[j].bodyTransform.forward;
                //Vector3 normalizedVelocity = teamPlayers.publicPlayerDatas[i].velocity;
                //normalizedVelocity.Normalize();
                float speed = attackTeam.publicPlayerDatas[j].speed;
                searchPlayData.SetPlayerPosition(Snode, i, position, speed, forward);
            }
            for (int i = teamDefense_start, j = 0; i < teamDefense_start + teamDefense_size; i++, j++)
            {
                if (defenseTeam.publicPlayerDatas[j].Equals(firstPublicPlayerData)) continue;
                Vector3 position = defenseTeam.publicPlayerDatas[j].position;
                Vector3 forward = defenseTeam.publicPlayerDatas[j].bodyTransform.forward;
                //Vector3 normalizedVelocity = teamPlayers.publicPlayerDatas[i].velocity;
                //normalizedVelocity.Normalize();
                float speed = defenseTeam.publicPlayerDatas[j].speed;
                searchPlayData.SetPlayerPosition(Snode, i, position, speed, forward);
            }
        }
    }
    public void UpdateInstantPlayerPositions2(Team defenseTeam, Team attackTeam, List<int> Snodes)
    {
        for (int k = 0; k < searchPlayData.searchPlayNodes.Count; k++)
        {
            int Snode = k;
            for (int i = 0; i < players.Count; i++)
            {
                PublicPlayerData publicPlayerData = players[i];
                if (publicPlayerData.Equals(firstPublicPlayerData)) continue;
                Vector3 position = publicPlayerData.position;
                Vector3 forward = publicPlayerData.bodyTransform.forward;
                //Vector3 normalizedVelocity = teamPlayers.publicPlayerDatas[i].velocity;
                //normalizedVelocity.Normalize();
                float speed = publicPlayerData.speed;
                searchPlayData.SetPlayerPosition(Snode, i, position, speed, forward);
            }
        }
    }

    public void UpdateOffsideLine(Vector3 ballPosition,string teamName, List<int> Snodes)
    {
        float offsideLine = FootballPositionCtrl.GetLastPlayerPosition(ballPosition, teamName).y;
        for (int i = 0; i < Snodes.Count; i++)
        {
            int node = Snodes[i];
            int cullCount = searchPlayData.getCullEntityCount(node);
            for (int j = 0; j < cullCount; j++)
            {
                int entityIndex = searchPlayData.getCullEntity(node, j);
                Entity entity = entities[entityIndex];

                CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
                CullPassPointsComponent.defenseTargetOffside = offsideLine;
                entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
            }

        }
    }
    public void UpdatePlayerPosition(Team team,int index,List<int> Snodes, Vector3 position,float speed,Vector3 forward)
    {
        for (int k = 0; k < Snodes.Count; k++)
        {
            int Snode = Snodes[k];
            searchPlayData.SetPlayerPosition(Snode, index, position, speed, forward);
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
                    PublicPlayerData publicPlayerData = GetPublicPlayerData(j);
                    playerPositionElement.maxSpeedForReachBall = publicPlayerData.team.Equals(defenseTeam) ? publicPlayerData.playerComponents.movementValues.maxSpeed.Value: publicPlayerData.movimentValues.maxSpeedForReachBall;
                    playerPositionElement.scope = publicPlayerData.playerComponents.ballScope;
                    playerPositionElement.maxAngleForRun = publicPlayerData.playerComponents.movementValues.maxAngleForRun;
                    playerPositionElement.minSpeedForRotate = publicPlayerData.playerComponents.movementValues.minSpeedForRotateBody;
                    playerPositionElement.minSpeedForRotate2 = publicPlayerData.playerComponents.movementValues.minSpeedForRotateBody2;
                    playerPositionElement.maxAngleForRun2 = publicPlayerData.playerComponents.movementValues.maxAngleForRun2;
                    playerPositionElement.acceleration = publicPlayerData.playerComponents.movementValues.forwardAcceleration;
                    playerPositionElement.decceleration = publicPlayerData.playerComponents.movementValues.forwardDeceleration;
                    playerPositionElement.maxSpeedRotation = publicPlayerData.playerComponents.movementValues.rotationSpeed;
                    playerPositionElement.maxSpeed = publicPlayerData.playerComponents.movementValues.maxSpeed.Value;
                    PlayerPositionElements[j] = playerPositionElement;
                }
            }
            
        }
    }
    
    public void setAttackTargetPosition(TestResultComponent TestResultComponent, GetV0DOTSResult GetV0DOTSResult)
    {
        PublicPlayerData publicPlayerData = players[TestResultComponent.attackLonelyPointReachIndex];
        Transform attackTransform = publicPlayerData.bodyTransform;

        Vector3 attackPosition = attackTransform.position;
        Vector3 dir = TestResultComponent.lonelyPosition - attackPosition;
        publicPlayerData.playerComponents.TargetPosition = TestResultComponent.lonelyPosition;
        publicPlayerData.playerComponents.ForwardDesiredSpeed = publicPlayerData.maxSpeed;
        publicPlayerData.playerComponents.DesiredLookDirection = dir;
    }
    public void setDefenseTargetPosition(TestResultComponent TestResultComponent, GetV0DOTSResult GetV0DOTSResult)
    {
        PublicPlayerData publicPlayerData = players[TestResultComponent.defenseLonelyPointReachIndex];
        Transform defenseTransform = publicPlayerData.bodyTransform;

        Vector3 defensePosition = defenseTransform.position;
        Vector3 reachPosition = TestResultComponent.closestPosition;
        Vector3 dir = reachPosition - defensePosition;
        publicPlayerData.playerComponents.TargetPosition = reachPosition;
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
    public void CreatePlayerTargetPositions()
    {
        for (int i = 0; i < searchPlayData.searchPlayNodes.Count; i++)
        {
            if (!searchPlayData.searchPlayNodes[i].playerTargetPositions.IsCreated)
                searchPlayData.SetPlayerTargetPositions(i, new NativeArray<float2>(players.Count, Allocator.Persistent));
            else
            {
                if (searchPlayData.searchPlayNodes[i].playerTargetPositions.Length != players.Count)
                {
                    searchPlayData.searchPlayNodes[i].playerTargetPositions.Dispose();
                    searchPlayData.SetPlayerTargetPositions(i, new NativeArray<float2>(players.Count, Allocator.Persistent));
                }
            }
        }
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
            Vector3 ballPosition = MatchComponents.ballPosition;
            Vector3 ballReachPosition2 = new Vector3(ballPosition.x, 0, ballPosition.z);
            float distance = Vector3.Distance(lonelyPointElements[i].position, ballReachPosition2);
            if (distance < 2) continue;
            LonelyPointElement2 lonelyPointElement2 = new LonelyPointElement2(lonelyPointElements[i]);
            bool isDuplicated = false ;
            for (int j = i; j >= 0; j--)
            {
                if (i == j) continue;
                if(Vector2.Distance(lonelyPointElement2.position, lonelyPointElements[j].position) < 0.1f)
                {
                    isDuplicated = true;
                    break;
                }
                
            }
            if (isDuplicated) continue;
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
        int z = 0;
        foreach (PublicPlayerData publicPlayerData in attackTeam.publicPlayerDatas)
        {
            if (lonelyPointCount >= cullPassPointsParams.entityPointSize)
            {
                searchPlayData.SetCullEntity(nodeIndex, entityIndex);
                CullPassPointsComponent.sizeLonelyPoints = lonelyPointCount;
                entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
                entityIndex++;
                entity = entities[entityIndex];
                lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entity);
                lonelyPointCount = 0;

            }
            LonelyPointElement2 lonelyPointElement2 = new LonelyPointElement2(publicPlayerData.position,bufferSizeComponent.lonelyPointsResultSize+z);
            lonelyPointElements2[lonelyPointCount] = lonelyPointElement2;
            z++;
            lonelyPointCount++;

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
    public LonelyPointElement2 GetLonelyPointOfEntity(int node, int index)
    {
        int cullEntityCount = searchPlayData.getCullEntityCount(node);
        for (int i = 0; i < cullEntityCount; i++)
        {
            int entityIndex = searchPlayData.getCullEntity(node,i);
            Entity entity = entities[entityIndex];
            DynamicBuffer<LonelyPointElement2> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement2>(entity);
            CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
            for (int j = 0; j < CullPassPointsComponent.sizeLonelyPoints; j++)
            {
                if (lonelyPointElements[j].index == index)
                {
                    return lonelyPointElements[j];
                }
            }
        }
        return default;
    }
    public void SetAllLonelyPointsCalculateNextPositionParameters(FieldPositionsData.HorizontalPositionType horizontalPositionType, Team team, List<int> Snodes,int nodeSizeTotal,int nodeSizePerNode,out int newNodesCount,int startNode,int nodeCalculationPerFrame, int totalNodeSize,int size2)
    {
        
        newNodesCount = 0;
        int size = Snodes.Count;
        for (int k = 0; k < size; k++)
        {
            int Snode = Snodes[k];
            //Snodes.RemoveAt(0);
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
                    if (lonelyPointElements[i].weight < 0 && !block&&false) continue;
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
                            if ((z == l && i == j) || lonelyPointElements2[j].weight < 0 && !block&&false) continue;
                            if (minWeight < lonelyPointElements2[j].weight)
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
                        if (exit)
                        {
                            break;
                        }
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
                        if (Snode == 0)
                        {
                            if (order > firstReachLonelyPoints.Count)
                            {
                                firstReachLonelyPoints.Add(lonelyPointElement);
                            }
                            else
                            {
                                firstReachLonelyPoints.Insert(order, lonelyPointElement);
                            }
                        }
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
            LonelyPointElement2 lonelyPoint = searchPlayData.GetBallLonelyPoint(node);
            
            int k = teamIsAttacker ? teamAttack_start : teamDefense_start;
            //clearAuxNextPositionPublicPlayerDatas();
            
            for (int j = 0; j < nextPlayerPositionSize; j++)
            {
                
                Vector2 normalNextPosition = nextPositionData.NextPositionData.Get(j), normalNextPosition2 = nextPositionData.symetricNextPositionData.Get(j);
                
                Vector3 nextPositionTarget = FootballPositionCtrl.getGlobalPosition(horizontalPositionType, normalNextPosition, team.SideOfField);
                FieldPositionsData.HorizontalPositionType otherHorizontalPositionType = FootballPositionCtrl.getOtherHorizontalPositionType(horizontalPositionType);

                Vector3 nextPositionTarget2 = FootballPositionCtrl.getGlobalPosition(otherHorizontalPositionType, normalNextPosition2, team.SideOfField);
                //nextPosition = getCloseNextPosition(team, ref lonelyPoint, nextPosition);
                //nextPosition2 = getCloseNextPosition(team, ref lonelyPoint, nextPosition2);
                Vector3 nextPosition = getOrderNextPosition(team, ref lonelyPoint, nextPositionTarget, j, 0,out float endSpeed1,out Vector3 endDirection1,out PublicPlayerData publicPlayerData1);
                Vector3 nextPosition2 = getOrderNextPosition(team, ref lonelyPoint, nextPositionTarget2, j, 1, out float endSpeed2, out Vector3 endDirection2, out PublicPlayerData publicPlayerData2);
                if (nextPosition != Vector3.positiveInfinity)
                {
                    int index = players.IndexOf(publicPlayerData1);
                    SetPlayerPosition(node, index, nextPosition, endSpeed1, endDirection1);
                    searchPlayData.SetPlayerTargetPosition(i, index, new Vector2(nextPositionTarget.x, nextPositionTarget.z));
                }
                if (nextPosition2 != Vector3.positiveInfinity)
                {
                    int index = players.IndexOf(publicPlayerData2);
                    SetPlayerPosition(node, index, nextPosition2, endSpeed2, endDirection2);
                    searchPlayData.SetPlayerTargetPosition(i, index, new Vector2(nextPositionTarget2.x, nextPositionTarget2.z));
                }
            }
            PublicPlayerData goalkeeperPublicPlayerData = team.getGoalkeeperPublicPlayerData();
            if (goalkeeperPublicPlayerData != null)
            {
                Vector3 goalkeeperPosition = goalkeeperPublicPlayerData.bodyTransform.position;
                //Vector3 nextPosition2 = getCloseNextPosition(team, ref lonelyPoint, goalkeeperPosition, calculationIndex);
                SetPlayerPosition(node, k, goalkeeperPosition,0, goalkeeperPublicPlayerData.bodyTransform.forward);
                searchPlayData.SetPlayerTargetPosition(i, k, new Vector2(goalkeeperPosition.x, goalkeeperPosition.z));
            }
        }
     }
    Vector3 getOrderNextPosition(Team team, ref LonelyPointElement2 lonelyPoint, Vector3 optimalDefensePosition,int indexFieldPosition,int sideFieldPosition,out float endSpeed,out Vector3 endDirection,out PublicPlayerData publicPlayerData)
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
        team.getPublicPlayerData(typeFieldPositions, out publicPlayerData);
        if (publicPlayerData == null){
            endSpeed = 0;
            endDirection = Vector3.zero;
            return Vector3.positiveInfinity;
        }
        Transform playerTransform = publicPlayerData.bodyTransform;
        MovimentValues movimentValues = publicPlayerData.movimentValues;
        Vector3 ballPosition = new Vector3(lonelyPoint.position.x,0, lonelyPoint.position.y);


        


        GetTimeToReachPointDOTS.accelerationGetPositionAtTime(playerTransform.position, publicPlayerData.speed, playerTransform.forward, optimalDefensePosition, lonelyPoint.GetBallReachTime(), publicPlayerData.playerComponents.scope, movimentValues.maxAngleForRun, movimentValues.maxAngleForRun2, publicPlayerData.movimentValues.minSpeedForRotateBody, publicPlayerData.movimentValues.minSpeedForRotateBody2, publicPlayerData.movimentValues.maxSpeedForReachBall, movimentValues.forwardAcceleration, movimentValues.forwardDeceleration, publicPlayerData.maxSpeed,0,out Vector3 nextPosition,out endSpeed,out endDirection);
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



            Vector3 nextPosition = GetTimeToReachPointDOTS.accelerationGetPosition(playerTransform.position, publicPlayerData.speed, playerTransform.forward, movimentValues.rotationSpeed, publicPlayerData.movimentValues.minSpeedForRotateBody, movimentValues.forwardAcceleration, movimentValues.forwardDeceleration, movimentValues.maxAngleForRun, publicPlayerData.playerComponents.scope, optimalDefensePosition, publicPlayerData.maxSpeed, lonelyPoint.GetBallReachTime(), out float endSpeed, out Vector3 endDirection);

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
    public void CalculateNextPositions(int node, Vector3 ballPosition, FieldPositionsData.HorizontalPositionType horizontalPositionType, Team team)
    {
        SetCalculateNextPositionParameters(0, ballPosition, FieldPositionsData.HorizontalPositionType.Right, team);
        
        calculateNextPositionShedule.SheduleJobs(1, searchPlayData, team.teamMaxFieldPlayers / 2, lineupName, pressureName);
    }
    public void SetCalculateNextPositionParameters(int node, Vector3 ballPosition, FieldPositionsData.HorizontalPositionType horizontalPositionType, Team team)
    {
        PressureFieldPositionDatas PressureFieldPositionDatas;
        if (!FootballPositionCtrl.getCurrentPressureFieldPositions(out PressureFieldPositionDatas)) return;
        Vector2 normalBallPosition = FootballPositionCtrl.getNormalizedPosition(horizontalPositionType, ballPosition, team.SideOfField);
        float offsideWeight;
        float offsideLineValueY = FootballPositionCtrl.GetOffsideLineGetValue(PressureFieldPositionDatas, normalBallPosition, out offsideWeight);
        //calculateNextPositionShedule.SetCalculateNextPositionParameters(index, normalBallPosition, offsideLineValueY, offsideWeight);
        searchPlayData.SetCalculateNextPositionParameters(node, normalBallPosition, offsideLineValueY, offsideWeight);

    }
    void SetLonelyPosition(ref DynamicBuffer<PointElement> points,int index,Vector3 position)
    {
        PointElement pointElement = points[index];
        pointElement.index = index;
        pointElement.position.x = position.x;
        pointElement.position.y = position.z;
        points[index] = pointElement;
    }
    void SetPlayerPosition(int node,int index, Vector3 position,float endSpeed,Vector3 direction)
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
    public bool CalculateFirstReachPlayerToBall(List<int> Snodes)
    {
        ballInterceptionSystem.Calculate();
        ballInterceptionSystem.getFirstPlayerReachBall(out PublicPlayerData firstPublicPlayerData,out float playerReachTime, out Vector3 ballPosition,out float endSpeed, out Vector3 endPlayerDirection);
        if (firstPublicPlayerData == null) { 
            return false; 
        }
        this.firstPublicPlayerData = firstPublicPlayerData;
        ballReachPosition = ballPosition;
        this.firstPlayerReachTime = playerReachTime;
       
        foreach (int node in Snodes)
        {
            searchPlayData.searchPlayNodes[node].attackReachTime = playerReachTime;
            searchPlayData.searchPlayNodes[node].attackPublicPlayerData = firstPublicPlayerData;
            searchPlayData.searchPlayNodes[0].ballLonelyPoint.straightPassData.ballReachTime = playerReachTime;
            searchPlayData.searchPlayNodes[0].ballLonelyPoint.parabolicPassData.ballReachTime = playerReachTime;
        }
        Teams.getTeamFromPlayer(firstPublicPlayerData.playerID, out Team team);
        SetTeamAttacker(team.TeamName);
        int passerIndex = players.IndexOf(firstPublicPlayerData);
        SetPasserPlayer(passerIndex);
        int firstPlayerIndex = players.IndexOf(firstPublicPlayerData);
        if (firstPlayerIndex != -1)
        {
            UpdatePlayerPosition(team, firstPlayerIndex, Snodes, ballPosition, endSpeed, endPlayerDirection);
        }
        return true;
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
    public void getDebugWeightPoints(List<int> Snodes)
    {
        CullPassPointsDebug.getDebugWeightPoints(Snodes);
    }

}
