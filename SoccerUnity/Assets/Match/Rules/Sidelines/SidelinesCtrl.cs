using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SidelinesCtrl : MonoBehaviour
{
    public SimplePass pass;
    SidelineEventArgs sideLineData;
    void Start()
    {
        MatchComponents.rulesComponents.sidelinesCtrl = this;
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
    private void FixedUpdate()
    {
        if (RulesCtrl.checkersEnabled)
        {
            check();
        }
    }
    void check()
    {
        bool isForward, isInside;
        foreach (var sideline in MatchComponents.footballField.sidelines)
        {
            //sideline.PointIsForward(MatchComponents.ballComponents.rigBall.position, out isInside, out isForward);
            Vector3 closestPoint;
            isInside = sideline.GetPoint(MatchComponents.ballComponents.rigBall.position,out closestPoint,out isForward);
            if (isInside && !isForward)
            {
                //print("throw-in");
                //Debug.DrawLine(MatchComponents.ballComponents.rigBall.position, closestPoint);
                
                MatchEvents.stopMatch.Invoke();
                //Vector3 passVelocity = pass.getRandomPassVelocity(corner.cornerPoint.forward);
                
                Vector3 passVelocity = pass.getRandomPassVelocity(sideline.transform.forward);
                closestPoint += sideline.transform.forward * MatchComponents.ballRadio;
                SidelineEventArgs args = new SidelineEventArgs(closestPoint, passVelocity,MatchData.lastTeamPossession);
                DebugsList.rules.print("SidelinesCtrl.notify()");
                RulesEvents.notifySideline.Invoke(args);
            }
        }
    }
    public void execute(SidelineEventArgs args)
    {
        MatchEvents.stopMatch.Invoke();
        
        sideLineData = args;
        Invoke(nameof(setBallData), MatchComponents.rulesComponents.settings.TimeWaitToSetBallData);
        DebugsList.rules.print("SidelinesCtrl.execute()");
        MatchComponents.rulesComponents.whistleAnimation.Play();
        StartCoroutine(checkSidelineInactivity(args));
    }
    void continueMatch()
    {
        MatchEvents.continueMatch.Invoke();
        stopAll();
    }
    void stopAll()
    {
        /*
        MatchComponents.rulesComponents.MessageWithCountDown.StopAllCoroutines();
        MatchComponents.rulesComponents.MessageWithCountDown.Hide();*/
        StopAllCoroutines();
        MatchEvents.kick.RemoveListener(continueMatch);
        Kick.ballLocked = false;
        MatchComponents.rulesComponents.invisibleCircularWall.Disable();
        OnlineBallCtrl.getRoutineData = true;
    }
    IEnumerator checkSidelineInactivity(SidelineEventArgs args)
    {
        float durationInactivity = MatchComponents.rulesComponents.settings.inactivityWait + MatchComponents.rulesComponents.settings.TimeWaitToSetBallData;
        yield return new WaitForSeconds(durationInactivity);
        Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        KickEventArgs kickArgs = new KickEventArgs(args.velocity, ballRigidbody.velocity, ballRigidbody.angularVelocity,ballRigidbody.position, "");
        Kick.AddForce( ForceMode.VelocityChange, kickArgs);
    }
    void setBallData()
    {
        Vector3 ballPosition = MyFunctions.setYToVector3(sideLineData.point, MatchComponents.ballRadio);
        Rigidbody ballRigidbody = MatchComponents.ballRigidbody;
        Transform ballTransform = MatchComponents.ballTransform;
        ballTransform.position = ballPosition;
        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        if (sideLineData!=null && sideLineData.Team!=null && sideLineData.Team.IsMine)
        {
            Kick.ballLocked = true;
            List<PublicPlayerData> publicPlayerDatas = Teams.MyTeam.getPublicPlayerDatasWithAssignedFieldPosition();
            float minDistanceCircularWall = MatchComponents.rulesSettings.distanceCircularWall;
            MatchComponents.rulesComponents.invisibleCircularWall.setParameters(publicPlayerDatas, MatchComponents.ballComponents.transBall, minDistanceCircularWall);
            MatchComponents.rulesComponents.invisibleCircularWall.Enable();
        }
        else
        {
            OnlineBallCtrl.getRoutineData = false;
        }
        MatchEvents.kick.AddListener(continueMatch);
    }
}
