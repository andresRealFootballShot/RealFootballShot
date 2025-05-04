using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using DOTS_ChaserDataCalculation;
using UnityEditor;

public class OptimalPointReference
{
    public Entity GetPerfectPassEntity;
    public bool entitiesAreCreated;
    public NativeArray<Entity> ChaserDataEntityArray;
    public NativeArray<Entity> PlayerComponentEntitiesArray;
    public NativeArray<Entity> pathComponentEntityArray;
    public NativeArray<Entity> optimalPointListEntityArray;
    public Entity connectionEntity;
    public OptimalPointParams optimalPointParams;
    public List<PublicPlayerData> publicPlayerDatas = new List<PublicPlayerData>();
    public int index;
    public OptimalPointReference(OptimalPointParams optimalPointParams)
    {
        this.optimalPointParams = optimalPointParams;
    }
}

[System.Serializable]
public class OptimalPointParams
{
    public int segmentedPathSize = 30;
    public int optimalPointLenght = 2;
    public int playerSize = 1;
    public int attackSize=3, defenseSize=11;
    public int defenseJobSize = 3;
    public float maxTime=3;
}
[System.Serializable]
public class SegmentedPathParams
{
    public float timeRange=3, timeIncrement=0.1f, minAngle, minVelocity, maxAngle=10, maxVelocity=4, startSegmentedTime;
    public int size=15;
    public SegmentedPathParams(int size, float timeRange, float timeIncrement, float minAngle, float minVelocity, float maxAngle, float maxVelocity, float startSegmentedTime)
    {
        this.size = size;
        this.timeRange = timeRange;
        this.timeIncrement = timeIncrement;
        this.minAngle = minAngle;
        this.minVelocity = minVelocity;
        this.maxAngle = maxAngle;
        this.maxVelocity = maxVelocity;
        this.startSegmentedTime = startSegmentedTime;
    }
}
public struct UpdateOptimalPointData
{
    public Vector3 v0,pos0;
}
public class OptimalPointDOTSCreator : MonoBehaviour
{
    
    
    public Rigidbody ballRigidbody;
    public SphereCollider ballSphereCollider;
    public OptimalPointSystem OptimalPointSystem { get; set; }
    
    EntityManager entityManager;
    
    
    private void Awake()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
    public void createAreaBuffer(Entity entity)
    {
        DynamicBuffer<AreaPlaneElement> areaPlaneElements = entityManager.AddBuffer<AreaPlaneElement>(entity);

        List<Plane> planes = MatchComponents.footballField.fullFieldArea.planes;
        AreaPlaneElement areaPlaneElement = new AreaPlaneElement(planes[0], planes[1], planes[2], planes[3]);
        areaPlaneElements.Add(areaPlaneElement);
    }
    public void createAreaBuffer2(Entity entity)
    {
        DynamicBuffer<AreaPlaneElement> areaPlaneElements = World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<AreaPlaneElement>(entity);

        List<Plane> planes = MatchComponents.footballField.fullFieldArea.planes;
        AreaPlaneElement areaPlaneElement = new AreaPlaneElement(planes[0], planes[1], planes[2], planes[3]);
        areaPlaneElements.Add(areaPlaneElement);
        foreach (var sideOfField in MatchComponents.footballField.sideOfFields)
        {
            planes = sideOfField.bigArea.planes;
            areaPlaneElement = new AreaPlaneElement(planes[0], planes[1], planes[2], planes[3]);
            areaPlaneElements.Add(areaPlaneElement);
        }
    }
    void clearChaserDataComponents(OptimalPointReference optimalPointReference)
    {
        foreach (var ChaserDataEntity in optimalPointReference.ChaserDataEntityArray)
        {
            DynamicBuffer<ChaserDataElement> chaserDataElements = entityManager.GetBuffer<ChaserDataElement>(ChaserDataEntity);
            chaserDataElements.Clear();
        }
    }
    public void clearSegmentedPaths(OptimalPointReference optimalPointReference)
    {
        int segmentedPathToRemoveCount = OptimalPointSystem.segmentedPathToRemoveCounts[optimalPointReference.index];
        
        DynamicBuffer<SegmentedPathElement> segmentedPaths = entityManager.GetBuffer<SegmentedPathElement>(optimalPointReference.connectionEntity);
        segmentedPathToRemoveCount = Mathf.Clamp(segmentedPathToRemoveCount, 0, segmentedPaths.Length);
        if (segmentedPaths.Length>0)
        segmentedPaths.RemoveRange(0, segmentedPathToRemoveCount);
    }
    public void updateChaserDataOfPublicPlayerData(OptimalPointReference optimalPointReference)
    {
        
        for (int i = 0; i < optimalPointReference.publicPlayerDatas.Count; i++)
        {

            Entity chaserDataEntity = optimalPointReference.ChaserDataEntityArray[i];
            ChaserDataElement chaserDataElement = new ChaserDataElement();
            if (getFirstChaserDataElement(chaserDataEntity, ref chaserDataElement))
            {
                ChaserData chaserData;
                PublicPlayerData publicPlayerData = optimalPointReference.publicPlayerDatas[i];
                publicPlayerData.getFirstChaserData(out chaserData);
                chaserData.ReachTheTarget = chaserDataElement.ReachTheTarget;
                chaserData.OptimalPoint = chaserDataElement.OptimalPoint;
                chaserData.OptimalTime = chaserDataElement.OptimalTime;
                chaserData.OptimalTargetTime = chaserDataElement.OptimalTargetTime;
                chaserData.thereIsClosestPoint = chaserDataElement.thereIsClosestPoint;
                chaserData.ClosestPoint = chaserDataElement.ClosestPoint;
                chaserData.ClosestTargetTime = chaserDataElement.ClosestTargetTime;
                chaserData.ClosestChaserTime = chaserDataElement.ClosestChaserTime;
                chaserData.thereIsIntercession = chaserDataElement.thereIsIntercession;
                chaserData.Intercession = chaserDataElement.Intercession;
                chaserData.TargetIntercessionTime = chaserDataElement.TargetIntercessionTime;
                chaserData.differenceClosestTime = chaserDataElement.differenceClosestTime;
            }
            else
            {
                DynamicBuffer<SegmentedPathElement> segmentedPathElements = entityManager.GetBuffer<SegmentedPathElement>(optimalPointReference.connectionEntity);
                if (segmentedPathElements.Length == 0)
                {
                    ChaserData chaserData;
                    PublicPlayerData publicPlayerData = optimalPointReference.publicPlayerDatas[i];
                    publicPlayerData.getFirstChaserData(out chaserData);
                    chaserData.ReachTheTarget = false;
                    chaserData.thereIsClosestPoint = false;
                    chaserData.thereIsIntercession = false;
                }
            }
        }
        clearChaserDataComponents(optimalPointReference);
    }
    bool getFirstChaserDataElement(Entity entity, ref ChaserDataElement firstChaserDataElement)
    {
        DynamicBuffer<ChaserDataElement> chaserDataElements = entityManager.GetBuffer<ChaserDataElement>(entity);
        float t = Mathf.Infinity;
        float closestT = Mathf.Infinity;
        int i = 0, index = 0;
        bool thereIsChaserData = false;
        foreach (var chaserDataElement in chaserDataElements)
        {
            if (chaserDataElement.ReachTheTarget && chaserDataElement.OptimalTime < t)
            {
                thereIsChaserData = true;
                index = i;
                t = chaserDataElement.OptimalTime;
                firstChaserDataElement.playerID = chaserDataElement.playerID;
                firstChaserDataElement.ReachTheTarget = true;
                firstChaserDataElement.OptimalPoint = chaserDataElement.OptimalPoint;
                firstChaserDataElement.OptimalTime = chaserDataElement.OptimalTime;
                firstChaserDataElement.OptimalTargetTime = chaserDataElement.OptimalTargetTime;
                firstChaserDataElement.chaserDataCalculationIndex = chaserDataElement.chaserDataCalculationIndex;
            }
            if (chaserDataElement.thereIsClosestPoint && chaserDataElement.differenceClosestTime< closestT)
            {
                thereIsChaserData = true;
                index = i;
                closestT = chaserDataElement.differenceClosestTime;
                firstChaserDataElement.ClosestPoint = chaserDataElement.ClosestPoint;
                firstChaserDataElement.ClosestChaserTime = chaserDataElement.ClosestChaserTime;
                firstChaserDataElement.ClosestTargetTime = chaserDataElement.ClosestTargetTime;
                firstChaserDataElement.TargetPositionInClosestTime = chaserDataElement.TargetPositionInClosestTime;
                firstChaserDataElement.differenceClosestTime = chaserDataElement.differenceClosestTime;
                firstChaserDataElement.thereIsClosestPoint = chaserDataElement.thereIsClosestPoint;
            }
            if (chaserDataElement.thereIsIntercession)
            {
                thereIsChaserData = true;
                index = i;
                firstChaserDataElement.thereIsIntercession = true;
                firstChaserDataElement.Intercession = chaserDataElement.Intercession;
                firstChaserDataElement.TargetIntercessionTime = chaserDataElement.TargetIntercessionTime;
                
            }
            i++;
        }
        /*if (chaserDataElements.Length > 0 && chaserDataElements[index].ReachTheTarget)
        {
            firstChaserDataElement = chaserDataElements[index];
            return true;
        }
        firstChaserDataElement = new ChaserDataElement();*/
        return thereIsChaserData;
    }
#if UNITY_EDITOR
    void DrawChaserDatas(OptimalPointReference optimalPointReference)
    {
        foreach (var ChaserDataEntity in optimalPointReference.ChaserDataEntityArray)
        {
            ChaserDataElement chaserDataElement = new ChaserDataElement();
            if (getFirstChaserDataElement(ChaserDataEntity, ref chaserDataElement))
            {

                Handles.Label(chaserDataElement.OptimalPoint + Vector3.up * 0.5f, "ID =" + chaserDataElement.playerID + " Index="+chaserDataElement.chaserDataCalculationIndex);

                if (chaserDataElement.ReachTheTarget)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(chaserDataElement.OptimalPoint, 0.1f);
                }
                if (chaserDataElement.thereIsClosestPoint)
                {
                    Gizmos.color = new Color(1, 0.5f, 0);
                    Gizmos.DrawSphere(chaserDataElement.ClosestPoint, 0.1f);
                }
                if (chaserDataElement.thereIsIntercession)
                {
                    Gizmos.color = new Color(1, 1f, 0.5f);
                    Gizmos.DrawSphere(chaserDataElement.Intercession, 0.1f);
                }
            }
        }

    }
#endif
    public void DrawChaserDatas2(OptimalPointReference optimalPointReference)
    {
        for (int i = 0; i < optimalPointReference.publicPlayerDatas.Count; i++)
        {
            ChaserData chaserData;
            PublicPlayerData publicPlayerData = optimalPointReference.publicPlayerDatas[i];
            if (publicPlayerData.getFirstChaserData(out chaserData))
            {
                if (chaserData.ReachTheTarget)
                {
                    //Handles.Label(publicPlayerData.position + Vector3.up * 0.5f, "ID =" + publicPlayerData.playerID);
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(publicPlayerData.position, chaserData.OptimalPoint);
                }
               
            }
        }

    }
   public void drawSegmentedPaths(OptimalPointReference optimalPointReference,bool debugSegmentedPathsText)
    {

        //print(segmentedPathElements.Length);

        //Handles.Label(ballRigidbody.position + Vector3.up * 0.25f, time.ToString("f2"));
        DynamicBuffer<SegmentedPathElement> segmentedPathElements = entityManager.GetBuffer<SegmentedPathElement>(optimalPointReference.connectionEntity);
            foreach (var segmentedPathElement in segmentedPathElements)
            {

#if UNITY_EDITOR
            if(debugSegmentedPathsText)
                Handles.Label(segmentedPathElement.Pos0 + Vector3.up * 0.25f, segmentedPathElement.index.ToString() + " | " + segmentedPathElement.t0.ToString("f2") + " | " + segmentedPathElement.V0Magnitude.ToString("f2"));
                //Handles.Label(segmentedPathElement.Pos0 + Vector3.up * 0.5f, segmentedPathElement.Pos0.ToString("f2") );
#endif
                Debug.DrawLine(segmentedPathElement.Pos0, segmentedPathElement.Posf, Color.red);
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(segmentedPathElement.Pos0, 0.05f);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(segmentedPathElement.Posf, 0.05f);
            if(segmentedPathElement.V0.magnitude!=0)
                DrawArrow.ForDebug(segmentedPathElement.Pos0, segmentedPathElement.V0.normalized * 0.1f, Color.yellow);
            }
        //segmentedPathElements.Clear();


    }
    public void drawSegmentedPaths2(OptimalPointReference optimalPointReference, bool debugSegmentedPathsText)
    {

        //print(segmentedPathElements.Length);

        //Handles.Label(ballRigidbody.position + Vector3.up * 0.25f, time.ToString("f2"));
        DynamicBuffer<SegmentedPathElement> segmentedPathElements = entityManager.GetBuffer<SegmentedPathElement>(optimalPointReference.connectionEntity);
        foreach (var segmentedPathElement in segmentedPathElements)
        {

#if UNITY_EDITOR
            if (debugSegmentedPathsText)
                Handles.Label(segmentedPathElement.Pos0 + Vector3.up * 0.25f," t=" + segmentedPathElement.t0.ToString("f2") + " v=" + segmentedPathElement.V0Magnitude.ToString("f2"));
            //Handles.Label(segmentedPathElement.Pos0 + Vector3.up * 0.5f, segmentedPathElement.Pos0.ToString("f2") );
#endif
            Debug.DrawLine(segmentedPathElement.Pos0, segmentedPathElement.Posf, Color.red);
        }
        //segmentedPathElements.Clear();


    }
   public static void getPlayerData(int index,List<PublicPlayerData> publicPlayerDatas, out PlayerDataComponent playerDataComponent)
    {
        
        index = Mathf.Clamp(index,0, publicPlayerDatas.Count-1);
        PublicPlayerData publicPlayerData = publicPlayerDatas[index];
        float maximumJumpHeight = 0;
        if (publicPlayerData.maximumJumpHeights.Count>0)
        {

            maximumJumpHeight = publicPlayerData.maximumJumpHeights.Keys[0];
        }
        MovimentValues movimentValues = publicPlayerData.movimentValues;
        PlayerComponents  playerComponents = publicPlayerData.playerComponents;
        Vector3 bodyY0Forward = playerComponents.bodyY0Forward;
        float maxSpeed = publicPlayerData.maxSpeed;
        bool isGoalkeeper = false;
        playerDataComponent = new PlayerDataComponent(publicPlayerData.useAccelerationInChaserDataCalculation, index, publicPlayerData.position, bodyY0Forward, playerComponents.Velocity, playerComponents.Velocity.normalized, maxSpeed, publicPlayerData.playerComponents.movementValues.forwardAcceleration, playerComponents.movementValues.forwardDeceleration, maximumJumpHeight, playerComponents.scope, playerComponents.Speed, movimentValues.minSpeedForRotateBody, movimentValues.maxAngleForRun, playerComponents.maxSpeedRotation, movimentValues.maxSpeedForReachBall,MatchComponents.ballRigidbody.drag, Vector3.up, MatchComponents.footballField.position,0, publicPlayerData.playerComponents.bodyBallRadio, publicPlayerData.playerData.height, isGoalkeeper, publicPlayerData.movimentValues.NormalMaximumJumpHeight,-1, publicPlayerData.playerData.height*0.75f,publicPlayerData.playerComponents.soccerPlayerData.maxKickForce);
    }

    public static void getPlayerData(PublicPlayerData publicPlayerData,int index, out PlayerDataComponent playerDataComponent)
    {

        float maximumJumpHeight = 0;
        if (publicPlayerData.maximumJumpHeights.Count > 0)
        {

            maximumJumpHeight = publicPlayerData.maximumJumpHeights.Keys[0];
        }
        MovimentValues movimentValues = publicPlayerData.movimentValues;
        PlayerComponents playerComponents = publicPlayerData.playerComponents;
        Vector3 bodyY0Forward = playerComponents.bodyY0Forward;
        float maxSpeed = publicPlayerData.maxSpeed;
        bool isGoalkeeper = false;
        playerDataComponent = new PlayerDataComponent(publicPlayerData.useAccelerationInChaserDataCalculation, index, publicPlayerData.position, bodyY0Forward, playerComponents.Velocity, playerComponents.Velocity.normalized, maxSpeed, publicPlayerData.playerComponents.movementValues.forwardAcceleration, playerComponents.movementValues.forwardDeceleration, maximumJumpHeight, playerComponents.scope, playerComponents.Speed, movimentValues.minSpeedForRotateBody, movimentValues.maxAngleForRun, playerComponents.maxSpeedRotation, movimentValues.maxSpeedForReachBall, MatchComponents.ballRigidbody.drag, Vector3.up, MatchComponents.footballField.position, 0, publicPlayerData.playerComponents.bodyBallRadio, publicPlayerData.playerData.height, isGoalkeeper, publicPlayerData.movimentValues.NormalMaximumJumpHeight, -1, publicPlayerData.playerData.height * 0.75f, publicPlayerData.playerComponents.soccerPlayerData.maxKickForce);
    }
    void checkEntitiesAreCreated(OptimalPointReference optimalPointReference) {
        if (!optimalPointReference.entitiesAreCreated)
        {
            createSegmentedPath_OptimalPointConnection(optimalPointReference);
        }
    }
    public void addPlayerToOptimalPointDOTS(OptimalPointReference optimalPointReference,PublicPlayerData publicPlayerData, Entity optimalPointEntity)
    {
        if (optimalPointReference.publicPlayerDatas.Count >= optimalPointReference.optimalPointParams.playerSize || !publicPlayerData.addToOptimalPoint) return;

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        checkEntitiesAreCreated(optimalPointReference);
        enabled = true;
        int j = optimalPointReference.publicPlayerDatas.Count;
        optimalPointReference.publicPlayerDatas.Add(publicPlayerData);
        for (int i = 0; i < optimalPointReference.optimalPointParams.optimalPointLenght; i++)
        {

            optimalPointReference.PlayerComponentEntitiesArray[i * optimalPointReference.optimalPointParams.playerSize + j] = optimalPointEntity;
            DynamicBuffer<OptimalPointEntityElement> OptimalPointEntityElementBuffer = entityManager.GetBuffer<OptimalPointEntityElement>(optimalPointReference.optimalPointListEntityArray[i]);
            OptimalPointEntityElementBuffer.Add(new OptimalPointEntityElement { entity = optimalPointEntity });
            PlayerDataComponent playerDataComponent;
            getPlayerData(j, optimalPointReference.publicPlayerDatas, out playerDataComponent);
            playerDataComponent.id = j;
            entityManager.AddComponentData<PlayerDataComponent>(optimalPointEntity, playerDataComponent);
            DynamicBuffer<SegmentedPathElement> SegmentedPathElementBuffer = entityManager.AddBuffer<SegmentedPathElement>(optimalPointEntity);
            NativeArray<SegmentedPathElement> segmentedPathElements = new NativeArray<SegmentedPathElement>(optimalPointReference.optimalPointParams.segmentedPathSize, Allocator.Temp);
            SegmentedPathElementBuffer.AddRange(segmentedPathElements);
            createAreaBuffer(optimalPointEntity);
            //SegmentedPathComponent segmentedPathComponent = new SegmentedPathComponent();
            //entityManager.AddComponentData(optimalPointEntity, segmentedPathComponent);
            /*ChaserDataComponent chaserDataComponent = new ChaserDataComponent();
            entityManager.AddComponentData(optimalPointEntity, chaserDataComponent);*/
            //entityManager.SetEnabled(optimalPointEntity, false);
        }
    }
    public void updatePlayerDataComponents(OptimalPointReference optimalPointReference)
    {
        //segmentedPathElements.Clear();

        foreach (var playerDataEntity in optimalPointReference.PlayerComponentEntitiesArray)
        {
            if (!entityManager.Exists(playerDataEntity))
            {
                continue;
            }
            PlayerDataComponent updatedPlayerData = entityManager.GetComponentData<PlayerDataComponent>(playerDataEntity);

            int id = Mathf.Clamp(updatedPlayerData.id, 0, optimalPointReference.publicPlayerDatas.Count - 1);
            PublicPlayerData publicPlayerData = optimalPointReference.publicPlayerDatas[id];
            if (publicPlayerData.maximumJumpHeights.Count > 0)
            {

                updatedPlayerData.maxJumpHeight = publicPlayerData.maximumJumpHeights.Keys[0];
            }
            updatedPlayerData.position = publicPlayerData.position;
            updatedPlayerData.ForwardVelocity = publicPlayerData.playerComponents.Velocity;
            updatedPlayerData.currentSpeed = publicPlayerData.playerComponents.Speed;
            updatedPlayerData.bodyY0Forward = publicPlayerData.playerComponents.bodyY0Forward;
            updatedPlayerData.normalizedBodyY0Forward = publicPlayerData.playerComponents.bodyY0Forward;
            updatedPlayerData.normalizedForwardVelocity = publicPlayerData.playerComponents.Velocity.normalized;
            entityManager.SetComponentData<PlayerDataComponent>(playerDataEntity, updatedPlayerData);


        }
        //clearSegmentedPaths(optimalPointReference);
        

    }
    public void updatePathComponents(OptimalPointReference optimalPointReference,SegmentedPathParams segmentedPathParams, UpdateOptimalPointData updateOptimalPointData)
    {
        DynamicBuffer<SegmentedPathElement> segmentedPathElements = entityManager.GetBuffer<SegmentedPathElement>(optimalPointReference.connectionEntity);
        if (segmentedPathElements.Length == 0)
        {
            if (optimalPointReference.pathComponentEntityArray.Length > 0)
            {
                float t = segmentedPathParams.startSegmentedTime;
                float startSegmentedTime = segmentedPathParams.startSegmentedTime;
                Vector3 pos0 = updateOptimalPointData.pos0;
                Vector3 v0 = updateOptimalPointData.v0;
                PathDataDOTS pathDataDOTS = entityManager.GetComponentData<PathComponent>(optimalPointReference.pathComponentEntityArray[0]).currentPath;
                pathDataDOTS.pathType = PathType.Parabolic;
                pathDataDOTS.t0 = 0;
                pathDataDOTS.Pos0 = pos0;
                pathDataDOTS.V0 = v0;
                pathDataDOTS.normalizedV0 = v0.normalized;
                pathDataDOTS.v0Magnitude = v0.magnitude;
                pathDataDOTS.index = 0;

                foreach (var pathEntity in optimalPointReference.pathComponentEntityArray)
                {
                    if (t >= optimalPointReference.optimalPointParams.maxTime) break;
                    PathComponent updatePathComponent = entityManager.GetComponentData<PathComponent>(pathEntity);

                    if (!getPathDataDOTS(t, ref pathDataDOTS, out pos0, out v0))
                    {
                        break;
                    }
                    updatePathComponent.currentPath = pathDataDOTS;
                    /*updatePathComponent.currentPath.index = 0;
                    updatePathComponent.currentPath.V0 = v0;
                    updatePathComponent.currentPath.normalizedV0 = v0.normalized;
                    updatePathComponent.currentPath.Pos0 = pos0;
                    updatePathComponent.currentPath.v0Magnitude = v0.magnitude;
                    updatePathComponent.currentPath.t0 = 0;*/
                    //updatePathComponent.isSegmented = false;
                    entityManager.SetComponentData<PathComponent>(pathEntity, updatePathComponent);
                    entityManager.SetEnabled(pathEntity, true);
                    t += segmentedPathParams.timeRange;
                    startSegmentedTime += segmentedPathParams.timeRange;

                }
            }

        }
        else
        {
            foreach (var pathEntity in optimalPointReference.pathComponentEntityArray)
            {
                entityManager.SetEnabled(pathEntity, false);
            }
        }
    }
    public void createPathComponents(OptimalPointReference optimalPointReference, SegmentedPathParams segmentedPathParams)
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        checkEntitiesAreCreated(optimalPointReference);

        optimalPointReference.pathComponentEntityArray = new NativeArray<Entity>(segmentedPathParams.size, Allocator.Persistent);
        EntityArchetype segmentedPathArchetype = entityManager.CreateArchetype(typeof(PathComponent));
        entityManager.CreateEntity(segmentedPathArchetype, optimalPointReference.pathComponentEntityArray);
        SegmentedPathCalculationData segmentedPathCalculationData = new SegmentedPathCalculationData(segmentedPathParams.timeRange, segmentedPathParams.timeIncrement, segmentedPathParams.startSegmentedTime, segmentedPathParams.minAngle, segmentedPathParams.minVelocity, segmentedPathParams.maxAngle, segmentedPathParams.maxVelocity, optimalPointReference.optimalPointParams.maxTime);
        Vector3 v0 = ballRigidbody.velocity;
        Vector3 pos0 = ballRigidbody.position;
        float drag = ballRigidbody.drag;
        float mass = ballRigidbody.mass;
        float g = 9.81f;
        float groundY = ballSphereCollider.radius * ballSphereCollider.transform.localScale.x;
        float radius = MatchComponents.ballComponents.radio;
        float friction = MatchComponents.ballComponents.friction;
        float dynamicFriction = MatchComponents.ballComponents.dynamicFriction;
        float bouciness = MatchComponents.ballComponents.bounciness;
        PathDataDOTS intPathDataDOTS = new PathDataDOTS(0, PathType.Parabolic, 0, pos0,Vector3.zero, v0, drag, mass, groundY, bouciness, friction, dynamicFriction, radius, g);
        float t = segmentedPathParams.startSegmentedTime;
        foreach (var entity in optimalPointReference.pathComponentEntityArray)
        {
            if(t>= optimalPointReference.optimalPointParams.maxTime) break;
            if(!getPathDataDOTS(t, ref intPathDataDOTS, out pos0, out v0))break;
            PathComponent pathComponent = new PathComponent( intPathDataDOTS, segmentedPathCalculationData);
            entityManager.SetComponentData(entity, pathComponent);
            entityManager.SetEnabled(entity, false);
            t += segmentedPathParams.timeRange;
            segmentedPathCalculationData.startSegmentedTime = t;
        }
        
    }
    bool getPathDataDOTS(float t,ref PathDataDOTS pathDataDOTS, out Vector3 pos0,out Vector3 v0)
    {
        pos0 = pathDataDOTS.Pos0;
        v0 = pathDataDOTS.V0;
        int attempts = 10;
        int count = 0;
        do
        {
            if (!BouncyPathDOTS.getBouncePath(t, ref pathDataDOTS,0))
            {
                //pathDataDOTS.t0 = t;
                //BouncyPathDOTS.getPositionAtTime(t, pathDataDOTS, ref pos0);
                //BouncyPathDOTS.getVelocityAtTime(t, pathDataDOTS, ref v0);
                return true;
            }
            count++;
        } while (count < attempts);
        return false;
    }
    void createSegmentedPath_OptimalPointConnection(OptimalPointReference optimalPointReference)
    {
        optimalPointReference.optimalPointListEntityArray = new NativeArray<Entity>(optimalPointReference.optimalPointParams.optimalPointLenght, Allocator.Persistent);
        optimalPointReference.connectionEntity = entityManager.CreateEntity();
        entityManager.AddBuffer<SegmentedPathElement>(optimalPointReference.connectionEntity);
        entityManager.AddBuffer<OptimalPointListEntityElement>(optimalPointReference.connectionEntity);
        optimalPointReference.ChaserDataEntityArray = new NativeArray<Entity>(optimalPointReference.optimalPointParams.playerSize, Allocator.Persistent);
        optimalPointReference.PlayerComponentEntitiesArray = new NativeArray<Entity>(optimalPointReference.optimalPointParams.playerSize * optimalPointReference.optimalPointParams.optimalPointLenght, Allocator.Persistent);
        EntityArchetype ChaserDataArchetype = entityManager.CreateArchetype(typeof(ChaserDataElement));
        entityManager.CreateEntity(ChaserDataArchetype, optimalPointReference.ChaserDataEntityArray);
        foreach (var chaserDataEntity in optimalPointReference.ChaserDataEntityArray)
        {
            entityManager.AddBuffer<ChaserDataElement>(chaserDataEntity);

        }
        for (int i = 0; i < optimalPointReference.optimalPointParams.optimalPointLenght; i++)
        {
            Entity optimalPointListEntity = entityManager.CreateEntity();
            optimalPointReference.optimalPointListEntityArray[i] = optimalPointListEntity;
            entityManager.AddBuffer<OptimalPointEntityElement>(optimalPointListEntity);
            DynamicBuffer<OptimalPointListEntityElement> OptimalPointListEntity = entityManager.GetBuffer<OptimalPointListEntityElement>(optimalPointReference.connectionEntity);
            OptimalPointListEntity.Add(new OptimalPointListEntityElement { entity = optimalPointListEntity, segmentedPathSize = optimalPointReference.optimalPointParams.segmentedPathSize });
        }
        optimalPointReference.entitiesAreCreated = true;
        OptimalPointSystem optimalPointSystem = OptimalPointSystem;
        optimalPointReference.index = optimalPointSystem.segmentedPath_OptimalPointConnections.Length;
        optimalPointSystem.segmentedPath_OptimalPointConnections.Add(optimalPointReference.connectionEntity);
        optimalPointSystem.segmentedPathToRemoveCounts.Add(0);
    }
    private void OnDestroy()
    {
        /*
        if (ChaserDataEntityArray.IsCreated)
        {
            ChaserDataEntityArray.Dispose();
            
        }
        if (pathComponentEntityArray.IsCreated)
            pathComponentEntityArray.Dispose();
        if (PlayerComponentEntitiesArray.IsCreated)
            PlayerComponentEntitiesArray.Dispose();
        if (optimalPointListEntityArray.IsCreated)
            optimalPointListEntityArray.Dispose();
        */
    }
}
