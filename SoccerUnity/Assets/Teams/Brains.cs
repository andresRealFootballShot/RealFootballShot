using CullPositionPoint;
using FieldTriangleV2;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Brains : MonoBehaviour
{
    public CullPassPoints CullPassPoints;
    public bool debug;
    Team attackTeam { get => CullPassPoints.attackTeam; }
    Team defenseTeam { get => CullPassPoints.defenseTeam; }
    public SearchPlayData searchPlayData { get => CullPassPoints.searchPlayData; }
    public List<LonelyPointElement2> firstReachLonelyPoints = new List<LonelyPointElement2>();
    LonelyPointElement2 currentFirstLonelyPoint,nextLonelyPoint;
    PublicPlayerData passerPublicPlayerData;
    Vector3 ballReachPosition;

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
    void checkFirstReachBall()
    {

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
    }
    void Attack_IsOn()
    {
        Passer();
        AttackersGoToLonelyPoint();
    }
    void Passer()
    {
        PublicPlayerData publicPlayerData = passerPublicPlayerData;
        if (!publicPlayerData.IsGoalkeeper)
        {
            publicPlayerData.playerComponents.scope = publicPlayerData.playerComponents.movementCtrl.ballScope;
            publicPlayerData.playerComponents.movementCtrl.SetTargetPosition(ballReachPosition);
            if (publicPlayerData.IsBot)
            {
                LonelyPointElement2 lonelyPoint = currentFirstLonelyPoint;
                PassData straightPassData = lonelyPoint.straightPassData;
                if (lonelyPoint.straightReachBall && publicPlayerData.Kick(straightPassData))
                {
                   changedPass = true;
                   currentFirstLonelyPoint = CullPassPoints.firstReachLonelyPoints[passIndex];
                }
                else
                {
                        if (lonelyPoint.parabolicReachBall)
                        {
                            PassData parabolicPassData = lonelyPoint.parabolicPassData;
                            if (publicPlayerData.Kick(parabolicPassData))
                            {
                                changedPass = true;
                                currentFirstLonelyPoint = CullPassPoints.firstReachLonelyPoints[passIndex];
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

    }
    void AttackersGoToLonelyPoint()
    {
        LonelyPointElement2 lonelyPointElement = currentFirstLonelyPoint;
        PublicPlayerData publicPlayerData = CullPassPoints.GetPublicPlayerData(lonelyPointElement.attackReachIndex);
        publicPlayerData = passerPublicPlayerData != publicPlayerData ? publicPlayerData : null;
        if (publicPlayerData!=null && !publicPlayerData.IsGoalkeeper && publicPlayerData.IsBot)
        {
            publicPlayerData.playerComponents.botMoveFunctions.SetTarget_AvoidOffside(publicPlayerData, lonelyPointElement);
        }
    }
    Vector3 GetPlayerTargetPosition(PublicPlayerData publicPlayerData,int node)
    {
        int index = CullPassPoints.players.IndexOf(publicPlayerData);
        return searchPlayData.GetPlayerTargetPosition(node, index, 0);
    }
    public void GetCullPassPointData()
    {
        if (!cullPassPointEnable||  CullPassPoints.firstReachLonelyPoints.Count <= passIndex || !changedPass) return;
        
        nextLonelyPoint = CullPassPoints.firstReachLonelyPoints[passIndex];
        passerPublicPlayerData = CullPassPoints.firstPublicPlayerData;
        ballReachPosition = CullPassPoints.ballReachPosition;
        changedPass = false;
        if (!thereIsCurrentData)
        {
            currentFirstLonelyPoint = nextLonelyPoint;
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

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(currentFirstLonelyPoint.Get3DPosition(0), 0.2f);
        }
    }
}
