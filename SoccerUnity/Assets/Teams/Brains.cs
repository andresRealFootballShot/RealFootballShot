using CullPositionPoint;
using DOTS_ChaserDataCalculation;
using FieldTriangleV2;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class Brains : MonoBehaviour
{
    public CullPassPoints CullPassPoints;
    public FootballPositionCtrl FootballPositionCtrl;
    public bool debug,debugAttackTeam;
    Team attackTeam;
    Team defenseTeam;
    public SearchPlayData searchPlayData { get => CullPassPoints.searchPlayData; }
    public List<LonelyPointElement2> firstReachLonelyPoints = new List<LonelyPointElement2>();
    public List<PublicPlayerData> busyPlayers = new List<PublicPlayerData>();
    LonelyPointElement2 currentFirstLonelyPoint,nextLonelyPoint;
    PublicPlayerData passerPublicPlayerData;
    Vector3 ballReachPosition;
    public float minReachBallTime=0.5f;
    [Header("Debug")]
    [Space(5)]
    public float timeScale = 0.5f;

    int node = 0;
    int passIndex=0;
    
    bool enable,cullPassPointEnable=true;
    bool changedPass=true,thereIsCurrentData;
    float currentWeight;
    void Start()
    {
        
    }
    void Update()
    {
        if (enable)
        {
            Play();
        }
    }
    public void Enable()
    {
        enable = true;
    }
    public void Play()
    {
        GetTeams();
        //checkFirstReachBall();
        Attack();
        Defense();
    }
    void Attack()
    {
        Attack_IsOn();
    }
    void Defense()
    {
        Defense_IsOn();

    }
    void GetTeams()
    {
        if(attackTeam!= CullPassPoints.attackTeam)
        {
            busyPlayers.Clear();
        }
        attackTeam = CullPassPoints.attackTeam;
        defenseTeam = CullPassPoints.defenseTeam;
    }
    void checkFirstReachBall()
    {
       
        Team cullAttackTeam = CullPassPoints.attackTeam;
        if (!attackTeam.Equals(cullAttackTeam))
        {
            PublicPlayerData firstPublicPlayerData = CullPassPoints.firstPublicPlayerData;
            float ballReachTime = CullPassPoints.firstPlayerReachTime;
            Vector3 ballReachPosition = CullPassPoints.ballReachPosition;
            PublicPlayerData previousPublicPlayerData = CullPassPoints.GetPublicPlayerData(currentFirstLonelyPoint.attackReachIndex);
            Vector3 lonelyPointPosition = currentFirstLonelyPoint.Get3DPosition();
            float distance = Vector3.Distance(ballReachPosition, lonelyPointPosition);
            if (ballReachTime <= minReachBallTime && distance > 4)
            {
                attackTeam = CullPassPoints.attackTeam;
                defenseTeam = CullPassPoints.defenseTeam;
            }
        }

    }
    void Defense_IsOn()
    {

        Team defenseTeam = this.defenseTeam;
        foreach (PublicPlayerData publicPlayerData in defenseTeam.outfieldPublicPlayerDatas)
        {
            if (!publicPlayerData.IsBot) continue;
            Vector3 defensePos = GetPlayerTargetPosition(publicPlayerData, node);
            Vector3 playerPos = publicPlayerData.bodyTransform.position;
            MovementCtrl movementCtrl = publicPlayerData.playerComponents.movementCtrl;
            Vector3 ballPosition = MatchComponents.ballPosition;
            //movementCtrl.scope = 0;
            if (Vector3.Distance(defensePos, playerPos) > publicPlayerData.playerComponents.scope+0.1f)
            {
                movementCtrl.SetTargetPosition(defensePos);
            }
            else
            {
                
                movementCtrl.SetStopped_LookTarget(ballPosition);
            }

        }
        DefenseGoBallReachPosition();
    }
    void DefenseGoBallReachPosition()
    {
        Team defenseTeam = this.defenseTeam;
        float minTime = Mathf.Infinity;
        PublicPlayerData firstDefenseReachBall = null;
        foreach (PublicPlayerData publicPlayerData in defenseTeam.outfieldPublicPlayerDatas)
        {

            float time = GetTimeToReachPointDOTS.accelerationGetTimeToReachPosition2(publicPlayerData.position, publicPlayerData.speed, publicPlayerData.playerComponents.bodyY0Forward, publicPlayerData.playerComponents.VelocityY0Direction,ballReachPosition,publicPlayerData.playerComponents.movementValues.maxAngleForRun, publicPlayerData.playerComponents.movementValues.maxAngleForRun2, publicPlayerData.playerComponents.movementValues.minSpeedForRotateBody, publicPlayerData.playerComponents.movementValues.minSpeedForRotateBody2, publicPlayerData.playerComponents.movementValues.forwardAcceleration, publicPlayerData.playerComponents.movementValues.forwardDeceleration, publicPlayerData.playerComponents.movementValues.maxSpeedForReachBall, publicPlayerData.playerComponents.scope, publicPlayerData.playerComponents.MaxSpeed);
            if (time < minTime)
            {
                firstDefenseReachBall = publicPlayerData;
                minTime = time;
            }
        }
        if (firstDefenseReachBall != null)
        {
            firstDefenseReachBall.playerComponents.movementCtrl.SetTargetPosition(ballReachPosition);
        }
    }
    void Attack_IsOn()
    {
        Attack_DefaultPosition();
        Passer();
        AttackersGoToLonelyPoint();
    }
    void Attack_DefaultPosition()
    {
        Team team = attackTeam;
        foreach (PublicPlayerData publicPlayerData in team.outfieldPublicPlayerDatas)
        {
            if (busyPlayers.Contains(publicPlayerData)) continue;
            if (!FootballPositionCtrl.GetFieldPositionDataPosition("Default", "Default Attack", publicPlayerData, MatchComponents.ballPosition, out Vector3 targetPosition)) continue;
            publicPlayerData.SetTargetPosition(targetPosition);
        }
    }
    bool wait=true;
    float delay=0.5f;
    void setWaitTrue() => wait = true;
    void Passer()
    {
        if (!CheckGoalKick())
        {
            PublicPlayerData publicPlayerData = passerPublicPlayerData;
            if (!publicPlayerData.IsGoalkeeper&&publicPlayerData.IsBot)
            {
                publicPlayerData.playerComponents.scope =  publicPlayerData.playerComponents.movementCtrl.defaultScope;
                publicPlayerData.playerComponents.movementCtrl.SetTargetPosition(ballReachPosition);
                LonelyPointElement2 lonelyPoint = currentFirstLonelyPoint;
                PassData straightPassData = lonelyPoint.straightPassData;
                if (publicPlayerData.playerComponents.BodyBallXZDistance <= publicPlayerData.playerComponents.ballScope+0.25f && wait)
                {
                    //EditorApplication.isPaused = true;
                    wait=false;
                    Invoke(nameof(setWaitTrue), delay);
                }
                if (!lonelyPoint.parabolicReachBall && publicPlayerData.Kick(straightPassData))
                {
                    Kick();
                    //currentFirstLonelyPoint = CullPassPoints.firstReachLonelyPoints[passIndex];
                    //EditorApplication.isPaused = true;
                    //Time.timeScale = timeScale;
                }
                else
                {
                    if (lonelyPoint.parabolicReachBall)
                    {
                        PassData parabolicPassData = lonelyPoint.parabolicPassData;
                        if (publicPlayerData.Kick(parabolicPassData))
                        {
                            Kick();
                            //EditorApplication.isPaused = true;
                            //Time.timeScale = timeScale;
                            //currentFirstLonelyPoint = CullPassPoints.firstReachLonelyPoints[passIndex];
                        }
                        
                        //EditorApplication.isPaused = true;
                    }
#if UNITY_EDITOR

                    //EditorApplication.isPaused = true;
#endif
                }
            }
        }
    }
    void Kick()
    {
        Invoke(nameof(ChangedPass), 0.2f);
        busyPlayers.Clear();
    }
    void AttackersGoToLonelyPoint()
    {
        LonelyPointElement2 lonelyPointElement = currentFirstLonelyPoint;
        PublicPlayerData publicPlayerData = CullPassPoints.GetPublicPlayerData(lonelyPointElement.attackReachIndex);
        publicPlayerData = passerPublicPlayerData != publicPlayerData ? publicPlayerData : null;
        if (publicPlayerData != null && !publicPlayerData.IsGoalkeeper && publicPlayerData.IsBot)
        {
            publicPlayerData.playerComponents.botMoveFunctions.SetTarget_AvoidOffside(publicPlayerData, lonelyPointElement);
            if(!busyPlayers.Contains(publicPlayerData))
                busyPlayers.Add(publicPlayerData);
        }
    }
    Vector3 GetPlayerTargetPosition(PublicPlayerData publicPlayerData,int node)
    {
        int index = CullPassPoints.players.IndexOf(publicPlayerData);
        return searchPlayData.GetPlayerTargetPosition(node, index, 0);
    }
    void ChangedPass()
    {
        changedPass = true;
    }
    public void GetCullPassPointData()
    {
        if (!cullPassPointEnable||  CullPassPoints.firstReachLonelyPoints.Count <= passIndex || !changedPass&&false) return;
        
        nextLonelyPoint = CullPassPoints.firstReachLonelyPoints[passIndex];
        passerPublicPlayerData = CullPassPoints.firstPublicPlayerData;
        float distance = Vector3.Distance(ballReachPosition, CullPassPoints.ballReachPosition);
        
        ballReachPosition = CullPassPoints.ballReachPosition;
        changedPass = false;
        currentFirstLonelyPoint = nextLonelyPoint;
        if (!thereIsCurrentData)
        {
            
            thereIsCurrentData = true;
        }
        //cullPassPointEnable = false;
    }

    public bool CheckGoalKick()
    {
        Vector2 ballPosition = new Vector2(MatchComponents.ballPosition.x, MatchComponents.ballPosition.z);
        Team rivalTeam = passerPublicPlayerData.rivalTeam;
        GoalComponents goalComponents = rivalTeam.SideOfField.goalComponents;
        Vector2 left = new Vector2(goalComponents.left.position.x, goalComponents.left.position.z);
        Vector2 right = new Vector2(goalComponents.right.position.x, goalComponents.right.position.z);
        Vector2 center = new Vector2(goalComponents.centerOptimalPosition.position.x, goalComponents.centerOptimalPosition.position.z);
        float distance = Vector3.Distance(ballPosition, center);
        Vector2 midfield = new Vector2(MatchComponents.footballField.center.x, MatchComponents.footballField.center.z);
        float maxFieldDistance = Vector2.Distance(center, midfield) * 2;

        Vector2 dir1 = left - ballPosition;
        Vector2 dir2 = right - ballPosition;
        float angle = Vector2.Angle(dir1, dir2);
        float weight2 = (angle / 21) + (1-(distance/21));
        currentWeight = CullPassPointsJob.EvaluatePosition(ballPosition,left,right,ballPosition,0, maxFieldDistance);
        float maxWeight = getMaxWeight(0);
        
        if (passerPublicPlayerData.playerComponents.botKick!=null&&currentWeight > maxWeight-5&& (isLookingToGoal(goalComponents)||true) &&passerPublicPlayerData.ReachBall())
        {
            if (CullPassPoints.bestShot.valid)
            {

                MatchComponents.ballRigidbody.velocity = CullPassPoints.bestShot.v0;
            }
            return CullPassPoints.bestShot.valid;
        }
        else
        {
            return false;
        }
    }
    bool isLookingToGoal(GoalComponents goalComponents)
    {
        Vector2 center = new Vector2(goalComponents.centerOptimalPosition.position.x, goalComponents.centerOptimalPosition.position.z);
        Vector2 ballPosition = new Vector2(MatchComponents.ballPosition.x, MatchComponents.ballPosition.z);
        Vector2 playerPos = new Vector2(passerPublicPlayerData.position.x,passerPublicPlayerData.position.z);
        Vector2 dir1 = center- ballPosition;
        Vector2 dir2 = ballPosition - playerPos;
        float angle = Vector2.Angle(dir1, dir2);
        float angle2 = Vector2.Angle(dir1, passerPublicPlayerData.playerComponents.bodyY0Forward);
        return angle < 90 && angle2<90;
    }
    public float getMaxWeight(int node)
    {
        float maxWeight = Mathf.NegativeInfinity;
        for (int i = 0; i < CullPassPoints.firstReachLonelyPoints.Count; i++)
        {
            LonelyPointElement2 lonelyPointElement2 = CullPassPoints.firstReachLonelyPoints[i];
            if (lonelyPointElement2.weight > maxWeight)
            {
                maxWeight = lonelyPointElement2.weight;
            }
        }
        return maxWeight;
    }
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && debug)
        {
            Color color = new Color(0.5f, 0.2f, 0.9f, 1);
            Vector3 position = currentFirstLonelyPoint.Get3DPosition();
            Gizmos.color = color;
            Gizmos.DrawSphere(position + Vector3.up * 0.25f, 0.2f);
            GUIStyle style = new GUIStyle();
            style.fontSize = 16;
            style.normal.textColor = color;
            string info = "firstLonelyPoint";
            Handles.Label(position + Vector3.up * 1.4f, info, style);

            string info2 = "ballReachPosition";
            Handles.Label(ballReachPosition + Vector3.up * 1.5f, info2, style);

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(currentFirstLonelyPoint.Get3DPosition(0), 0.2f);

            
            if (passerPublicPlayerData != null)
            {
                Vector3 passerPos = passerPublicPlayerData.position;
                bool passerAvailable = passerPublicPlayerData.playerComponents.botMoveFunctions.CheckPasserAvailable();
                int index = CullPassPoints.players.IndexOf(passerPublicPlayerData);
                info = "passer " + index + " avaliable " + passerAvailable + " | " + passerPublicPlayerData.playerID;
                Handles.Label(passerPos + Vector3.up * 1.6f, info, style);
            }
            PublicPlayerData playerMakinRun = CullPassPoints.GetPublicPlayerData(currentFirstLonelyPoint.attackReachIndex);
            if(playerMakinRun != null)
            {
                Vector3 playerMakinRunPos = playerMakinRun.position;
                info = "player " + currentFirstLonelyPoint.attackReachIndex + " Makin a Run | " + playerMakinRun.playerID;
                style.normal.textColor = new Color(0.8f, 0.5f, 0.9f);
                Handles.Label(playerMakinRunPos + Vector3.up * 1.5f, info, style);
            }

            if (CullPassPoints.firstPublicPlayerData != null&& attackTeam != null)
            {

                position = CullPassPoints.firstPublicPlayerData.position;

                style.normal.textColor = attackTeam.Color;

                info = "firstPlayerReachBall";
                Handles.Label(position + Vector3.up * 0.7f, info, style);
            }

            Team team = attackTeam;
            if (team != null&& debugAttackTeam)
            {
                foreach (PublicPlayerData publicPlayerData in team.outfieldPublicPlayerDatas)
                {
                    if (!FootballPositionCtrl.GetFieldPositionDataPosition("Default", "Default Attack", publicPlayerData, MatchComponents.ballPosition, out Vector3 targetPosition)) continue;
                    if (!team.getTypeFieldPositionOfPlayer(publicPlayerData.playerID, out TypeFieldPosition.Type fieldPositionType)) continue;
                    info = fieldPositionType.ToString();
                    Handles.Label(targetPosition + Vector3.up * 0.5f, info, style);
                }
            }
            

                position = MatchComponents.ballPosition;

                style.normal.textColor = new Color(0.4f, 0.7f, 0.9f);
                info = "weight=" + currentWeight * 100;
                Handles.Label(position + Vector3.up * 0.5f, info, style);
            if (true)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(CullPassPoints.bestShot.target, 0.2f);
            }
        }
    }
}
