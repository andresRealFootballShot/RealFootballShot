using CullPositionPoint;
using DOTS_ChaserDataCalculation;
using FieldTriangleV2;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using static UnityEngine.Networking.UnityWebRequest;

public class Brains : MonoBehaviour
{
    public CullPassPoints CullPassPoints;
    public FootballPositionCtrl FootballPositionCtrl;
    public bool debug,debugAttackTeam;
    Team attackTeam;
    Team defenseTeam;
    public SearchPlayData searchPlayData { get => CullPassPoints.searchPlayData; }
    public List<LonelyPointElement2> firstReachLonelyPoints { get => CullPassPoints.firstReachLonelyPoints; }
    public List<PublicPlayerData> busyPlayers = new List<PublicPlayerData>();
    LonelyPointElement2 currentFirstLonelyPoint,nextLonelyPoint,attackLonelyPoint;
    public PublicPlayerData passerPublicPlayerData;
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
    Vector3 previousBallDir;
    public float reflex = 0.2f,userReachBallReflex=0.7f, minControlTime = 1f,randomControlTime=2;
    float controlTime;
    float startReflexTime, startUserReflexTime;
    bool reflexAvailable { get => Time.time - startReflexTime >= reflex; }
    bool userReflexAvailable { get => Time.time - startUserReflexTime >= userReachBallReflex; }
    
    string previousPlayerKicker;
    PublicPlayerData firstDefenseReachBall = null;
    float startNextLonelyPointTime;
    public float updateNextLonelyPointPeriod = 0.25f;
    bool updateNextLonelyPoint { get => Time.time - startNextLonelyPointTime >= controlTime; }
    public bool controlPause;

    void Start()
    {
        MatchEvents.kick.AddListener(kick);
    }
    void Update()
    {
        if (enable)
        {
            Play();
        }

        //ChangePlayerType();
    }
    
    void ChangePlayerType()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
           MatchComponents.currentPublicPlayerData.ChangeOtherPlayerType();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            MatchComponents.currentPublicPlayerData.SetNextNoPossesionMode();
        }
    }
    public void Enable()
    {
        enable = true;
    }
    public void Play()
    {
        
        checkReflex();
        UpdateNextLonelyPoint();
        GetTeams();
        //checkFirstReachBall();
        Attack();
        Defense();
    }
    public LonelyPointElement2 GetReachableLonelyPoint(int index)
    {

        return firstReachLonelyPoints[index];
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
    void kick(KickEventArgs args)
    {
        float angle = Vector3.Angle(args.kickVelocity, args.previousVelocity);
        float m = Mathf.Abs(args.kickVelocity.magnitude - args.previousVelocity.magnitude);
        if (angle>45|| m > 5)
        {
            startReflexTime = Time.time;

            if (MatchComponents.currentPublicPlayerData!=null&&!MatchComponents.currentPublicPlayerData.playerID.Equals(args.playerID))
            {
                startUserReflexTime = Time.time;
            }
        }
        previousPlayerKicker = args.playerID;
        if (!previousPlayerKicker.Equals(args.playerID))
        {
            startNextLonelyPointTime = Time.time;
        }
    }
    void checkReflex()
    {
        Vector3 ballVelocity = MatchComponents.ballVelocity;
        ballVelocity.y= 0;
        previousBallDir.y = 0;
        float angle = Vector3.Angle(ballVelocity, previousBallDir);
        float m = Mathf.Abs(ballVelocity.magnitude - previousBallDir.magnitude);
        if (angle > 45|| m > 5)
        {
            startReflexTime = Time.time;
        }
        previousBallDir = MatchComponents.ballVelocity;
        

       
    }
    void Defense_IsOn()
    {

        Team defenseTeam = this.defenseTeam;
        foreach (PublicPlayerData publicPlayerData in defenseTeam.outfieldPublicPlayerDatas)
        {
            if (!publicPlayerData.IsBot) continue;
            publicPlayerData.movimentValues.maxSpeedForReachBall = 0;
            if (reflexAvailable){
                if (firstDefenseReachBall == null || !firstDefenseReachBall.Equals(publicPlayerData))
                    publicPlayerData.movimentValues.maxForwardSpeed = publicPlayerData.movimentValues.defaultMaxForwardSpeed;
                Vector3 defensePos = GetPlayerTargetPosition(publicPlayerData, node);
                Vector3 playerPos = publicPlayerData.bodyTransform.position;
                MovementCtrl movementCtrl = publicPlayerData.playerComponents.movementCtrl;
                Vector3 ballPosition = MatchComponents.ballPosition;
                //movementCtrl.scope = 0;
                if (Vector3.Distance(defensePos, playerPos) > publicPlayerData.playerComponents.scope + 0.1f)
                {
                    movementCtrl.SetTargetPosition(defensePos);
                }
                else
                {

                    movementCtrl.SetStopped_LookTarget(ballPosition);
                }
            }
            

        }
        DefenseGoBallReachPosition();
    }
    void DefenseGoBallReachPosition()
    {
        Team defenseTeam = this.defenseTeam;
        firstDefenseReachBall = defenseTeam.firstReachBallPublicPlayerData;
        if (firstDefenseReachBall == null|| firstDefenseReachBall.IsGoalkeeper) return;
        bool reflex = attackTeam.firstReachBallPublicPlayerData.IsBot ? reflexAvailable : userReflexAvailable;
        if (reflex || firstDefenseReachBall.playerID.Equals(previousPlayerKicker))
        {
            if(!attackTeam.firstReachBallPublicPlayerData.IsBot)
                firstDefenseReachBall.movimentValues.maxForwardSpeed = 5;
            else
                firstDefenseReachBall.movimentValues.maxForwardSpeed = defenseTeam.firstReachBallTime<1 ? firstDefenseReachBall.movimentValues.defaultMaxForwardSpeed: firstDefenseReachBall.movimentValues.defaultMaxForwardSpeed;
            firstDefenseReachBall.playerComponents.movementCtrl.SetTargetPosition(ballReachPosition);
        }
        
    }
    void Attack_IsOn()
    {
        Attack_DefaultPosition();
        AttackReachBall();
        Passer();
        AttackersGoToLonelyPoint();
    }
    void Attack_DefaultPosition()
    {
        Team team = attackTeam;
        foreach (PublicPlayerData publicPlayerData in team.outfieldPublicPlayerDatas)
        {
            publicPlayerData.movimentValues.maxForwardSpeed = publicPlayerData.movimentValues.defaultMaxForwardSpeed;
            //publicPlayerData.movimentValues.maxSpeedForReachBall = 5;
            if(Vector3.Distance(publicPlayerData.playerComponents.TargetPosition,publicPlayerData.position)<publicPlayerData.playerComponents.scope)busyPlayers.Remove(publicPlayerData);
            if (busyPlayers.Contains(publicPlayerData)) continue;
            if (!FootballPositionCtrl.GetFieldPositionDataPosition("Default", FootballPositionCtrl.AttackPressureTypeNormalMatch[TypeMatch.typeNormalMatch], publicPlayerData, MatchComponents.ballPosition, out Vector3 targetPosition)) continue;
            publicPlayerData.SetTargetPosition(targetPosition);
        }
    }
    bool wait=true;
    float delay=0.5f;
    void setWaitTrue() => wait = true;
    void ClearingBall()
    {

    }
    
    
    void AttackReachBall()
    {
        PublicPlayerData publicPlayerData = passerPublicPlayerData;
        if (!publicPlayerData.playerID.Equals(previousPlayerKicker) || MatchComponents.currentPublicPlayerData !=null&& MatchComponents.currentPublicPlayerData.Equals(publicPlayerData))
        {
            publicPlayerData.movimentValues.maxSpeedForReachBall = 0;
        }
        else
        {
            publicPlayerData.movimentValues.maxSpeedForReachBall = publicPlayerData.playerComponents.TargetPositionForwardAngle>publicPlayerData.playerComponents.playerSkills.MaxAngleControl?0:5;
        }
        if (publicPlayerData.IsBot)
        {
            publicPlayerData.playerComponents.scope = publicPlayerData.playerComponents.movementCtrl != null ? publicPlayerData.playerComponents.movementCtrl.defaultScope : 0;
            
            //publicPlayerData.movimentValues.maxForwardSpeed = 5;
            if (publicPlayerData.playerComponents.movementCtrl != null)
                publicPlayerData.playerComponents.movementCtrl.SetTargetPosition(ballReachPosition);
        }
    }
    void Passer()
    {
        if (!CheckGoalKick())
        {
            foreach(PublicPlayerData publicPlayerData in CullPassPoints.players)
            {
                if (publicPlayerData.IsBot)
                {
                    LonelyPointElement2 lonelyPoint = currentFirstLonelyPoint;
                    if (!CheckBallControl())
                    {
                        //publicPlayerData.movimentValues.maxSpeedForReachBall = 5;
                        PassData straightPassData = lonelyPoint.straightPassData;
                        if (publicPlayerData.playerComponents.BodyBallXZDistance <= publicPlayerData.playerComponents.ballScope + 0.25f && wait && publicPlayerData.kickAvailable)
                        {
                            //EditorApplication.isPaused = true;
                            wait = false;
                            Invoke(nameof(setWaitTrue), delay);
                        }
                        if ((lonelyPoint.straightReachBall || (lonelyPoint.straightPassData.distanceDefenseReachBall <= lonelyPoint.parabolicPassData.distanceDefenseReachBall+1f )) && publicPlayerData.Kick(straightPassData))
                        {
                            Kick();
                            //currentFirstLonelyPoint = CullPassPoints.firstReachLonelyPoints[passIndex];
                            //EditorApplication.isPaused = true;
                            //Time.timeScale = timeScale;
                            break;
                        }
                        else
                        {
                            if (lonelyPoint.parabolicReachBall || (lonelyPoint.straightPassData.distanceDefenseReachBall > lonelyPoint.parabolicPassData.distanceDefenseReachBall && !lonelyPoint.straightReachBall && !lonelyPoint.parabolicReachBall))
                            {
                                PassData parabolicPassData = lonelyPoint.parabolicPassData;
                                if (publicPlayerData.Kick(parabolicPassData))
                                {
                                    Kick();
                                    //EditorApplication.isPaused = true;
                                    //Time.timeScale = timeScale;
                                    //currentFirstLonelyPoint = CullPassPoints.firstReachLonelyPoints[passIndex];
                                    break;
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
            
        }
    }
    bool CheckBallControl()
    {
        Vector3 targetPosition = nextLonelyPoint.Get3DPosition();
        Vector3 ballPosition = MatchComponents.ballPosition;
        PublicPlayerData passer = passerPublicPlayerData;
        PlayerSkills playerSkills = passer.playerComponents.playerSkills;
        Vector3 velocity = MatchComponents.ballVelocity;
        Vector3 playerPosition = passer.position;
        Vector3 dir1 = targetPosition - ballPosition;
        dir1.y = 0;
        Vector3 dir2 = ballPosition - playerPosition;
        dir2.y = 0;
        float angle = Vector3.Angle(dir1, dir2);
        float precisionRadio = 0.25f;
        float precisionLerp = precisionRadio / 10;

        float maxVelocity = Mathf.Lerp(playerSkills.MaxVelocityControl, playerSkills.MinVelocityControl, dir1.magnitude / playerSkills.MaxVelocityDistanceControl);
        maxVelocity = Mathf.Lerp(maxVelocity, playerSkills.MaxVelocityControl, precisionLerp);
        PassData passData = nextLonelyPoint.GetPassData();
        //Time.timeScale = 0.2f;
        if (passer.IsBot && (angle > playerSkills.MaxAngleControl || MatchComponents.ballSpeed >= maxVelocity || !passer.BotKick.controlTimeAvailable) && passer.ReachBall())
        {
            if (!SearchControlPoint(targetPosition, out LonelyPointElement2 LonelyPointElement2)) return false;
            if (!passer.playerID.Equals(previousPlayerKicker))
            {
                passer.BotKick.startControlTime = Time.time;

                controlTime = minControlTime + Random.Range(0, randomControlTime);
                passer.BotKick.controlTime = controlTime;
            }
            //EditorApplication.isPaused = controlPause;
            passer.Kick(LonelyPointElement2.straightPassData);
            Kick();

            return true;
        }
        return false;
    }
    bool SearchControlPoint(Vector3 targetPosition, out LonelyPointElement2 result)
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        float weight = Mathf.NegativeInfinity;
        result = default;
        bool valid = false;
        foreach (CullPassPoints.ControlPointIndex controlPointIndex in CullPassPoints.controlPointIndices)
        {
            Entity entity = CullPassPoints.entities[controlPointIndex.entity];
            DynamicBuffer<LonelyPointElement2> lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entity);
            LonelyPointElement2 lonelyPointElement2 = lonelyPointElements2[controlPointIndex.index];
            PassData passData = lonelyPointElement2.GetPassData();
            if (lonelyPointElement2.weight > weight && (passData.distanceDefenseReachBall < -0.5f&& lonelyPointElement2.weight>0))
            {
                result = lonelyPointElement2;
                weight = lonelyPointElement2.weight;
                valid = true;
            }
        }
        return valid;
    }
    void Kick()
    {
        Invoke(nameof(ChangedPass), 0.2f);
        busyPlayers.Clear();
        passerPublicPlayerData.BotKick.startControlTime = Time.time;

        controlTime = minControlTime + Random.Range(0, randomControlTime);
        passerPublicPlayerData.BotKick.controlTime = controlTime;
        if (nextLonelyPoint.weight < 0)
        {
            print("no lonely points " + nextLonelyPoint.weight);
            //Time.timeScale = 0;
        }
    }
    void AttackersGoToLonelyPoint()
    {
        LonelyPointElement2 lonelyPointElement = attackLonelyPoint;
        PublicPlayerData publicPlayerData = CullPassPoints.GetPublicPlayerData(lonelyPointElement.attackReachIndex);
        publicPlayerData = passerPublicPlayerData != publicPlayerData ? publicPlayerData : null;
        if (publicPlayerData != null && !publicPlayerData.IsGoalkeeper && publicPlayerData.IsBot&&reflexAvailable)
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
    void UpdateNextLonelyPoint()
    {
        int index=0;
        for (int i = 0; i < CullPassPoints.firstReachLonelyPoints.Count; i++)
        {
            LonelyPointElement2 lonelyPointElement = CullPassPoints.firstReachLonelyPoints[i];
            PublicPlayerData publicPlayerData = CullPassPoints.GetPublicPlayerData(lonelyPointElement.attackReachIndex);
            if(publicPlayerData != null && passerPublicPlayerData != null)
            {
                if (!publicPlayerData.Equals(passerPublicPlayerData))
                {
                    index = i;
                    break;
                }
            }
        }
        if(passIndex< firstReachLonelyPoints.Count)
        {
            nextLonelyPoint = CullPassPoints.firstReachLonelyPoints[passIndex];
            currentFirstLonelyPoint = nextLonelyPoint;
            attackLonelyPoint = CullPassPoints.firstReachLonelyPoints[index];
        }
        
    }
    public void GetCullPassPointData()
    {
        if (!cullPassPointEnable||  CullPassPoints.firstReachLonelyPoints.Count <= passIndex || !changedPass&&false) return;
        
        
        passerPublicPlayerData = CullPassPoints.firstPublicPlayerData;
        float distance = Vector3.Distance(ballReachPosition, CullPassPoints.ballReachPosition);
        
        ballReachPosition = CullPassPoints.ballReachPosition;
        changedPass = false;
        
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
        
        LonelyPointElement2 maxWeightLonelyPoint = getMaxWeight(0);
        PassData passData = maxWeightLonelyPoint.GetPassData();
        currentWeight = CullPassPointsJob.EvaluatePosition(ballPosition, left, right, ballPosition, 0, maxFieldDistance,false);
        if (passerPublicPlayerData.playerComponents.botKick!=null&&(currentWeight > maxWeightLonelyPoint.weight-0.025f|| currentWeight >=0.5f) && (isLookingToGoal(goalComponents)||true) &&passerPublicPlayerData.ReachBall()&&passerPublicPlayerData.IsBot)
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
    public LonelyPointElement2 getMaxWeight(int node)
    {
        float maxWeight = Mathf.NegativeInfinity;
        float distance = 0;
        LonelyPointElement2 result=default;
        for (int i = 0; i < CullPassPoints.firstReachLonelyPoints.Count; i++)
        {
            LonelyPointElement2 lonelyPointElement2 = CullPassPoints.firstReachLonelyPoints[i];
            lonelyPointElement2.GetPassData(true,out PassData passData);
            if ((lonelyPointElement2.weight > maxWeight|| distance>-1f&& passData.distanceDefenseReachBall< distance))
            {
                maxWeight = lonelyPointElement2.weight;
                result = lonelyPointElement2;
                distance = passData.distanceDefenseReachBall;
            }
        }
        return result;
    }
#if UNITY_EDITOR

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

            
            if (passerPublicPlayerData != null&& passerPublicPlayerData.IsBot)
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
                    if (!FootballPositionCtrl.GetFieldPositionDataPosition("Default", FootballPositionCtrl.AttackPressureTypeNormalMatch[TypeMatch.typeNormalMatch], publicPlayerData, MatchComponents.ballPosition, out Vector3 targetPosition)) continue;
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
#endif
}
