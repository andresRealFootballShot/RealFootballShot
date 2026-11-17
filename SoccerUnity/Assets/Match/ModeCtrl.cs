using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Photon.Pun.UtilityScripts.PunTeams;

public enum UserMode
{
    OnePlayer, AllTeam,OnlyBots,OnePlayerAutomaticReachBall
}
public class ModeCtrl : MonoBehaviour
{
    public UserMode UserMode;
    public bool FreeCameraLoseBall=true;
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
        MatchEvents.changeTeamPossession.AddListener(ChangePossession);
        //if (startMyTeam) SetMyTeam(startMyTeamName);
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
            case UserMode.OnePlayerAutomaticReachBall:
                OnePlayerAutomaticReachBall();
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
        changePlayerType(MatchComponents.currentMyPublicPlayerData, PlayerTypeID.Bot);
    }
    void OnePlayerAutomaticReachBall()
    {
        if (MatchComponents.currentMyPublicPlayerData == null)
        {
            MatchComponents.myTeam.getPublicPlayerData(currentFieldPositionType, out PublicPlayerData publicPlayerData);

            MatchComponents.currentMyPublicPlayerData = publicPlayerData;
        }
        else
        {
            if (MatchComponents.currentMyPublicPlayerData.fieldPositionType != currentFieldPositionType)
            {
                MatchComponents.myTeam.getPublicPlayerData(currentFieldPositionType, out PublicPlayerData publicPlayerData);
                changePlayerType(MatchComponents.currentMyPublicPlayerData, PlayerTypeID.Bot);
                MatchComponents.currentMyPublicPlayerData = publicPlayerData;
                changePlayerType(publicPlayerData, PlayerTypeID.Puppet);
            }
        }
        if (MatchComponents.myTeam == null) return;
        if (MatchComponents.currentMyPublicPlayerData != null && MatchComponents.currentMyPublicPlayerData.IsBot)
        {
            changePlayerType(MatchComponents.currentMyPublicPlayerData, PlayerTypeID.Puppet);
        }
        if (MatchComponents.currentMyPublicPlayerData != null && MatchComponents.currentMyPublicPlayerData.playerData.noPossessionMode == NoPossessionMode.Automatic)
        {
            MatchComponents.currentMyPublicPlayerData.playerData.noPossessionMode = NoPossessionMode.Freelance;
        }
        if(MatchComponents.currentMyPublicPlayerData != null && MatchComponents.currentMyPublicPlayerData.Equals( MatchComponents.myTeam.firstReachBallPublicPlayerData))
        {
            MatchComponents.currentMyPublicPlayerData.playerData.noPossessionMode = NoPossessionMode.Automatic;
        }
        else
        {
            MatchComponents.currentMyPublicPlayerData.playerData.noPossessionMode = NoPossessionMode.Freelance;
        }
    }
    void OnePlayerMode()
    {
        if (MatchComponents.currentMyPublicPlayerData == null)
        {
            MatchComponents.myTeam.getPublicPlayerData(currentFieldPositionType, out PublicPlayerData publicPlayerData);
            
            MatchComponents.currentMyPublicPlayerData = publicPlayerData;
        }
        else
        {
            if (MatchComponents.currentMyPublicPlayerData.fieldPositionType != currentFieldPositionType)
            {
                MatchComponents.myTeam.getPublicPlayerData(currentFieldPositionType, out PublicPlayerData publicPlayerData);
                changePlayerType(MatchComponents.currentMyPublicPlayerData, PlayerTypeID.Bot);
                MatchComponents.currentMyPublicPlayerData = publicPlayerData;
                changePlayerType(publicPlayerData, PlayerTypeID.Puppet);
            }
        }
        if (MatchComponents.currentMyPublicPlayerData != null&&MatchComponents.currentMyPublicPlayerData.IsBot)
        {
            changePlayerType(MatchComponents.currentMyPublicPlayerData, PlayerTypeID.Puppet);
        }
        if(MatchComponents.currentMyPublicPlayerData != null && MatchComponents.currentMyPublicPlayerData.playerData.noPossessionMode==NoPossessionMode.Automatic)
        {
            MatchComponents.currentMyPublicPlayerData.playerData.noPossessionMode = NoPossessionMode.Freelance;
        }
    }
    void AllTeamMode()
    {
        if (MatchComponents.currentMyPublicPlayerData == null)
        {
            if (MatchComponents.myTeam.firstReachBallPublicPlayerData != null)
            {
                MatchComponents.myTeam.firstReachBallPublicPlayerData.ChangePlayerType(PlayerTypeID.Puppet);
            }
        }
        PublicPlayerData currentPublicPlayer = MatchComponents.currentMyPublicPlayerData;

        if (currentPublicPlayer==null||currentPublicPlayer.team == null) return;
        PublicPlayerData firstPublicPlayerData = MatchComponents.myTeam.firstReachBallPublicPlayerData;
        if (firstPublicPlayerData == null) return;
        if (MatchComponents.possessionTeam.Equals(MatchComponents.myTeam))
        {
            float timeDiference = firstPublicPlayerData.playerData.ballReachTime - currentPublicPlayer.playerData.ballReachTime;
            bool change1 = firstPublicPlayerData.playerData.ballReachTime < 0.1f && firstPublicPlayerData.playerData.ballReachTime < currentPublicPlayer.playerData.ballReachTime;
            bool change2 =  currentPublicPlayer.ballReachTime > 0.5f&& currentPublicPlayer.playerData.playerMode != PlayerState.WithPossession;
            
            if (!firstPublicPlayerData.Equals(currentPublicPlayer) &&change2 && (changeAttackPlayerAvailable && timeDiference <= attackMaxChangePlayerReachTime ))
            {
                changeCurrentPlayer(firstPublicPlayerData, currentPublicPlayer);
                currentPublicPlayer.ChangePlayerState(PlayerState.LookingBall);
                firstPublicPlayerData.ChangeNoPossessionMode(NoPossessionMode.Automatic);
                
            }
        }
        else
        {
            bool availableChangeDefense = false;
            float timeDiference = firstPublicPlayerData.playerData.ballReachTime - currentPublicPlayer.playerData.ballReachTime;
            bool change1 = firstPublicPlayerData.playerData.ballReachTime < 0.1f && firstPublicPlayerData.playerData.ballReachTime < currentPublicPlayer.playerData.ballReachTime;
            if (!firstPublicPlayerData.Equals(currentPublicPlayer)&&availableChangeDefense &&((changeDefensePlayerAvailable && timeDiference <= defenseMaxChangePlayerReachTime) || change1))
            {
                changeCurrentPlayer(firstPublicPlayerData, currentPublicPlayer);
                //currentPublicPlayer.ChangeNoPossessionMode(NoPossessionMode.Freelance);
                firstPublicPlayerData.ChangePlayerState(PlayerState.Free);
            }
        }
    }
    void ChangePossession()
    {
        PublicPlayerData currentPublicPlayer = MatchComponents.currentMyPublicPlayerData;
        if (!MatchComponents.possessionTeam.Equals(MatchComponents.myTeam))
        {
            currentPublicPlayer.ChangePlayerState(PlayerState.Free);
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
