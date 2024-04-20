using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPerfectPass : SoccerPlayerComponent
{
    public float timeRange, timeIncrement, minAngle, minVelocity, maxAngle, maxControlSpeed;
    public bool drawGizmos,drawChaserData,drawClosestPoint, drawOptimalPoint;
    CalculateChaserDatas calculateChaserDatas;
    List<ChaserData> rivalsChaserDatas = new List<ChaserData>();
    List<ChaserData> partnersChaserDatas = new List<ChaserData>();
    List<ChaserData> allChaserDatas = new List<ChaserData>();
    ChaserData myChaserData;
    public List<PublicPlayerData> partners;
    public List<PublicPlayerData> rivals;
    PublicPlayerData chaserTest;
    public Transform testKickDirection;
    public Transform targetPositionTrans;
    public Transform origintTrans;
    public Transform initPlayerDirection;
    Vector3 targetPosition { get => targetPositionTrans.position; }
    public float force;
    public float vt;
    SegmentedPath segmentedPath;
    public bool DragAndFriction;
    public bool addAccelerationGetTimeToReachPosition;
    public StraightPass straightPass;
    public ParabolicPass parabolicPass;
    public ChaserDataCalculationParameters ChaserDataCalculationParameters;
    void Start()
    {
        
        buildCalculateChaserDatas();
        //MatchEvents.addedPublicPlayerDataToList.AddListener(addChaserData);
        MatchEvents.matchLoaded.AddListenerConsiderInvoked(addChaserTest);
        straightPass.chaserDataCalculationParameters = ChaserDataCalculationParameters;
        parabolicPass.chaserDataCalculationParameters = ChaserDataCalculationParameters;
        //addChaserTest();
        
    }

    void addChaserTest()
    {
        myChaserData = new ChaserData(Vector3.zero, publicPlayerData, MatchComponents.footballField.fullFieldArea, scope,true,publicPlayerData.playerComponents.name);
        allChaserDatas.Add(myChaserData);
        foreach (var item in rivals)
        {
            ChaserData chaserData = new ChaserData(Vector3.zero, item, MatchComponents.footballField.fullFieldArea,scope, true, item.playerComponents.name);
            
            rivalsChaserDatas.Add(chaserData);
            allChaserDatas.Add(chaserData);
        }
        foreach (var item in partners)
        {
            ChaserData chaserData = new ChaserData(Vector3.zero, item, MatchComponents.footballField.fullFieldArea,scope, true, item.playerComponents.name);
            partnersChaserDatas.Add(chaserData);
            allChaserDatas.Add(chaserData);
        }
    }
    
    void buildCalculateChaserDatas()
    {
        calculateChaserDatas = new CalculateChaserDatas(timeRange, timeIncrement, minAngle, minVelocity, maxAngle, maxControlSpeed);
    }
    // Update is called once per frame
    
    
    SegmentedPath buildSegmentedPath(Vector3 pos0,Vector3 v0)
    {
        float radius = MatchComponents.ballComponents.radio;
        //print("a "+radius);
        Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        float drag = ballRigidbody.drag;
        PhysicMaterial ballPhysicsMaterial = MatchComponents.ballComponents.physicMaterial;
        PhysicMaterial footballFieldPhysicMaterial = MatchComponents.footballField.footballFieldPhysicMaterial;
        float bounciness = GetPhysicMaterialCombine.getCombination(ballPhysicsMaterial.bounciness, footballFieldPhysicMaterial.bounciness, GetPhysicMaterialCombine.getPhysicMaterialCombine(ballPhysicsMaterial.bounceCombine, footballFieldPhysicMaterial.bounceCombine));
        float dynamicFriction = GetPhysicMaterialCombine.getCombination(ballPhysicsMaterial.dynamicFriction, footballFieldPhysicMaterial.dynamicFriction, GetPhysicMaterialCombine.getPhysicMaterialCombine(ballPhysicsMaterial.frictionCombine, footballFieldPhysicMaterial.frictionCombine));
        float staticFriction = GetPhysicMaterialCombine.getCombination(ballPhysicsMaterial.staticFriction, footballFieldPhysicMaterial.staticFriction, GetPhysicMaterialCombine.getPhysicMaterialCombine(ballPhysicsMaterial.frictionCombine, footballFieldPhysicMaterial.frictionCombine));
        float friction = staticFriction > dynamicFriction ? staticFriction : dynamicFriction;
        ParabolaWithDrag trajectory = new ParabolaWithDrag(pos0, v0, 0, 9.81f, drag);
        Path path;
        if (DragAndFriction || true)
        {

            StraightXZDragAndFrictionPath straightXZDragAndFrictionPath = new StraightXZDragAndFrictionPath(drag, radius, friction, ballRigidbody.mass);
            path = straightXZDragAndFrictionPath;
        }
        else
        {

            StraightXZDragPath straightXZDragPath = new StraightXZDragPath(drag);
            path = straightXZDragPath;
        }
        BouncyPath bouncyPath = new BouncyPath(trajectory, path, radius, 0.1f, bounciness,friction);
        bouncyPath.info = "perfectPass";
        SegmentedPath segmentedPath = new SegmentedPath(bouncyPath);
        return segmentedPath;
    }
    void getPassV0()
    {
        Vector3 dir = targetPosition - MyFunctions.setYToVector3(ballPosition, targetPosition.y);
        dir.Normalize();
       
        PassParameters passParameters = new PassParameters(myChaserData, partnersChaserDatas, rivalsChaserDatas, MyFunctions.setY0ToVector3(ballPosition) , targetPosition,maxControlSpeed,maxKickForce,0.1f,1,Vector3.zero,5,0,0.4f);
        PassResult passResult = straightPass.getPassResult(passParameters);
        //print("a " + passResult);
        print(passParameters.receiverReachPointTime+ " "+ passResult.v0.magnitude);
        if (passResult.resultFounded || passResult.somePartnerIsAhead)
        {
            if (passResult.noRivalReachTheTargetBeforeMe && !passResult.somePartnerIsAhead)
            {
                print("b " + passResult);
                ballRigidbody.velocity = passResult.v0;
            }
            else
            {
                float maxReceiverHeight = myChaserData.publicPlayerData.playerData.height*0.75f;
                ParabolicPassParameters parabolicPassParameters = new ParabolicPassParameters(myChaserData, partnersChaserDatas, rivalsChaserDatas, MyFunctions.setY0ToVector3(ballPosition), targetPosition, maxControlSpeed, maxKickForce, 0.1f, 1, passResult.v0, 0.1f,100, maxReceiverHeight,0.5f,5, passParameters.receiverReachPointTime,0.5f);
                passResult = parabolicPass.getPassResult(parabolicPassParameters);
                if (!MyFunctions.Vector3IsNan(passResult.v0) || passResult.v0.Equals(Vector3.positiveInfinity))
                {
                    ballRigidbody.velocity = passResult.v0;
                }
                print("c " + passResult);
            }
        }
        setRivalsDestiny(passParameters);
        setMyPlayerDestiny(passParameters);
    }
    void setRivalsDestiny(PassParameters passParameters)
    {

        foreach (ChaserData rivalChaserData in passParameters.rivalsChaserDatas)
        {
            Vector3 dir = rivalChaserData.OptimalPoint - rivalChaserData.position;
            dir.Normalize();
            rivalChaserData.publicPlayerData.playerComponents.ForwardDesiredDirection = dir;
            rivalChaserData.publicPlayerData.playerComponents.ForwardDesiredSpeed = rivalChaserData.publicPlayerData.playerComponents.MaxSpeed;
            
            rivalChaserData.publicPlayerData.playerComponents.DesiredLookDirection = dir;
            rivalChaserData.publicPlayerData.playerComponents.MinForwardSpeed = 0;
            rivalChaserData.publicPlayerData.playerComponents.TargetPosition = rivalChaserData.OptimalPoint;
            rivalChaserData.publicPlayerData.playerComponents.stopOffset = MatchComponents.ballComponents.radio;
        }
    }
    void setMyPlayerDestiny(PassParameters passParameters)
    {
        ChaserData myChaserData = passParameters.ReceiverChaserData;
        Vector3 dir = passParameters.Posf - myChaserData.position;
        dir.Normalize();
        myChaserData.publicPlayerData.playerComponents.ForwardDesiredDirection = dir;
        myChaserData.publicPlayerData.playerComponents.ForwardDesiredSpeed = movementValues.maxForwardSpeed;
        myChaserData.publicPlayerData.playerComponents.DesiredLookDirection = dir;
        myChaserData.publicPlayerData.playerComponents.MinForwardSpeed = 0;
        myChaserData.publicPlayerData.playerComponents.TargetPosition = passParameters.Posf;
        myChaserData.publicPlayerData.playerComponents.stopOffset = MatchComponents.ballComponents.radio;
    }
    #region tests
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            getPassV0();
            //StartCoroutine(checkVelocityInTargetPosition());
            //StartCoroutine(test());
            //StartCoroutine(v0Test());
        }
    }
    void addChaserData(PublicPlayerData publicPlayerData)
    {
        ChaserData chaserData = new ChaserData(Vector3.zero, publicPlayerData, MatchComponents.footballField.fullFieldArea, scope, "Body");
        rivalsChaserDatas.Add(chaserData);
    }
    void getPerfectPass(PublicPlayerData targetPartner, Vector3 targetPosition)
    {
        ChaserData partnerChaserData = new ChaserData(Vector3.zero, targetPartner, MatchComponents.footballField.fullFieldArea, scope, "Body");
        SegmentedPath segmentedPath = buildSegmentedPath(ballPosition, ballVelocity);
        this.segmentedPath = segmentedPath;
        calculateChaserDatas.getChaserDatas(allChaserDatas, segmentedPath, true, 0, null);

        bool targetPartnerIsFirstChaser = ChaserData.checkOnlyFirstChaserDatas(partnerChaserData, rivalsChaserDatas, 1);
    }
    void chaserDataResult(List<ChaserData> chaserDatas)
    {
        print("chaserDataResult");
    }

    bool getV0WithVt(ChaserData chaserData, Vector3 targetPosition, out Vector3 result, out float t)
    {
        float distance = Vector3.Distance(MyFunctions.setYToVector3(chaserData.position, targetPosition.y), targetPosition);
        t = chaserData.getTimeToReachPoint(targetPosition, scope);
        StraightXZDragAndFrictionPath straightXZDragAndFrictionPath = new StraightXZDragAndFrictionPath(ballPosition, targetPosition, Vector3.zero, Vector3.zero, 0, t, drag, ballRadio, friction, m);
        return straightXZDragAndFrictionPath.getV0WithVt(vt, 0.1f, 20, out result);
    }
    void printDistance()
    {
        float d = Vector3.Distance(ballPosition, MyFunctions.setYToVector3(targetPosition, ballRadio));
        print("d=" + d);
    }
    void test2()
    {
        StraightXZDragAndFrictionPath straightXZDragAndFrictionPath = new StraightXZDragAndFrictionPath(ballPosition, targetPosition, Vector3.zero, Vector3.zero, 0, 0, drag, ballRadio, friction, m);
        float result;
        Vector3 dir = targetPosition - ballPosition;
        dir.y = 0;
        dir.Normalize();
        ballRigidbody.velocity = dir * force;
        bool a = straightXZDragAndFrictionPath.getT(dir * force, ballPosition, targetPosition, drag, 0.1f, 100, out result);
        if (a)
        {
            Invoke(nameof(printDistance), result);
        }
    }

    IEnumerator checkVelocityInTargetPosition()
    {
        while (true)
        {
            float d = Vector3.Distance(ballPosition, MyFunctions.setYToVector3(targetPosition, ballRadio));
            if (d < 0.25f)
            {
                print("ballSpeed=" + ballSpeed);
                break;
            }
            yield return null;
        }
    }
    IEnumerator test()
    {
        /*Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        ballRigidbody.velocity = testKickDirection.forward * force;
        
        yield return new WaitForSeconds(1);*/
        ForwardDesiredDirection = initPlayerDirection.forward;
        movementValues.DesiredLookDirection = initPlayerDirection.forward;
        ForwardDesiredSpeed = MaxSpeed;
        yield return new WaitForSeconds(1);
        ballPosition = testKickDirection.position;
        ballRigidbody.velocity = testKickDirection.forward * force;
        SegmentedPath segmentedPath = buildSegmentedPath(ballPosition, ballVelocity);
        this.segmentedPath = segmentedPath;
        StartCoroutine(calculateChaserDatas.getChaserDatas(allChaserDatas, segmentedPath,true,0, null));
        ChaserData chaserData = myChaserData;
        Vector3 targetPosition = chaserData.OptimalPoint;
        float t = 0;
        PublicPlayerData publicPlayerData = chaserData.publicPlayerData;
        Vector3 dir = MyFunctions.setY0ToVector3(targetPosition - publicPlayerData.bodyTransform.position);
        
        while (t < chaserData.OptimalTime)
        {
            if (chaserData.ReachTheTarget)
            {
                Vector3 optimalPoint = chaserData.OptimalPoint;
                Vector3 chaserDirection = MyFunctions.setY0ToVector3(optimalPoint - bodyPosition);
                ForwardDesiredDirection = chaserDirection;

                movementValues.DesiredLookDirection = chaserDirection;
                ForwardDesiredSpeed = MaxSpeed;
                MinForwardSpeed = 0;
                TargetPosition = optimalPoint;
                stopOffset = bodyBallRadio;
            }
            yield return null;
        }
        //print("distance=" + Vector3.Distance(ballPosition, publicPlayerData.bodyTransform.position));
    }


        IEnumerator testTime()
    {
        /*Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        ballRigidbody.velocity = testKickDirection.forward * force;
        
        yield return new WaitForSeconds(1);*/

        Vector3 targetPosition = targetPositionTrans.position;
        float t = 0;

        Transform botTrans = chaserTest.bodyTransform;
        Vector3 botDir = MyFunctions.setY0ToVector3(targetPosition - publicPlayerData.bodyTransform.position);
        float t2 = botDir.magnitude / chaserTest.maxSpeed;
        StraightXZDragAndFrictionPath straightXZDragAndFrictionPath = new StraightXZDragAndFrictionPath(ballPosition, targetPosition, Vector3.zero, Vector3.zero, 0, t2, drag, ballRadio, friction, m);
        Vector3 ballDir = targetPosition - ballPosition;
        float vt = force / 1.5f;
        float t3 = straightXZDragAndFrictionPath.getTime(ballDir.normalized * force, vt);
        ballRigidbody.velocity = ballDir.normalized * force;
        yield return new WaitForSeconds(t3);
        print("vt=" + vt + " | " + ballRigidbody.velocity.magnitude);
    }
    Vector3 pos2;
    IEnumerator testPosVt()
    {
        /*Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        ballRigidbody.velocity = testKickDirection.forward * force;
        
        yield return new WaitForSeconds(1);*/

        
        Vector3 targetPosition = targetPositionTrans.position;
        float t = 0;

        Transform botTrans = chaserTest.bodyTransform;
        Vector3 botDir = MyFunctions.setY0ToVector3(targetPosition - publicPlayerData.bodyTransform.position);
        float t2 = botDir.magnitude / chaserTest.maxSpeed;
        StraightXZDragAndFrictionPath straightXZDragAndFrictionPath = new StraightXZDragAndFrictionPath(ballPosition, targetPosition, Vector3.zero, Vector3.zero, 0, t2, drag, ballRadio, friction, m);
        Vector3 ballDir = MyFunctions.setY0ToVector3(targetPosition - ballPosition);
        Vector3 pos = straightXZDragAndFrictionPath.getPositionAtVt(vt, ballDir.normalized * force);
        pos2 = pos;
        ballRigidbody.velocity = ballDir.normalized * force;
        
        while (t<15)
        {
            //print("distance=" + Vector3.Distance(ballPosition, pos));
            //print("v=" + ballRigidbody.velocity.magnitude);
            if (Vector3.Distance(ballRigidbody.position, pos)<0.1f)
            {
                print("vt=" + vt + " | " + ballRigidbody.velocity.magnitude);
            }
            yield return new WaitForFixedUpdate();
            t += Time.deltaTime;
        }
    }
    void OnDrawGizmos()
    {
        if (enabled)
        {
            
            if (Application.isPlaying && drawGizmos && calculateChaserDatas != null)
            {
                Debug.DrawLine(origintTrans.position, targetPosition);
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(pos2, ballRadio);
                calculateChaserDatas.printChaserListResult(rivalsChaserDatas, drawGizmos, drawChaserData, drawOptimalPoint, drawClosestPoint);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(myChaserData.OptimalPoint, ballRadio);
                if (segmentedPath != null)
                {
                    segmentedPath.DrawPath("", force * 5, 0.1f, false);

                }
                foreach (var item in rivalsChaserDatas)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(item.OptimalPoint, 0.1f);
                }
            }
        }
    }
    #endregion

}
