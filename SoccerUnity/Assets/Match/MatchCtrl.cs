using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering;

public class MatchCtrl : MonoBehaviour
{
    public string startAttackTeam = "Red";
    public AudioSource audioSource;
    public AudioClip pitido, pitidoLargo;
    public MatchRulesSettings MatchRulesSettings;
    public float volume = 0.1f;
    public float startTime = 2;
    public MatchData MatchData { get => MatchComponents.MatchData; set => MatchComponents.MatchData = value; }
    void Start()
    {
        MatchComponents.MatchCtrl = this;
        MatchComponents.rulesComponents.settings = MatchRulesSettings;
        MatchComponents.MatchData.startAttackTeamName = startAttackTeam;
        foreach (Team team in Teams.teamsList)
        {
            team.startAttack = MatchComponents.MatchData.startAttackTeamName.Equals(team.TeamName);
           
            MatchComponents.MatchData.possessionTeam = team.startAttack ? team : MatchComponents.MatchData.possessionTeam;
            MatchComponents.MatchData.noPossessionTeam = !team.startAttack ? team : MatchComponents.MatchData.noPossessionTeam;
        }
        
        StartContinueMatch();
        MatchEvents.kick.AddListener(Kick);
    }

    public void changeSideOfField()
    {
        MatchComponents.MatchData.startAttackTeamName = MatchComponents.MatchData.startAttackTeamName.Equals("Red") ? "Blue" : "Red";
        foreach (Team team in Teams.teamsList)
        {

            team.startAttack = MatchComponents.MatchData.startAttackTeamName.Equals(team.TeamName);
            SideOfFieldID sideOfFieldID = team.SideOfField.Value == SideOfFieldID.One ? SideOfFieldID.Two : SideOfFieldID.One;
            SideOfFieldCtrl.setTeamSide(team.TeamName, sideOfFieldID);
            team.setSideOfField(sideOfFieldID);
            team.SideOfField.goalComponents.goalkeeper = team.getGoalkeeperPublicPlayerData().gameObject;
            PublicGoalkeeperData publicGoalkeeperData = team.getGoalkeeperPublicPlayerData() as PublicGoalkeeperData;
            publicGoalkeeperData.components.goalkeeperCtrl.setSideOfField(team.SideOfField);
            publicGoalkeeperData.components.goalkeeperCtrl.SetStartPosition();
        }
    }
    public void StartContinueMatch()
    {
        foreach (Team team in Teams.teamsList)
        {
            team.startAttack = team.TeamName == MatchData.startAttackTeamName;
        }
        InitialMatch();
        MatchComponents.ballPosition = MatchComponents.footballField.center;
        MatchComponents.ballVelocity = Vector3.zero;
        MatchComponents.ballAngularVelocity = Vector3.zero;
        foreach (Team team in Teams.teamsList)
        {
            team.StartPressurePosition();
        }

        audioSource.clip = pitido;
        audioSource.volume = volume;
        audioSource.Play();
        Invoke(nameof(EnableGame), startTime);
    }
    public void EndMatch()
    {

        audioSource.clip = pitidoLargo;
        audioSource.volume = volume;
        audioSource.Play();
        MatchData.endMatch = true;
        MatchData.inGame = false;
        MatchData.enabledRules = false;
        MatchEvents.matchStateChanged.Invoke();
    }
    public void EnableGame()
    {
        MatchData.inGame = true;
        MatchData.enabledRules = true;
        MatchData.inCorner = false;
        MatchData.inGoal = false;
        MatchEvents.continueMatch.Invoke();
        MatchEvents.matchStateChanged.Invoke();
    }
    void Kick(KickEventArgs args)
    {
        if (MatchData.inGame)
        {
            MatchData.posssessionPlayer = args.kickerPublicPlayerData;
            MatchData.possessionTeam = args.kickerTeam;
            MatchData.noPossessionTeam = Teams.getRivalTeam(MatchData.possessionTeam.TeamName);

        }else if (MatchData.initialMatch && MatchData.startAttackTeam.ContainsPlayer(args.playerID))
        {
            EnableGame();
        }
        else if (MatchData.inCorner && MatchData.possessionTeam.ContainsPlayer(args.playerID))
        {
            EnableGame();
        }
    }

    void InitialMatch()
    {
        MatchEvents.initialMatch.Invoke();
        MatchData.initialMatch = true;
        MatchData.inGame = false;
        MatchData.enabledRules = false;
        MatchData.inCorner = false;
        MatchData.inGoal = false;
        MatchEvents.matchStateChanged.Invoke();
    }
    public void Corner()
    {
        MatchComponents.CullPassPoints.pressureName = FootballPositionCtrl.CornerPressureTypeNormalMatch[TypeMatch.typeNormalMatch];
        MatchData.inCorner = true;
        MatchData.inGame = false;
        MatchData.enabledRules = false;
        MatchEvents.stopMatch.Invoke();
        MatchEvents.matchStateChanged.Invoke();
        MatchEvents.corner.Invoke();
    }
    public void Goal(GoalData args)
    {
        MatchData.inGoal = true;
        MatchData.inGame = false;
        MatchData.enabledRules = false;
        MatchEvents.stopMatch.Invoke();
        MatchEvents.matchStateChanged.Invoke();
        MatchEvents.goal.Invoke(args);
    }
}
