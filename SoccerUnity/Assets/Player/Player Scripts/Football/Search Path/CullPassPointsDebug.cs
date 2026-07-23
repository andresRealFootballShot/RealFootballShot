using CullPositionPoint;
using DOTS_ChaserDataCalculation;
using FieldTriangleV2;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public bool _debugLonelyPointIndex, debugReachableLonelyPoints, debugAllAttackPass, debugIndexAttackPass;
    public int debugNode = 0;
    public int debugLonelyPointIndex = 0;
    public bool debugPlayerIndex;
    public bool debugText;
    public int lonelyPointIndexPassTest;
    public int searchNodeDebug;
    public bool debugBall;
    public bool debugArrow,debugAttackTeam;
    [Header("Kick")]
    [Space(5)]
    public float force;
    public float startPlayerSpeed, maxSpeedForReachBall;
    public bool debugStraightPass;
    public bool pause;
    public bool updateDebug=true,stopUpdatePassStarted;
    public float timeScale=1;
    string teamName_Defense { get => CullPassPoints.teamName_Defense; }
    Team defenseTeam { get => CullPassPoints.defenseTeam; }
    Team attackTeam { get => CullPassPoints.attackTeam; }
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
    float firstReachBallTime;
    int firstReachPlayerIndex;
    string teamAttackNamePass, teamDefenseNamePass;
    bool passStarted;
    PublicPlayerData attackPublicPlayerDataPass, defensePublicPlayerDataPass;
    void Start()
    {

    }

#if UNITY_EDITOR
    
    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.T))
        {
            startPass();
        }*/
        if (updateDebug && (!passStarted))
        {
            GetDebugData();
        }
        //checkBallPlayerDefense();
        checkBallVelocity();
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
        //debugPassLonelyPoint = true;
        _debugAllLonelyPointsOfNode = !stopUpdatePassStarted;
        
    }
    void getPos()
    {
        bool passDataAvailable = debugLonelyPointElement.GetPassData(debugStraightPass, out PassData passData);
        Vector2 attackPos2D = searchPlayData.GetPlayerPosition(debugNode, debugLonelyPointElement.attackReachIndex);
        attackPos = new Vector3(attackPos2D.x, 0.5f, attackPos2D.y);
        Vector2 defensePos2D = searchPlayData.GetPlayerPosition(debugNode, passData.defenseReachIndex);
        defensePos = new Vector3(defensePos2D.x, 0.5f, defensePos2D.y);
        firstReachPlayerIndex = CullPassPoints.players.IndexOf(CullPassPoints.firstPublicPlayerData);
    }
    public void PlayDebug()
    {
        EditorApplication.isPaused = pause;
        Time.timeScale = timeScale;
        MatchEvents.CullPassPointsEnd.RemoveListener(PlayDebug);

        GetDebugData();


        SetPlayerTargets();
        passStarted = true;
        Invoke(nameof(SearchNodePass), CullPassPoints.firstPlayerReachTime);
    }
    void GetDebugData()
    {
        debugLonelyPointElement = GetDebugLonelyPoint(debugLonelyPointIndex);
        debugPreviousLonelyPointElement = CullPassPoints.searchPlayData.GetBallLonelyPoint(debugNode);
        getPos();
        teamAttackNamePass = CullPassPoints.teamName_Attacker;
        teamDefenseNamePass = CullPassPoints.teamName_Defense;
        attackPublicPlayerDataPass= CullPassPoints.GetPublicPlayerData(debugLonelyPointElement.attackReachIndex);
        defensePublicPlayerDataPass = CullPassPoints.GetPublicPlayerData(debugLonelyPointElement.GetPassData().defenseReachIndex);
        firstReachBallTime = CullPassPoints.firstPlayerReachTime;
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
        if (attackPublicPlayerData.playerComponents.movementCtrl != null){
            SetOffsideLineTarget();


        }
        if (!debugLonelyPointElement.GetPassData(debugStraightPass, out PassData passData)) return;
        Team team = Teams.getTeamByName(teamDefenseNamePass);
        foreach(PublicPlayerData defensePublicPlayerData in team.outfieldPublicPlayerDatas)
        {
            int index = CullPassPoints.players.IndexOf(defensePublicPlayerData);
            Vector3 targetPosition = searchPlayData.GetPlayerTargetPosition(debugNode, index,0);
            defensePublicPlayerData.playerComponents.movementCtrl.SetTargetPosition(targetPosition);
        }
    }
    void SetOffsideLineTarget()
    {
        Team team = CullPassPoints.defenseTeam;
        Vector3 goalPosition = team.goalPosition;
        PublicPlayerData attackPublicPlayerData = attackPublicPlayerDataPass;
        Vector3 playerPosition = attackPublicPlayerData.position;
        playerPosition.y= 0;
        Vector3 offsideLine = GetOffsideLine();
        Vector3 forward = goalPosition - offsideLine;
        Vector3 dir = playerPosition - offsideLine;
        forward.y = 0;
        Debug.DrawLine(offsideLine,offsideLine+Vector3.up*4,Color.yellow);
        Vector3 targetPosition = debugWeightLonelyPooints.Find(x=>x.index==debugLonelyPointIndex).Get3DPosition(0);

        if (Vector2.Dot(forward, dir) <= 0 && SegmentLineIntersectionXZ(playerPosition, targetPosition,offsideLine, offsideLine+Vector3.right, out Vector3 offsidePoint))
        {
            Debug.DrawLine(offsidePoint, offsidePoint + Vector3.up*3, Color.black);
            attackPublicPlayerData.playerComponents.movementCtrl.scope = 0.1f;
            attackPublicPlayerData.playerComponents.movementCtrl.SetTargetPosition(offsidePoint);
            
            Invoke(nameof(SetTargetPositionBeforeOffsideLine), firstReachBallTime);
        }
        else
        {
            attackPublicPlayerData.playerComponents.movementCtrl.SetTargetPosition(debugLonelyPointElement.Get3DPosition());
        }
    }
    void SetTargetPositionBeforeOffsideLine()
    {
        print("reach offsideLine");
        attackPublicPlayerDataPass.playerComponents.movementCtrl.SetTargetPosition(debugLonelyPointElement.Get3DPosition());
        attackPublicPlayerDataPass.playerComponents.movementCtrl.scope = attackPublicPlayerDataPass.playerComponents.movementCtrl.defaultScope;
        if(defensePublicPlayerDataPass.playerComponents.movementCtrl!=null)
        defensePublicPlayerDataPass.playerComponents.movementCtrl.SetTargetPosition(debugLonelyPointElement.Get3DPosition());
    }
    public static bool SegmentLineIntersectionXZ(
    Vector3 segA, Vector3 segB,     // segmento (finito)
    Vector3 lineA, Vector3 lineB,   // línea infinita
    out Vector3 intersection)
    {
        intersection = Vector3.zero;

        // Convertimos a 2D (XZ)
        Vector2 p1 = new Vector2(segA.x, segA.z);
        Vector2 p2 = new Vector2(segB.x, segB.z);
        Vector2 p3 = new Vector2(lineA.x, lineA.z);
        Vector2 p4 = new Vector2(lineB.x, lineB.z);

        Vector2 r = p2 - p1;
        Vector2 s = p4 - p3;

        float cross = r.x * s.y - r.y * s.x;

        // Paralelas
        if (Mathf.Approximately(cross, 0f))
            return false;

        Vector2 diff = p3 - p1;

        float t = (diff.x * s.y - diff.y * s.x) / cross;
        // float u = (diff.x * r.y - diff.y * r.x) / cross;
        // u no hace falta porque la línea es infinita

        // ✅ Aquí está la clave: solo comprobamos el segmento
        if (t < 0f || t > 1f)
            return false;

        Vector2 hit2D = p1 + t * r;

        intersection = new Vector3(hit2D.x, 0f, hit2D.y);
        return true;
    }
    Vector3 GetOffsideLine()
    {
        Vector3 ballPosition = CullPassPoints.ballReachPosition;
        Vector3 goalPosition = defenseTeam.goalPosition;
        ballPosition.x = goalPosition.x;
        ballPosition.y = 0;
        Vector3 midfieldPos = MatchComponents.footballField.center;
        midfieldPos.x = goalPosition.x;
        midfieldPos.y = 0;
        Vector3 forward = (goalPosition - midfieldPos).normalized;

        float max1 = float.MinValue; // defensa más cercano a portería
        float max2 = float.MinValue; // segundo más cercano

        // Buscar los dos defensas más retrasados
        foreach(PublicPlayerData publicPlayerData in defenseTeam.publicPlayerDatas)
        {
            int index = CullPassPoints.players.IndexOf(publicPlayerData);
            Vector3 targetPosition = searchPlayData.GetPlayerPosition(debugNode, index,0);

            Vector3 playerPos = new Vector3(goalPosition.x,0, targetPosition.z);
            float projection = Vector3.Dot(forward, playerPos - midfieldPos);

            if (projection > max1)
            {
                max2 = max1;
                max1 = projection;
            }
            else if (projection > max2)
            {
                max2 = projection;
            }
        }

        // Si hay menos de 2 defensas
        if (max2 == float.MinValue)
            return midfieldPos;

        // Proyección del balón
        float ballProjection = Vector3.Dot(forward, ballPosition - midfieldPos);
        if (ballProjection <= 0f && max2 <= 0f)
        {
            return midfieldPos;
        }
        // La línea es el más cercano a portería entre:
        // - el balón
        // - el penúltimo defensa (max2)
        float finalProjection = Mathf.Max(ballProjection, max2);

        return midfieldPos + forward * finalProjection;
    }
    void SetDefenseTarget()
    {
        if (!debugLonelyPointElement.GetPassData(debugStraightPass, out PassData passData)) return;
        PublicPlayerData defensePublicPlayerData = CullPassPoints.GetPublicPlayerData(passData.defenseReachIndex);
        if (defensePublicPlayerData.playerComponents.movementCtrl == null) return;
        defensePublicPlayerData.playerComponents.movementCtrl.SetTargetPosition(passData.GetDefenseReach3DPosition());
    }
    void SearchNodePass()
    {

        EditorApplication.isPaused = pause;
        MatchComponents.ballTransform.position = CullPassPoints.ballReachPosition;
        debugLonelyPointElement.GetPassData(debugStraightPass, out PassData passData);
        
        MatchComponents.ballRigidbody.velocity = passData.passVelocity;
        SetDefenseTarget();


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
                    _debugArrow(searchPlayData.GetBallLonelyPoint(debugNode), debugWeightLonelyPooints[i]);
                    DrawReachPlayers(debugWeightLonelyPooints[i]);
                }
             }
         }
         if (_debugLonelyPointIndex)
         {
             for (int i = 0; i < debugWeightLonelyPooints.Count; i++)
             {
                 if (debugLonelyPointIndex < debugWeightLonelyPooints.Count)
                     DrawLonelyPoint(debugWeightLonelyPooints[debugLonelyPointIndex], searchPlayData.GetBallLonelyPoint(debugNode), debugNode, i, "Lonely Point", new Color(0.6f, 0.9f, 0.75f));
             }
         }
        if (debugPassLonelyPoint)
        {
            DrawLonelyPoint(debugLonelyPointElement, debugPreviousLonelyPointElement, debugNode, 0, "", Color.white);
            _debugArrow(debugPreviousLonelyPointElement, debugLonelyPointElement);
            DrawReachPlayers(debugLonelyPointElement);
        }
        if (debugPlayerIndex)
        {

            Team defenseTeam = Teams.getTeamByName(teamName_Defense);
            Team attackTeam = Teams.getTeamByName(teamName_Attacker);
            DebugPlayerIndex(defenseTeam, attackTeam);
        }
        if (debugAttackTeam)
        {
           printAttackTeam(CullPassPoints.searchPlayData.GetBallLonelyPoint(debugNode));
        }
        //CheckOffsideLastPlayer();
        //debugBallInfo();
        //CheckDuplicatedLonelyPoints();
     }
 }
 void CheckOffsideLastPlayer()
{
        GUIStyle style = new GUIStyle();
        style.fontSize = 12;
        style.normal.textColor = Color.black;
        string info = "Offside Last Player";
        Vector3 lastPlayerPos = CullPassPoints.FootballPositionCtrl.GetLastPlayerPosition(MatchComponents.ballPosition, CullPassPoints.teamName_Defense,"Default", FootballPositionCtrl.DefensePressureTypeNormalMatch[TypeMatch.typeNormalMatch]);
        Handles.Label(lastPlayerPos + Vector3.up * 1.5f, info, style);
}
void DrawReachPlayers(LonelyPointElement2 lonelyPointElement)
{
    bool passDataAvailable = lonelyPointElement.GetPassData(debugStraightPass,out PassData passData);
        
    Gizmos.color = new Color(1f, 0.7f, 0.7f);
    Gizmos.DrawSphere(attackPos, 0.5f);
    _debugArrow(attackPos, lonelyPointElement.Get3DPosition(0.5f));

    GUIStyle style = new GUIStyle();
    style.fontSize = 12;
    style.normal.textColor = Teams.getTeamByName(teamAttackNamePass).Color;
    string info = "Attack Reach Time = " + lonelyPointElement.attackReachTime.ToString("f2");
    Handles.Label(attackPos + Vector3.up*1.5f, info, style);

    if (passDataAvailable)
    {
        Gizmos.color = new Color(0.7f, 0.7f, 1f);
        Gizmos.DrawSphere(defensePos, 0.5f);
        _debugArrow(defensePos, new Vector3(passData.defenseReachPosition.x, 0.5f, passData.defenseReachPosition.y));

        style = new GUIStyle();
        style.fontSize = 12;
        style.normal.textColor = Teams.getTeamByName(teamDefenseNamePass).Color;
        info = "Defense Reach Time = " + passData.defenseReachTime.ToString("f2");
        Handles.Label(defensePos + Vector3.up * 1.5f, info, style);
    }
        if (firstReachPlayerIndex != -1)
        {
            style = new GUIStyle();
            style.fontSize = 12;
            style.normal.textColor = Color.green;
            string info2 = "First Player Reach";
            Vector3 pos = CullPassPoints.players[firstReachPlayerIndex].position;
            Handles.Label(pos + Vector3.up * 1.7f, info2, style);
        }
}
void printAttackTeam(LonelyPointElement2 lonelyPointElement)
{
    GUIStyle style = new GUIStyle();
    style.fontSize = 12;
    style.normal.textColor = Teams.getTeamByName(teamAttackNamePass).Color;
    Vector3 pos = new Vector3(lonelyPointElement.position.x, 1.5f, lonelyPointElement.position.y);
    string info = "Attack Team = " + teamAttackNamePass;
    Handles.Label(pos, info, style);
}
 void _debugArrow(LonelyPointElement2 previousLonelyPoint, LonelyPointElement2 lonelyPointElement)
{
    if (!debugArrow) return;
    Vector3 pos3 = new Vector3(lonelyPointElement.position.x, 1, lonelyPointElement.position.y);
    Vector3 pos4 = new Vector3(previousLonelyPoint.position.x, 1, previousLonelyPoint.position.y);
    DrawArrow.ForDebug(pos4, pos3 - pos4, 0.5f);
}
void _debugArrow(Vector3 pos1, Vector3 pos2)
{
        if (!debugArrow) return;
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
     for (int i = 0; i < CullPassPoints.players.Count; i++)
     {
         Vector3 position2 = CullPassPoints.players[i].position;
         Vector3 position = CullPassPoints.searchPlayData.GetPlayerTargetPosition(debugNode, i, 0);
         Vector2 position2D = CullPassPoints.searchPlayData.GetPlayerPosition(debugNode, i);
         Vector3 position3 = new Vector3(position2D.x,0, position2D.y);
            GUIStyle style = new GUIStyle();
         style.fontSize = 14;
         
         style.normal.textColor = Teams.getTeamByName(teamAttackNamePass).Color;
         Handles.Label(position3 + Vector3.up * 1.25f, "player index=" + i, style);
        if (defenseTeam.publicPlayerDatas.Contains(CullPassPoints.players[i]))
        {
            Handles.DrawLine(position2, position3);
        }
     }

 }
    Vector3 GetPointOnOffsideLine(Vector3 playerPosition, Vector3 targetPosition, Vector2 offside)
    {
        // Línea horizontal (x ignorada, usas eje Z realmente)
        float offsideZ = offside.y;

        Vector3 dir = (targetPosition - playerPosition);

        if (Mathf.Abs(dir.z) < 0.001f)
            return targetPosition;

        float t = (offsideZ - playerPosition.z) / dir.z;

        t = Mathf.Clamp01(t);

        return playerPosition + dir * t;
    }
    void CheckDuplicatedLonelyPoints()
{
        print("Duplicated LonelyPoints");
        for (int i = 0; i < debugWeightLonelyPooints.Count; i++)
        {
            for (int j = 0; j < debugWeightLonelyPooints.Count; j++)
            {
                if(i==j) continue;
                if (Vector2.Distance(debugWeightLonelyPooints[i].position, debugWeightLonelyPooints[j].position) < 0.1f)
                {
                    print("Duplicated LonelyPoints:" + debugWeightLonelyPooints[i].index + " and " + debugWeightLonelyPooints[j].index);
                }
            }
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
     Gizmos.DrawSphere(pos + Vector3.up * 0.25f, 0.1f);
     GUIStyle style = new GUIStyle();
     style.fontSize = 16;
     style.normal.textColor = infoColor;
     Handles.Label(pos + Vector3.up * 1.7f, info, style);
     style.fontSize = 14;
     style.normal.textColor = color;
        //string text = "ballReachPosTime=" + TestResultComponent.ballReachTargetPositionTime + " defenseIndex=" + TestResultComponent.defenseLonelyPointReachIndex + " defenseReachLonelyPosTime=" + TestResultComponent.defenseLonelyPointReachTime + " closestDistanceDefenseBall=" + TestResultComponent.closestDistanceDefenseBall;
        //string text = "straightReachBall=" + lonelyPointElement.straightReachBall + " parabolicReachBall=" + lonelyPointElement.parabolicReachBall + " i="+lonelyPointElement.index;
        float value = lonelyPointElement.weight * 100;
        string text = "i=" + lonelyPointElement.index + " weight="+ value;
     //string text = "ballReachPosTime=" + TestResultComponent.ballReachTargetPositionTime + " maximumControlSpeedReached=" + TestResultComponent.GetV0DOTSResult1.maximumControlSpeedReached + " maxKickForceReached=" + TestResultComponent.GetV0DOTSResult1.maxKickForceReached + " parabolicReachBall=" + TestResultComponent.parabolicReachBall + " straightReachBall=" + TestResultComponent.straightReachBall;


     Handles.Label(pos + Vector3.up * 0.5f, text, style);
     Color c = Color.Lerp(Color.green, Color.red, lonelyPointElement.weight);
     style.normal.textColor = c;
     
     lonelyPointElement.GetPassData(debugStraightPass, out PassData passData);
       text = "weight=" + value.ToString("f2")  + " order=" + lonelyPointElement.order + " node=" + node + " index=" + lonelyPointElement.index + " Pos=" + lonelyPointElement.position.ToString("f2");
     if (debugText)
         Handles.Label(pos + Vector3.up * 1.25f, text, style);
     
     if (debugAllAttackPass||(debugIndexAttackPass && (CullPassPoints.firstReachLonelyPoints[0].index==lonelyPointElement.index || lonelyPointElement.index== debugLonelyPointIndex)))
     {
        Team attackTeam = Teams.getTeamByName(teamName_Attacker);
        Vector2 playerPos2 = searchPlayData.GetPlayerPosition(0, lonelyPointElement.attackReachIndex);
        Vector3 playerPos = new Vector3(playerPos2.x, 0, playerPos2.y);
        Debug.DrawLine(playerPos + Vector3.up * 0.25f, pos + Vector3.up * 0.25f, Color.black);
        string pass = "straight=" + lonelyPointElement.straightReachBall + " parabolic=" + lonelyPointElement.parabolicReachBall+ " pass force="+ passData.passVelocity.magnitude + " StraightDistanceDefenseReachBall=" + lonelyPointElement.straightPassData.distanceDefenseReachBall.ToString("f2") + " straightDefense=" + lonelyPointElement.straightPassData.defenseReachIndex + " ParabolicDistanceDefenseReachBall=" + lonelyPointElement.parabolicPassData.distanceDefenseReachBall.ToString("f2") + " parabolicDefense=" + + lonelyPointElement.parabolicPassData.defenseReachIndex;
        
        Vector3 pos3 = new Vector3(lonelyPointElement.position.x, 1, lonelyPointElement.position.y);
        Vector3 pos4 = new Vector3(previousLonelyPoint.position.x, 1, previousLonelyPoint.position.y);
        Vector3 pos2 = (pos4 - pos3) * 0.5f;
        Handles.Label(pos3 + pos2 + Vector3.up * 0.5f, pass, style);
     }

 }
#endif

}
