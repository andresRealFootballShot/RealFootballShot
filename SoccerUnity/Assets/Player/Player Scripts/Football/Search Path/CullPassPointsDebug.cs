using CullPositionPoint;
using FieldTriangleV2;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using UnityEditor;
using UnityEngine;
using static SearchPlayData;

public class CullPassPointsDebug : MonoBehaviour
{
    public bool debug;
    [Header("Search Lonely Point")]
    [Space(5)]
    public bool debugTestLonelyPoints;
    public bool debugPointResults, _debugNode;
    public bool _debugAllLonelyPointsOfNode;
    public bool debugPassLonelyPoint;
    public bool _debugLonelyPointIndex, debugReachableLonelyPoints, debugAttackPass;
    public int debugNode = 0;
    public int debugLonelyPointIndex = 0;
    public bool debugPlayerIndex;
    public bool debugText;
    public int lonelyPointIndexPassTest;
    public int searchNodeDebug;
    public bool debugBall;
    [Header("Kick")]
    [Space(5)]
    public float force;
    public float startPlayerSpeed, maxSpeedForReachBall;
    public bool debugStraightPass;
    public bool pause;
    public float timeScale=1;
    string teamName_Defense { get => CullPassPoints.teamName_Defense; }
    string teamName_Attacker { get => CullPassPoints.teamName_Attacker; }
    int teamAttack_start { get => CullPassPoints.teamAttack_start; }
    List<Entity> entities { get => CullPassPoints.entities; }
    EntityManager entityManager { get => CullPassPoints.entityManager; }
    public List<LonelyPointElement2> debugWeightLonelyPooints = new List<LonelyPointElement2>();
    [Space(20)]
    public CullPassPoints CullPassPoints;
    public BallInterceptionSystem BallInterceptionSystem;
    public SearchPlayData searchPlayData { get => CullPassPoints.searchPlayData; }
    LonelyPointElement2 debugLonelyPointElement, debugPreviousLonelyPointElement;
    Vector3 attackPos, defensePos;
    string teamAttack, teamDefense;
    bool passStarted;
    void Start()
    {
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            startPass();
        }
        checkBallPlayerDefense();
    }
    void checkBallVelocity()
    {
        if (!passStarted) return;
        PublicPlayerData attackPublicPlayerData = CullPassPoints.GetPublicPlayerData(debugLonelyPointElement.attackReachIndex);
        float ballPlayerDistance = attackPublicPlayerData.playerComponents.BodyBallXZScpDistance;
        if (ballPlayerDistance < 0.05f)
        {
            print("ball Velocity="+MatchComponents.ballRigidbody.velocity.magnitude);
            passStarted = false;
        }
    }
    void checkBallPlayerDefense()
    {
        if (!passStarted) return;
        if (!debugLonelyPointElement.GetPassData(debugStraightPass, out PassData passData)) return;
        PublicPlayerData defensePublicPlayerData = CullPassPoints.GetPublicPlayerData(passData.defenseReachIndex);
        float ballPlayerDistance = defensePublicPlayerData.playerComponents.BodyBallXZScpDistance;
        if (ballPlayerDistance < 0.05f)
        {
            print("defense ball position =" + MatchComponents.ballPosition+ " ballPlayerDistance"+ ballPlayerDistance);
            passStarted = false;
        }
    }
    void startPass()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Kick();
        SetInstantVelocity();
        CullPassPoints.startCullPassPointsSystem = true;
        MatchEvents.CullPassPointsEnd.AddListener(PlayDebug);
        debugPassLonelyPoint = true;
        _debugAllLonelyPointsOfNode = false;
        
    }
    void getPos()
    {
        bool passDataAvailable = debugLonelyPointElement.GetPassData(debugStraightPass, out PassData passData);
        Vector2 attackPos2D = searchPlayData.GetPlayerPosition(debugNode, debugLonelyPointElement.attackReachIndex);
        attackPos = new Vector3(attackPos2D.x, 0.5f, attackPos2D.y);
        Vector2 defensePos2D = searchPlayData.GetPlayerPosition(debugNode, passData.defenseReachIndex);
        defensePos = new Vector3(defensePos2D.x, 0.5f, defensePos2D.y);
    }
    public void PlayDebug()
    {
        EditorApplication.isPaused = pause;
        Time.timeScale = timeScale;
        MatchEvents.CullPassPointsEnd.RemoveListener(PlayDebug);

        debugLonelyPointElement = GetDebugLonelyPoint(debugLonelyPointIndex);
        debugPreviousLonelyPointElement = CullPassPoints.searchPlayData.GetBallLonelyPoint(debugNode);
        getPos();
        teamAttack = CullPassPoints.teamName_Attacker;
        teamDefense = CullPassPoints.teamName_Defense;
        
        SetPlayerTargets();
        passStarted = true;
        Invoke(nameof(SearchNodePass), CullPassPoints.firstPlayerReachTime);
    }
    void Kick()
    {
        MatchComponents.ballRigidbody.velocity = MatchComponents.ballTransform.forward.normalized * force;
        
    }
    void SetInstantVelocity()
    {
        for (int i = 0; i < Teams.allPlayers.Count; i++)
        {
            PublicPlayerData publicPlayerData = Teams.allPlayers[i];
            if (publicPlayerData.playerComponents.movementCtrl == null) continue;
            publicPlayerData.playerComponents.movementCtrl.SetInstantVelocity(publicPlayerData.playerComponents.bodyY0Forward, startPlayerSpeed);
            publicPlayerData.movimentValues.maxSpeedForReachBall = maxSpeedForReachBall;
        }
    }
    void SetPlayerTargets()
    {
        PublicPlayerData publicPlayerData = CullPassPoints.firstPublicPlayerData;
        publicPlayerData.playerComponents.movementCtrl.SetTargetPosition(CullPassPoints.ballReachPosition);

        PublicPlayerData attackPublicPlayerData = CullPassPoints.GetPublicPlayerData(debugLonelyPointElement.attackReachIndex);
        attackPublicPlayerData.playerComponents.movementCtrl.debug = true;
        attackPublicPlayerData.playerComponents.movementCtrl.debugMoveTimes = true;
        attackPublicPlayerData.playerComponents.movementCtrl.SetTargetPosition(debugLonelyPointElement.Get3DPosition());
        if (!debugLonelyPointElement.GetPassData(debugStraightPass, out PassData passData)) return;
        PublicPlayerData defensePublicPlayerData = CullPassPoints.GetPublicPlayerData(passData.defenseReachIndex);
        defensePublicPlayerData.playerComponents.movementCtrl.debug = true;
        defensePublicPlayerData.playerComponents.movementCtrl.debugMoveTimes = true;
        defensePublicPlayerData.playerComponents.movementCtrl.SetTargetPosition(passData.GetDefenseReach3DPosition());
    }
    void SearchNodePass()
    {

        
        MatchComponents.ballTransform.position = CullPassPoints.ballReachPosition;
        if (!debugLonelyPointElement.GetPassData(debugStraightPass, out PassData passData)) return;
        
        MatchComponents.ballRigidbody.velocity = passData.passVelocity;
        
        
    }
    private void OnDrawGizmos()
 {
     if (Application.isPlaying && debug)
     {
         if (debugPointResults)
         {

             if (searchPlayData.searchPlayNodes.Count > 0)
             {

                 int node = 0;
                 List<int> nodes = new List<int>();
                 nodes.Add(node);
                 for (int i = 0; i < nodes.Count; i++)
                 {
                     int nextNode = nodes[i];
                     nodes.AddRange(searchPlayData.GetNextNodes(nextNode));
                     LonelyPointElement2 lonelyPointElement = searchPlayData.GetBallLonelyPoint(nextNode);
                     int previousNode = searchPlayData.GetPreviousNode(nextNode);
                     LonelyPointElement2 previousLonelyPoint = searchPlayData.GetBallLonelyPoint(previousNode);
                    
                     DrawLonelyPoint(lonelyPointElement, searchPlayData.GetBallLonelyPoint(debugNode), nextNode, 0, "", Color.white);
                     Vector3 pos3 = new Vector3(lonelyPointElement.position.x, 1, lonelyPointElement.position.y);
                     Vector3 pos4 = new Vector3(previousLonelyPoint.position.x, 1, previousLonelyPoint.position.y);
                     DrawArrow.ForDebug(pos4, pos3 - pos4, 0.5f);
                 }
             }
         }
         if (_debugNode)
         {
             int node = 0;
             List<int> nodes = new List<int>();
             nodes.Add(node);
             for (int i = 0; i < nodes.Count; i++)
             {
                 int nextNode = nodes[i];
                 nodes.AddRange(searchPlayData.GetNextNodes(nextNode));



                 if (nextNode == debugNode)
                 {
                     LonelyPointElement2 lonelyPointElement = searchPlayData.GetBallLonelyPoint(nextNode);
                     DrawLonelyPoint(lonelyPointElement, searchPlayData.GetBallLonelyPoint(debugNode), nextNode, 0, "Node", new Color(0.5f, 0.75f, 0.25f));
                 }
             }
         }
         if (_debugAllLonelyPointsOfNode)
         {

             for (int i = 0; i < debugWeightLonelyPooints.Count; i++)
             {
                if (!debugReachableLonelyPoints || debugWeightLonelyPooints[i].weight != Mathf.Infinity)
                {
                    DrawLonelyPoint(debugWeightLonelyPooints[i], searchPlayData.GetBallLonelyPoint(debugNode), debugNode, i, "", Color.white);
                    debugArrow(searchPlayData.GetBallLonelyPoint(debugNode), debugWeightLonelyPooints[i]);
                    //DrawReachPlayers(debugWeightLonelyPooints[i]);
                }
             }
         }
         if (_debugLonelyPointIndex)
         {
             for (int i = 0; i < debugWeightLonelyPooints.Count; i++)
             {
                 if (debugLonelyPointIndex == debugWeightLonelyPooints[i].index)
                     DrawLonelyPoint(debugWeightLonelyPooints[i], searchPlayData.GetBallLonelyPoint(debugNode), debugNode, i, "Lonely Point", new Color(0.6f, 0.9f, 0.75f));
             }
         }
        if (debugPassLonelyPoint)
        {
            DrawLonelyPoint(debugLonelyPointElement, debugPreviousLonelyPointElement, debugNode, 0, "", Color.white);
            debugArrow(debugPreviousLonelyPointElement, debugLonelyPointElement);
            DrawReachPlayers(debugLonelyPointElement);
        }
         if (debugPlayerIndex)
         {

             Team defenseTeam = Teams.getTeamByName(teamName_Defense);
             Team attackTeam = Teams.getTeamByName(teamName_Attacker);
             DebugPlayerIndex(defenseTeam, attackTeam);
         }
         //debugBallInfo();
     }
 }
void DrawReachPlayers(LonelyPointElement2 lonelyPointElement)
{
    bool passDataAvailable = lonelyPointElement.GetPassData(debugStraightPass,out PassData passData);
        
    Gizmos.color = new Color(1f, 0.7f, 0.7f);
    Gizmos.DrawSphere(attackPos, 0.5f);
    debugArrow(attackPos, lonelyPointElement.Get3DPosition(0.5f));

    GUIStyle style = new GUIStyle();
    style.fontSize = 12;
    style.normal.textColor = Teams.getTeamByName(teamAttack).Color;
    string info = "Attack Reach Time = " + lonelyPointElement.attackReachTime.ToString("f2");
    Handles.Label(attackPos + Vector3.up*1.5f, info, style);

    if (passDataAvailable)
    {
        Gizmos.color = new Color(0.7f, 0.7f, 1f);
        Gizmos.DrawSphere(defensePos, 0.5f);
        debugArrow(defensePos, new Vector3(passData.defenseReachPosition.x, 0.5f, passData.defenseReachPosition.y));

        style = new GUIStyle();
        style.fontSize = 12;
        style.normal.textColor = Teams.getTeamByName(teamDefense).Color;
        info = "Defense Reach Time = " + passData.defenseReachTime.ToString("f2");
        Handles.Label(defensePos + Vector3.up * 1.5f, info, style);
    }
    style = new GUIStyle();
    style.fontSize = 12;
    style.normal.textColor = Teams.getTeamByName(teamAttack).Color;
    LonelyPointElement2 lonelyPointElement2 = debugPreviousLonelyPointElement;
    Vector3 pos = new Vector3(lonelyPointElement2.position.x, 1.5f,lonelyPointElement2.position.y);
    info ="Attack Team = "+ teamAttack;
    Handles.Label(pos , info, style);
}
 void debugArrow(LonelyPointElement2 previousLonelyPoint, LonelyPointElement2 lonelyPointElement)
{
        Vector3 pos3 = new Vector3(lonelyPointElement.position.x, 1, lonelyPointElement.position.y);
        Vector3 pos4 = new Vector3(previousLonelyPoint.position.x, 1, previousLonelyPoint.position.y);
        DrawArrow.ForDebug(pos4, pos3 - pos4, 0.5f);
}
void debugArrow(Vector3 pos1, Vector3 pos2)
{
    DrawArrow.ForDebug(pos1, pos2 - pos1, 0.5f);
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
                        CullPassPoints.setAttackTargetPosition(TestResultComponent, GetV0DOTSResult);
                        CullPassPoints.setDefenseTargetPosition(TestResultComponent, GetV0DOTSResult);
                     //StartCoroutine(TestCoroutine(TestResultComponent, GetV0DOTSResult));
                     //StartCoroutine(TestCoroutineDefenseLonleyPosition(TestResultComponent));
                     return;
                 }
             }
         }
     }

 }
LonelyPointElement2 GetDebugLonelyPoint(int index)
{
    foreach (var debugWeightLonelyPooint in debugWeightLonelyPooints)
    {
        if(debugWeightLonelyPooint.index==index) return debugWeightLonelyPooint;
    }
    return default;
}
 public void getDebugWeightPoints(List<int> Snodes)
 {
     if (Snodes.Contains(debugNode))
     {
         debugWeightLonelyPooints.Clear();
         int node = debugNode;
         int entityCount = searchPlayData.getCullEntityCount(node);
         for (int i = 0; i < entityCount; i++)
         {
             int entityIndex = searchPlayData.getCullEntity(node, i);
             Entity entity = entities[entityIndex];
             CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
             DynamicBuffer<LonelyPointElement2> lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement2>(entity);
             for (int j = 0; j < CullPassPointsComponent.sizeLonelyPoints; j++)
             {
                 LonelyPointElement2 lonelyPointElement2 = lonelyPointElements2[j];
                 debugWeightLonelyPooints.Add(lonelyPointElement2);
             }
         }
     }
 }
 void debugBallInfo()
 {
     GUIStyle style = new GUIStyle();
     style.fontSize = 16;
     style.normal.textColor = Color.green;
     Vector3 ballPos = MatchComponents.ballRigidbody.position;
     string info = ballPos.ToString("f2");
     Handles.Label(ballPos + Vector3.up * 1.7f, info, style);
 }
 public void DebugPlayerIndex(Team defenseTeam, Team attackTeam)
 {
     for (int i = teamAttack_start, j = 0; i < teamAttack_start + CullPassPoints.teamAttack_size; i++, j++)
     {

         Vector3 position = attackTeam.publicPlayerDatas[j].position;
         GUIStyle style = new GUIStyle();
         style.fontSize = 14;
         style.normal.textColor = Color.cyan;
         Handles.Label(position + Vector3.up * 1.25f, "player index=" + i+" "+ position.ToString("f1"), style);

     }
     for (int i = CullPassPoints.teamDefense_start, j = 0; i < CullPassPoints.teamDefense_start + CullPassPoints.teamDefense_size; i++, j++)
     {

         Vector3 position = defenseTeam.publicPlayerDatas[j].position;
         GUIStyle style = new GUIStyle();
         style.fontSize = 14;
         style.normal.textColor = Color.white;
         Handles.Label(position + Vector3.up * 1.25f, "player index=" + i + " " + position.ToString("f1"), style);
     }

 }
 void DrawLonelyPoint(LonelyPointElement2 lonelyPointElement, LonelyPointElement2 previousLonelyPoint, int node, int index, string info, Color infoColor)
 {
     //if (!lonelyPointElement.parabolicReachBall) return;
     Vector3 pos = new Vector3(lonelyPointElement.position.x, 0, lonelyPointElement.position.y);
     Color color;
     if (lonelyPointElement.order == 0)
     {
         color = Color.cyan;
     }
     else if (lonelyPointElement.straightReachBall && lonelyPointElement.parabolicReachBall)
     {
         color = Color.green;
     }
     else if (lonelyPointElement.straightReachBall && !lonelyPointElement.parabolicReachBall)
     {
         color = Color.blue;
     }
     else if (!lonelyPointElement.straightReachBall && lonelyPointElement.parabolicReachBall)
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
     style.fontSize = 16;
     style.normal.textColor = infoColor;
     Handles.Label(pos + Vector3.up * 1.7f, info, style);
     style.fontSize = 14;
     style.normal.textColor = color;
     //string text = "ballReachPosTime=" + TestResultComponent.ballReachTargetPositionTime + " defenseIndex=" + TestResultComponent.defenseLonelyPointReachIndex + " defenseReachLonelyPosTime=" + TestResultComponent.defenseLonelyPointReachTime + " closestDistanceDefenseBall=" + TestResultComponent.closestDistanceDefenseBall;
     //string text = "straightReachBall=" + lonelyPointElement.straightReachBall + " parabolicReachBall=" + lonelyPointElement.parabolicReachBall + " i="+lonelyPointElement.index;
     string text = "i=" + lonelyPointElement.index;
     //string text = "ballReachPosTime=" + TestResultComponent.ballReachTargetPositionTime + " maximumControlSpeedReached=" + TestResultComponent.GetV0DOTSResult1.maximumControlSpeedReached + " maxKickForceReached=" + TestResultComponent.GetV0DOTSResult1.maxKickForceReached + " parabolicReachBall=" + TestResultComponent.parabolicReachBall + " straightReachBall=" + TestResultComponent.straightReachBall;


     Handles.Label(pos + Vector3.up * 0.5f, text, style);
     Color c = Color.Lerp(Color.green, Color.red, lonelyPointElement.weight);
     style.normal.textColor = c;
     float value = lonelyPointElement.weight * 100;
     text = "weight=" + value.ToString("f2") + " order=" + lonelyPointElement.order + " node=" + node + " index=" + lonelyPointElement.index + " Pos=" + lonelyPointElement.position.ToString("f2");
     if (debugText)
         Handles.Label(pos + Vector3.up * 1.25f, text, style);
     lonelyPointElement.GetPassData(debugStraightPass,out PassData passData);
     if (debugAttackPass)
     {
        Team attackTeam = Teams.getTeamByName(teamName_Attacker);
        Vector2 playerPos2 = searchPlayData.GetPlayerPosition(0, lonelyPointElement.attackReachIndex);
        Vector3 playerPos = new Vector3(playerPos2.x, 0, playerPos2.y);
        Debug.DrawLine(playerPos + Vector3.up * 0.25f, pos + Vector3.up * 0.25f, Color.black);
        string pass = "straight=" + lonelyPointElement.straightReachBall + " parabolic=" + lonelyPointElement.parabolicReachBall+ " pass force="+ passData.passVelocity.magnitude;
        
        Vector3 pos3 = new Vector3(lonelyPointElement.position.x, 1, lonelyPointElement.position.y);
        Vector3 pos4 = new Vector3(previousLonelyPoint.position.x, 1, previousLonelyPoint.position.y);
        Vector3 pos2 = (pos4 - pos3) * 0.5f;
        Handles.Label(pos3 + pos2 + Vector3.up * 0.5f, pass, style);
     }

 }
#endif

}
