using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchComponents : MonoBehaviour
{
    public static IRequestFieldPosition requestFieldPosition;
    public static ISetupTeams setupTeams;
    public static BallComponents ballComponents;
    public static Rigidbody ballRigidbody { get => ballComponents.rigBall; }
    public static Transform ballTransform { get => ballComponents.transBall; }
    public static Vector3 ballPosition { get => ballComponents.transBall.position; }
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
}
