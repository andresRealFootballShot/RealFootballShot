using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Photon.Pun.UtilityScripts.PunTeams;

public class MatchCtrl : MonoBehaviour
{
    public string startAttackTeam = "Red";
    public int partsSize = 2;
    [Header("Time")]
    [Space(5)]
    public int minutes=5;
    public int seconds = 0;
    [Space(5)]
    public AudioSource audioSource;
    public AudioClip pitido,pitidoLargo;
    public float volume=0.1f;
    float totalSeconds{ get => minutes * 60 + seconds; }
    float partTime { get => (float)totalSeconds / partsSize; }
    int currentPart=1;
    public float currentMatchTime { get; set; }
    public float normalizedTime { get=>currentMatchTime/(minutes*60+seconds); }
    public int currentMatchMinutes { get; set; }
    public int currentMatchSeconds { get; set; }
    public int restMatchMinutes { get=>minutes-currentMatchMinutes;}
    public int restMatchSeconds { get => seconds - currentMatchSeconds; }
    bool inGame;
    bool endGame;
    void Start()
    {
        foreach(Team team in Teams.teamsList)
        {
            team.startAttack = startAttackTeam.Equals(team.TeamName);
        }
        StartMatch();
        StartPart();
        
    }
    void StartMatch()
    {
        inGame = true;
        endGame = false;
    }
    void Update()
    {
        updateMatchTime();
        checkEndMatch();
        checkEndPart();
    }
    void updateMatchTime()
    {
        if (inGame)
        {

            currentMatchTime = Mathf.Clamp(currentMatchTime + Time.deltaTime, 0, totalSeconds);
            currentMatchMinutes = currentMatchMinutes / 60;
            currentMatchSeconds = Mathf.FloorToInt(currentMatchTime % 60);
        }
    }
    void checkEndPart()
    {
        if (currentMatchTime>=partTime*currentPart&&inGame)
        {
            currentPart++;
            StartPart();
            if (currentPart > 0)
            {
                changeSideOfField();
            }
        }
    }
    void checkEndMatch()
    {
        if (currentMatchTime >= totalSeconds&& !endGame)
        {

            audioSource.clip = pitidoLargo;
            audioSource.volume = volume;
            audioSource.Play();
            inGame = false;
            endGame = true;
        }
    }
    void changeSideOfField()
    {
        startAttackTeam = startAttackTeam.Equals("Red") ? "Blue" : "Red";
        foreach (Team team in Teams.teamsList)
        {
            team.startAttack = startAttackTeam.Equals(team.TeamName);
            SideOfFieldID sideOfFieldID = team.SideOfField.Value == SideOfFieldID.One ? SideOfFieldID.Two : SideOfFieldID.One;
            team.setSideOfField(sideOfFieldID);
            team.SideOfField.goalComponents.goalkeeper = team.getGoalkeeperPublicPlayerData().gameObject;
            PublicGoalkeeperData publicGoalkeeperData = team.getGoalkeeperPublicPlayerData() as PublicGoalkeeperData;
            publicGoalkeeperData.components.goalkeeperCtrl.setSideOfField(team.SideOfField);
            publicGoalkeeperData.components.goalkeeperCtrl.SetStartPosition();
        }
    }
    void StartPart()
    {
       
        foreach (Team team in Teams.teamsList)
        {
            team.teamSetup.StartPosition();
        }
        MatchComponents.ballPosition = MatchComponents.footballField.center;
        MatchComponents.ballVelocity = Vector3.zero;
        MatchComponents.ballAngularVelocity = Vector3.zero;
        audioSource.clip = pitido;
        audioSource.volume = volume;
        audioSource.Play();
    }
}
