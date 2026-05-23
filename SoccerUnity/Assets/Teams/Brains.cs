using CullPositionPoint;
using DOTS_ChaserDataCalculation;
using FieldTriangleV2;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class Brains : MonoBehaviour
{
    public CullPassPoints CullPassPoints;
    public bool debug;
    Team attackTeam;
    Team defenseTeam;
    public SearchPlayData searchPlayData { get => CullPassPoints.searchPlayData; }
    public List<LonelyPointElement2> firstReachLonelyPoints = new List<LonelyPointElement2>();
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
            movementCtrl.scope = 0;
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
        Passer();
        AttackersGoToLonelyPoint();
    }
    void Passer()
    {
        PublicPlayerData publicPlayerData = passerPublicPlayerData;
        if (!publicPlayerData.IsGoalkeeper && publicPlayerData.IsBot)
        {
            publicPlayerData.playerComponents.scope = publicPlayerData.playerComponents.movementCtrl.ballScope;
            publicPlayerData.playerComponents.movementCtrl.SetTargetPosition(ballReachPosition);
            LonelyPointElement2 lonelyPoint = currentFirstLonelyPoint;
            PassData straightPassData = lonelyPoint.straightPassData;
            if (lonelyPoint.straightReachBall && publicPlayerData.Kick(straightPassData))
            {
                Invoke(nameof(ChangedPass), 0.2f);
                //currentFirstLonelyPoint = CullPassPoints.firstReachLonelyPoints[passIndex];
            }
            else
            {
                    if (lonelyPoint.parabolicReachBall)
                    {
                        PassData parabolicPassData = lonelyPoint.parabolicPassData;
                        if (publicPlayerData.Kick(parabolicPassData))
                        {
                            Invoke(nameof(ChangedPass), 0.2f);
                            //currentFirstLonelyPoint = CullPassPoints.firstReachLonelyPoints[passIndex];
                        }
                        Time.timeScale = timeScale;
                        //EditorApplication.isPaused = true;
                    }
#if UNITY_EDITOR

                //EditorApplication.isPaused = true;
#endif
            }
        }

    }
    void AttackersGoToLonelyPoint()
    {
        LonelyPointElement2 lonelyPointElement = currentFirstLonelyPoint;
        PublicPlayerData publicPlayerData = CullPassPoints.GetPublicPlayerData(lonelyPointElement.attackReachIndex);
        publicPlayerData = passerPublicPlayerData != publicPlayerData ? publicPlayerData : null;
        if (publicPlayerData != null && !publicPlayerData.IsGoalkeeper && publicPlayerData.IsBot)
        {
            publicPlayerData.playerComponents.botMoveFunctions.SetTarget_AvoidOffside(publicPlayerData, lonelyPointElement);
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

            if (CullPassPoints.firstPublicPlayerData != null)
            {

                position = CullPassPoints.firstPublicPlayerData.position;

                style.normal.textColor = new Color(0.2f, 0.6f, 0.8f);
                info = "firstPlayerReachBall";
                Handles.Label(position + Vector3.up * 0.7f, info, style);
            }
        }
    }
}
