using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CornerGoalKickCtrl : MonoBehaviour
{
    public SimplePass cornerKickPass,goalKickPass;

    string cornerMessageFormat = "Corner kick in {0} seconds";
    string goalKickMessageFormat = "Goal kick in {0} seconds";
    Coroutine messageCoroutine,checkInactivity;
    void Start()
    {
        MatchComponents.rulesComponents.cornerCtrl = this;
        enabled = false;
        MatchEvents.footballFieldLoaded.AddListenerConsiderInvoked(footballFieldIsLoadedEvent);
        MatchEvents.endPart.AddListenerConsiderInvoked(endPart);
    }
    void endPart()
    {
        stopAll();
    }
    void footballFieldIsLoadedEvent()
    {
        enabled = true;
    }
    void FixedUpdate()
    {
        if (RulesCtrl.checkersEnabled)
        {
            checkCorner();
        }
    }
    void checkCorner()
    {
        bool isForward, isInside;
        foreach (var sideOfField in MatchComponents.footballField.sideOfFields)
        {
            foreach (var corner in sideOfField.corners)
            {
                foreach (var plane in corner.planes)
                {
                    plane.PointIsForward(MatchComponents.ballComponents.rigBall.position, out isInside, out isForward, corner.name);
                    if (isInside && !isForward)
                    {
                        MatchEvents.stopMatch.Invoke();
                        Vector3 cornerVelocity = cornerKickPass.getRandomPassVelocity(corner.cornerPoint.forward);
                        Vector3 goalKickVelocity = goalKickPass.getRandomPassVelocity(sideOfField.goalKickPoint.forward);
                        CornerEventArgs args = new CornerEventArgs(sideOfField,corner,MatchData.lastPlayerIDPossession, cornerVelocity, goalKickVelocity);
                        RulesEvents.notifyCorner.Invoke(args);
                    }
                }
            }
        }
    }
    IEnumerator checkCornerInactivity(CornerEventArgs args)
    {
        float durationInactivity = MatchComponents.rulesComponents.settings.inactivityWait+ MatchComponents.rulesComponents.settings.TimeWaitToSetBallData;
        yield return new WaitForSeconds(durationInactivity);
        playWhistle();
        args.teamName = Teams.teamsList.Find(x => x.TeamName != args.teamName).TeamName;
        goalKick(args);
    }
    public void dispatchCorner(CornerEventArgs args)
    {
        MatchEvents.stopMatch.Invoke();
        //DebugsList.rules.print("CornerCtrl.dispatchCorner()");
        
        SideOfField sideOfFieldOfTeamPlayer;
        SideOfFieldCtrl.getSideOfFieldOfTeam(args.teamName, out sideOfFieldOfTeamPlayer);
        if (sideOfFieldOfTeamPlayer.Value.Equals(args.sideOfFieldID))
        {
            cornerKick(args);
        }
        else
        {
            
            goalKick(args);
        }
        MatchEvents.losePossession.Invoke();
        MatchEvents.stopMatch.Invoke();
    }
    
    void cornerKick(CornerEventArgs args)
    {
        bool _isMyTeam = isMyTeam(args);
        if (TypeMatch.getTeamMaxPlayersWithGoalkeepers() < 4)
        {
            float duration = MatchComponents.rulesComponents.settings.MessageKickWait;
            float waitToStart = MatchComponents.rulesComponents.settings.TimeWaitToSetBallData;
            messageCoroutine = StartCoroutine(MatchComponents.rulesComponents.MessageWithCountDown.countDown<Vector3>(duration, waitToStart, 0.25f, 1, cornerMessageFormat, _isMyTeam, args.cornerVelocity, messageEndCountDown));
        }
        else
        {
            checkInactivity = StartCoroutine(checkCornerInactivity(args));
            MatchEvents.kick.AddListener(stopChechInactivity);
        }
        StartCoroutine(waitSetBallData(args.corner.cornerPoint,args));
        MatchEvents.corner.Invoke();
    }
    void goalKick(CornerEventArgs args)
    {
        Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        bool _isMyTeam = isMyTeam(args);
        float duration = MatchComponents.rulesComponents.settings.MessageKickWait;
        float waitToStart = MatchComponents.rulesComponents.settings.TimeWaitToSetBallData;
        StartCoroutine(waitSetBallData(args.sideOfField.goalKickPoint,args));
        messageCoroutine = StartCoroutine(MatchComponents.rulesComponents.MessageWithCountDown.countDown<CornerEventArgs>(duration, waitToStart, 0.25f, 1, goalKickMessageFormat, _isMyTeam, args, kickMessageEndCountDown));
        //KickEventArgs kickArgs = new KickEventArgs(args.goalKickVelocity, ballRigidbody.velocity, ballRigidbody.angularVelocity, "");
        //Kick.AddForce(MatchComponents.ballComponents.rigBall, ForceMode.VelocityChange, kickArgs);
    }
   
    void stopChechInactivity()
    {
        if (checkInactivity != null)
        {
            StopCoroutine(checkInactivity);
        }
        MatchEvents.kick.RemoveListener(stopChechInactivity);
    }
    void continueMatch()
    {
        MatchEvents.continueMatch.Invoke();
        //StopCoroutine(messageCoroutine);
        stopAll();
    }
    void stopAll()
    {
        StopAllCoroutines();
        MatchComponents.rulesComponents.MessageWithCountDown.StopAllCoroutines();
        MatchComponents.rulesComponents.MessageWithCountDown.Hide();
        MatchEvents.kick.RemoveListener(continueMatch);
        MatchEvents.kick.RemoveListener(stopChechInactivity);
        Kick.ballLocked = false;
        MatchComponents.rulesComponents.invisibleCircularWall.Disable();
    }
    void playWhistle()
    {
        MatchComponents.rulesComponents.whistleAnimation.Play();
    }
    IEnumerator waitSetBallData(Transform point, CornerEventArgs args)
    {
        yield return new WaitForSeconds(MatchComponents.rulesComponents.settings.TimeWaitToSetBallData);
        
        setBallData(point,args);
    }
    void messageEndCountDown(Vector3 velocity)
    {
        StopCoroutine(messageCoroutine);
        Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        KickEventArgs kickArgs = new KickEventArgs(velocity, ballRigidbody.velocity, ballRigidbody.angularVelocity,ballRigidbody.position, "");
        Kick.AddForce(ForceMode.VelocityChange,kickArgs);
        //DebugsList.rules.print("CornerCtrl.endCountDown velocity=" + velocity);
    }
    void kickMessageEndCountDown(CornerEventArgs args)
    {
        StopCoroutine(messageCoroutine);
        Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        KickEventArgs kickArgs = new KickEventArgs(args.goalKickVelocity, ballRigidbody.velocity, ballRigidbody.angularVelocity, ballRigidbody.position, Teams.getTeamByName(args.teamName).getGoalkeeper());
        Kick.AddForce(ForceMode.VelocityChange, kickArgs);
        //DebugsList.rules.print("CornerCtrl.endCountDown velocity=" + args.goalKickVelocity);
    }
    void setBallData(Transform point, CornerEventArgs args)
    {
        Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        ballRigidbody.position = point.position;
        ballRigidbody.rotation = point.rotation;
        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        Team myTeam;
        Teams.getTeamFromPlayer(ComponentsPlayer.myMonoPlayerID.playerIDStr, out myTeam);
        if (myTeam.TeamName.Equals(args.teamName))
        {
            Kick.ballLocked = true;
            List<PublicPlayerData> publicPlayerDatas = myTeam.getPublicPlayerDatasWithAssignedFieldPosition();
            float minDistanceCircularWall = MatchComponents.rulesSettings.distanceCircularWall;
            MatchComponents.rulesComponents.invisibleCircularWall.setParameters(publicPlayerDatas, MatchComponents.ballComponents.transBall, minDistanceCircularWall);
            MatchComponents.rulesComponents.invisibleCircularWall.Enable();
        }
        else
        {
            Kick.ballLocked = false;
            List<PublicPlayerData> publicPlayerDatas = myTeam.getPublicPlayerDatasWithAssignedFieldPosition();
            float minDistanceCircularWall = MatchComponents.rulesSettings.distanceCircularWall;
            //MatchComponents.rulesComponents.invisibleCircularWall.setParameters(publicPlayerDatas, MatchComponents.ballComponents.transBall, minDistanceCircularWall);

            MatchComponents.rulesComponents.invisibleCircularWall.Disable();
        }
        MatchEvents.kick.AddListener(continueMatch);
    }
    bool isMyTeam(CornerEventArgs args)
    {
        Team myTeam;
        Teams.getTeamFromPlayer(ComponentsPlayer.myMonoPlayerID.playerIDStr, out myTeam);
        return myTeam.TeamName.Equals(args.teamName);
    }
    /*
    private void OnDrawGizmos()
    {
        foreach (var corner in corners)
        {
            foreach (var plane in corner.planes)
            {
                Gizmos.color = Color.cyan;
                if (plane.pointNormalTrans != null)
                {
                    Gizmos.DrawSphere(plane.pointNormalTrans.TransformPoint(-Vector3.forward * ballRadio), 0.15f);
                    foreach (var item in plane.limitsTrans)
                    {
                        Gizmos.DrawSphere(item.TransformPoint(-Vector3.forward * ballRadio),0.15f);
                    }
                }
            }
        }
    }*/
}
