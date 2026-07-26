using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TeamSetup : MonoBehaviour {
    public Team team;
    public SideOfFieldID sideOfFieldID;
    public Transform parent;
    public Transform startPositionTestParent;
    public GameObject playerPrefab;
    public FootballPositionCtrl FootballPositionCtrl;
    public PlaytimeCtrl PlaytimeCtrl;
    public Lineup.TypeLineup typeLineup;
    public bool startPositionTest;
    public TypeFieldPosition.Type kickoffTypeFielPosition= TypeFieldPosition.Type.RightForward;
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

            StartPosition(PublicPlayerData);
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
    public void StartPosition()
    {
        if (startPositionTest)
        {
            StartPositionTest();
        }
        else
        {
            foreach (PublicPlayerData publicPlayerData in team.outfieldPublicPlayerDatas)
            {
                if (FootballPositionCtrl.GetFieldPositionDataPosition("Default", team.startPressure, publicPlayerData, MatchComponents.ballPosition, out Vector3 position))
                {
                    publicPlayerData.rigidbody.velocity = Vector3.zero;
                    if (publicPlayerData.fieldPositionType.Equals(kickoffTypeFielPosition)&& team.startAttack)
                    {
                        publicPlayerData.position = MatchComponents.ballPosition+publicPlayerData.SideOfField.forwardTransform.TransformDirection(new Vector3(0.3f,0,0.3f));
                        if(publicPlayerData.IsPuppet)
                            publicPlayerData.playerData.playerMode = PlayerState.WithPossession;
                        Vector3 dir = MatchComponents.ballPosition - publicPlayerData.position;
                        dir.y = 0;

                        publicPlayerData.bodyTransform.rotation = Quaternion.LookRotation(dir, publicPlayerData.bodyTransform.up);
                    }
                    else
                    {
                        publicPlayerData.position = position;
                        Vector3 dir = MatchComponents.ballPosition - publicPlayerData.position;
                        dir.y = 0;

                        publicPlayerData.bodyTransform.rotation = Quaternion.LookRotation(dir, publicPlayerData.bodyTransform.up);
                    }
                }
            }
        }
    }
    public void StartPositionTest()
    {
        Transform[] transforms = startPositionTestParent.GetComponentsInChildren<Transform>();
        for (int i = 0;i < team.outfieldPublicPlayerDatas.Count;i++)
        {
            PublicPlayerData publicPlayerData = team.outfieldPublicPlayerDatas[i];
            if(!publicPlayerData.IsGoalkeeper)
                publicPlayerData.position = transforms[i].position;
            
        }
    }
    public void StartPositionTest(PublicPlayerData publicPlayerData)
    {
        TypeFieldPosition[] TypeFieldPositions = startPositionTestParent.GetComponentsInChildren<TypeFieldPosition>();
        for (int i = 0; i < TypeFieldPositions.Length; i++)
        {
            TypeFieldPosition typeFieldPosition = TypeFieldPositions[i];
            if (typeFieldPosition.Value == publicPlayerData.fieldPositionType)
            {
                if (!publicPlayerData.IsGoalkeeper)
                    publicPlayerData.position = typeFieldPosition.transform.position;
            }

        }
    }
    public void SetMyTeam()
    {
        if(isMyTeam)
         MatchComponents.myTeam = team;

    }
    public void StartPosition(PublicPlayerData publicPlayerData)
    {
        if (startPositionTest)
        {
            StartPositionTest(publicPlayerData);
        }
        else
        {
            if (FootballPositionCtrl.GetFieldPositionDataPosition("Default", team.startPressure, publicPlayerData, MatchComponents.ballPosition, out Vector3 position))
            {
                publicPlayerData.rigidbody.velocity = Vector3.zero;
                if (publicPlayerData.fieldPositionType.Equals(kickoffTypeFielPosition) && team.startAttack)
                {
                    publicPlayerData.position = MatchComponents.ballPosition + publicPlayerData.SideOfField.forwardTransform.TransformDirection(new Vector3(0.5f, 0, 0.5f));
                    Vector3 dir = MatchComponents.ballPosition - publicPlayerData.position;
                    dir.y = 0;


                    if (publicPlayerData.IsPuppet)
                        publicPlayerData.playerData.playerMode = PlayerState.WithPossession;
                    publicPlayerData.bodyTransform.rotation = Quaternion.LookRotation(dir, publicPlayerData.bodyTransform.up);
                }
                else
                {
                    publicPlayerData.position = position;
                    Vector3 dir = MatchComponents.ballPosition - publicPlayerData.position;
                    dir.y = 0;

                    publicPlayerData.bodyTransform.rotation = Quaternion.LookRotation(dir, publicPlayerData.bodyTransform.up);
                }
            }
        }
        
    }
}