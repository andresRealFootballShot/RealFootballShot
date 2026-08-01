using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TeamSetup : MonoBehaviour {
    public Team team;
    public SideOfFieldID sideOfFieldID;
    public Transform parent;
    
    public GameObject playerPrefab;
    
    public PlaytimeCtrl PlaytimeCtrl;
    public Lineup.TypeLineup typeLineup;
    
   
    public bool isMyTeam;

    void Start()
    {
        SetMyTeam();
        SideOfFieldCtrl.setTeamSide(team.TeamName, sideOfFieldID);
        team.setLineup(typeLineup);
        createPlayers();
    }
    void createPlayers()
    {
        for (int i = 0; i < TypeMatch.maxTeamPlayers; i++)
        {
            TypeFieldPosition.Type typeFieldPosition = TypeMatch.fieldPositioinsInTypeMatch[TypeMatch.typeNormalMatch][i];
            if (typeFieldPosition == TypeFieldPosition.Type.GoalKeeper) continue;
            GameObject playerGObj = Instantiate(playerPrefab,parent);
            AddPlayerToTeam AddPlayerToTeam = playerGObj.GetComponent<AddPlayerToTeam>();
            AddPlayerToTeam.fieldPositionType = typeFieldPosition;
            AddPlayerToTeam.teamName = team.TeamName;
            
            PublicPlayerData PublicPlayerData = playerGObj.GetComponent<PublicPlayerData>();
            PublicPlayerData.playerIDMono.LocalLoad(0);
            PublicPlayerData.fieldPositionType = typeFieldPosition;
            playerGObj.SetActive(true);

            if (typeFieldPosition == MatchComponents.ModeCtrl.startFieldPositionType&& isMyTeam)
            {
                MatchComponents.currentPublicPlayerData = PublicPlayerData;
                MatchComponents.ModeCtrl.changePlayerType(PublicPlayerData, PlayerTypeID.Puppet);

            }

            team.StartPressurePosition(PublicPlayerData);
            List<Kick> kicks = MyFunctions.GetComponentsInChilds<Kick>(playerGObj, true,false);
            foreach (Kick script in kicks)
            {
                if (script.GetType() == typeof(TouchWithDirect))
                {
                    script.setAddForceOffline();
                    script.setBallControlOffline();
                }
                else
                {
                    script.setAddForceAtPositionOffline();
                }
            }
            ComponentsPlayer componentsPlayer = PublicPlayerData.playerComponents.ComponentsPlayer;
            if (componentsPlayer != null)
            {
                componentsPlayer.EnableAll();
                componentsPlayer.scriptsPlayer.hudCtrl.ShowHUD();
                componentsPlayer.scriptsPlayer.hudCtrl.HideGunSight();
                componentsPlayer.scriptsPlayer.cameraPosition.currentSpeedCamera = componentsPlayer.scriptsPlayer.cameraPosition.speedCameraLookingBall;
                componentsPlayer.scriptsPlayer.cameraRotation.currentSpeedCamera = componentsPlayer.scriptsPlayer.cameraRotation.speedCameraLookAtBall;
            }
        }
    }
   
    public void SetMyTeam()
    {
        if(isMyTeam)
         MatchComponents.myTeam = team;

    }

    
}