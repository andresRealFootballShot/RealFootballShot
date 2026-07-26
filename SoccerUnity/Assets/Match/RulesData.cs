using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MatchState
{
    Corner, InGame,EndGame
}
public class MatchStateEventArgs {
    public MatchState previousMatchState;
    public MatchState newMatchState;
    public MatchStateEventArgs(MatchState previousMatchState, MatchState newMatchState)
    {
        this.previousMatchState = previousMatchState;
        this.newMatchState = newMatchState;
    }
}

public class RulesData : MonoBehaviour
{
    public bool enabledRules { get; set; }
    MatchState _matchState { get; set; }
    public MatchState matchState { get => _matchState; set => changeState(value); }
    public float currentMatchTime { get; set; }
    public int currentMatchMinutes { get => Mathf.FloorToInt(currentMatchTime / 60); }
    public int currentMatchSeconds { get => Mathf.FloorToInt(currentMatchTime % 60); }
    public bool inGame { get; set; }
    public bool endGame { get; set; }
    public CornerComponents currentCorner { get; set; }
    public int minutes = 5;
    public int seconds = 0;
    public int restMatchMinutes { get => minutes - currentMatchMinutes; }
    public int restMatchSeconds { get => seconds - currentMatchSeconds; }
    public Team possessionTeam { get; set; }
    public Team noPossessionTeam { get; set; }
    public PublicPlayerData posssessionPlayer { get; set; }
    private void Start()
    {
        MatchComponents.RulesData = this;
        MatchEvents.kick.AddListener(Kick);
    }
    void Kick(KickEventArgs args)
    {
        if (inGame)
        {
            posssessionPlayer = args.kickerPublicPlayerData;
            possessionTeam = args.kickerTeam;
            noPossessionTeam = Teams.getRivalTeam(possessionTeam.TeamName);
        }
    }
    void changeState(MatchState matchState)
    {
        matchState = _matchState;
        MatchEvents.fieldPositionsChanged
        switch (matchState)
        {
            case MatchState.Corner:
                MatchComponents.CullPassPoints.pressureName = FootballPositionCtrl.CornerPressureTypeNormalMatch[TypeMatch.typeNormalMatch];
                MatchComponents.Brains.attackPressure = FootballPositionCtrl.CornerPressureTypeNormalMatch[TypeMatch.typeNormalMatch];
                MatchComponents.Brains.defensePressure = FootballPositionCtrl.CornerPressureTypeNormalMatch[TypeMatch.typeNormalMatch];
                inGame = false;
                endGame = false;
                break;
            case MatchState.InGame:
                MatchComponents.CullPassPoints.pressureName = FootballPositionCtrl.DefensePressureTypeNormalMatch[TypeMatch.typeNormalMatch];
                MatchComponents.Brains.attackPressure = FootballPositionCtrl.AttackPressureTypeNormalMatch[TypeMatch.typeNormalMatch];
                MatchComponents.Brains.defensePressure = FootballPositionCtrl.DefensePressureTypeNormalMatch[TypeMatch.typeNormalMatch];
                inGame = true;
                endGame = false;
                break;
            case MatchState.EndGame:
                inGame = false;
                endGame = true;
                break;
        }
    }
}
