using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEditor;
using static UnityEditor.PlayerSettings;

public class BallInterceptionSystem : MonoBehaviour
{
    public BallTrajectorySimulator trajectory;
    [Header("Debug")]
    public bool debug;
    public Transform forceTransform;
    public float force=10;
    public float timeScale=1;
   
    private NativeArray<float3> ballPositions;
    private NativeArray<float> ballTimes;

    public NativeArray<int> reachableIndices;
    private NativeArray<float> timeToReach;

    private NativeArray<float> accelerations;
    private NativeArray<float> deccelerations;
    private NativeArray<float> maxSpeeds;
    private NativeArray<float> rotationSpeeds;
    private NativeArray<float> jumpHeights;
    private NativeArray<float> desiredSpeeds;
    public NativeArray<float> maxAngleForRuns;
    public NativeArray<float> minSpeedForRotates;
    public NativeArray<float> scopes;

    private NativeArray<float3> playerPositions;
    private NativeArray<float3> playerVelocities;
    private NativeArray<float3> playerDirections;
    float timeDebug;
    bool enablePlayersGoTarget;
    private void Start()
    {
        //testKick3();

    }
    void Update()
    {
        testKick();
        if(enablePlayersGoTarget) setPlayersTarget();
        //setPlayersTarget2();

        //Calculate();
#if UNITY_EDITOR
        if (debug)
        {
            for (int i = 0; i < trajectory.positions.Count - 1; i++)
            {
               Debug.DrawLine(trajectory.positions[i], trajectory.positions[i + 1], Color.cyan);
                
            }
            // Mostrar resultados
            for (int i = 0; i < Teams.allPlayers.Count; i++)
            {


                if (i >= reachableIndices.Length) break; 
                
                int index = reachableIndices[i];
                if (index >= 0)
                {
                    Debug.DrawLine(Teams.allPlayers[i].position, trajectory.positions[index], Color.green);
                    
                }
                Debug.DrawLine(Teams.allPlayers[i].position, Teams.allPlayers[i].playerComponents.TargetPosition, Color.red);


                
            }
            timeDebug += Time.deltaTime;
        }
#endif
    }
    public void Calculate()
    {
        trajectory.Simulate();

        ResizeIfNeeded();

        // Copiar datos de trayectorias
        for (int i = 0; i < ballPositions.Length; i++)
        {
            ballPositions[i] = trajectory.positions[i];
            ballTimes[i] = trajectory.times[i];
        }

        // Copiar datos de jugadores
        for (int i = 0; i < Teams.allPlayers.Count; i++)
        {
            PublicPlayerData publicPlayerData = Teams.allPlayers[i];

            accelerations[i] = publicPlayerData.playerComponents.movementValues.forwardAcceleration;
            deccelerations[i] = publicPlayerData.playerComponents.movementValues.forwardDeceleration;
            maxSpeeds[i] = publicPlayerData.maxSpeed;
            rotationSpeeds[i] = publicPlayerData.playerComponents.maxSpeedRotation;
            float maximumJumpHeight = 0;
            if (publicPlayerData.maximumJumpHeights.Count > 0)
            {

                maximumJumpHeight = publicPlayerData.maximumJumpHeights.Keys[0];
            }
            jumpHeights[i] = maximumJumpHeight;
            desiredSpeeds[i] = publicPlayerData.maxSpeed;
            maxAngleForRuns[i] = publicPlayerData.playerComponents.movementValues.maxAngleForRun;
            minSpeedForRotates[i] = publicPlayerData.playerComponents.movementValues.minSpeedForRotateBody;
            playerPositions[i] = publicPlayerData.position;
            playerVelocities[i] = publicPlayerData.playerComponents.Velocity;
            playerDirections[i] = publicPlayerData.playerComponents.bodyY0Forward;
            scopes[i] = publicPlayerData.playerComponents.scope;
        }

        // Ejecutar el Job
        var job = new PlayerInterceptionJob
        {
            ballPositions = ballPositions,
            ballTimes = ballTimes,
            reachableIndex = reachableIndices,
            timePlayerToReachIndex = timeToReach,
            accelerations = accelerations,
            deccelerations = deccelerations,
            maxSpeeds = maxSpeeds,
            rotationSpeeds = rotationSpeeds,
            jumpHeights = jumpHeights,
            desiredSpeeds = desiredSpeeds,
            maxAngleForRuns = maxAngleForRuns,
            minSpeedForRotates = minSpeedForRotates,
            scopes = scopes,
            playerPositions = playerPositions,
            playerVelocities = playerVelocities,
            playerDirections = playerDirections,
        };

        var handle = job.Schedule(Teams.allPlayers.Count, 1);
        handle.Complete();
    }
    void testKick()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            //testKick2();
            testMovePlayers();
        }
    }
    void setPlayersTarget2()
    {
        for (int i = 0; i < Teams.allPlayers.Count; i++)
        {
                PublicPlayerData publicPlayerData = Teams.allPlayers[i];
                Vector3 pos = MatchComponents.ballPosition;
                pos.y = 0;
                publicPlayerData.playerComponents.movementCtrl.SetTargetPosition(pos);
            
        }
    }
    void setPlayersTarget()
    {
        for (int i = 0; i < Teams.allPlayers.Count; i++)
        {
            if (i >= reachableIndices.Length) break;

            int index = reachableIndices[i];
            if (index >= 0)
            {
                PublicPlayerData publicPlayerData = Teams.allPlayers[i];
                Vector3 pos = trajectory.positions[index];
                pos.y = 0;
                publicPlayerData.playerComponents.movementCtrl.SetTargetPosition(pos);
            }
        }
    }
   
    void ResizeIfNeeded()
    {
        int trajectoryLength = trajectory.positions.Count;
        int playerCount = Teams.allPlayers.Count;

        // Trajectory arrays
        if (!ballPositions.IsCreated || ballPositions.Length != trajectoryLength)
        {
            if (ballPositions.IsCreated) ballPositions.Dispose();
            ballPositions = new NativeArray<float3>(trajectoryLength, Allocator.Persistent);
        }

        if (!ballTimes.IsCreated || ballTimes.Length != trajectoryLength)
        {
            if (ballTimes.IsCreated) ballTimes.Dispose();
            ballTimes = new NativeArray<float>(trajectoryLength, Allocator.Persistent);
        }

        // Player-related arrays
        if (!reachableIndices.IsCreated || reachableIndices.Length != playerCount)
        {
            DisposePlayerArrays();

            reachableIndices = new NativeArray<int>(playerCount, Allocator.Persistent);
            timeToReach = new NativeArray<float>(playerCount, Allocator.Persistent);
            accelerations = new NativeArray<float>(playerCount, Allocator.Persistent);
            deccelerations = new NativeArray<float>(playerCount, Allocator.Persistent);
            maxSpeeds = new NativeArray<float>(playerCount, Allocator.Persistent);
            rotationSpeeds = new NativeArray<float>(playerCount, Allocator.Persistent);
            jumpHeights = new NativeArray<float>(playerCount, Allocator.Persistent);
            desiredSpeeds = new NativeArray<float>(playerCount, Allocator.Persistent);
            minSpeedForRotates = new NativeArray<float>(playerCount, Allocator.Persistent);
            maxAngleForRuns = new NativeArray<float>(playerCount, Allocator.Persistent);
            scopes = new NativeArray<float>(playerCount, Allocator.Persistent);
            playerPositions = new NativeArray<float3>(playerCount, Allocator.Persistent);
            playerVelocities = new NativeArray<float3>(playerCount, Allocator.Persistent);
            playerDirections = new NativeArray<float3>(playerCount, Allocator.Persistent);
        }
    }

    void DisposePlayerArrays()
    {
        if (reachableIndices.IsCreated) reachableIndices.Dispose();
        if (timeToReach.IsCreated) timeToReach.Dispose();
        if (accelerations.IsCreated) accelerations.Dispose();
        if (deccelerations.IsCreated) deccelerations.Dispose();
        if (maxSpeeds.IsCreated) maxSpeeds.Dispose();
        if (rotationSpeeds.IsCreated) rotationSpeeds.Dispose();
        if (jumpHeights.IsCreated) jumpHeights.Dispose();
        if (desiredSpeeds.IsCreated) desiredSpeeds.Dispose();
        if (minSpeedForRotates.IsCreated) minSpeedForRotates.Dispose();
        if (maxAngleForRuns.IsCreated) maxAngleForRuns.Dispose();
        if (scopes.IsCreated) scopes.Dispose();
        if (playerPositions.IsCreated) playerPositions.Dispose();
        if (playerVelocities.IsCreated) playerVelocities.Dispose();
        if (playerDirections.IsCreated) playerDirections.Dispose();
    }
    void testKick3()
    {

        EditorApplication.isPaused = true;
        Time.timeScale = timeScale;
        MatchComponents.ballComponents.rigBall.velocity = forceTransform.forward * force;
        timeDebug = 0;
        Calculate();
    }
    void testKick2()
    {
        //MatchComponents.ballComponents.rigBall.velocity = forceTransform.forward * force;
        //Time.timeScale = timeScale;
        testKick3();
        enablePlayersGoTarget = true;
        //Calculate();
        setPlayersTarget();
    }
    void testMovePlayers()
    {
        for (int i = 0; i < Teams.allPlayers.Count; i++)
        {
            PublicPlayerData publicPlayerData = Teams.allPlayers[i];
            Vector3 pos = publicPlayerData.position + publicPlayerData.bodyTransform.forward*10;
            pos.y = 0;
            if (publicPlayerData.playerComponents.movementCtrl != null)
            {
                publicPlayerData.playerComponents.movementCtrl.SetTargetPosition(pos);
            }
        }
        Invoke(nameof(testKick2), 1);
    }
    public void getClosePlayerBall(out PublicPlayerData publicPlayerDataResult,out float playerReachTimeResult, out Vector3 ballPositionResult )
    {
        publicPlayerDataResult = null;
        ballPositionResult = Vector3.positiveInfinity;
        playerReachTimeResult = Mathf.Infinity;
        int indexResult=-1;
        for (int i = 0; i < Teams.allPlayers.Count; i++)
        {
            if (i >= reachableIndices.Length) break;

            int index = reachableIndices[i];
            if (index >= 0)
            {
                float playerReachTime = timeToReach[i];
                if (playerReachTime <= playerReachTimeResult)
                {
                    indexResult = index;
                    playerReachTimeResult = playerReachTime;
                    ballPositionResult = ballPositions[index];
                    publicPlayerDataResult = Teams.allPlayers[i];
                    
                }
            }
        }
    }

    void OnDestroy()
    {
        if (ballPositions.IsCreated) ballPositions.Dispose();
        if (ballTimes.IsCreated) ballTimes.Dispose();
        DisposePlayerArrays();
    }
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && debug)
        {
            for (int i = 0; i < trajectory.positions.Count; i++)
            {
                float t = trajectory.times[i];
                if (t <= timeDebug) {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(trajectory.positions[i], 0.1f);
                    GUIStyle style = new GUIStyle();
                    style.fontSize = 12;
                    style.normal.textColor = Color.white;
                    //string info =i +"-"+ trajectory.times[i].ToString("f2");
                    string info =i.ToString();
                    Handles.Label(trajectory.positions[i] + Vector3.up * 0.5f, info, style);
                }
               

            }
        }
    }
}