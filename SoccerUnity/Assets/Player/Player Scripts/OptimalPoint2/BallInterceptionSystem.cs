using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEditor;
using static UnityEditor.PlayerSettings;
using System.Reflection;

public class BallInterceptionSystem : MonoBehaviour
{
    public BallTrajectorySimulator trajectory;
    [Header("Debug")]
    public bool debug;
    public bool debugTargetPosition,debugPlayerReach,debugTrayectoryPositions;
    public Transform forceTransform;
    public float force=10;
    public float timeScale=1;
    public float distanceTestPlayer = 10;
    public float timeTestPlayer = 1;
    public float maxSpeedForReachBall,startSpeed;
    private NativeArray<float3> ballPositions;
    private NativeArray<float> ballTimes;

    public NativeArray<int> reachableIndices;
    private NativeArray<float> timeToReach;
    private NativeArray<float3> endPlayerDirections;

    private NativeArray<float> accelerations;
    private NativeArray<float> deccelerations;
    private NativeArray<float> maxSpeeds;
    private NativeArray<float> rotationSpeeds;
    private NativeArray<float> jumpHeights;
    private NativeArray<float> reachBallSpeeds;
    public NativeArray<float> maxAngleForRuns;
    public NativeArray<float> minSpeedForRotates;
    public NativeArray<float> scopes;
    public NativeArray<float> maxAngleForRuns2;
    public NativeArray<float> minSpeedForRotates2;
    public NativeArray<float> kickPeriods;
    public NativeArray<float> kickRecoverTimes;
    private NativeArray<float3> playerPositions;
    private NativeArray<float3> playerVelocities;
    private NativeArray<float3> playerDirections;
    private NativeArray<bool> isGoalkeepers;
    public float timeDebug;
    bool enablePlayersGoTarget;
    private void Start()
    {
        //testKick3();

    }
    void Update()
    {
        //testKick();

        //Calculate();
#if UNITY_EDITOR
        if (debug)
        {
            
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
            jumpHeights[i] = 1.8f;
            reachBallSpeeds[i] = publicPlayerData.movimentValues.maxSpeedForReachBall;
            maxAngleForRuns[i] = publicPlayerData.playerComponents.movementValues.maxAngleForRun;
            maxAngleForRuns2[i] = publicPlayerData.playerComponents.movementValues.maxAngleForRun2;
            minSpeedForRotates[i] = publicPlayerData.playerComponents.movementValues.minSpeedForRotateBody;
            minSpeedForRotates2[i] = publicPlayerData.playerComponents.movementValues.minSpeedForRotateBody2;
            playerPositions[i] = publicPlayerData.position;
            playerVelocities[i] = publicPlayerData.playerComponents.Velocity;
            playerDirections[i] = publicPlayerData.playerComponents.bodyY0Forward;
            scopes[i] = publicPlayerData.playerComponents.ballScope;
            isGoalkeepers[i] = publicPlayerData.IsGoalkeeper;
            kickPeriods[i] = publicPlayerData.playerComponents.botKick != null ? publicPlayerData.playerComponents.botKick.kickPeriod :-1;
            
            kickRecoverTimes[i] = publicPlayerData.playerComponents.botKick!=null&& publicPlayerData.playerComponents.botKick.startKickTime!=-1 ? Time.time - publicPlayerData.playerComponents.botKick.startKickTime : -1;
        }

        // Ejecutar el Job
        var job = new PlayerInterceptionJob
        {
            ballPositions = ballPositions,
            ballTimes = ballTimes,
            reachableIndex = reachableIndices,
            timePlayerToReachIndex = timeToReach,
            endPlayerDirections = endPlayerDirections,
            accelerations = accelerations,
            deccelerations = deccelerations,
            maxSpeeds = maxSpeeds,
            rotationSpeeds = rotationSpeeds,
            jumpHeights = jumpHeights,
            reachBallSpeeds = reachBallSpeeds,
            maxAngleForRuns = maxAngleForRuns,
            minSpeedForRotates = minSpeedForRotates,
            minSpeedForRotates2 = minSpeedForRotates2,
            maxAngleForRuns2 = maxAngleForRuns2,
            scopes = scopes,
            playerPositions = playerPositions,
            playerVelocities = playerVelocities,
            playerDirections = playerDirections,
            isGoalkeepers = isGoalkeepers,
            kickPeriods = kickPeriods,
            kickRecoverTimes=kickRecoverTimes
        };

        var handle = job.Schedule(Teams.allPlayers.Count, 1);
        handle.Complete();
        UpdateChaserData();
    }
    void UpdateChaserData()
    {
        for (int i = 0; i < Teams.allPlayers.Count; i++)
        {
            PublicPlayerData publicPlayerData = Teams.allPlayers[i];
            publicPlayerData.getFirstChaserData(out ChaserData chaserData);
            int ballPosIndex = reachableIndices[i];
            chaserData.ReachTheTarget = ballPosIndex != -1;
            if (ballPosIndex != -1)
            {
                chaserData.OptimalPoint = ballPositions[ballPosIndex];
                chaserData.ClosestPoint = ballPositions[ballPosIndex];
                chaserData.OptimalTime = timeToReach[i];
                chaserData.OptimalTargetTime = ballTimes[ballPosIndex];
            }
        }
    }
    void testKick()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            //testKick2();
            testMovePlayers2();
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
   void printTimes()
    {
        for (int i = 0; i < Teams.allPlayers.Count; i++)
        {
            PublicPlayerData publicPlayerData = Teams.allPlayers[i];
            print("Prediction Player "+publicPlayerData.playerName + " reachTime="+ timeToReach[i]);
        }
    }
    
    void testKick3()
    {

        EditorApplication.isPaused = true;
        Time.timeScale = timeScale;
        MatchComponents.ballComponents.rigBall.velocity = forceTransform.forward * force;
        timeDebug = 0;

        for (int i = 0; i < Teams.allPlayers.Count; i++)
        {
            PublicPlayerData publicPlayerData = Teams.allPlayers[i];
            publicPlayerData.movimentValues.maxSpeedForReachBall = maxSpeedForReachBall;
        }

        Calculate();
    }
    void testKick2()
    {
        testKick3();
        setPlayersTarget();
        //printTimes();
    }
    void testMovePlayers2()
    {
        for (int i = 0; i < Teams.allPlayers.Count; i++)
        {
            PublicPlayerData publicPlayerData = Teams.allPlayers[i];
            publicPlayerData.playerComponents.movementCtrl.SetInstantVelocity(publicPlayerData.playerComponents.bodyY0Forward, startSpeed);
            publicPlayerData.movimentValues.maxSpeedForReachBall = 10.5f;
        }
        testKick2();
    }
    void testMovePlayers()
    {
        for (int i = 0; i < Teams.allPlayers.Count; i++)
        {
            PublicPlayerData publicPlayerData = Teams.allPlayers[i];
            Vector3 pos = publicPlayerData.position + publicPlayerData.bodyTransform.forward* distanceTestPlayer;
            pos.y = 0;
            if (publicPlayerData.playerComponents.movementCtrl != null)
            {
                publicPlayerData.playerComponents.movementCtrl.SetTargetPosition(pos);
            }
        }
        Invoke(nameof(testKick2), timeTestPlayer);
    }
    public void getFirstPlayerReachBall(out PublicPlayerData publicPlayerDataResult,out float ballReachTimeResult, out Vector3 ballPositionResult,out float endSpeed,out Vector3 endPlayerDirection)
    {
        publicPlayerDataResult = null;
        ballPositionResult = Vector3.positiveInfinity;
        ballReachTimeResult = Mathf.Infinity;
        endSpeed = Mathf.Infinity;
        endPlayerDirection = Vector3.positiveInfinity;
        int indexResult=-1;
        float firstPlayerReachTime = Mathf.Infinity;
        for (int i = 0; i < Teams.allPlayers.Count; i++)
        {
            if (i >= reachableIndices.Length) break;

            int index = reachableIndices[i];
            if (index >= 0)
            {
                float playerReachTime = timeToReach[i];
                float ballTime = ballTimes[index] == Mathf.Infinity ? playerReachTime : ballTimes[index];
                bool kickAvailable = Teams.allPlayers[i].playerComponents.botKick != null ?  Teams.allPlayers[i].playerComponents.botKick.kickAvailable : true;
                if (ballTime < ballReachTimeResult)
                {
                    indexResult = index;
                    ballReachTimeResult = ballTime;
                    ballPositionResult = ballPositions[index];
                    publicPlayerDataResult = Teams.allPlayers[i];
                    firstPlayerReachTime = playerReachTime;
                    endSpeed = reachBallSpeeds[i];
                    endPlayerDirection = endPlayerDirections[i];
                    Teams.getTeamFromPlayer(Teams.allPlayers[i].playerID, out Team team);
                    team.firstReachBallPublicPlayerData = Teams.allPlayers[i];
                    team.firstReachBallTime = ballTime;
                }
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
            reachBallSpeeds = new NativeArray<float>(playerCount, Allocator.Persistent);
            minSpeedForRotates = new NativeArray<float>(playerCount, Allocator.Persistent);
            minSpeedForRotates2 = new NativeArray<float>(playerCount, Allocator.Persistent);
            maxAngleForRuns = new NativeArray<float>(playerCount, Allocator.Persistent);
            maxAngleForRuns2 = new NativeArray<float>(playerCount, Allocator.Persistent);
            scopes = new NativeArray<float>(playerCount, Allocator.Persistent);
            playerPositions = new NativeArray<float3>(playerCount, Allocator.Persistent);
            playerVelocities = new NativeArray<float3>(playerCount, Allocator.Persistent);
            playerDirections = new NativeArray<float3>(playerCount, Allocator.Persistent);
            endPlayerDirections = new NativeArray<float3>(playerCount, Allocator.Persistent);
            isGoalkeepers = new NativeArray<bool>(playerCount, Allocator.Persistent);
            kickPeriods = new NativeArray<float>(playerCount, Allocator.Persistent);
            kickRecoverTimes = new NativeArray<float>(playerCount, Allocator.Persistent);
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
        if (reachBallSpeeds.IsCreated) reachBallSpeeds.Dispose();
        if (minSpeedForRotates.IsCreated) minSpeedForRotates.Dispose();
        if (minSpeedForRotates2.IsCreated) minSpeedForRotates2.Dispose();
        if (maxAngleForRuns.IsCreated) maxAngleForRuns.Dispose();
        if (maxAngleForRuns2.IsCreated) maxAngleForRuns2.Dispose();
        if (scopes.IsCreated) scopes.Dispose();
        if (playerPositions.IsCreated) playerPositions.Dispose();
        if (playerVelocities.IsCreated) playerVelocities.Dispose();
        if (playerDirections.IsCreated) playerDirections.Dispose();
        if (endPlayerDirections.IsCreated) endPlayerDirections.Dispose();
        if (isGoalkeepers.IsCreated) isGoalkeepers.Dispose();
        if (kickPeriods.IsCreated) kickPeriods.Dispose();
        if (kickRecoverTimes.IsCreated) kickRecoverTimes.Dispose();
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
            if (debugTrayectoryPositions)
            {
                for (int i = 0; i < trajectory.positions.Count; i++)
                {
                    float t = trajectory.times[i];
                    if (t <= timeDebug)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawSphere(trajectory.positions[i], 0.1f);
                        GUIStyle style = new GUIStyle();
                        style.fontSize = 12;
                        style.normal.textColor = Color.white;
                        //string info =i +"-"+ trajectory.times[i].ToString("f2");
                        string info = i.ToString();
                        Handles.Label(trajectory.positions[i] + Vector3.up * 0.5f, info, style);
                    }


                }
            }
                
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
                    if (debugPlayerReach)
                        Debug.DrawLine(Teams.allPlayers[i].position, trajectory.positions[index], Color.green);
                   
                }
                if (debugTargetPosition)
                    Debug.DrawLine(Teams.allPlayers[i].position, Teams.allPlayers[i].playerComponents.TargetPosition, Color.red);



            }
            timeDebug += Time.deltaTime;
        }
    }

}