using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Photon.Pun.UtilityScripts.PunTeams;

public enum UserMode
{
    OnePlayer, AllTeam,OnlyBots
}
public class ModeCtrl : MonoBehaviour
{
    public UserMode UserMode;
    public TypeFieldPosition.Type startFieldPositionType;
    TypeFieldPosition.Type currentFieldPositionType;
    float startChangePlayerTime;
    public float attackChangePlayerPeriod = 0.3f, defenseChangePlayerPeriod=2;
    public float attackMaxChangePlayerReachTime = 0.25f, defenseMaxChangePlayerReachTime = 0.7f;
    bool changeAttackPlayerAvailable { get => Time.time - startChangePlayerTime > attackChangePlayerPeriod; }
    bool changeDefensePlayerAvailable { get => Time.time - startChangePlayerTime > defenseChangePlayerPeriod; }
    public string startAttackTeam = "Red";
    public Team team1, team2;
    void Start()
    {
        MatchComponents.ModeCtrl = this;
        

            
        currentFieldPositionType = startFieldPositionType;

        //if (startMyTeam) SetMyTeam(startMyTeamName);
    }
    void Setup()
    {
        TypeMatch.SizeFootballField = SizeFootballFieldID.ElevenVSEleven;
        TypeMatch.typeMatch = TypeMatchID.Playtime;
    }
    // Update is called once per frame
    void Update()
    {
        MatchModeCtrl();
    }
    void MatchModeCtrl()
    {
        switch (MatchComponents.UserMode)
        {
            case UserMode.OnePlayer:
                OnePlayerMode();
                break;
            case UserMode.AllTeam:
                AllTeamMode();
                break;
            case UserMode.OnlyBots:
                OnlyBots();
                break;
        }
    }
    void OnlyBots()
    {
        changePlayerType(MatchComponents.currentPublicPlayerData, PlayerTypeID.Bot);
    }
    void OnePlayerMode()
    {
        if (MatchComponents.currentPublicPlayerData == null)
        {
            MatchComponents.myTeam.getPublicPlayerData(currentFieldPositionType, out PublicPlayerData publicPlayerData);
            
            MatchComponents.currentPublicPlayerData = publicPlayerData;
        }
        else
        {
            if (MatchComponents.currentPublicPlayerData.fieldPositionType != currentFieldPositionType)
            {
                MatchComponents.myTeam.getPublicPlayerData(currentFieldPositionType, out PublicPlayerData publicPlayerData);
                changePlayerType(MatchComponents.currentPublicPlayerData, PlayerTypeID.Bot);
                MatchComponents.currentPublicPlayerData = publicPlayerData;
                changePlayerType(publicPlayerData, PlayerTypeID.Puppet);
            }
        }
        if (MatchComponents.currentPublicPlayerData != null&&MatchComponents.currentPublicPlayerData.IsBot)
        {
            changePlayerType(MatchComponents.currentPublicPlayerData, PlayerTypeID.Puppet);
        }
        if(MatchComponents.currentPublicPlayerData != null && MatchComponents.currentPublicPlayerData.playerData.noPossessionMode==NoPossessionMode.Automatic)
        {
            MatchComponents.currentPublicPlayerData.playerData.noPossessionMode = NoPossessionMode.Freelance;
        }
    }
    
    void AllTeamMode()
    {
        if (MatchComponents.currentPublicPlayerData == null)
        {
            if(MatchComponents.myTeam.firstReachBallPublicPlayerData!=null)
                MatchComponents.myTeam.firstReachBallPublicPlayerData.ChangePlayerType(PlayerTypeID.Puppet);
        }
        PublicPlayerData currentPublicPlayer = MatchComponents.currentPublicPlayerData;

        if (currentPublicPlayer==null||currentPublicPlayer.team == null) return;
        PublicPlayerData firstPublicPlayerData = MatchComponents.myTeam.firstReachBallPublicPlayerData;
        if (firstPublicPlayerData == null) return;
        if (MatchComponents.currentReachBallTeam.Equals(MatchComponents.myTeam))
        {
            float timeDiference = firstPublicPlayerData.playerData.ballReachTime - currentPublicPlayer.playerData.ballReachTime;
            bool change1 = firstPublicPlayerData.playerData.ballReachTime < 0.1f && firstPublicPlayerData.playerData.ballReachTime < currentPublicPlayer.playerData.ballReachTime;
            if ((changeAttackPlayerAvailable && timeDiference <= attackMaxChangePlayerReachTime) || change1)
            {
                changeCurrentPlayer(firstPublicPlayerData, currentPublicPlayer);
            }
        }
        else
        {
            float timeDiference = firstPublicPlayerData.playerData.ballReachTime - currentPublicPlayer.playerData.ballReachTime;
            bool change1 = firstPublicPlayerData.playerData.ballReachTime < 0.1f && firstPublicPlayerData.playerData.ballReachTime < currentPublicPlayer.playerData.ballReachTime;
            if ((changeDefensePlayerAvailable && timeDiference <= defenseMaxChangePlayerReachTime) || change1)
            {
                changeCurrentPlayer(firstPublicPlayerData, currentPublicPlayer);
                //firstPublicPlayerData.ChangeNoPossessionMode(NoPossessionMode.Freelance);
            }
        }
    }
    void changeCurrentPlayer(PublicPlayerData firstPublicPlayerData,PublicPlayerData currentPublicPlayer)
    {
        if (firstPublicPlayerData.IsGoalkeeper || currentPublicPlayer.Equals(firstPublicPlayerData) || firstPublicPlayerData.playerType.Value == PlayerTypeID.Puppet) return;
        firstPublicPlayerData.ChangePlayerType(PlayerTypeID.Puppet);
        currentPublicPlayer.ChangePlayerType(PlayerTypeID.Bot);
        startChangePlayerTime=Time.time;
        currentFieldPositionType = firstPublicPlayerData.fieldPositionType;
    }
    public void changePlayerType(PublicPlayerData publicPlayerData, PlayerTypeID playerTypeID)
    {
        if (publicPlayerData==null||publicPlayerData.IsGoalkeeper||publicPlayerData.playerType.Value == playerTypeID) return;
        publicPlayerData.ChangePlayerType(playerTypeID);
        startChangePlayerTime = Time.time;
        currentFieldPositionType = publicPlayerData.fieldPositionType;
    }
}
