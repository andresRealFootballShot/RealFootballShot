using DOTS_ChaserDataCalculation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using Unity.Entities.UniversalDelegates;
using UnityEditorInternal;
using UnityEngine;
using UnityStandardAssets.Vehicles.Ball;
using static UnityEngine.Networking.UnityWebRequest;

public class IndividualPlay : SoccerPlayerComponent
{
    float maxForce { get => playerSkills.maxForce; }
    float minForce { get => playerSkills.minForce; }
    float maxDistance = 9;
    public float maxAngleDriving = 5;
    public float angleDrivingSkill { get => Mathf.Lerp(0, maxAngleDriving, 1 - playerSkills.drivingSkill); }
    float minHitTime { get => playerSkills.minHitTime; }
    float maxHitTime { get => playerSkills.maxHitTime; }
    public float bodyAdjustAngle, bodyAdjustSpeed;
    public float maxSpeedBodyPercent { get => playerSkills.maxSpeedBodyPercent; }
    public float lastKickDistanceOffset { get => playerSkills.lastKickDistanceOffset; set => playerSkills.lastKickDistanceOffset=value; }
    Coroutine coroutine;
    SegmentedPathElement segmentedPath = new SegmentedPathElement();
    
    public Transform testTransform, testTransform2;
    public float maxAngle = 70;
    float angleTest = 10;
    bool test;
    public PublicPlayerData driverPublicPlayerData;
    float timeTest;
    int kickCountDrive;
    float restTimeDrive;
    Vector3 kickDriveV0Drive;
    float kickDriveTime;
    Vector3 kickDrivePosition1;
    public float totalTimeDrive = 3;
    float kickDriveDivertTime;
    private Vector3 reachPositionDrive, firstReachPositionDrive;
    private float kickDefenseTime1;
    private Vector3 kickDefensePosition1;
    private float totalTimeDefense;
    float kickDriveTotalTime;
    private float defenseReachTime;
    private Vector3 defenseReachPosition;
    private float defenseReachTime2;
    private float driverLastKickTime2;
    private float driverLastKickTime;
    private int kickCountDefenseReach2;
    private Vector3 reachPositionDriftDefense;
    Vector3 driverLastKickPos;
    int kickCountDefenseReach;
    private float restTimeDivertDrive;
    private Vector3 posDefenseLastKick;
    private Vector3 lastKickDistanceOffsetPosition;

    Vector3 endBallReachPoint, endDriverTestingPoint, defenseTestPoint, startAcPoint, driverStartAc, posDriverLastKick, driverTargetPosition;
    float endStartAcT, tlastKickDriver;
    void Start()
    {
        //StartCoroutine(testGetPerfectPass());
    }
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        /*
       Gizmos.DrawSphere(startAcPoint, 0.1f);
       Gizmos.color = Color.magenta;
       Gizmos.DrawSphere(posDriverLastKick, 0.1f);
       Gizmos.color = Color.yellow;
       Gizmos.DrawSphere(firstReachPositionDrive, 0.1f);
       Gizmos.color = Color.blue;
       Gizmos.DrawSphere(reachPositionDriftDefense, 0.2f);

       Gizmos.color = Color.green;
       Gizmos.DrawSphere(endBallReachPoint, 0.1f);
       Gizmos.color = Color.red;
       Gizmos.DrawSphere(endDriverTestingPoint + Vector3.up*.2f,0.1f);
       Gizmos.color = Color.cyan;
       Gizmos.DrawSphere(defenseTestPoint, 0.1f);
       Gizmos.color = Color.black;
       Gizmos.DrawSphere(driverStartAc, 0.1f);
       Gizmos.color = new Color(0.5f,0,0.5f);
       Gizmos.DrawSphere(lastKickDistanceOffsetPosition, 0.1f);
        /*
       /*
       Gizmos.color = Color.magenta;
       Gizmos.DrawSphere(kickDrivePosition1, 0.1f);
       Gizmos.color = Color.yellow;
       Gizmos.DrawSphere(reachPositionDrive, 0.1f);
       Gizmos.color = Color.green;
       Gizmos.DrawSphere(endBallReachPoint, 0.1f);

       Gizmos.color = Color.blue;
       Gizmos.DrawSphere(reachPositionDriftDefense, 0.2f);

       Gizmos.color = Color.red;
       Gizmos.DrawSphere(posDefenseLastKick, 0.2f);
       */
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(defenseReachPosition, 0.2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(driverStartAc, 0.2f);
        Gizmos.color = new Color(0.5f, 0, 0.5f);
        Gizmos.DrawSphere(reachPositionDrive, 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(lastKickDistanceOffsetPosition, 0.1f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            /*driverPublicPlayerData.movimentValues.maxSpeedForReachBall = 7;
            StartCoroutine(testAccelerationGetTimeToReachPosition());
            return;*/

            playerSkills.maxDrivingDistance = 8;

            driverPublicPlayerData.movimentValues.maxSpeedForReachBall = 7;
            driverTargetPosition = testTransform.position;
            Vector3 targetPosition = testTransform.position;
            ballTransform.position = driverPublicPlayerData.position + driverPublicPlayerData.bodyTransform.forward * 0.2f + Vector3.up * MatchComponents.ballRadio;
            kickDriveV0Drive = GetKickDriveV0(driverPublicPlayerData, driverPublicPlayerData.position, targetPosition);

            PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent);

            GetDrivingData(playerDataComponent, targetPosition, totalTimeDrive, out int kickCount, out float restTime, out Vector3 kickDrivePosition1, out float kickDriveTime1, out Vector3 reachPositionDrive);

            this.firstReachPositionDrive = reachPositionDrive;
            this.reachPositionDrive = reachPositionDrive;
            this.reachPositionDriftDefense = reachPositionDrive;

            getIntersectionPoint1(targetPosition, out PlayerDataComponent defense, out float defenseReachTime, out int defenseIndex);
            //StartCoroutine(testGetOptimalPointForReachTargetWhitAccelerationDriving2());
            
            divertDriving2();
            StartCoroutine(driving5Coroutine());
        }
        Debug.DrawRay(driverStartAc, dirr);
        //print(Speed);
        //print(BodyTargetXZDistance);
    }
    
    void divertDriving2()
    {
        int defenseIndex = 0;
        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent driverDataComponent);
        PlayerDataComponent defenseDataComponent = rivalsDataComponents[defenseIndex];
        Vector3 targetPosition = defenseReachPosition;
        Vector3 targetPosition2 = testTransform.position;


        Vector3 dir5 = this.defenseReachPosition - driverDataComponent.position;
        dir5.y = 0;
        dir5.Normalize();
        lastKickDistanceOffsetPosition = this.defenseReachPosition - dir5 * (lastKickDistanceOffset);
        tlastKickDriver = GetTimeToReachPointDOTS.accelerationGetTimeToReachPositionDriving(ref driverDataComponent, lastKickDistanceOffsetPosition, playerSkills, out int kickCount2, out float restTime2, out float kickDriveDistance, out float kickDriveTime1, out float startAcVel);
        posDriverLastKick = GetTimeToReachPointDOTS.accelerationGetPositionDriving(ref driverDataComponent, lastKickDistanceOffsetPosition, tlastKickDriver, playerSkills, out int kickCount4, out float restTime, out Vector3 kickDrivePosition12, out float kickDriveTime4, out driverStartAc, out Vector3 driverStartAcDir, out float startAcVel3, out float startAcT);

        driverDataComponent.position = posDriverLastKick;
        driverDataComponent.ForwardVelocity = driverStartAcDir * startAcVel3;
        driverDataComponent.normalizedForwardVelocity = driverStartAcDir;
        driverDataComponent.currentSpeed = startAcVel3;
        driverDataComponent.bodyY0Forward = driverStartAcDir;
        driverDataComponent.normalizedBodyY0Forward = driverStartAcDir;


        Vector3 dirV = targetPosition - posDriverLastKick;
        dirV.y = 0;
        dirV.Normalize();

        segmentedPath.Pos0 = posDriverLastKick;
        segmentedPath.Posf = targetPosition;
        segmentedPath.V0 = dirV * segmentedPath.V0Magnitude;

        PathDataDOTS currentPath2 = new PathDataDOTS();
        currentPath2.Pos0 = segmentedPath.Pos0;
        currentPath2.Posf = targetPosition;
        currentPath2.k = ballRigidbody.drag;
        currentPath2.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
        currentPath2.friction = MatchComponents.ballComponents.friction;
        currentPath2.g = 9.8f;
        currentPath2.mass = MatchComponents.ballComponents.mass;
        currentPath2.bounciness = MatchComponents.ballComponents.bounciness;
        currentPath2.ballRadio = MatchComponents.ballRadio;
        currentPath2.pathType = PathType.InGround;
        currentPath2.vfMagnitude = (currentPath2.g / currentPath2.k);

        currentPath2.normalizedV0 = segmentedPath.V0.normalized;
        currentPath2.V0 = segmentedPath.V0;
        currentPath2.v0Magnitude = segmentedPath.V0Magnitude;

        kickCountDrive = kickCount2;
        restTimeDrive = restTime2;
        timeTest = totalTimeDrive;
        this.kickDrivePosition1 = segmentedPath.Pos0;
        this.kickDriveTime = kickDriveTime1;



        posDefenseLastKick = GetTimeToReachPointDOTS.accelerationGetPosition(defenseDataComponent.position, defenseDataComponent.currentSpeed, defenseDataComponent.bodyY0Forward, defenseDataComponent.maxSpeedRotation, defenseDataComponent.minSpeedForRotate, defenseDataComponent.acceleration, defenseDataComponent.decceleration, defenseDataComponent.maxAngleForRun, defenseDataComponent.scope, defenseReachPosition, defenseDataComponent.maxSpeed, tlastKickDriver, out float endSpeedLastKickDefense, out Vector3 endDirectionLastKickDefense);

        //float driverV = GetTimeToReachPointDOTS.accelerationGetVelocity(ref driverDataComponent, restTime2, lastKickDistanceOffsetPosition);



        Vector3 dir6 = defenseReachPosition - driverDataComponent.position;
        dir6.y = 0;
        dir6.Normalize();
        float defenseV = GetTimeToReachPointDOTS.accelerationGetVelocity(ref defenseDataComponent, driverLastKickTime, posDriverLastKick);
        defenseDataComponent.position = posDefenseLastKick;
        defenseDataComponent.ForwardVelocity = dir6 * defenseV;
        defenseDataComponent.normalizedForwardVelocity = dir6;
        defenseDataComponent.currentSpeed = defenseV;
        defenseDataComponent.bodyY0Forward = endDirectionLastKickDefense;
        defenseDataComponent.normalizedBodyY0Forward = endDirectionLastKickDefense;
        //GetOptimalPointForReachTargetDOTS.getOptimalPointForReachTargetWhitAccelerationDriving3(segmentedPath, ref defenseDataComponent, ref driverDataComponent, 0, scope, 0.25f, playerSkills, out defenseReachTime, out Vector3 defenseReachPosition2, out int kickCountDefenseReach, out float restTimeDrive2, currentPath2, kickDriveDivertTime, kickDrivePosition1, totalTimeDrive, out Vector3 driverLastKickPos, out float driverLastKickTime2, out float differenceReach, out endBallReachPoint, out endDriverTestingPoint, out float endDefenseTime, out startAcPoint, out float startAcVel2);

        //defenseTestPoint = GetTimeToReachPointDOTS.accelerationGetPosition(defenseDataComponent.position, defenseDataComponent.currentSpeed, defenseDataComponent.bodyY0Forward, defenseDataComponent.maxSpeedRotation, defenseDataComponent.minSpeedForRotate, defenseDataComponent.acceleration, defenseDataComponent.decceleration, defenseDataComponent.maxAngleForRun, defenseDataComponent.scope, defenseReachPosition, defenseDataComponent.maxSpeed, endDefenseTime, out float endSpeedLastKickDefense2, out Vector3 endDirectionLastKickDefense2);
        
        /////////////////////////////////////////////////////////////////////
        
        float d = Vector3.Distance(segmentedPath.Pos0, posDriverLastKick) - lastKickDistanceOffset;
        float d5 = Vector3.Distance(segmentedPath.Pos0, targetPosition2) - scope;
        float tDriver = d / segmentedPath.V0Magnitude;
        Vector3 dir1 = defenseDataComponent.position - segmentedPath.Pos0;
        dir1.y = 0;
        dir1.Normalize();
        Vector3 dir2 = posDriverLastKick - segmentedPath.Pos0;
        float d3 = dir2.magnitude;
        dir2.y = 0;
        dir2.Normalize();
        segmentedPath.Posf = targetPosition2;
       Vector3 dir3 = segmentedPath.Posf - segmentedPath.Pos0;

        dir3.Normalize();
        dir3.y = 0;
        float sign = Mathf.Sign(Vector3.Cross(dir1, dir2).y);
        float angle = 0;

        segmentedPath.Pos0 = lastKickDistanceOffsetPosition;
        
        while (angle < maxAngle)
        {
            angle += angleTest;
            if (!getReachTimeDivertKick2(angle, sign, dir3, defenseDataComponent, driverDataComponent, d5, defenseIndex, currentPath2, startAcVel3, driverStartAcDir)) break;
            //if (!getReachTimeDivertKick2(angle, -sign, dir3, defenseDataComponent, driverDataComponent, d5, defenseIndex, currentPath2)) break;
            //if (angle == 50) break;
        }
    }
    Vector3 dirr;
    bool getReachTimeDivertKick2(float angle, float sign, Vector3 dir3, PlayerDataComponent defenseDataComponent, PlayerDataComponent driverDataComponent, float d5, int defenseIndex, PathDataDOTS currentPath,float startAcVel3,Vector3 driverStartAcDir)
    {

        //GetOptimalPointForReachTargetDOTS.getOptimalPointForReachTargetWhitAccelerationDriving(segmentedPath, ref defenseDataComponent,ref playerDataComponent, 0, scope, 0.1f, playerSkills, ref times, out Vector3 reachPosition2, out int kickCount, out float restTime, currentPath2);

        //reachPositionTest = reachPosition2;

        //GetDrivingData(driverDataComponent, nextPos, totalTimeDrive, out int kickCount, out float restTime, out Vector3 kickDrivePosition1, out float kickDriveTime1_2, out Vector3 reachPositionDrive);
        //getIntersectionPoint2(defenseDataComponent, defenseIndex, driverDataComponent, nextPos, out float defenseReachTime, angle);


        //posDriverLastKick = GetTimeToReachPointDOTS.accelerationGetPositionDriving(ref driverDataComponent, lastKickDistanceOffsetPosition, tlastKickDriver, playerSkills, out int kickCount4, out float restTime, out Vector3 kickDrivePosition12, out float kickDriveTime4, out driverStartAc, out Vector3 driverStartAcDir, out float startAcVel3, out float startAcT2);
        Vector3 dir2 = dir3;
        dir2.Normalize();
        dir2.y = 0;
        
        segmentedPath.Pos0 = lastKickDistanceOffsetPosition;
        Vector3 dir = segmentedPath.Posf - segmentedPath.Pos0;
        dir.Normalize();
        dir.y = 0;
        Vector3 dir4 = Quaternion.AngleAxis(angle * sign, Vector3.up) * dir3;
        Vector3 nextPos = lastKickDistanceOffsetPosition + dir4 * d5;
        dirr = dir4;
        segmentedPath.Posf = nextPos;
        segmentedPath.V0 = dir4 * segmentedPath.V0Magnitude;
        currentPath.Posf = nextPos;
        currentPath.normalizedV0 = segmentedPath.V0.normalized;
        currentPath.V0 = segmentedPath.V0;
        driverDataComponent.position = lastKickDistanceOffsetPosition;
        driverDataComponent.ForwardVelocity = dir4 * startAcVel3;
        driverDataComponent.normalizedForwardVelocity = dir4;
        driverDataComponent.currentSpeed = startAcVel3;
        driverDataComponent.bodyY0Forward = driverStartAcDir;
        driverDataComponent.normalizedBodyY0Forward = driverStartAcDir;
        if (angle == maxAngle)
        {
            int a = 0;
        }
        GetOptimalPointForReachTargetDOTS.getOptimalPointForReachTargetWhitAccelerationDriving3(segmentedPath, ref defenseDataComponent, ref driverDataComponent, 0, scope, 0.25f, playerSkills, out float defenseReachTime, out Vector3 defenseReachPosition2, out int kickCountDefenseReach, out float restTimeDrive2, currentPath, kickDriveDivertTime, kickDrivePosition1, totalTimeDrive, out Vector3 driverLastKickPos, out float driverLastKickTime2, out float differenceReach, out endBallReachPoint, out endDriverTestingPoint, out float endDefenseTime, out startAcPoint, out float startAcVel2, out float startAcT, playerSkills.maxDrivingDistance);
        defenseTestPoint = GetTimeToReachPointDOTS.accelerationGetPosition(defenseDataComponent.position, defenseDataComponent.currentSpeed, defenseDataComponent.bodyY0Forward, defenseDataComponent.maxSpeedRotation, defenseDataComponent.minSpeedForRotate, defenseDataComponent.acceleration, defenseDataComponent.decceleration, defenseDataComponent.maxAngleForRun, defenseDataComponent.scope, endDriverTestingPoint, defenseDataComponent.maxSpeed, endDefenseTime, out float endSpeedLastKickDefense2, out Vector3 endDirectionLastKickDefense2);


        if (defenseReachTime > 0)
        {
            this.kickCountDefenseReach = kickCountDefenseReach;
            restTimeDivertDrive = restTimeDrive2;
            timeTest = totalTimeDrive;
            this.kickDrivePosition1 = driverLastKickPos;
            this.kickDriveDivertTime = driverLastKickTime2;
            this.defenseReachTime = endDefenseTime;

            this.reachPositionDriftDefense = defenseReachPosition2;
            reachPositionDrive = endDriverTestingPoint;
            this.endStartAcT = startAcT;
            print("defense reach a=" + angle);
            return true;
        }
        else
        {
            reachPositionDrive = nextPos;
            print("no reach a=" + angle);
            return false;
        }
    }
    public IEnumerator driving5Coroutine()
    {

        /////////////////////////////////////////////////////////////////////

        int defenseIndex = 0;

        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent);
        float time = timeTest;
        //GetTimeToReachPointDOTS.accelerationGetPositionDriving(ref playerDataComponent, targetPosition, time, playerSkills,out int kickCount,out float restTime);
        //restTimeTest = restTime;
        //kickCountTest = kickCount;

        Vector3 reachTargetPosition = posDriverLastKick;
        Vector3 dir2 = reachTargetPosition - driverPublicPlayerData.position;

        dir2.y = 0;
        dir2.Normalize();
        float tBallAll = 0;

        Vector3 defenseDir = defenseReachPosition - rivals[defenseIndex].position;

        defenseDir.y = 0;
        defenseDir.Normalize();
        rivals[defenseIndex].playerComponents.TargetPosition = defenseReachPosition;
        rivals[defenseIndex].playerComponents.ForwardDesiredDirection = defenseDir;
        rivals[defenseIndex].playerComponents.ForwardDesiredSpeed = 10.5f;
        rivals[defenseIndex].playerComponents.DesiredLookDirection = defenseDir;

        for (int i = 0; i < kickCountDrive; i++)
        {
            driverPublicPlayerData.playerComponents.TargetPosition = driverPublicPlayerData.position + dir2 * playerSkills.maxDrivingDistance;
            driverPublicPlayerData.playerComponents.ForwardDesiredDirection = dir2;
            driverPublicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;
            driverPublicPlayerData.playerComponents.DesiredLookDirection = dir2;


            GetV0DOTSResult result = new GetV0DOTSResult();
            PathDataDOTS currentPath = new PathDataDOTS();
            currentPath.Pos0 = driverPublicPlayerData.position;
            currentPath.Posf = driverPublicPlayerData.playerComponents.TargetPosition;
            currentPath.k = ballRigidbody.drag;
            currentPath.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
            currentPath.friction = MatchComponents.ballComponents.friction;
            currentPath.g = 9.8f;
            currentPath.mass = MatchComponents.ballComponents.mass;
            currentPath.bounciness = MatchComponents.ballComponents.bounciness;
            currentPath.ballRadio = MatchComponents.ballRadio;
            currentPath.pathType = PathType.InGround;
            currentPath.vfMagnitude = (currentPath.g / currentPath.k);
            GetStraightV0Params getV0Params = GetV0Params();


            PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent2);

            float tBall2 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent2, driverPublicPlayerData.playerComponents.TargetPosition);

            StraightXZDragAndFrictionPathDOTS2.getV02(ref result, ref currentPath, 33, ref getV0Params, tBall2);
            ballRigidbody.angularVelocity = Vector3.zero;
            ballRigidbody.velocity = dir2 * result.v0Magnitude;
            //yield return new WaitUntil(() => BodyTargetXZDistance < 0.1f);
            tBallAll += tBall2;
            yield return new WaitForSeconds(tBall2);
            //print(Speed);

        }
        reachTargetPosition = startAcPoint;

        Vector3 dir = reachTargetPosition - driverPublicPlayerData.position;
        dir.y = 0;
        dir.Normalize();

        driverPublicPlayerData.playerComponents.TargetPosition = reachTargetPosition;
        driverPublicPlayerData.playerComponents.ForwardDesiredDirection = dir;
        driverPublicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;
        driverPublicPlayerData.playerComponents.DesiredLookDirection = dir;

        GetV0DOTSResult result2 = new GetV0DOTSResult();
        PathDataDOTS currentPath2 = new PathDataDOTS();
        currentPath2.Pos0 = ballPosition;
        currentPath2.Posf = startAcPoint;
        currentPath2.k = ballRigidbody.drag;
        currentPath2.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
        currentPath2.friction = MatchComponents.ballComponents.friction;
        currentPath2.g = 9.8f;
        currentPath2.mass = MatchComponents.ballComponents.mass;
        currentPath2.bounciness = MatchComponents.ballComponents.bounciness;
        currentPath2.ballRadio = MatchComponents.ballRadio;
        currentPath2.pathType = PathType.InGround;
        currentPath2.vfMagnitude = (currentPath2.g / currentPath2.k);
        GetStraightV0Params getV0Params2 = GetV0Params();


        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent3);
        //playerDataComponent3.position = firstReachPositionDrive;
        float tBall3 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent3, startAcPoint);

        StraightXZDragAndFrictionPathDOTS2.getV02(ref result2, ref currentPath2, 33, ref getV0Params2, tBall3 + endStartAcT);
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = dir * result2.v0Magnitude;

        yield return new WaitForSeconds(restTimeDrive);


        reachTargetPosition = reachPositionDriftDefense;

        defenseDir = reachTargetPosition - rivals[defenseIndex].position;

        defenseDir.y = 0;
        defenseDir.Normalize();
        rivals[defenseIndex].playerComponents.TargetPosition = reachTargetPosition;
        rivals[defenseIndex].playerComponents.ForwardDesiredDirection = defenseDir;
        rivals[defenseIndex].playerComponents.ForwardDesiredSpeed = 10.5f;
        rivals[defenseIndex].playerComponents.DesiredLookDirection = defenseDir;


        Vector3 dir3 = driverTargetPosition - driverPublicPlayerData.position;
        dir3.y = 0;
        dir3.Normalize();

        publicPlayerData.playerComponents.TargetPosition = driverTargetPosition;
        //driverPublicPlayerData.playerComponents.TargetPosition = driverPublicPlayerData.position + dir *100;
        publicPlayerData.playerComponents.ForwardDesiredDirection = dir3;

        publicPlayerData.playerComponents.DesiredLookDirection = dir3;
        publicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;

        yield return new WaitForSeconds(endStartAcT);
        reachTargetPosition = reachPositionDrive;
        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out playerDataComponent3);
        dir3 = reachTargetPosition - ballPosition;
        dir3.y = 0;
        dir3.Normalize();
        result2 = new GetV0DOTSResult();
        currentPath2 = new PathDataDOTS();
        currentPath2.Pos0 = ballPosition;
        currentPath2.Posf = ballPosition + dir3 * playerSkills.maxDrivingDistance;
        currentPath2.k = ballRigidbody.drag;
        currentPath2.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
        currentPath2.friction = MatchComponents.ballComponents.friction;
        currentPath2.g = 9.8f;
        currentPath2.mass = MatchComponents.ballComponents.mass;
        currentPath2.bounciness = MatchComponents.ballComponents.bounciness;
        currentPath2.ballRadio = MatchComponents.ballRadio;
        currentPath2.pathType = PathType.InGround;
        currentPath2.vfMagnitude = (currentPath2.g / currentPath2.k);
        getV0Params2 = GetV0Params();
        //playerDataComponent3.position = firstReachPositionDrive;
        float tBall4 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent3, currentPath2.Posf);
        StraightXZDragAndFrictionPathDOTS2.getV02(ref result2, ref currentPath2, 33, ref getV0Params2, tBall4);

        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = dir3 * segmentedPath.V0Magnitude;

        dir2 = reachTargetPosition - driverPublicPlayerData.position;

        dir2.y = 0;
        dir2.Normalize();

        Invoke(nameof(pause), defenseReachTime);
        for (int i = 0; i < kickCountDefenseReach2 + 3; i++)
        {
            driverPublicPlayerData.playerComponents.TargetPosition = driverPublicPlayerData.position + dir2 * playerSkills.maxDrivingDistance;
            driverPublicPlayerData.playerComponents.ForwardDesiredDirection = dir2;
            driverPublicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;
            driverPublicPlayerData.playerComponents.DesiredLookDirection = dir2;


            GetV0DOTSResult result = new GetV0DOTSResult();
            PathDataDOTS currentPath = new PathDataDOTS();
            currentPath.Pos0 = driverPublicPlayerData.position;
            currentPath.Posf = driverPublicPlayerData.playerComponents.TargetPosition;
            currentPath.k = ballRigidbody.drag;
            currentPath.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
            currentPath.friction = MatchComponents.ballComponents.friction;
            currentPath.g = 9.8f;
            currentPath.mass = MatchComponents.ballComponents.mass;
            currentPath.bounciness = MatchComponents.ballComponents.bounciness;
            currentPath.ballRadio = MatchComponents.ballRadio;
            currentPath.pathType = PathType.InGround;
            currentPath.vfMagnitude = (currentPath.g / currentPath.k);
            GetStraightV0Params getV0Params = GetV0Params();


            PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent2);

            float tBall2 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent2, driverPublicPlayerData.playerComponents.TargetPosition);

            StraightXZDragAndFrictionPathDOTS2.getV02(ref result, ref currentPath, 33, ref getV0Params, tBall2);
            ballRigidbody.angularVelocity = Vector3.zero;
            ballRigidbody.velocity = dir2 * result.v0Magnitude;
            //yield return new WaitUntil(() => BodyTargetXZDistance < 0.1f);
            tBallAll += tBall2;
            yield return new WaitForSeconds(tBall2);
            //print(Speed);

        }
        //yield return new WaitForSeconds(defenseReachTime - endStartAcT);
        //yield return new WaitForSeconds(defenseReachTime);
        //print(restTime +" "+ tBall3+" "+Speed);
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = Vector3.zero;
        Time.timeScale = 0;

    }
    void pause()
    {
        Time.timeScale = 0;
    }

    void GetDrivingData(PlayerDataComponent playerDataComponent, Vector3 targetPosition, float time, out int kickCount, out float restTime, out Vector3 kickDrivePosition1, out float kickDriveTime1, out Vector3 reachPositionDrive)
    {
        reachPositionDrive = GetTimeToReachPointDOTS.accelerationGetPositionDriving(ref playerDataComponent, targetPosition, time, playerSkills, out kickCount, out restTime, out kickDrivePosition1, out kickDriveTime1, out Vector3 startAcPoint, out Vector3 startAcDir, out float startAcVel, out float startAcT);

        //float tReach = GetTimeToReachPointDOTS.accelerationGetTimeToReachPositionDriving(ref playerDataComponent, targetPosition, playerSkills, out int kickCount2, out float restTime2, out float kickDriveDistance1, out float reachLastKickTime);
        /*float tReach = GetTimeToReachPointDOTS.accelerationGetTimeToReachPositionDriving(ref playerDataComponent, targetPosition, playerSkills, out  kickCount, out  restTime, out float kickDriveDistance, out kickDriveTime1);
         kickDrivePosition1 = targetPosition;
         print("rReach " + tReach);
         Invoke(nameof(printTest), tReach);*/
    }

    float getIntersectionPoint1(Vector3 targetPosition, out PlayerDataComponent result, out float defenseReachTime, out int defenseIndex)
    {
        result = new PlayerDataComponent();
        segmentedPath.Pos0 = MyFunctions.setY0ToVector3(ballPosition);
        segmentedPath.Posf = targetPosition;
        Vector3 dir = segmentedPath.Posf - segmentedPath.Pos0;
        segmentedPath.V0Magnitude = kickDriveV0Drive.magnitude;
        segmentedPath.V0 = dir.normalized * kickDriveV0Drive.magnitude;
        segmentedPath.Vf = dir.normalized * segmentedPath.V0Magnitude;
        segmentedPath.t0 = 0;
        segmentedPath.tf = kickDriveTime;
        defenseReachTime = -1;
        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent);
        int j = 0;
        foreach (var rivalDataComponent in rivalsDataComponents)
        {

            PlayerDataComponent playerDataComponent2 = rivalDataComponent;

            PathDataDOTS currentPath2 = new PathDataDOTS();
            currentPath2.Pos0 = driverPublicPlayerData.position;
            currentPath2.Posf = targetPosition;
            currentPath2.k = ballRigidbody.drag;
            currentPath2.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
            currentPath2.friction = MatchComponents.ballComponents.friction;
            currentPath2.g = 9.8f;
            currentPath2.mass = MatchComponents.ballComponents.mass;
            currentPath2.bounciness = MatchComponents.ballComponents.bounciness;
            currentPath2.ballRadio = MatchComponents.ballRadio;
            currentPath2.pathType = PathType.InGround;
            currentPath2.vfMagnitude = (currentPath2.g / currentPath2.k);

            currentPath2.normalizedV0 = segmentedPath.V0.normalized;
            currentPath2.V0 = segmentedPath.V0;
            currentPath2.v0Magnitude = segmentedPath.V0Magnitude;
            GetOptimalPointForReachTargetDOTS.getOptimalPointForReachTargetWhitAccelerationDriving3(segmentedPath, ref playerDataComponent2, ref playerDataComponent, 0, scope, 0.1f, playerSkills, out defenseReachTime, out Vector3 defenseReachPosition, out int kickCountDrive, out float restTimeDrive, currentPath2, kickDriveDivertTime, kickDrivePosition1, totalTimeDrive, out Vector3 driverLastKickPos, out float driverLastKickTime, out float differenceReach, out Vector3 endBallReachPoint, out Vector3 endDriverTestingPoint, out float endDefenseTime, out Vector3 startAcPoint, out float startAcVel, out float startAcT, playerSkills.maxDrivingDistance);

            //GetOptimalPointForReachTargetDOTS.getOptimalPointForReachTargetWhitAccelerationDriving(segmentedPath, ref playerDataComponent, 0, scope, 0.1f, ref times, out Vector3 reachPosition);
            if (defenseReachTime > 0)
            {
                this.defenseReachTime = defenseReachTime;
                this.defenseReachPosition = defenseReachPosition;
                this.driverLastKickPos = driverLastKickPos;
                this.driverLastKickTime = driverLastKickTime;
                //restTimeTest = restTime;
                //kickCountTest = kickCount;

                this.kickCountDefenseReach = kickCountDrive;
                //timeTest = t;
                defenseIndex = j;
                result = playerDataComponent2;
                break;
            }
            j++;
        }
        test = true;
        defenseIndex = 0;
        return 0;
    }
    public IEnumerator testGetOptimalPointForReachTargetWhitAccelerationDriving2()
    {
        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent driverDataComponent);
        PlayerDataComponent defenseDataComponent = rivalsDataComponents[0];
        Vector3 targetPosition = defenseReachPosition;
        Vector3 targetPosition2 = testTransform2.position;


        Vector3 dir5 = this.defenseReachPosition - driverDataComponent.position;
        dir5.y = 0;
        dir5.Normalize();
        lastKickDistanceOffsetPosition = this.defenseReachPosition - dir5 * (lastKickDistanceOffset);
        float tlastKickDriver = GetTimeToReachPointDOTS.accelerationGetTimeToReachPositionDriving(ref driverDataComponent, lastKickDistanceOffsetPosition, playerSkills, out int kickCount2, out float restTime2, out float kickDriveDistance, out float kickDriveTime1, out float startAcVel);
        posDriverLastKick = GetTimeToReachPointDOTS.accelerationGetPositionDriving(ref driverDataComponent, lastKickDistanceOffsetPosition, tlastKickDriver, playerSkills, out int kickCount4, out float restTime, out Vector3 kickDrivePosition12, out float kickDriveTime4, out driverStartAc, out Vector3 driverStartAcDir, out float startAcVel3, out float startAcT);

        driverDataComponent.position = posDriverLastKick;
        driverDataComponent.ForwardVelocity = driverStartAcDir * startAcVel3;
        driverDataComponent.normalizedForwardVelocity = driverStartAcDir;
        driverDataComponent.currentSpeed = startAcVel3;
        driverDataComponent.bodyY0Forward = driverStartAcDir;
        driverDataComponent.normalizedBodyY0Forward = driverStartAcDir;


        Vector3 dirV = targetPosition2 - lastKickDistanceOffsetPosition;
        dirV.y = 0;
        dirV.Normalize();

        segmentedPath.Pos0 = lastKickDistanceOffsetPosition;
        segmentedPath.Posf = targetPosition2;
        segmentedPath.V0 = dirV * segmentedPath.V0Magnitude;

        PathDataDOTS currentPath2 = new PathDataDOTS();
        currentPath2.Pos0 = segmentedPath.Pos0;
        currentPath2.Posf = targetPosition2;
        currentPath2.k = ballRigidbody.drag;
        currentPath2.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
        currentPath2.friction = MatchComponents.ballComponents.friction;
        currentPath2.g = 9.8f;
        currentPath2.mass = MatchComponents.ballComponents.mass;
        currentPath2.bounciness = MatchComponents.ballComponents.bounciness;
        currentPath2.ballRadio = MatchComponents.ballRadio;
        currentPath2.pathType = PathType.InGround;
        currentPath2.vfMagnitude = (currentPath2.g / currentPath2.k);

        currentPath2.normalizedV0 = segmentedPath.V0.normalized;
        currentPath2.V0 = segmentedPath.V0;
        currentPath2.v0Magnitude = segmentedPath.V0Magnitude;

        kickCountDrive = kickCount2;
        restTimeDrive = restTime2;
        timeTest = totalTimeDrive;
        this.kickDrivePosition1 = segmentedPath.Pos0;
        this.kickDriveTime = kickDriveTime1;



        posDefenseLastKick = GetTimeToReachPointDOTS.accelerationGetPosition(defenseDataComponent.position, defenseDataComponent.currentSpeed, defenseDataComponent.bodyY0Forward, defenseDataComponent.maxSpeedRotation, defenseDataComponent.minSpeedForRotate, defenseDataComponent.acceleration, defenseDataComponent.decceleration, defenseDataComponent.maxAngleForRun, defenseDataComponent.scope, defenseReachPosition, defenseDataComponent.maxSpeed, tlastKickDriver, out float endSpeedLastKickDefense, out Vector3 endDirectionLastKickDefense);
        //float driverV = GetTimeToReachPointDOTS.accelerationGetVelocity(ref driverDataComponent, restTime2, lastKickDistanceOffsetPosition);



        Vector3 dir6 = defenseReachPosition - driverDataComponent.position;
        dir6.y = 0;
        dir6.Normalize();
        float defenseV = GetTimeToReachPointDOTS.accelerationGetVelocity(ref defenseDataComponent, driverLastKickTime, defenseReachPosition);
        defenseDataComponent.position = posDefenseLastKick;
        defenseDataComponent.ForwardVelocity = dir6 * defenseV;
        defenseDataComponent.normalizedForwardVelocity = dir6;
        defenseDataComponent.currentSpeed = defenseV;
        defenseDataComponent.bodyY0Forward = endDirectionLastKickDefense;
        defenseDataComponent.normalizedBodyY0Forward = endDirectionLastKickDefense;
        GetOptimalPointForReachTargetDOTS.getOptimalPointForReachTargetWhitAccelerationDriving3(segmentedPath, ref defenseDataComponent, ref driverDataComponent, 0, scope, 0.25f, playerSkills, out defenseReachTime, out Vector3 defenseReachPosition2, out int kickCountDefenseReach, out float restTimeDrive2, currentPath2, kickDriveDivertTime, kickDrivePosition1, totalTimeDrive, out Vector3 driverLastKickPos, out float driverLastKickTime2, out float differenceReach, out endBallReachPoint, out endDriverTestingPoint, out float endDefenseTime, out startAcPoint, out float startAcVel2, out float startAcT2, playerSkills.maxDrivingDistance);

        defenseTestPoint = GetTimeToReachPointDOTS.accelerationGetPosition(defenseDataComponent.position, defenseDataComponent.currentSpeed, defenseDataComponent.bodyY0Forward, defenseDataComponent.maxSpeedRotation, defenseDataComponent.minSpeedForRotate, defenseDataComponent.acceleration, defenseDataComponent.decceleration, defenseDataComponent.maxAngleForRun, defenseDataComponent.scope, defenseReachPosition, defenseDataComponent.maxSpeed, endDefenseTime, out float endSpeedLastKickDefense2, out Vector3 endDirectionLastKickDefense2);
        this.reachPositionDriftDefense = defenseReachPosition2;
        reachPositionDrive = defenseTestPoint;
        /////////////////////////////////////////////////////////////////////

        int defenseIndex = 0;
        this.endStartAcT = startAcT2;
        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent);
        float time = timeTest;
        //GetTimeToReachPointDOTS.accelerationGetPositionDriving(ref playerDataComponent, targetPosition, time, playerSkills,out int kickCount,out float restTime);
        //restTimeTest = restTime;
        //kickCountTest = kickCount;

        Vector3 reachTargetPosition = posDriverLastKick;
        Vector3 dir2 = reachTargetPosition - driverPublicPlayerData.position;

        dir2.y = 0;
        dir2.Normalize();
        float tBallAll = 0;

        Vector3 defenseDir = defenseReachPosition - rivals[defenseIndex].position;

        defenseDir.y = 0;
        defenseDir.Normalize();
        rivals[defenseIndex].playerComponents.TargetPosition = defenseReachPosition;
        rivals[defenseIndex].playerComponents.ForwardDesiredDirection = defenseDir;
        rivals[defenseIndex].playerComponents.ForwardDesiredSpeed = 10.5f;
        rivals[defenseIndex].playerComponents.DesiredLookDirection = defenseDir;

        for (int i = 0; i < kickCountDrive; i++)
        {
            driverPublicPlayerData.playerComponents.TargetPosition = driverPublicPlayerData.position + dir2 * playerSkills.maxDrivingDistance;
            driverPublicPlayerData.playerComponents.ForwardDesiredDirection = dir2;
            driverPublicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;
            driverPublicPlayerData.playerComponents.DesiredLookDirection = dir2;


            GetV0DOTSResult result = new GetV0DOTSResult();
            PathDataDOTS currentPath = new PathDataDOTS();
            currentPath.Pos0 = driverPublicPlayerData.position;
            currentPath.Posf = driverPublicPlayerData.playerComponents.TargetPosition;
            currentPath.k = ballRigidbody.drag;
            currentPath.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
            currentPath.friction = MatchComponents.ballComponents.friction;
            currentPath.g = 9.8f;
            currentPath.mass = MatchComponents.ballComponents.mass;
            currentPath.bounciness = MatchComponents.ballComponents.bounciness;
            currentPath.ballRadio = MatchComponents.ballRadio;
            currentPath.pathType = PathType.InGround;
            currentPath.vfMagnitude = (currentPath.g / currentPath.k);
            GetStraightV0Params getV0Params = GetV0Params();


            PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent2);

            float tBall2 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent2, driverPublicPlayerData.playerComponents.TargetPosition);

            StraightXZDragAndFrictionPathDOTS2.getV02(ref result, ref currentPath, 33, ref getV0Params, tBall2);
            ballRigidbody.angularVelocity = Vector3.zero;
            ballRigidbody.velocity = dir2 * result.v0Magnitude;
            //yield return new WaitUntil(() => BodyTargetXZDistance < 0.1f);
            tBallAll += tBall2;
            yield return new WaitForSeconds(tBall2);
            //print(Speed);

        }
        reachTargetPosition = startAcPoint;

        Vector3 dir = reachTargetPosition - driverPublicPlayerData.position;
        dir.y = 0;
        dir.Normalize();

        driverPublicPlayerData.playerComponents.TargetPosition = reachTargetPosition;
        driverPublicPlayerData.playerComponents.ForwardDesiredDirection = dir;
        driverPublicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;
        driverPublicPlayerData.playerComponents.DesiredLookDirection = dir;

        GetV0DOTSResult result2 = new GetV0DOTSResult();
        currentPath2 = new PathDataDOTS();
        currentPath2.Pos0 = ballPosition;
        currentPath2.Posf = startAcPoint;
        currentPath2.k = ballRigidbody.drag;
        currentPath2.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
        currentPath2.friction = MatchComponents.ballComponents.friction;
        currentPath2.g = 9.8f;
        currentPath2.mass = MatchComponents.ballComponents.mass;
        currentPath2.bounciness = MatchComponents.ballComponents.bounciness;
        currentPath2.ballRadio = MatchComponents.ballRadio;
        currentPath2.pathType = PathType.InGround;
        currentPath2.vfMagnitude = (currentPath2.g / currentPath2.k);
        GetStraightV0Params getV0Params2 = GetV0Params();


        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent3);
        //playerDataComponent3.position = firstReachPositionDrive;
        float tBall3 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent3, startAcPoint);

        StraightXZDragAndFrictionPathDOTS2.getV02(ref result2, ref currentPath2, 33, ref getV0Params2, tBall3 + endStartAcT);
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = dir * result2.v0Magnitude;

        yield return new WaitForSeconds(restTimeDrive);


        reachTargetPosition = reachPositionDriftDefense;

        defenseDir = reachTargetPosition - rivals[defenseIndex].position;

        defenseDir.y = 0;
        defenseDir.Normalize();
        rivals[defenseIndex].playerComponents.TargetPosition = reachTargetPosition;
        rivals[defenseIndex].playerComponents.ForwardDesiredDirection = defenseDir;
        rivals[defenseIndex].playerComponents.ForwardDesiredSpeed = 10.5f;
        rivals[defenseIndex].playerComponents.DesiredLookDirection = defenseDir;


        Vector3 dir3 = reachTargetPosition - driverPublicPlayerData.position;
        dir3.y = 0;
        dir3.Normalize();

        publicPlayerData.playerComponents.TargetPosition = reachTargetPosition;
        //driverPublicPlayerData.playerComponents.TargetPosition = driverPublicPlayerData.position + dir *100;
        publicPlayerData.playerComponents.ForwardDesiredDirection = dir3;

        publicPlayerData.playerComponents.DesiredLookDirection = dir3;
        publicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;

        yield return new WaitForSeconds(endStartAcT);


        result2 = new GetV0DOTSResult();
        currentPath2 = new PathDataDOTS();
        currentPath2.Pos0 = ballPosition;
        currentPath2.Posf = reachTargetPosition;
        currentPath2.k = ballRigidbody.drag;
        currentPath2.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
        currentPath2.friction = MatchComponents.ballComponents.friction;
        currentPath2.g = 9.8f;
        currentPath2.mass = MatchComponents.ballComponents.mass;
        currentPath2.bounciness = MatchComponents.ballComponents.bounciness;
        currentPath2.ballRadio = MatchComponents.ballRadio;
        currentPath2.pathType = PathType.InGround;
        currentPath2.vfMagnitude = (currentPath2.g / currentPath2.k);
        getV0Params2 = GetV0Params();
        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out playerDataComponent3);
        //playerDataComponent3.position = firstReachPositionDrive;
        float tBall4 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent3, reachTargetPosition);
        StraightXZDragAndFrictionPathDOTS2.getV02(ref result2, ref currentPath2, 33, ref getV0Params2, tBall4);
        dir3 = reachTargetPosition - ballPosition;
        dir3.y = 0;
        dir3.Normalize();
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = dir3 * segmentedPath.V0Magnitude;

        yield return new WaitForSeconds(defenseReachTime - endStartAcT);

        //print(restTime +" "+ tBall3+" "+Speed);
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = Vector3.zero;
        //Time.timeScale = 0;

    }
    

    bool getReachTimeDivertKick(float angle, float sign, Vector3 dir3, PlayerDataComponent defenseDataComponent, PlayerDataComponent driverDataComponent, float d5, int defenseIndex)
    {
        Vector3 dir4 = Quaternion.AngleAxis(angle * sign, Vector3.up) * dir3;
        Vector3 nextPos = driverLastKickPos + dir4 * d5;
        segmentedPath.Posf = nextPos;

        PathDataDOTS currentPath2 = new PathDataDOTS();
        currentPath2.Pos0 = segmentedPath.Pos0;
        currentPath2.Posf = nextPos;
        currentPath2.k = ballRigidbody.drag;
        currentPath2.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
        currentPath2.friction = MatchComponents.ballComponents.friction;
        currentPath2.g = 9.8f;
        currentPath2.mass = MatchComponents.ballComponents.mass;
        currentPath2.bounciness = MatchComponents.ballComponents.bounciness;
        currentPath2.ballRadio = MatchComponents.ballRadio;
        currentPath2.pathType = PathType.InGround;
        currentPath2.vfMagnitude = (currentPath2.g / currentPath2.k);

        //GetOptimalPointForReachTargetDOTS.getOptimalPointForReachTargetWhitAccelerationDriving(segmentedPath, ref defenseDataComponent,ref playerDataComponent, 0, scope, 0.1f, playerSkills, ref times, out Vector3 reachPosition2, out int kickCount, out float restTime, currentPath2);

        //reachPositionTest = reachPosition2;

        GetDrivingData(driverDataComponent, nextPos, totalTimeDrive, out int kickCount, out float restTime, out Vector3 kickDrivePosition1, out float kickDriveTime1_2, out Vector3 reachPositionDrive);
        getIntersectionPoint2(defenseDataComponent, defenseIndex, driverDataComponent, nextPos, out float defenseReachTime, angle);


        if (defenseReachTime > 0)
        {
            kickCountDefenseReach = kickCount;
            restTimeDivertDrive = restTime;
            timeTest = totalTimeDrive;
            this.kickDrivePosition1 = kickDrivePosition1;
            this.kickDriveDivertTime = kickDriveTime1_2;
            this.reachPositionDrive = reachPositionDrive;
            this.defenseReachTime = defenseReachTime;
            print("defense reach a=" + angle);
            return true;
        }
        else
        {
            print("no reach a=" + angle);
            return false;
        }
    }
    float getIntersectionPoint2(PlayerDataComponent rivalDataComponent, int rivalIndex, PlayerDataComponent driverDataComponent, Vector3 targetPosition, out float defenseReachTime, float angle)
    {
        //segmentedPath.Pos0 = MyFunctions.setY0ToVector3(ballPosition);
        segmentedPath.Posf = targetPosition;
        Vector3 dir = segmentedPath.Posf - segmentedPath.Pos0;
        segmentedPath.V0Magnitude = kickDriveV0Drive.magnitude;
        segmentedPath.V0 = dir.normalized * kickDriveV0Drive.magnitude;
        segmentedPath.Vf = dir.normalized * segmentedPath.V0Magnitude;
        segmentedPath.t0 = 0;
        segmentedPath.tf = kickDriveTime;
        defenseReachTime = -1;

        PlayerDataComponent playerDataComponent2 = rivalDataComponent;

        PathDataDOTS currentPath2 = new PathDataDOTS();
        currentPath2.Pos0 = driverDataComponent.position;
        currentPath2.Posf = targetPosition;
        currentPath2.k = ballRigidbody.drag;
        currentPath2.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
        currentPath2.friction = MatchComponents.ballComponents.friction;
        currentPath2.g = 9.8f;
        currentPath2.mass = MatchComponents.ballComponents.mass;
        currentPath2.bounciness = MatchComponents.ballComponents.bounciness;
        currentPath2.ballRadio = MatchComponents.ballRadio;
        currentPath2.pathType = PathType.InGround;
        currentPath2.vfMagnitude = (currentPath2.g / currentPath2.k);

        currentPath2.normalizedV0 = segmentedPath.V0.normalized;
        currentPath2.V0 = segmentedPath.V0;
        currentPath2.v0Magnitude = segmentedPath.V0Magnitude;
        GetOptimalPointForReachTargetDOTS.getOptimalPointForReachTargetWhitAccelerationDriving3(segmentedPath, ref playerDataComponent2, ref driverDataComponent, 0, scope, 0.25f, playerSkills, out defenseReachTime, out Vector3 defenseReachPosition, out int kickCountDefenseReach, out float restTimeDrive, currentPath2, kickDriveDivertTime, kickDrivePosition1, totalTimeDrive, out Vector3 driverLastKickPos, out float driverLastKickTime, out float differenceReach, out endBallReachPoint, out endDriverTestingPoint, out float endDefenseTime, out startAcPoint, out float startAcVel2, out float startAcT, playerSkills.maxDrivingDistance);

        //GetOptimalPointForReachTargetDOTS.getOptimalPointForReachTargetWhitAccelerationDriving(segmentedPath, ref playerDataComponent, 0, scope, 0.1f, ref times, out Vector3 reachPosition);

        if (defenseReachTime > 0)
        {
            //print("driver llega antes angle = " + angle);
            this.defenseReachTime2 = defenseReachTime;
            //restTimeTest = restTime;
            //kickCountTest = kickCount;
            this.driverLastKickTime2 = driverLastKickTime;
            this.kickCountDefenseReach2 = kickCountDefenseReach;
            this.reachPositionDriftDefense = defenseReachPosition;
            //print("defenseReachTime=" + defenseReachTime + " angle=" + angle);
        }

        test = true;
        return 0;
    }

    void divertDriving(PlayerDataComponent defenseDataComponent,int defenseIndex, Vector3 reachPosition, float t,Vector3 targetPosition)
    {

        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent driverDataComponent);
        Vector3 dir5 = defenseReachPosition - driverDataComponent.position;
        dir5.y = 0;
        dir5.Normalize();

        lastKickDistanceOffsetPosition = defenseReachPosition - dir5 * (lastKickDistanceOffset-scope);
        float tlastKickDriver = GetTimeToReachPointDOTS.accelerationGetTimeToReachPositionDriving(ref driverDataComponent, lastKickDistanceOffsetPosition, playerSkills, out int kickCount2, out float restTime2, out float kickDriveDistance, out float kickDriveTime1,out float startAcVel);


        kickCountDrive = kickCount2;
        restTimeDrive = restTime2;
        timeTest = totalTimeDrive;
        this.kickDrivePosition1 = lastKickDistanceOffsetPosition;
        this.kickDriveTime = kickDriveTime1;

        segmentedPath.Pos0 = lastKickDistanceOffsetPosition;


        //Invoke(nameof(pause), tlastKickDriver);
        driverDataComponent.position = defenseReachPosition + dir5 * kickDriveDistance;
       float driverV = GetTimeToReachPointDOTS.accelerationGetVelocity(ref driverDataComponent, restTime2, lastKickDistanceOffsetPosition);
        driverDataComponent.position = defenseReachPosition - dir5* lastKickDistanceOffset;
        driverDataComponent.ForwardVelocity = dir5 * driverV;
        driverDataComponent.normalizedForwardVelocity = dir5;
        driverDataComponent.currentSpeed = driverV;

        posDefenseLastKick = GetTimeToReachPointDOTS.accelerationGetPosition(defenseDataComponent.position,defenseDataComponent.currentSpeed,defenseDataComponent.bodyY0Forward,defenseDataComponent.maxSpeedRotation,defenseDataComponent.minSpeedForRotate,defenseDataComponent.acceleration,defenseDataComponent.decceleration,defenseDataComponent.maxAngleForRun,defenseDataComponent.scope, defenseReachPosition,defenseDataComponent.maxSpeed, tlastKickDriver,out float endSpeedLastKickDefense,out Vector3 endDirectionLastKickDefense);

        defenseDataComponent.position = posDefenseLastKick;
       
        Vector3 dir6 = defenseReachPosition - driverDataComponent.position;
        dir6.y = 0;
        dir6.Normalize();
        float defenseV = GetTimeToReachPointDOTS.accelerationGetVelocity(ref defenseDataComponent, driverLastKickTime, firstReachPositionDrive);
        defenseDataComponent.ForwardVelocity = dir6*defenseV;
        defenseDataComponent.normalizedForwardVelocity = dir6;
        defenseDataComponent.currentSpeed = defenseV;
        defenseDataComponent.bodyY0Forward = endDirectionLastKickDefense;
        defenseDataComponent.normalizedBodyY0Forward = endDirectionLastKickDefense;

        float d = Vector3.Distance(segmentedPath.Pos0, firstReachPositionDrive) - lastKickDistanceOffset;
        float d5 = Vector3.Distance(segmentedPath.Pos0, targetPosition) - scope;
        float tDriver = d / segmentedPath.V0Magnitude;
        if (defenseReachTime <= 0)
        {
            //print("driver llega antes del divert");
            
        }


        int maxDistanceKicks = (int)(d / playerSkills.maxDrivingDistance);
        float lastKickDistance = d - maxDistanceKicks * playerSkills.maxDrivingDistance;

        float d2 = d - playerSkills.lastKickDistanceOffset;

        Vector3 dir1 = defenseDataComponent.position - segmentedPath.Pos0;
        dir1.y = 0;
        dir1.Normalize();
        Vector3 dir2 = reachPosition - segmentedPath.Pos0;
        float d3 = dir2.magnitude;
        dir2.y = 0;
        dir2.Normalize();

        Vector3 dir3 = segmentedPath.Posf - segmentedPath.Pos0;
        
        dir3.Normalize();
        dir3.y = 0;

        float sign = Mathf.Sign(Vector3.Cross(dir1, dir2).y);
        float angle = 0;

        segmentedPath.Pos0 = lastKickDistanceOffsetPosition;
        while (angle < maxAngle)
        {
            angle += angleTest;
            if (!getReachTimeDivertKick(angle, sign, dir3, defenseDataComponent, driverDataComponent, d5, defenseIndex)) break;
            if (!getReachTimeDivertKick(angle, -sign, dir3, defenseDataComponent, driverDataComponent, d5, defenseIndex)) break;
            
            //if (angle == 10) break;
        }

        //Invoke(nameof(pause), defenseReachTime);
    }

    


    public IEnumerator driving2Coroutine(Vector3 reachPosition, int kickCount, float restTime, int defenseIndex)
    {
        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent);
        float time = timeTest;
        //GetTimeToReachPointDOTS.accelerationGetPositionDriving(ref playerDataComponent, targetPosition, time, playerSkills,out int kickCount,out float restTime);
        //restTimeTest = restTime;
        //kickCountTest = kickCount;

        Vector3 dir2 = firstReachPositionDrive - driverPublicPlayerData.position;

        dir2.y = 0;
        dir2.Normalize();
        float tBallAll = 0;

        Vector3 defenseDir = defenseReachPosition - rivals[defenseIndex].position;

        defenseDir.y = 0;
        defenseDir.Normalize();
        rivals[defenseIndex].playerComponents.TargetPosition = defenseReachPosition;
        rivals[defenseIndex].playerComponents.ForwardDesiredDirection = defenseDir;
        rivals[defenseIndex].playerComponents.ForwardDesiredSpeed = 10.5f;
        rivals[defenseIndex].playerComponents.DesiredLookDirection = defenseDir;

        for (int i = 0; i < kickCountDrive; i++)
        {
            driverPublicPlayerData.playerComponents.TargetPosition = driverPublicPlayerData.position + dir2 * playerSkills.maxDrivingDistance;
            driverPublicPlayerData.playerComponents.ForwardDesiredDirection = dir2;
            driverPublicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;
            driverPublicPlayerData.playerComponents.DesiredLookDirection = dir2;


            GetV0DOTSResult result = new GetV0DOTSResult();
            PathDataDOTS currentPath = new PathDataDOTS();
            currentPath.Pos0 = driverPublicPlayerData.position;
            currentPath.Posf = driverPublicPlayerData.playerComponents.TargetPosition;
            currentPath.k = ballRigidbody.drag;
            currentPath.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
            currentPath.friction = MatchComponents.ballComponents.friction;
            currentPath.g = 9.8f;
            currentPath.mass = MatchComponents.ballComponents.mass;
            currentPath.bounciness = MatchComponents.ballComponents.bounciness;
            currentPath.ballRadio = MatchComponents.ballRadio;
            currentPath.pathType = PathType.InGround;
            currentPath.vfMagnitude = (currentPath.g / currentPath.k);
            GetStraightV0Params getV0Params = GetV0Params();


            PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent2);

            float tBall2 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent2, driverPublicPlayerData.playerComponents.TargetPosition);

            StraightXZDragAndFrictionPathDOTS2.getV02(ref result, ref currentPath, 33, ref getV0Params, tBall2);
            ballRigidbody.angularVelocity = Vector3.zero;
            ballRigidbody.velocity = dir2 * result.v0Magnitude;
            //yield return new WaitUntil(() => BodyTargetXZDistance < 0.1f);
            tBallAll += tBall2;
            yield return new WaitForSeconds(tBall2);
            //print(Speed);

        }
        Vector3 dir = lastKickDistanceOffsetPosition - driverPublicPlayerData.position;
        dir.y = 0;
        dir.Normalize();

        driverPublicPlayerData.playerComponents.TargetPosition = lastKickDistanceOffsetPosition;
        driverPublicPlayerData.playerComponents.ForwardDesiredDirection = dir;
        driverPublicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;
        driverPublicPlayerData.playerComponents.DesiredLookDirection = dir;

        GetV0DOTSResult result2 = new GetV0DOTSResult();
        PathDataDOTS currentPath2 = new PathDataDOTS();
        currentPath2.Pos0 = MyFunctions.setY0ToVector3(ballPosition);
        currentPath2.Posf = lastKickDistanceOffsetPosition;
        currentPath2.k = ballRigidbody.drag;
        currentPath2.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
        currentPath2.friction = MatchComponents.ballComponents.friction;
        currentPath2.g = 9.8f;
        currentPath2.mass = MatchComponents.ballComponents.mass;
        currentPath2.bounciness = MatchComponents.ballComponents.bounciness;
        currentPath2.ballRadio = MatchComponents.ballRadio;
        currentPath2.pathType = PathType.InGround;
        currentPath2.vfMagnitude = (currentPath2.g / currentPath2.k);
        GetStraightV0Params getV0Params2 = GetV0Params();


        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent3);
        //playerDataComponent3.position = firstReachPositionDrive;
        float tBall3 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent3, lastKickDistanceOffsetPosition);

        StraightXZDragAndFrictionPathDOTS2.getV02(ref result2, ref currentPath2, 33, ref getV0Params2, tBall3);
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = dir * result2.v0Magnitude;
        
        yield return new WaitForSeconds(restTimeDrive);


        Vector3 reachTargetPosition = reachPositionDriftDefense;

        defenseDir = reachTargetPosition - rivals[defenseIndex].position;

        defenseDir.y = 0;
        defenseDir.Normalize();
        rivals[defenseIndex].playerComponents.TargetPosition = reachTargetPosition;
        rivals[defenseIndex].playerComponents.ForwardDesiredDirection = defenseDir;
        rivals[defenseIndex].playerComponents.ForwardDesiredSpeed = 10.5f;
        rivals[defenseIndex].playerComponents.DesiredLookDirection = defenseDir;


        Vector3 dir3 = reachTargetPosition - driverPublicPlayerData.position;
        dir3.y = 0;
        dir3.Normalize();

        publicPlayerData.playerComponents.TargetPosition = reachTargetPosition;
        //driverPublicPlayerData.playerComponents.TargetPosition = driverPublicPlayerData.position + dir *100;
        publicPlayerData.playerComponents.ForwardDesiredDirection = dir3;
        
        publicPlayerData.playerComponents.DesiredLookDirection = dir3;
        publicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;

        result2 = new GetV0DOTSResult();
        currentPath2 = new PathDataDOTS();
        currentPath2.Pos0 = publicPlayerData.position;
        currentPath2.Posf = reachTargetPosition;
        currentPath2.k = ballRigidbody.drag;
        currentPath2.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
        currentPath2.friction = MatchComponents.ballComponents.friction;
        currentPath2.g = 9.8f;
        currentPath2.mass = MatchComponents.ballComponents.mass;
        currentPath2.bounciness = MatchComponents.ballComponents.bounciness;
        currentPath2.ballRadio = MatchComponents.ballRadio;
        currentPath2.pathType = PathType.InGround;
        currentPath2.vfMagnitude = (currentPath2.g / currentPath2.k);
        getV0Params2 = GetV0Params();


        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out playerDataComponent3);
        //playerDataComponent3.position = firstReachPositionDrive;
        float tBall4 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent3, reachTargetPosition);

        StraightXZDragAndFrictionPathDOTS2.getV02(ref result2, ref currentPath2, 33, ref getV0Params2, tBall4);
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = dir3 * segmentedPath.V0Magnitude;
        yield return new WaitForSeconds(kickDriveDivertTime);
        //print(restTime +" "+ tBall3+" "+Speed);
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = Vector3.zero;
        Time.timeScale = 0;

    }

    
    void printTest()
    {
        print("a");
        Time.timeScale = 0;
    }
    
    
    public IEnumerator testAccelerationGetTimeToReachPosition()
    {
        Vector3 dir = Vector3.forward;

        dir.y = 0;
        dir.Normalize();
        driverPublicPlayerData.playerComponents.TargetPosition = testTransform.position;
        driverPublicPlayerData.playerComponents.ForwardDesiredDirection = dir;
        driverPublicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;
        driverPublicPlayerData.playerComponents.DesiredLookDirection = dir;

        yield return new WaitForSeconds(1);

        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent);
        float d = Vector3.Distance(playerDataComponent.position, testTransform.position) - scope;
        float t = GetTimeToReachPointDOTS.accelerationGetTimeToReachPosition(ref playerDataComponent,testTransform.position, out Vector3 startAcPoint, out float startAcTime, out Vector3 endPoint);
        this.startAcPoint = startAcPoint;
        dir = testTransform.position - startAcPoint;

        dir.y = 0;
        dir.Normalize();
        driverPublicPlayerData.playerComponents.TargetPosition = testTransform.position;
        driverPublicPlayerData.playerComponents.ForwardDesiredDirection = dir;
        driverPublicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;
        driverPublicPlayerData.playerComponents.DesiredLookDirection = dir;

        print("t="+t);
        //print("start to decelerate vStartDa=" + vStartDa + " currentSpeed=" + driverPublicPlayerData.speed + " startDaT=" + startDaT + " t2=" + t2);
        yield return new WaitForSeconds(t);
        print("end");
        Time.timeScale = 0;
    }
    IEnumerator testGetPerfectPass()
    {

        PerfectGetPositionParms perfectGetPositionParms = new PerfectGetPositionParms();
        perfectGetPositionParms.k = ballRigidbody.drag;
        perfectGetPositionParms.u = MatchComponents.ballComponents.friction;
        perfectGetPositionParms.g = 9.8f;
        perfectGetPositionParms.v0 = 7;
        perfectGetPositionParms.t = 2;



        float d = StraightXZDragAndFrictionPathDOTS2.getPerfectPositionAtTime(perfectGetPositionParms);
        ballRigidbody.velocity = Vector3.forward * perfectGetPositionParms.v0;
        yield return new WaitForSeconds(perfectGetPositionParms.t);
        float d2 = Vector3.Distance(ballRigidbody.position, ballRigidbody.position + Vector3.forward * d);
        print("d=" + d + " " + "d2=" + d2);
    }
    public IEnumerator testGetX_StartToDesacelerate()
    {
        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent);
        float d = Vector3.Distance(playerDataComponent.position, testTransform.position) - scope;
        float tDa = AccelerationPath.getT_StartToDesacelerate(playerDataComponent.currentSpeed, playerDataComponent.maxSpeedForReachBall, playerDataComponent.acceleration, -playerDataComponent.decceleration, d);
        
        //float dA = d - dDa;
        AccelerationPath.getT(tDa, playerDataComponent.currentSpeed, playerDataComponent.acceleration, out float r);
        float t1 = tDa;
        Vector3 dir = testTransform.position - playerDataComponent.position;

        dir.y = 0;
        dir.Normalize();
        driverPublicPlayerData.playerComponents.TargetPosition = testTransform.position;
        driverPublicPlayerData.playerComponents.ForwardDesiredDirection = dir;
        driverPublicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;
        driverPublicPlayerData.playerComponents.DesiredLookDirection = dir;

        
        //print("start to decelerate vStartDa=" + vStartDa + " currentSpeed=" + driverPublicPlayerData.speed + " startDaT=" + startDaT + " t2=" + t2);
        yield return new WaitForSeconds(t1);

        float vStartDa = AccelerationPath.getV2(playerDataComponent.currentSpeed, t1, playerDataComponent.acceleration);
        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out  playerDataComponent);
        float t2 = Mathf.Abs(AccelerationPath.getT(playerDataComponent.maxSpeedForReachBall, playerDataComponent.currentSpeed, playerDataComponent.decceleration));
        float t = t1 + t2;
        print("startDaT=" + t1 + " t2="+ t2 + " currentSpeed="+playerComponents.Speed+ " vStartDa="+ vStartDa);
        yield return new WaitForSeconds(t2);
        print("end");
        Time.timeScale = 0;
    }
    public IEnumerator testGetT_StartToDesacelerate()
    {
        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent);
        float d = Vector3.Distance(playerDataComponent.position, testTransform.position);
        float startDaT = AccelerationPath.getT_StartToDesacelerate(playerDataComponent.currentSpeed, playerDataComponent.maxSpeedForReachBall, playerDataComponent.acceleration, -playerDataComponent.decceleration, d);

        Vector3 dir = testTransform.position - playerDataComponent.position;

        dir.y = 0;
        dir.Normalize();
        driverPublicPlayerData.playerComponents.TargetPosition = testTransform.position;
        driverPublicPlayerData.playerComponents.ForwardDesiredDirection = dir;
        driverPublicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;
        driverPublicPlayerData.playerComponents.DesiredLookDirection = dir;


        //print("start to decelerate vStartDa=" + vStartDa + " currentSpeed=" + driverPublicPlayerData.speed + " startDaT=" + startDaT + " t2=" + t2);
        yield return new WaitForSeconds(startDaT);
        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out  playerDataComponent);
        float vStartDa = AccelerationPath.getV2(playerDataComponent.currentSpeed, startDaT, playerDataComponent.acceleration);
        float t2 = Mathf.Abs(AccelerationPath.getT(playerDataComponent.maxSpeedForReachBall, playerDataComponent.currentSpeed, playerDataComponent.decceleration));
        float t = startDaT + t2;
        print("startDaT=" + startDaT + " t2=" + t2);
        yield return new WaitForSeconds(t2);
        print("end");
        Time.timeScale = 0;
    }
    public IEnumerator driving4Coroutine(Vector3 reachPosition, int kickCount, float restTime, int defenseIndex)
    {
        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent);
        float time = timeTest;
        //GetTimeToReachPointDOTS.accelerationGetPositionDriving(ref playerDataComponent, targetPosition, time, playerSkills,out int kickCount,out float restTime);
        //restTimeTest = restTime;
        //kickCountTest = kickCount;

        Vector3 dir2 = firstReachPositionDrive - driverPublicPlayerData.position;

        dir2.y = 0;
        dir2.Normalize();
        float tBallAll = 0;


        for (int i = 0; i < kickCount; i++)
        {
            driverPublicPlayerData.playerComponents.TargetPosition = driverPublicPlayerData.position + dir2 * playerSkills.maxDrivingDistance;
            driverPublicPlayerData.playerComponents.ForwardDesiredDirection = dir2;
            driverPublicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;
            driverPublicPlayerData.playerComponents.DesiredLookDirection = dir2;


            GetV0DOTSResult result = new GetV0DOTSResult();
            PathDataDOTS currentPath = new PathDataDOTS();
            currentPath.Pos0 = driverPublicPlayerData.position;
            currentPath.Posf = driverPublicPlayerData.playerComponents.TargetPosition;
            currentPath.k = ballRigidbody.drag;
            currentPath.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
            currentPath.friction = MatchComponents.ballComponents.friction;
            currentPath.g = 9.8f;
            currentPath.mass = MatchComponents.ballComponents.mass;
            currentPath.bounciness = MatchComponents.ballComponents.bounciness;
            currentPath.ballRadio = MatchComponents.ballRadio;
            currentPath.pathType = PathType.InGround;
            currentPath.vfMagnitude = (currentPath.g / currentPath.k);
            GetStraightV0Params getV0Params = GetV0Params();


            PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent2);

            float tBall2 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent2, driverPublicPlayerData.playerComponents.TargetPosition);

            StraightXZDragAndFrictionPathDOTS2.getV02(ref result, ref currentPath, 33, ref getV0Params, tBall2);
            ballRigidbody.angularVelocity = Vector3.zero;
            ballRigidbody.velocity = dir2 * result.v0Magnitude;
            //yield return new WaitUntil(() => BodyTargetXZDistance < 0.1f);
            tBallAll += tBall2;
            yield return new WaitForSeconds(tBall2);
            //print(Speed);

        }
        Vector3 dir = reachPositionDrive - driverPublicPlayerData.position;
        dir.y = 0;
        dir.Normalize();

        driverPublicPlayerData.playerComponents.TargetPosition = reachPositionDrive;
        //driverPublicPlayerData.playerComponents.TargetPosition = driverPublicPlayerData.position + dir *100;
        driverPublicPlayerData.playerComponents.ForwardDesiredDirection = dir;
        driverPublicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;
        driverPublicPlayerData.playerComponents.DesiredLookDirection = dir;

        GetV0DOTSResult result2 = new GetV0DOTSResult();
        PathDataDOTS currentPath2 = new PathDataDOTS();
        currentPath2.Pos0 = firstReachPositionDrive;
        currentPath2.Posf = reachPositionDrive;
        currentPath2.k = ballRigidbody.drag;
        currentPath2.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
        currentPath2.friction = MatchComponents.ballComponents.friction;
        currentPath2.g = 9.8f;
        currentPath2.mass = MatchComponents.ballComponents.mass;
        currentPath2.bounciness = MatchComponents.ballComponents.bounciness;
        currentPath2.ballRadio = MatchComponents.ballRadio;
        currentPath2.pathType = PathType.InGround;
        currentPath2.vfMagnitude = (currentPath2.g / currentPath2.k);
        GetStraightV0Params getV0Params2 = GetV0Params();


        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent3);
        //playerDataComponent3.position = firstReachPositionDrive;
        float tBall3 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent3, reachPositionDrive);

        StraightXZDragAndFrictionPathDOTS2.getV02(ref result2, ref currentPath2, 33, ref getV0Params2, tBall3 + tBallAll);
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = dir * result2.v0Magnitude;
        yield return new WaitForSeconds(restTime);
        //print(restTime +" "+ tBall3+" "+Speed);
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = Vector3.zero;
        //Time.timeScale = 0;

    }

    



    public IEnumerator driving3Coroutine(int defenseIndex)
    {

        Vector3 dir = defenseReachPosition - rivals[defenseIndex].position;

        dir.y = 0;
        dir.Normalize();
        rivals[defenseIndex].playerComponents.TargetPosition = defenseReachPosition;
        rivals[defenseIndex].playerComponents.ForwardDesiredDirection = dir;
        rivals[defenseIndex].playerComponents.ForwardDesiredSpeed = 10.5f;
        rivals[defenseIndex].playerComponents.DesiredLookDirection = dir;

        yield return new WaitForSeconds(driverLastKickTime);

        Vector3 dir2 = reachPositionDriftDefense - rivals[defenseIndex].position;
        dir2.y = 0;
        dir2.Normalize();

        rivals[defenseIndex].playerComponents.TargetPosition = reachPositionDriftDefense;
        rivals[defenseIndex].playerComponents.ForwardDesiredDirection = dir2;
        rivals[defenseIndex].playerComponents.ForwardDesiredSpeed = 10.5f;
        rivals[defenseIndex].playerComponents.DesiredLookDirection = dir2;
    }









    public IEnumerator drivingCoroutine()
    {
        Vector3 targetPosition = testTransform.position;
        Vector3 dir = targetPosition - driverPublicPlayerData.position;
        dir.y = 0;
        dir.Normalize();
        
        

        for (int i = 0; i < 3; i++)
        {
            driverPublicPlayerData.playerComponents.TargetPosition = driverPublicPlayerData.position + dir * (playerSkills.maxDrivingDistance);
            driverPublicPlayerData.playerComponents.ForwardDesiredDirection = dir;
            driverPublicPlayerData.playerComponents.ForwardDesiredSpeed = 10.5f;
            driverPublicPlayerData.playerComponents.DesiredLookDirection = dir;
            driverPublicPlayerData.movimentValues.maxSpeedForReachBall = 0;

            GetV0DOTSResult result = new GetV0DOTSResult();
            PathDataDOTS currentPath = new PathDataDOTS();
            currentPath.Pos0 = driverPublicPlayerData.position;
            currentPath.Posf = driverPublicPlayerData.playerComponents.TargetPosition;
            currentPath.k = ballRigidbody.drag;
            currentPath.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
            currentPath.friction = MatchComponents.ballComponents.friction;
            currentPath.g = 9.8f;
            currentPath.mass = MatchComponents.ballComponents.mass;
            currentPath.bounciness = MatchComponents.ballComponents.bounciness;
            currentPath.ballRadio = MatchComponents.ballRadio;
            currentPath.pathType = PathType.InGround;
            currentPath.vfMagnitude = (currentPath.g / currentPath.k);
            GetStraightV0Params getV0Params= GetV0Params();
            float tBall = driverPublicPlayerData.getTimeToReachPosition(driverPublicPlayerData.playerComponents.TargetPosition, 0);

            
            PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent);
            
            float tBall2 = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent, driverPublicPlayerData.playerComponents.TargetPosition);
            
            StraightXZDragAndFrictionPathDOTS2.getV02(ref result, ref currentPath, 33, ref getV0Params, tBall2);
            ballRigidbody.angularVelocity = Vector3.zero;
            ballRigidbody.velocity=kickDriveV0Drive;
            //yield return new WaitUntil(() => BodyTargetXZDistance < 0.1f);


            yield return new WaitForSeconds(tBall2);
            //print(Speed);
            //Time.timeScale = 0;
        }
        
    }
    GetStraightV0Params GetV0Params()
    {
        GetStraightV0Params getV0Params = new GetStraightV0Params();
        getV0Params.maxAttempts = 20;
        getV0Params.maxControlSpeed = 15;
        getV0Params.accuracy = 0.1f;
        getV0Params.maxControlSpeedLerpDistance = 5f;
        getV0Params.searchVyIncrement = 0.5f;
        return getV0Params;
    }




    public IEnumerator testCoroutine(float t,Vector3 targetPosition,int index, PlayerDataComponent playerDataComponent)
    {
        /*float t = times.Get(0);
        Vector3 dir2 = segmentedPath.Posf - segmentedPath.Pos0;
        dir2.y = 0;
        dir2.Normalize();
        Vector3 reachPoint = segmentedPath.Pos0 + dir2 * segmentedPath.V0Magnitude * t;
        StartCoroutine(testCoroutine(t, reachPoint, j, playerDataComponent));*/
        float t2 = 0;
        Vector3 dir = segmentedPath.Posf - segmentedPath.Pos0;
        dir.y = 0;
        dir.Normalize();
        Vector3 dir2 = targetPosition - playerDataComponent.position;
        dir2.y = 0;
        dir2.Normalize();
        rivals[index].playerComponents.TargetPosition = targetPosition;
        rivals[index].playerComponents.ForwardDesiredDirection = dir2;
        rivals[index].playerComponents.ForwardDesiredSpeed = rivals[index].maxSpeed;
        rivals[index].playerComponents.DesiredLookDirection = dir2;
        testTransform.position = segmentedPath.Pos0;
        while (t2 < t)
        {
            testTransform.position += dir * Time.deltaTime * segmentedPath.V0Magnitude;
            yield return null;
            t2 += Time.deltaTime;
        }
        //Time.timeScale = 0;
    }
    Vector3 GetKickDriveV0(PublicPlayerData publicPlayerData, Vector3 posBall, Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - driverPublicPlayerData.position;
        dir.y = 0;
        dir.Normalize();
        Vector3 d = posBall + dir * playerSkills.maxDrivingDistance;

        PublicPlayerData.getPlayerData(driverPublicPlayerData, 0, out PlayerDataComponent playerDataComponent);
        float tBall = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent, d);
        kickDriveTime = tBall;
        return GetBallV0(tBall, posBall, d);
    }
    Vector3 GetBallV0(float t, Vector3 pos0, Vector3 posf)
    {
        GetV0DOTSResult result = new GetV0DOTSResult();
        PathDataDOTS currentPath = new PathDataDOTS();
        currentPath.Pos0 = pos0;
        currentPath.Posf = posf;
        currentPath.k = ballRigidbody.drag;
        currentPath.slidingFriction = MatchComponents.ballComponents.dynamicFriction;
        currentPath.friction = MatchComponents.ballComponents.friction;
        currentPath.g = 9.8f;
        currentPath.mass = MatchComponents.ballComponents.mass;
        currentPath.bounciness = MatchComponents.ballComponents.bounciness;
        currentPath.ballRadio = MatchComponents.ballRadio;
        currentPath.pathType = PathType.InGround;
        currentPath.vfMagnitude = (currentPath.g / currentPath.k);
        GetStraightV0Params getV0Params = GetV0Params();

        StraightXZDragAndFrictionPathDOTS2.getV02(ref result, ref currentPath, 33, ref getV0Params, t);

        return result.v0;
    }
    Vector3 getDirection()
    {
        return Vector3.zero;
    }

    public void StartProcess()
    {
        if (!enabled) return;
        coroutine = StartCoroutine(run());

    }
    public void StopProcess()
    {
        if (!enabled) return;
        StopCoroutine(coroutine);
    }
    public IEnumerator run()
    {

        while (true)
        {

            if (ballIsOrientedControlled)
            {

                if (Speed < MaxSpeed * maxSpeedBodyPercent)
                {
                    //getSpeed();

                    yield return new WaitForSeconds(UnityEngine.Random.Range(minHitTime, maxHitTime));
                }
                else
                {
                    yield return null;
                }
            }
            else
            {
                yield return null;
            }
            //yield return null;
        }
    }
}
