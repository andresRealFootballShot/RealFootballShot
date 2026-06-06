using CullPositionPoint;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotMoveFunctions : PlayerComponent
{
    CullPassPoints CullPassPoints;
    Team attackTeam { get => CullPassPoints.attackTeam; }
    Team defenseTeam { get => CullPassPoints.defenseTeam; }
    public SearchPlayData searchPlayData { get => CullPassPoints.searchPlayData; }
    Vector3 targetPosition;
    Vector3 lonelyPointPosition;
    public bool avoidOffside{ get; set; }
    bool available=true;
    bool goLonelyPoint;
    void Start()
    {
        MatchEvents.kick.AddListener(SomeOneKickBall);
        CullPassPoints = MatchComponents.CullPassPoints;
    }
    void SomeOneKickBall(KickEventArgs KickEventArgs)
    {
        if (avoidOffside)
        {
            publicPlayerData.SetTargetPosition(targetPosition);
        }
    }
    public void SetTarget_AvoidOffside(PublicPlayerData publicPlayerData,LonelyPointElement2 lonelyPointElement)
    {
        Team team = CullPassPoints.defenseTeam;
        Vector3 goalPosition = team.goalPosition;
        Vector3 playerPosition = publicPlayerData.position;
        playerPosition.y = 0;
        Vector3 offsideLine = GetOffsideLine();
        Vector3 forward = goalPosition - offsideLine;
        Vector3 dir = playerPosition - offsideLine;
        forward.y = 0;
        Debug.DrawLine(offsideLine, offsideLine + Vector3.up * 4, Color.yellow);
        Vector3 targetPosition = lonelyPointElement.Get3DPosition(0);
        available = false;
        goLonelyPoint = true;
        lonelyPointPosition = lonelyPointElement.Get3DPosition(0);
        if (Vector2.Dot(forward, dir) <= 0 && SegmentLineIntersectionXZ(playerPosition, targetPosition, offsideLine, offsideLine + Vector3.right, out Vector3 offsidePoint))
        {
            Debug.DrawLine(offsidePoint, offsidePoint + Vector3.up * 3, Color.black);
            publicPlayerData.playerComponents.movementCtrl.scope = 0.1f;
            publicPlayerData.SetTargetPosition(offsidePoint);
            avoidOffside = true;
            this.targetPosition = targetPosition;
        }
        else
        {
            avoidOffside = false;
            publicPlayerData.playerComponents.movementCtrl.scope = publicPlayerData.playerComponents.movementCtrl.defaultScope;
            publicPlayerData.playerComponents.movementCtrl.SetTargetPosition(targetPosition);
        }
    }
    public bool CheckPasserAvailable()
    {
        if(goLonelyPoint && CheckBallGoPoint()&&!publicPlayerData.playerComponents.botKick.ReachBall())
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public bool CheckBallGoPoint()
    {
        Vector3 ballPosition = MatchComponents.ballPosition;
        Vector3 velocity = MatchComponents.ballRigidbody.velocity;
        Vector3 position = lonelyPointPosition;
        ballPosition.y = 0;
        velocity.y = 0;
        position.y = 0;
        return MyFunctions.DistancePointAndInfiniteLine(position, ballPosition, velocity)<0.5f;
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
        ballPosition.y= 0;
        Vector3 midfieldPos = MatchComponents.footballField.center;
        midfieldPos.x = goalPosition.x;
        midfieldPos.y = 0;
        Vector3 forward = (goalPosition - midfieldPos).normalized;

        float max1 = float.MinValue; // defensa más cercano a portería
        float max2 = float.MinValue; // segundo más cercano

        // Buscar los dos defensas más retrasados
        foreach (PublicPlayerData publicPlayerData in defenseTeam.publicPlayerDatas)
        {
            Vector3 playerPos = new Vector3(goalPosition.x, 0, publicPlayerData.position.z);
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
}
