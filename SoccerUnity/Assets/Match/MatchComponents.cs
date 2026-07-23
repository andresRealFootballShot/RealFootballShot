using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchComponents : MonoBehaviour
{
    public static IRequestFieldPosition requestFieldPosition;
    public static ISetupTeams setupTeams;
    public static BallComponents ballComponents;
    public static Rigidbody ballRigidbody { get => ballComponents.rigBall; }
    public static Vector3 ballVelocity { get => ballComponents.rigBall.velocity; set => ballComponents.rigBall.velocity = value; }
    public static Vector3 ballAngularVelocity { get => ballComponents.rigBall.angularVelocity; set => ballComponents.rigBall.angularVelocity = value; }
    public static float ballSpeed { get => ballComponents.rigBall.velocity.magnitude; }
    public static Transform ballTransform { get => ballComponents.transBall; }
    public static Vector3 ballPosition { get => ballComponents.transBall.position; set => ballComponents.transBall.position = value; }
    public static float ballRadio { get => ballComponents.radio; }
    
    public static FootballFieldComponents footballField = new FootballFieldComponents();
    public static List<ChaserData> chaserList = new List<ChaserData>();
    public static RulesComponents rulesComponents = new RulesComponents();
    public static IKickOff kickOff;
    public static IKickNotifier kickNotifier;
    public static Canvas matchHUDCanvas;
    public static Timer timer;
    public static MatchRulesSettings rulesSettings { get =>rulesComponents.settings; }
    public static GameSounds gameSounds;
    public static CullPassPoints CullPassPoints;
    public static PublicPlayerData currentPublicPlayerData;
    public static Team myTeam { get; set; }
    public static Team currentReachBallTeam { get=>Teams.getTeamFromPlayer(firstReachBalPublicPlayerData.playerID); }
    public static PublicPlayerData firstReachBalPublicPlayerData { get=>CullPassPoints.firstPublicPlayerData; }
    
    public static ModeCtrl MatchCtrl;
    public static UserMode MatchMode{ get => MatchCtrl.matchMode; set => MatchCtrl.matchMode = value; }
}
