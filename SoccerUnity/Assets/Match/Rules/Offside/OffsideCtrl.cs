using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsideCtrl : MonoBehaviour
{
    KickEventArgs lastKickData;
    OffsideData data;
    public OffsideAnimation offsideAnimation;
    void Start()
    {
        MatchEvents.kick.AddListener(kick);
        MatchComponents.rulesComponents.offsideCtrl = this;
        enabled = false;
        MatchEvents.footballFieldLoaded.AddListenerConsiderInvoked(footballFieldIsLoadedEvent);
        MatchEvents.endPart.AddListenerConsiderInvoked(endPart);
    }
    void endPart()
    {
        stopAll();
    }
    void stopAll()
    {
        StopAllCoroutines();
        MatchEvents.kick.RemoveListener(continueMatch);
        Kick.ballLocked = false;
        MatchComponents.rulesComponents.invisibleCircularWall.Disable();
        OnlineBallCtrl.getRoutineData = true;
    }
    void footballFieldIsLoadedEvent()
    {
        enabled = true;
    }
    void Update()
    {
        if (RulesCtrl.checkersEnabled && TypeMatch.getTeamMaxPlayersWithGoalkeepers()>1)
        {
            check();
        }
    }
    void kick(KickEventArgs args)
    {
        lastKickData = args;
    }
    void check()
    {
        Vector3 ballPosition = MatchComponents.ballTransform.position;
        string lastPlayerIDPossession = MatchData.lastPlayerIDPossession;
        //DebugsList.rules.print("OffsideCtrl.check()");
        
        foreach (var publicPlayerData in PublicPlayerDataList.all.Values)
        {
            float distanceBallPlayer = Vector3.Distance(ballPosition, publicPlayerData.position);
            if (distanceBallPlayer < MatchComponents.rulesSettings.offsideDistanceBallPlayer)
            {
                Team team;
                Teams.getTeamFromPlayer(publicPlayerData.playerID, out team);
                if (!publicPlayerData.playerID.Equals(lastPlayerIDPossession) && team.TeamName.Equals(MatchData.lastTeamPossession))
                {
                    Team rivalTeam = Teams.getRivalTeamOfPlayer(publicPlayerData.playerID);
                    SideOfField sideOfField;
                    SideOfFieldCtrl.getSideOfFieldOfTeam(rivalTeam.TeamName, out sideOfField);
                    
                    if (sideOfField.pointIsInside(ballPosition))
                    {
                        Plane planeKick = new Plane(sideOfField.limit.forward,lastKickData.pointKick);
                        //DebugsList.rules.print("OffsideCtrl.check() 1");
                        
                        if (planeKick.GetSide(publicPlayerData.bodyTransform.position))
                        {
                            Vector3 defenseLine;
                            getDefenseLine(rivalTeam, out defenseLine);
                            Plane planeDefenseLine = new Plane(sideOfField.limit.forward, defenseLine);
                            //Debug.DrawLine(ballPosition, defenseLine);
                            //DebugsList.rules.print("OffsideCtrl.check() 2");
                            
                            if (planeDefenseLine.GetSide(publicPlayerData.position))
                            {
                                MatchEvents.stopMatch.Invoke();
                                OffsideData offsideData = new OffsideData(ballPosition, publicPlayerData.playerID, team.TeamName);
                                RulesEvents.notifyOffside.Invoke(offsideData);
                            }
                        }
                    }
                }
            }
        }
    }
    public void execute(OffsideData data)
    {
        this.data = data;
        DebugsList.rules.print("OffsideCtrl.execute()");
        Invoke(nameof(playWhistle), 0.5f);
        offsideAnimation.Play();
    }
    void setBallData()
    {
        Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        ballRigidbody.position = data.point;
        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        //OnlineBallCtrl.getRoutineData = false;
        if (data.team.IsMine)
        {
            Kick.ballLocked = true;
            List<PublicPlayerData> publicPlayerDatas = Teams.MyTeam.getPublicPlayerDatasWithAssignedFieldPosition();
            float minDistanceCircularWall = MatchComponents.rulesSettings.distanceCircularWall;
            MatchComponents.rulesComponents.invisibleCircularWall.setParameters(publicPlayerDatas, MatchComponents.ballComponents.transBall, minDistanceCircularWall);
            MatchComponents.rulesComponents.invisibleCircularWall.Enable();
        }
        MatchEvents.kick.AddListener(continueMatch);
    }
    void playWhistle()
    {
       MatchEvents.stopMatch.Invoke();
       
       MatchComponents.rulesComponents.whistleAnimation.Play();
       Invoke(nameof(setBallData), MatchComponents.rulesSettings.TimeWaitToSetBallData);
    }
    void continueMatch()
    {
        MatchEvents.continueMatch.Invoke();
        //StopCoroutine(messageCoroutine);
        stopAll();
    }
    bool getDefenseLine(Team team,out Vector3 result)
    {
        
        SideOfField sideOfField;
        SideOfFieldCtrl.getSideOfFieldOfTeam(team.TeamName, out sideOfField);
        List<Vector3> list = new List<Vector3>();
        //print("getDefenseLine");
        foreach (var playerID in team.fieldPositionOfPlayers.Values)
        {
            if (playerID == "None")
            {
                continue;
            }
            PublicPlayerData publicPlayerData = PublicPlayerDataList.all[playerID];
            if (sideOfField.pointIsInside(publicPlayerData.position) || true)
            {
                int index = 0;
                foreach (var item in list)
                {
                    Plane plane = new Plane(sideOfField.limit.forward, item);
                    if (!plane.GetSide(publicPlayerData.position))
                    {
                        index = list.IndexOf(item) + 1;
                        break;
                    }
                }
                //print("insert "+index+" "+publicPlayerData.playerName);
                list.Insert(index, publicPlayerData.position);
            }
        }
        if (list.Count > 1)
        {
            result = list[1];
            return true;
        }
        else
        {
            result = Vector3.zero;
            return false;
        }
    }
}
