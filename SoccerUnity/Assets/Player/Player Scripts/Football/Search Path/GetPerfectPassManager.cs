using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using DOTS_ChaserDataCalculation;
using Unity.Collections;
using UnityEditor;

public class GetPerfectPassManager : MonoBehaviour, ILoad
{
    
    EntityManager entityManager;
    public static int _loadLevel = MatchEvents.staticLoadLevel + 1;
    public int loadLevel { get => _loadLevel; set => _loadLevel = value; }
    public List<PublicPlayerData> publicPlayerDatas { get; set; } = new List<PublicPlayerData>();
    public List<PublicPlayerData> defensePublicPlayerDatas { get; set; } = new List<PublicPlayerData>();
    public List<PublicPlayerData> publicPlayerAttacks2 { get; set; } = new List<PublicPlayerData>();
    public List<PublicPlayerData> testDefensePublicPlayerDatas;

    Entity GetTimeToReachPointEntity;
    Entity GetPassV0Entity;
    public Transform targetPosition;
    public PublicPlayerData publicPlayerTest;
    public List<PublicPlayerData> publicPlayerAttacks;
    public OptimalPointSystem OptimalPointSystem { get; set; } = new OptimalPointSystem();
    SearchPathSystem SearchPathSystem;
    
    
    public OptimalPointDOTSCreator OptimalPointDOTSCreator;
    public List<OptimalPointReference> OptimalPointReferences { get; set; } = new List<OptimalPointReference>();
    [Header("Version")]
    public bool v1 = true;
    [Header("Parameters")]
    public OptimalPointParams firstOptimalPointParams;
    [Space(10)]
    public int getPerfectPassOptimalPointSize = 10;
    public OptimalPointParams getPerfectPassOptimalPointParams;
    [Space(10)]
    public SegmentedPathParams segmentedPathParams;

    [Header("Debug")]
    public bool debug;
    public bool debugChaserData,debugSegmentedPaths, debugSegmentedPathsText,parabolicPass;

    public int batchesPerChunk = 1;
    public bool useOptimization;
    public bool useTest;
    public bool printText,onlyPlayersTest;
    public float rival_Thrower_OptimalTimeDifference;

    public Transform testPlane, testPlanePoint;
    public int testJobSize = 100;
    public Vector3 v0Test;
    public void Load(int level)
    {

        if (loadLevel == level)
        {
            Load();
        }
    }
    void Load()
    {
        if (enabled)
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            SearchPathSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SearchPathSystem>();
            SearchPathSystem.SearchPathManager = this;

           
            if (v1)
            {
                searchPathV1();
            }
            else
            {

                searchPathV2();
            }
        }
    }
    void TestJob()
    {
        MatchEvents.footballFieldLoaded.AddListenerConsiderInvoked(() => MatchEvents.ballPhysicsMaterialLoaded.AddListener(createTestJobs));

        
    }
    void createTestJobs()
    {
        for (int i = 0; i < testJobSize; i++)
        {
            //EntityArchetype archetype = entityManager.CreateArchetype(typeof(PathComponent),typeof(AreaPlaneElement));
            EntityArchetype archetype = entityManager.CreateArchetype(typeof(AreaPlaneElement));
            Entity testEntity = entityManager.CreateEntity(archetype);
            SegmentedPathCalculationData segmentedPathCalculationData = new SegmentedPathCalculationData(segmentedPathParams.timeRange, segmentedPathParams.timeIncrement, segmentedPathParams.startSegmentedTime, segmentedPathParams.minAngle, segmentedPathParams.minVelocity, segmentedPathParams.maxAngle, segmentedPathParams.maxVelocity, getPerfectPassOptimalPointParams.maxTime);
            Vector3 v0 = Vector3.zero;
            Vector3 pos0 = Vector3.zero;
            float drag = MatchComponents.ballComponents.rigBall.drag;
            float mass = MatchComponents.ballComponents.rigBall.mass;
            float g = 9.81f;
            float groundY = MatchComponents.ballComponents.sphereCollider.radius * MatchComponents.ballComponents.sphereCollider.transform.localScale.x;
            float radius = MatchComponents.ballComponents.radio;
            float friction = MatchComponents.ballComponents.friction;
            float dynamicFriction = MatchComponents.ballComponents.dynamicFriction;
            float bouciness = MatchComponents.ballComponents.bounciness;
            PathDataDOTS intPathDataDOTS = new PathDataDOTS(0, PathType.Parabolic, 0, pos0, Vector3.zero, v0, drag, mass, groundY, bouciness, friction, dynamicFriction, radius, g);
            PathComponent pathComponent = new PathComponent(intPathDataDOTS, segmentedPathCalculationData);
            //entityManager.SetComponentData(testEntity, pathComponent);
            OptimalPointDOTSCreator.createAreaBuffer2(testEntity);

        }
    }
    void searchPathV2()
    {
        

        
        GetTimeToReachPointEntity = entityManager.CreateEntity();
        entityManager.AddBuffer<GetTimeToReachPointElement>(GetTimeToReachPointEntity);
        entityManager.AddBuffer<PlayerAttackElement>(GetTimeToReachPointEntity);
        MatchEvents.footballFieldLoaded.AddListenerConsiderInvoked(() => MatchEvents.addedPublicPlayerDataToList.AddListenerConsiderInvoked(addPlayerToOptimalPointV2));
        
        SearchPathSystem.GetTimeToReachPointEntity = GetTimeToReachPointEntity;
        OptimalPointDOTSCreator.OptimalPointSystem = OptimalPointSystem;
        OptimalPointSystem.OnCreate();
        OptimalPointSystem.SearchPathManager = this;
        MatchEvents.ballPhysicsMaterialLoaded.AddListenerConsiderInvoked(() => createGetPassV0());
        MatchEvents.ballPhysicsMaterialLoaded.AddListener(createGerPerfectPassPathComponents);
    }
    void searchPathV1()
    {
        for (int i = 0; i < getPerfectPassOptimalPointSize; i++)
        {
            OptimalPointReference optimalPointReference = getOrCreateOptimalPointReference(i, getPerfectPassOptimalPointParams);
            EntityArchetype archetype = entityManager.CreateArchetype(typeof(PlayerAttackElement), typeof(GetPerfectPassComponent), typeof(PlayerDefenseElement), typeof(AreaPlaneElement), typeof(ChaserDataElement), typeof(PathComponent), typeof(SegmentedPathElement));
            optimalPointReference.GetPerfectPassEntity = entityManager.CreateEntity(archetype);
            //createChaserDataBuffer(optimalPointReference);
        }
        MatchEvents.footballFieldLoaded.AddListenerConsiderInvoked(() => MatchEvents.addedPublicPlayerDataToList.AddListenerConsiderInvoked(addPlayerToOptimalPointV1));
        MatchEvents.footballFieldLoaded.AddListenerConsiderInvoked(() => createFullAreaPlaneBuffer());
        MatchEvents.ballPhysicsMaterialLoaded.AddListenerConsiderInvoked(() => createGetPerfectPassComponent());
        MatchEvents.ballPhysicsMaterialLoaded.AddListener(createGerPerfectPassPathComponentsV1);

        
    }
    void createFullAreaPlaneBuffer()
    {
        for (int i = 0; i < getPerfectPassOptimalPointSize; i++)
        {
            OptimalPointReference optimalPointReference = getOrCreateOptimalPointReference(i, getPerfectPassOptimalPointParams);
            OptimalPointDOTSCreator.createAreaBuffer2(optimalPointReference.GetPerfectPassEntity);
        }
    }
    void createChaserDataBuffer(OptimalPointReference optimalPointReference)
    {
        DynamicBuffer<ChaserDataElement> playerDataComponentElements = entityManager.GetBuffer<ChaserDataElement>(optimalPointReference.GetPerfectPassEntity);
        NativeArray<ChaserDataElement> ChaserDataElements = new NativeArray<ChaserDataElement>(optimalPointReference.optimalPointParams.defenseSize, Allocator.Temp);
        playerDataComponentElements.AddRange(ChaserDataElements);
    }
    void createGetPerfectPassComponent()
    {
        for (int i = 0; i < getPerfectPassOptimalPointSize; i++)
        {
            OptimalPointReference optimalPointReference = getOrCreateOptimalPointReference(i, getPerfectPassOptimalPointParams);

            GetPerfectPassComponent GetPerfectPassComponent = new GetPerfectPassComponent();

            GetTimeToReachPointElement getTimeToReachPointElement = new GetTimeToReachPointElement();
            getTimeToReachPointElement.playerID = 0;
            getTimeToReachPointElement.targetPosition = targetPosition.position;
            GetPerfectPassComponent.getTimeToReachPoint = getTimeToReachPointElement;
            GetPerfectPassComponent.rival_Thrower_OptimalTimeDifference = 0.1f;
            GetPerfectPassComponent.partnerIsAheadMinTime = 0.4f;
            GetPassV0Element GetPassV0Element = new GetPassV0Element();

            GetPassV0Element.maxAttempts = 20;
            GetPassV0Element.maxControlSpeed = 15;
            GetPassV0Element.accuracy = 0.1f;
            GetPassV0Element.maxControlSpeedLerpDistance = 5f;
            GetPassV0Element.Pos0 = MyFunctions.setY0ToVector3(MatchComponents.ballPosition);
            GetPassV0Element.Posf = targetPosition.position;
            GetPassV0Element.k = MatchComponents.ballRigidbody.drag;
            GetPassV0Element.g = Physics.gravity.magnitude;
            GetPassV0Element.ballRadio = MatchComponents.ballRadio;
            GetPassV0Element.friction = MatchComponents.ballComponents.friction;
            GetPassV0Element.searchVyIncrement = 0.5f;
            GetPerfectPassComponent.straightGetPassV0 = GetPassV0Element;
            GetPerfectPassComponent.parabolicGetPassV0 = GetPassV0Element;

            GetPerfectPassComponent.defenseGoalAreaIndex = 1;
            GetPerfectPassComponent.useOptimization = useOptimization;
            GetPerfectPassComponent.useTest = useTest;
            entityManager.SetComponentData<GetPerfectPassComponent>(optimalPointReference.GetPerfectPassEntity, GetPerfectPassComponent);
            
        }
    }
    public void addPlayerToOptimalPointV1(PublicPlayerData publicPlayerData)
    {
        //if (OptimalPointDOTSCreator.publicPlayerDatas.Count >= OptimalPointDOTSCreator.teamSize || !publicPlayerData.addToOptimalPoint) return;
        
        for (int i = 0; i < getPerfectPassOptimalPointSize; i++)
        {
            OptimalPointReference optimalPointReference = getOrCreateOptimalPointReference(i, getPerfectPassOptimalPointParams);
            
            if (publicPlayerData.playerID.Equals(publicPlayerTest.playerID))
            {
                int j = publicPlayerDatas.Count;
                publicPlayerDatas.Add(publicPlayerData);
                PlayerDataComponent playerDataComponent;
                OptimalPointDOTSCreator.getPlayerData(j, publicPlayerDatas, out playerDataComponent);
                PlayerAttackElement playerDataComponentElement = new PlayerAttackElement();
                playerDataComponentElement.PlayerDataComponent = playerDataComponent;
                playerDataComponentElement.checkGetTimeToReachPosition = true;
                DynamicBuffer<PlayerAttackElement> playerDataComponentElements = entityManager.GetBuffer<PlayerAttackElement>(optimalPointReference.GetPerfectPassEntity);
                playerDataComponentElements.Add(playerDataComponentElement);
                DynamicBuffer<ChaserDataElement> ChaserDataElements = entityManager.GetBuffer<ChaserDataElement>(optimalPointReference.GetPerfectPassEntity);
                ChaserDataElements.Add(new ChaserDataElement());
                
                publicPlayerAttacks2.Add(publicPlayerData);
            }
            else if (publicPlayerAttacks.Contains(publicPlayerData))
            {
                int j = publicPlayerDatas.Count;
                publicPlayerDatas.Add(publicPlayerData);
                PlayerDataComponent playerDataComponent;
                OptimalPointDOTSCreator.getPlayerData(j, publicPlayerDatas, out playerDataComponent);
                PlayerAttackElement playerDataComponentElement = new PlayerAttackElement();
                playerDataComponentElement.PlayerDataComponent = playerDataComponent;
                playerDataComponentElement.checkGetTimeToReachPosition = false;
                DynamicBuffer<PlayerAttackElement> playerDataComponentElements = entityManager.GetBuffer<PlayerAttackElement>(optimalPointReference.GetPerfectPassEntity);
                playerDataComponentElements.Add(playerDataComponentElement);
                DynamicBuffer<ChaserDataElement> ChaserDataElements = entityManager.GetBuffer<ChaserDataElement>(optimalPointReference.GetPerfectPassEntity);
                ChaserDataElements.Add(new ChaserDataElement());
                publicPlayerAttacks2.Add(publicPlayerData);
            }
            else
            {
                if (onlyPlayersTest)
                {
                    if (testDefensePublicPlayerDatas.Contains(publicPlayerData))
                    {
                        DynamicBuffer<PlayerDefenseElement> playerDataComponentElements = entityManager.GetBuffer<PlayerDefenseElement>(optimalPointReference.GetPerfectPassEntity);
                        if (playerDataComponentElements.Length >= optimalPointReference.optimalPointParams.defenseSize) return;
                        //if (defensePublicPlayerDatas.Count >= optimalPointReference.optimalPointParams.defenseJobSize*(i+1)) continue;
                        int j = publicPlayerDatas.Count;
                        publicPlayerDatas.Add(publicPlayerData);
                        defensePublicPlayerDatas.Add(publicPlayerData);
                        PlayerDataComponent playerDataComponent;
                        OptimalPointDOTSCreator.getPlayerData(j, publicPlayerDatas, out playerDataComponent);
                        PlayerDefenseElement playerDataComponentElement = new PlayerDefenseElement();
                        playerDataComponentElement.PlayerDataComponent = playerDataComponent;

                        playerDataComponentElements.Add(playerDataComponentElement);
                        DynamicBuffer<ChaserDataElement> ChaserDataElements = entityManager.GetBuffer<ChaserDataElement>(optimalPointReference.GetPerfectPassEntity);
                        ChaserDataElements.Add(new ChaserDataElement());
                    }
                }
                else
                {
                    DynamicBuffer<PlayerDefenseElement> playerDataComponentElements = entityManager.GetBuffer<PlayerDefenseElement>(optimalPointReference.GetPerfectPassEntity);
                    if (playerDataComponentElements.Length >= optimalPointReference.optimalPointParams.defenseSize) return;
                    //if (defensePublicPlayerDatas.Count >= optimalPointReference.optimalPointParams.defenseJobSize*(i+1)) continue;
                    int j = publicPlayerDatas.Count;
                    publicPlayerDatas.Add(publicPlayerData);
                    defensePublicPlayerDatas.Add(publicPlayerData);
                    PlayerDataComponent playerDataComponent;
                    OptimalPointDOTSCreator.getPlayerData(j, publicPlayerDatas, out playerDataComponent);
                    PlayerDefenseElement playerDataComponentElement = new PlayerDefenseElement();
                    playerDataComponentElement.PlayerDataComponent = playerDataComponent;

                    playerDataComponentElements.Add(playerDataComponentElement);
                    DynamicBuffer<ChaserDataElement> ChaserDataElements = entityManager.GetBuffer<ChaserDataElement>(optimalPointReference.GetPerfectPassEntity);
                    ChaserDataElements.Add(new ChaserDataElement());
                }

               
            }
        }
    }
    void UpdateChaserPositions()
    {
        foreach (var OptimalPointReference in OptimalPointReferences)
        {
            DynamicBuffer<PlayerDefenseElement> playerDataComponentElements = entityManager.GetBuffer<PlayerDefenseElement>(OptimalPointReference.GetPerfectPassEntity);
            for (int i = 0; i < playerDataComponentElements.Length; i++)
            {
                PlayerDefenseElement playerDefenseElement = playerDataComponentElements[i];
                playerDefenseElement.PlayerDataComponent.position = defensePublicPlayerDatas[i].position;
                playerDataComponentElements[i] = playerDefenseElement;
            }
            

            GetPerfectPassComponent GetPerfectPassComponent = entityManager.GetComponentData<GetPerfectPassComponent>(OptimalPointReference.GetPerfectPassEntity);
            GetPerfectPassComponent.getTimeToReachPoint.targetPosition = targetPosition.position;
            GetPerfectPassComponent.straightGetPassV0.Posf = targetPosition.position;
            GetPerfectPassComponent.parabolicGetPassV0.Posf = targetPosition.position;
            GetPerfectPassComponent.rival_Thrower_OptimalTimeDifference = rival_Thrower_OptimalTimeDifference;
            entityManager.SetComponentData<GetPerfectPassComponent>(OptimalPointReference.GetPerfectPassEntity, GetPerfectPassComponent);

            DynamicBuffer<PlayerAttackElement> PlayerAttackElements = entityManager.GetBuffer<PlayerAttackElement>(OptimalPointReference.GetPerfectPassEntity);
             for (int i = 0; i < PlayerAttackElements.Length; i++)
            {
                PlayerAttackElement playerAttackElement = PlayerAttackElements[i];
                playerAttackElement.PlayerDataComponent.position = publicPlayerAttacks[i].position;
                PlayerAttackElements[i] = playerAttackElement;
            }
        }
    }
    OptimalPointReference getOrCreateOptimalPointReference(int index, OptimalPointParams OptimalPointParams)
    {
        if (index < OptimalPointReferences.Count)
        {
            return OptimalPointReferences[index];
        }
        else
        {
            OptimalPointReference optimalPointReference = new OptimalPointReference(OptimalPointParams);
            OptimalPointReferences.Add(optimalPointReference);
            return optimalPointReference;
        }
    }
    void createGerPerfectPassPathComponentsV1()
    {
        for (int i = 0; i < getPerfectPassOptimalPointSize; i++)
        {
            OptimalPointReference optimalPointReference = getOrCreateOptimalPointReference(i, getPerfectPassOptimalPointParams);
            SegmentedPathCalculationData segmentedPathCalculationData = new SegmentedPathCalculationData(segmentedPathParams.timeRange, segmentedPathParams.timeIncrement, segmentedPathParams.startSegmentedTime, segmentedPathParams.minAngle, segmentedPathParams.minVelocity, segmentedPathParams.maxAngle, segmentedPathParams.maxVelocity, optimalPointReference.optimalPointParams.maxTime);
            Vector3 v0 = Vector3.zero;
            Vector3 pos0 = Vector3.zero;
            float drag = MatchComponents.ballComponents.rigBall.drag;
            float mass = MatchComponents.ballComponents.rigBall.mass;
            float g = 9.81f;
            float groundY = MatchComponents.ballComponents.sphereCollider.radius * MatchComponents.ballComponents.sphereCollider.transform.localScale.x;
            float radius = MatchComponents.ballComponents.radio;
            float friction = MatchComponents.ballComponents.friction;
            float dynamicFriction = MatchComponents.ballComponents.dynamicFriction;
            float bouciness = MatchComponents.ballComponents.bounciness;
            PathDataDOTS intPathDataDOTS = new PathDataDOTS(0, PathType.Parabolic, 0, pos0,Vector3.zero, v0, drag, mass, groundY, bouciness, friction, dynamicFriction, radius, g);
            PathComponent pathComponent = new PathComponent(intPathDataDOTS, segmentedPathCalculationData);
            entityManager.SetComponentData(optimalPointReference.GetPerfectPassEntity, pathComponent);
        }
    }
    void createGerPerfectPassPathComponents()
    {
        for (int i = 0; i < getPerfectPassOptimalPointSize; i++)
        {
            OptimalPointReference optimalPointReference = getOrCreateOptimalPointReference(i, getPerfectPassOptimalPointParams);
            OptimalPointDOTSCreator.createPathComponents(optimalPointReference, segmentedPathParams);
        }
    }
    void createGetPassV0()
    {
        GetPassV0Entity = entityManager.CreateEntity();
        DynamicBuffer<GetPassV0Element> GetPassV0Elements = entityManager.AddBuffer<GetPassV0Element>(GetPassV0Entity);

        SearchPathSystem.GetPassV0Entity = GetPassV0Entity;
        GetPassV0Element GetPassV0Element = new GetPassV0Element();

        GetPassV0Element.maxAttempts = 20;
        GetPassV0Element.maxControlSpeed = 15;
        GetPassV0Element.accuracy = 0.1f;
        GetPassV0Element.maxControlSpeedLerpDistance = 5f;
        GetPassV0Element.Pos0 = MyFunctions.setY0ToVector3(MatchComponents.ballPosition);
        GetPassV0Element.Posf = targetPosition.position;
        GetPassV0Element.k = MatchComponents.ballRigidbody.drag;
        GetPassV0Element.g = Physics.gravity.magnitude;
        GetPassV0Element.ballRadio = MatchComponents.ballRadio;
        GetPassV0Element.friction = MatchComponents.ballComponents.friction;
        GetPassV0Elements.Add(GetPassV0Element);
    }
    void updateBallPosition()
    {
        DynamicBuffer<GetPassV0Element> GetPassV0Elements = entityManager.GetBuffer<GetPassV0Element>(GetPassV0Entity);
        GetPassV0Element GetPassV0Element = GetPassV0Elements[0];
        GetPassV0Element.Pos0 = MyFunctions.setY0ToVector3(MatchComponents.ballPosition);
    }
    public void updateGetTimeToReachPoint()
    {
        DynamicBuffer<GetTimeToReachPointElement> getTimeToReachPointElements = entityManager.GetBuffer<GetTimeToReachPointElement>(GetTimeToReachPointEntity);
        for (int i = 0; i < getTimeToReachPointElements.Length; i++)
        {
            GetTimeToReachPointElement getTimeToReachPointElement = getTimeToReachPointElements[i];
            getTimeToReachPointElement.targetPosition = targetPosition.position;
            getTimeToReachPointElements[i] = getTimeToReachPointElement;
        }
    }
    public void addPlayerToOptimalPointV2(PublicPlayerData publicPlayerData)
    {
        //if (OptimalPointDOTSCreator.publicPlayerDatas.Count >= OptimalPointDOTSCreator.teamSize || !publicPlayerData.addToOptimalPoint) return;
        if (publicPlayerData.playerID.Equals(publicPlayerTest.playerID))
        {
            int j = publicPlayerDatas.Count;
            publicPlayerDatas.Add(publicPlayerData);
            PlayerDataComponent playerDataComponent;
            OptimalPointDOTSCreator.getPlayerData(j, publicPlayerDatas, out playerDataComponent);
            PlayerAttackElement playerDataComponentElement = new PlayerAttackElement();
            playerDataComponentElement.PlayerDataComponent = playerDataComponent;
            GetTimeToReachPointElement getTimeToReachPointElement = new GetTimeToReachPointElement();
            getTimeToReachPointElement.playerID = playerDataComponent.id;
            getTimeToReachPointElement.targetPosition = targetPosition.position;
            DynamicBuffer<GetTimeToReachPointElement> getTimeToReachPointElements = entityManager.GetBuffer<GetTimeToReachPointElement>(GetTimeToReachPointEntity);
            getTimeToReachPointElements.Add(getTimeToReachPointElement);
            DynamicBuffer<PlayerAttackElement> playerDataComponentElements = entityManager.GetBuffer<PlayerAttackElement>(GetTimeToReachPointEntity);
            playerDataComponentElements.Add(playerDataComponentElement);
        }

        for (int i = 0; i < getPerfectPassOptimalPointSize; i++)
        {
            OptimalPointReference optimalPointReference = getOrCreateOptimalPointReference(i,getPerfectPassOptimalPointParams);
            if (optimalPointReference.publicPlayerDatas.Count >= optimalPointReference.optimalPointParams.playerSize) return;
            Entity entity = entityManager.CreateEntity();
            OptimalPointDOTSCreator.addPlayerToOptimalPointDOTS(optimalPointReference, publicPlayerData, entity);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            testV0Pass();
        }
        UpdateChaserPositions();
        //testPlane2();
    }
    void testPlane2()
    {
        float currentLenght = Mathf.Infinity;
        Ray ray = new Ray(targetPosition.position, targetPosition.forward);
        List<Plane> planes = MatchComponents.footballField.fullFieldArea.planes;
        AreaPlaneElement areaPlaneElement = new AreaPlaneElement(planes[0], planes[1], planes[2], planes[3]);
        Vector3 resutl;
        bool a = areaPlaneElement.GetPoint(testPlanePoint.position, testPlane.position, out resutl);
        //Vector3 intercession = ray.origin + ray.direction * currentLenght;
        print(a);
        Debug.DrawLine(testPlane.position, resutl);
    }
    void testV0Pass()
    {
        GetPerfectPassComponent GetPerfectPassComponent = entityManager.GetComponentData<GetPerfectPassComponent>(OptimalPointReferences[0].GetPerfectPassEntity);
        GetPassV0Element straightGetPassV0Element = GetPerfectPassComponent.straightGetPassV0;
        GetPassV0Element parabolicGetPassV0Element = GetPerfectPassComponent.parabolicGetPassV0;
        GetPassV0Element result = parabolicGetPassV0Element;
        if (straightGetPassV0Element.result.noRivalReachTheTargetBeforeMe)
        {
            result = straightGetPassV0Element;
        }
        else if(parabolicGetPassV0Element.result.noRivalReachTheTargetBeforeMe)
        {
            result = parabolicGetPassV0Element;
            
        }

        MatchComponents.ballRigidbody.velocity = result.result.v0;
        Vector3 dir = targetPosition.position - publicPlayerTest.position;
        dir.y = 0;
        dir.Normalize();
        publicPlayerTest.playerComponents.ForwardDesiredDirection = dir;
        publicPlayerTest.playerComponents.ForwardDesiredSpeed = publicPlayerTest.movimentValues.maxForwardSpeed;
        publicPlayerTest.playerComponents.DesiredLookDirection = dir;
        publicPlayerTest.playerComponents.MinForwardSpeed = 0;
        publicPlayerTest.playerComponents.TargetPosition = targetPosition.position;
        publicPlayerTest.playerComponents.stopOffset = 0;

        foreach (var OptimalPointReference in OptimalPointReferences)
        {

            DynamicBuffer<ChaserDataElement> chaserDataElements = entityManager.GetBuffer<ChaserDataElement>(OptimalPointReference.GetPerfectPassEntity);
            for (int i = 0; i < chaserDataElements.Length; i++)
            {
                if (chaserDataElements[i].ReachTheTarget)
                {
                    PublicPlayerData publicPlayerData = defensePublicPlayerDatas[i];
                    Vector3 dir2 = chaserDataElements[i].OptimalPoint - publicPlayerData.position;
                    dir2.y = 0;
                    dir2.Normalize();
                    publicPlayerData.playerComponents.ForwardDesiredDirection = dir2;
                    publicPlayerData.playerComponents.ForwardDesiredSpeed = publicPlayerTest.movimentValues.maxForwardSpeed;
                    publicPlayerData.playerComponents.DesiredLookDirection = dir2;
                    publicPlayerData.playerComponents.MinForwardSpeed = 0;
                    publicPlayerData.playerComponents.TargetPosition = chaserDataElements[i].OptimalPoint;
                    publicPlayerData.playerComponents.stopOffset = 0;
                }
            }
            
        }
            //DynamicBuffer<GetTimeToReachPointElement> GetTimeToReachPointElements = entityManager.GetBuffer<GetTimeToReachPointElement>(GetTimeToReachPointEntity);
            //GetTimeToReachPointElement GetTimeToReachPointElement = GetTimeToReachPointElements[0];

            //Invoke("printtest", GetTimeToReachPointElement.reachTime);
            //print(GetTimeToReachPointElement.reachTime + " "+ getPassV0Element.result.v0.magnitude);
        }
    void printtest()
    {
        print("eo");
    }
    void debugParabolicPassDOTS()
    {

    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && enabled && debug)
        {
            if (debugSegmentedPaths)
            {
                foreach (var OptimalPointReference in OptimalPointReferences)
                {
                    DynamicBuffer<SegmentedPathElement> SegmentedPathElements = entityManager.GetBuffer<SegmentedPathElement>(OptimalPointReference.GetPerfectPassEntity);
                    foreach (var segmentedPathElement in SegmentedPathElements)
                    {
#if UNITY_EDITOR
                        if (debugSegmentedPathsText)
                            Handles.Label(segmentedPathElement.Pos0 + Vector3.up * 0.25f, " t=" + segmentedPathElement.t0.ToString("f2") + " v=" + segmentedPathElement.V0Magnitude.ToString("f2"));
                        //Handles.Label(segmentedPathElement.Pos0 + Vector3.up * 0.5f, segmentedPathElement.Pos0.ToString("f2") );
#endif
                        Debug.DrawLine(segmentedPathElement.Pos0, segmentedPathElement.Posf, Color.red);
                    }
                }
            }
            if (debugChaserData)
            {
                if (v1)
                {
                    foreach (var OptimalPointReference in OptimalPointReferences)
                    {
                        DynamicBuffer<ChaserDataElement> chaserDataElements = entityManager.GetBuffer<ChaserDataElement>(OptimalPointReference.GetPerfectPassEntity);
                        DynamicBuffer<PlayerDefenseElement> PlayerDefenseElements = entityManager.GetBuffer<PlayerDefenseElement>(OptimalPointReference.GetPerfectPassEntity);
                        GetPerfectPassComponent GetPerfectPassComponent = entityManager.GetComponentData<GetPerfectPassComponent>(OptimalPointReference.GetPerfectPassEntity);
                        float difference = GetPerfectPassComponent.parabolicGetPassV0.result.ballReachTargetPositionTime- GetPerfectPassComponent.parabolicGetPassV0.result.receiverReachTargetPositionTime;
                        if (printText)
                        {
                            print("Straight noRivalReachTheTargetBeforeMe=" + GetPerfectPassComponent.straightGetPassV0.result.noRivalReachTheTargetBeforeMe + " noPartnerIsAhead=" + GetPerfectPassComponent.straightGetPassV0.result.noPartnerIsAhead + " differenceTime=" + GetPerfectPassComponent.straightGetPassV0.result.differenceTimeWithRival + " Parabolic noRivalReachTheTargetBeforeMe=" + GetPerfectPassComponent.parabolicGetPassV0.result.noRivalReachTheTargetBeforeMe + " noPartnerIsAhead=" + GetPerfectPassComponent.parabolicGetPassV0.result.noPartnerIsAhead + " differenceTime=" + GetPerfectPassComponent.parabolicGetPassV0.result.differenceTimeWithRival + " v0=" + GetPerfectPassComponent.parabolicGetPassV0.result.v0Magnitude + " difference=" + difference);
                            //print("Straight noRivalReachTheTargetBeforeMe=" + GetPerfectPassComponent.straightGetPassV0.result.noRivalReachTheTargetBeforeMe + " noPartnerIsAhead=" + GetPerfectPassComponent.straightGetPassV0.result.noPartnerIsAhead + " differenceTime=" + GetPerfectPassComponent.straightGetPassV0.result.differenceTimeWithRival + " Parabolic noRivalReachTheTargetBeforeMe=" + GetPerfectPassComponent.parabolicGetPassV0.result.noRivalReachTheTargetBeforeMe + " noPartnerIsAhead=" + GetPerfectPassComponent.parabolicGetPassV0.result.noPartnerIsAhead + " differenceTime=" + GetPerfectPassComponent.parabolicGetPassV0.result.differenceTimeWithRival + " v0=" + GetPerfectPassComponent.parabolicGetPassV0.result.v0Magnitude + " difference=" + difference);
                            //print("Straight noRivalReachTheTargetBeforeMe=" + GetPerfectPassComponent.straightGetPassV0.result.noRivalReachTheTargetBeforeMe + "   | Parabolic noRivalReachTheTargetBeforeMe=" + GetPerfectPassComponent.parabolicGetPassV0.result.noRivalReachTheTargetBeforeMe + " maxKickForceReached=" + GetPerfectPassComponent.parabolicGetPassV0.result.maxKickForceReached + " maximumControlSpeedReached=" + GetPerfectPassComponent.parabolicGetPassV0.result.maximumControlSpeedReached);
                        }
                        
                        for (int i = 0; i < PlayerDefenseElements.Length; i++)
                        {
                            if (i >= chaserDataElements.Length) break;
                            ChaserDataElement chaserDataElement = chaserDataElements[i];

                            if (chaserDataElement.ReachTheTarget)
                            {
                                Gizmos.color = Color.red;
                                Gizmos.DrawLine(PlayerDefenseElements[i].PlayerDataComponent.position+Vector3.up*0.1f, chaserDataElement.OptimalPoint + Vector3.up * 0.1f);
                                Gizmos.DrawSphere(chaserDataElement.OptimalPoint, 0.2f);
                            }
#if UNITY_EDITOR
                            if (printText)
                            {
                                GUIStyle style = new GUIStyle();
                                style.fontSize = 10;
                                style.normal.textColor = Color.yellow;
                                Handles.color = Color.green;
                                Handles.Label(PlayerDefenseElements[i].PlayerDataComponent.position + Vector3.up * 1.8f, "ReachTheTarget=" + chaserDataElement.ReachTheTarget, style);
                            }
#endif
                        }

                    }
                }
                else
                {
                    foreach (var OptimalPointReference in OptimalPointReferences)
                    {
                        OptimalPointDOTSCreator.DrawChaserDatas2(OptimalPointReference);
                    }
                }
                
            }
        }
    }
#endif
    private void OnDestroy()
    {
        if (!v1)
        {
            foreach (var OptimalPointReference in OptimalPointReferences)
            {
                if (OptimalPointReference.ChaserDataEntityArray.IsCreated)
                {
                    OptimalPointReference.ChaserDataEntityArray.Dispose();

                }
                if (OptimalPointReference.pathComponentEntityArray.IsCreated)
                    OptimalPointReference.pathComponentEntityArray.Dispose();
                if (OptimalPointReference.PlayerComponentEntitiesArray.IsCreated)
                    OptimalPointReference.PlayerComponentEntitiesArray.Dispose();
                if (OptimalPointReference.optimalPointListEntityArray.IsCreated)
                    OptimalPointReference.optimalPointListEntityArray.Dispose();
            }
            OptimalPointSystem.OnDestroy();
        }
    }
}
