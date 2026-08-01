using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class MatchData : MonoBehaviour
{
    public bool enabledRules { get; set; }
    public float currentMatchTime { get; set; }
    public int currentMatchMinutes { get => Mathf.FloorToInt(currentMatchTime / 60); }
    public int currentMatchSeconds { get => Mathf.FloorToInt(currentMatchTime % 60); }
    public bool inGame { get; set; }
    public bool endMatch { get; set; }
    public CornerComponents currentCorner { get; set; }
    public int minutes = 5;
    public int seconds = 0;
    public int restMatchMinutes { get => minutes - currentMatchMinutes; }
    public int restMatchSeconds { get => seconds - currentMatchSeconds; }
    public Team possessionTeam { get; set; }
    public Team noPossessionTeam { get; set; }
    public Team attackTeam { get; set; }
    public Team defenseTeam { get; set; }
    public PublicPlayerData posssessionPlayer { get; set; }
    public string startAttackTeamName { get; set; }
    public Team startAttackTeam { get => Teams.getTeamByName(startAttackTeamName); }
    public bool centerBall { get; set; }
    public bool initialMatch { get; set; }
    public bool inCorner { get; set;}
    public bool enableKick { get; set;}
    public bool inGoal { get; set;}
    private void Start()
    {
        MatchComponents.MatchData = this;

       
    }

}
