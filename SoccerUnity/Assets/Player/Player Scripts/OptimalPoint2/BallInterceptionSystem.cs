using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEditor;

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

    private NativeArray<float3> playerPositions;
    private NativeArray<float3> playerVelocities;
    private NativeArray<float3> playerDirections;
    float timeDebug;
    void Update()
    {
        testKick();
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
            playerPositions[i] = publicPlayerData.position;
            playerVelocities[i] = publicPlayerData.playerComponents.Velocity;
            playerDirections[i] = publicPlayerData.playerComponents.bodyY0Forward;
        }

        // Ejecutar el Job
        var job = new PlayerInterceptionJob
        {
            ballPositions = ballPositions,
            ballTimes = ballTimes,
            reachableIndex = reachableIndices,
            timeToReachIndex = timeToReach,
            accelerations = accelerations,
            deccelerations = deccelerations,
            maxSpeeds = maxSpeeds,
            rotationSpeeds = rotationSpeeds,
            jumpHeights = jumpHeights,
            desiredSpeeds = desiredSpeeds,
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
            //testMovePlayers();
            testKick3();
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
                publicPlayerData.playerComponents.movementCtrl.SetTargetPosition(trajectory.positions[index]);
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
        if (playerPositions.IsCreated) playerPositions.Dispose();
        if (playerVelocities.IsCreated) playerVelocities.Dispose();
        if (playerDirections.IsCreated) playerDirections.Dispose();
    }
    void testKick3()
    {
        MatchComponents.ballComponents.rigBall.velocity = forceTransform.forward * force;
    }
    void testKick2()
    {
        MatchComponents.ballComponents.rigBall.velocity = forceTransform.forward * force;
        Time.timeScale = timeScale;
        timeDebug = 0;

        Calculate();
        setPlayersTarget();
    }
    void testMovePlayers()
    {
        for (int i = 0; i < Teams.allPlayers.Count; i++)
        {
            PublicPlayerData publicPlayerData = Teams.allPlayers[i];
            Vector3 pos = publicPlayerData.position + publicPlayerData.bodyTransform.forward*10;
            if (publicPlayerData.playerComponents.movementCtrl != null)
            {
                publicPlayerData.playerComponents.movementCtrl.SetTargetPosition(pos);
            }
        }
        Invoke(nameof(testKick2), 1);
    }
    public void getClosePlayerBall(out PublicPlayerData publicPlayerDataResult,out float ballTimeResult, out float playerReachTimeResult, out Vector3 ballPositionResult )
    {
        publicPlayerDataResult = null;
        ballTimeResult = Mathf.Infinity;
        ballPositionResult = Vector3.positiveInfinity;
        playerReachTimeResult = Mathf.Infinity;
        int indexResult=-1;
        for (int i = 0; i < Teams.allPlayers.Count; i++)
        {
            if (i >= reachableIndices.Length) break;

            int index = reachableIndices[i];
            if (index >= 0)
            {
                float ballTime = ballTimes[index];
                float playerReachTime = timeToReach[i];
                if (ballTime <= ballTimeResult && playerReachTime <= playerReachTimeResult)
                {
                    indexResult = index;
                    ballTimeResult = ballTime;
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
                }
               

            }
        }
    }
}