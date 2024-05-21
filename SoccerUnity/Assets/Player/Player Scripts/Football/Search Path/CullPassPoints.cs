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
    public List<Transform> testLonelyPoints;
    public bool test,debug,debugPointResults,debugPassTest;
    public int lonelyPointIndexPassTest;
    public List<Entity> entities = new List<Entity>();
    EntityManager entityManager;
    List<PublicPlayerData> players = new List<PublicPlayerData>();
    public float v0y = 5;
    public float y = 2;
    public int batchesPerChunk = 1;
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
    private void Update()
    {
        TestDebug();
        Vector3 v = new Vector3(MatchComponents.ballRigidbody.velocity.x, 0, MatchComponents.ballRigidbody.velocity.z);
        //print("velocity=" + MatchComponents.ballRigidbody.velocity.magnitude + " " + v.magnitude);
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
                        Vector3 pos = new Vector3(lonelyPointElements[i].position.x, 0, lonelyPointElements[i].position.y);
                        Color color;
                        if (lonelyPointElement.straightReachBall && lonelyPointElement.parabolicReachBall)
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
                        string text = "straightReachBall=" + lonelyPointElement.straightReachBall + " parabolicReachBall=" + lonelyPointElement.parabolicReachBall + " i="+lonelyPointElement.index;
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

        }
    }
#endif
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
        DynamicBuffer<LonelyPointElement2> lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entity);
        //lonelyPointElements2.Clear();
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
        //lonelyPointElements.Clear();
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
    public void SortAllLonelyPoints()
    {

        for (int k = 0; k < entities.Count; k++)
        {
            DynamicBuffer<LonelyPointElement2> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement2>(entities[k]);
            for (int i = 0; i < lonelyPointElements.Length; i++)
            {
                if (lonelyPointElements[i].weight == Mathf.Infinity) continue;
                float minWeight = lonelyPointElements[i].weight;
                int order = 0;
                for (int z = 0; z < entities.Count; z++)
                {
                        DynamicBuffer<LonelyPointElement2> lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entities[z]);
                
                
                        for (int j = 0; j < lonelyPointElements2.Length; j++)
                        {
                            if ((z==k && i == j) || lonelyPointElements2[j].weight == Mathf.Infinity) continue;
                            if (minWeight > lonelyPointElements2[j].weight)
                            {
                                //order = lonelyPointElements2[j].order;
                                order++;
                                minWeight = lonelyPointElements2[j].weight;
                            }
                            else
                            {
                                LonelyPointElement2 lonelyPointElement = lonelyPointElements2[j];
                                //order = lonelyPointElement.order;
                                //lonelyPointElement.order=order+1;
                                lonelyPointElements2[j] = lonelyPointElement;
                                
                            }
                        }
                    }

                LonelyPointElement2 lonelyPointElement2 = lonelyPointElements[i];
                lonelyPointElement2.order = order;
                lonelyPointElements[i] = lonelyPointElement2;
            }
            
        }
    }
    void SortLonelyPoints(ref DynamicBuffer<LonelyPointElement2> lonelyPointElements,int index,float minWeight)
    {

    }
}
