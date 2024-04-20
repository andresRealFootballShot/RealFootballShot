using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PathWithAccelerationTest : MonoBehaviour
{
    SegmentedPath segmentedPath;
    public Transform chaserTransform, targetTransform;
    public PublicPlayerData chaserPublicPlayerData;
    public MovementCtrl movementCtrl;
    public float v0, chaserSpeed, scope,time;
    public bool withAcceleration = true;
    public bool drawGizmos;
    Vector3 targetPosition { get => targetTransform.position; set => targetTransform.position = value; }
    Vector3 chaserPosition { get => chaserTransform.position; set => chaserTransform.position = value; }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(withAccelerationTest3());
        }
    }
    List<Path> getPathList()
    {
        List<Path> list = new List<Path>();
        float t0 = 0;
        float increase = 0.1f;
        float v = v0;
        StraightXZDragAndFrictionPath straightXZDragAndFrictionPath = getStraightXZDragAndFrictionPath();
        for (int i = 0; i < 60; i++)
        {
            float tf = t0 + increase;
            Vector3 pos0 = straightXZDragAndFrictionPath.getPositionAtTime(t0);
            Vector3 posf = straightXZDragAndFrictionPath.getPositionAtTime(tf);
            Vector3 v0 = straightXZDragAndFrictionPath.getVelocityAtTime(t0);
            Vector3 vf = straightXZDragAndFrictionPath.getVelocityAtTime(tf);
            Path path = new Path(pos0, posf, v0, vf, t0, tf);
            list.Add(path);
            t0 = tf;
        }
        return list;
    }
    List<Path> getPathList2()
    {
        List<Path> list = new List<Path>();
        float t0 = 0;
        float increase = 0.2f;
        float v = v0;
        for (int i = 0; i < 20; i++)
        {
            float tf = t0 + increase;
            Vector3 pos0 = targetPosition + targetTransform.forward * v * t0;
            Vector3 posf = targetPosition + targetTransform.forward * v * tf;
            Path path = new Path(pos0, posf, targetTransform.forward * v, targetTransform.forward * v, t0, tf);
            list.Add(path);
            t0 = tf;
        }
        return list;
    }
    bool getTofPathList(List<Path> list,bool useAcceleration, out float t,out Vector3 point)
    {
        t = 0;
        scope = chaserPublicPlayerData.playerComponents.scope;
        int i = 0;
        point = Vector3.zero;
        foreach (var path in list)
        {
            //scope = 0;
            List<float> results;
            //print("path " + i);
            if (useAcceleration)
            {
                ChaserData chaserData;
                chaserPublicPlayerData.getFirstChaserData(out chaserData);
                path.getOptimalPointForReachTargetWhitAcceleration(chaserData, path.t0, scope, 0.1f, out results);
            }
            else
            {
                path.getOptimalPointForReachTarget(chaserPublicPlayerData.position,chaserPublicPlayerData.maxSpeed, path.t0, scope, out results);
                
            }
            if (results.Count > 0)
            {
                t = results[0] + path.t0;
                point = path.Pos0 + path.V0 * results[0];
                return true;
            }
            i++;
        }
        return false;
    }
    bool getTofStraightXZDragAndFrictionPath(out float t, out Vector3 point)
    {
        t = 0;
        //scope = chaserPublicPlayerData.playerComponents.playerData.scope;
        point = Vector3.zero;
        List<float> results;
        StraightXZDragAndFrictionPath path = getStraightXZDragAndFrictionPath();
        ChaserData chaserData;
        chaserPublicPlayerData.getFirstChaserData(out chaserData);
        path.getOptimalPointForReachTargetWhitAcceleration(chaserData, path.t0, scope, 0.1f, out results);
        if (results.Count > 0)
        {
            t = results[0] + path.t0;
            point = path.getPositionAtTime(t);
            return true;
        }
        
        return false;
    }
    IEnumerator withAccelerationTest3()
    {
        movementCtrl.DesiredDirection = chaserPublicPlayerData.bodyTransform.forward;
        movementCtrl.ForwardDesiredSpeed = chaserPublicPlayerData.movimentValues.maxForwardSpeed;
        movementCtrl.MinForwardSpeed = 0;
        yield return new WaitForSeconds(time);
        Rigidbody ballRigidbody = MatchComponents.ballRigidbody;
        ballRigidbody.position = MyFunctions.setYToVector3(targetPosition, MatchComponents.ballRadio);
        ballRigidbody.velocity = targetTransform.forward * v0;
        ChaserData chaserData;
        chaserPublicPlayerData.getFirstChaserData(out chaserData);
        while (true)
        {
            if (chaserData.ReachTheTarget)
            {
                Vector3 optimalPoint = chaserData.OptimalPoint;
                Vector3 chaserDirection = MyFunctions.setY0ToVector3(optimalPoint - chaserPublicPlayerData.position);
                movementCtrl.DesiredDirection = chaserDirection;

                movementCtrl.DesiredLookDirection = chaserDirection;
                movementCtrl.ForwardDesiredSpeed = chaserPublicPlayerData.movimentValues.maxForwardSpeed;
                movementCtrl.MinForwardSpeed = 0;
                movementCtrl.TargetPosition = optimalPoint;
                chaserPublicPlayerData.movimentValues.stopOffset = chaserPublicPlayerData.playerComponents.bodyBallRadio;
            }
            
            yield return null;
        }
    }
    IEnumerator withAccelerationTest2()
    {
        /*Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        ballRigidbody.velocity = testKickDirection.forward * force;
        
        yield return new WaitForSeconds(1);*/
        movementCtrl.DesiredDirection = chaserPublicPlayerData.bodyTransform.forward;
        movementCtrl.ForwardDesiredSpeed = chaserPublicPlayerData.movimentValues.maxForwardSpeed;
        movementCtrl.MinForwardSpeed = 0;

        yield return new WaitForSeconds(time);


        Path path = new Path(targetPosition, targetTransform.forward * v0, 0);
        List<float> results;
        scope = chaserPublicPlayerData.playerComponents.scope;
        //scope = 2;
        //path.getOptimalPointForReachTargetWhitAcceleration(chaserPublicPlayerData, -0.0f,scope, 0.01f, out results);
        Rigidbody ballRigidbody = MatchComponents.ballRigidbody;
        ballRigidbody.position = MyFunctions.setYToVector3(targetPosition, MatchComponents.ballRadio);
        ballRigidbody.velocity = targetTransform.forward * v0;
        list = getPathList();
        getTofPathList(list,true, out optimalTime, out optimalPoint2);
        float t3 = optimalTime;
        float t = 0;
        StraightXZDragAndFrictionPath straightXZDragAndFrictionPath = getStraightXZDragAndFrictionPath();
        while (t < t3)
        {
            list = getPathList();
            getTofPathList(list, true, out optimalTime, out optimalPoint);
            getTofPathList(list, false, out optimalTimeWithoutAcceleration, out optimalPointWithoutAcceleration);
            if (true)
            {
                //optimalPoint = MyFunctions.setYToVector3(ballRigidbody.position, 0) + targetTransform.forward * v0 * optimalTime;

                Vector3 targetDirection = (optimalPoint - targetPosition).normalized;
                Vector3 chaserDirection = (optimalPoint - chaserPublicPlayerData.position).normalized;
                movementCtrl.DesiredDirection = chaserDirection;
                movementCtrl.DesiredLookDirection = chaserDirection;
                movementCtrl.ForwardDesiredSpeed = chaserPublicPlayerData.movimentValues.maxForwardSpeed;
                movementCtrl.MinForwardSpeed = 0;
                movementCtrl.TargetPosition = optimalPoint;
                chaserPublicPlayerData.movimentValues.stopOffset = chaserPublicPlayerData.playerComponents.bodyBallRadio;
                float t2 = 0;

                Vector3 calculatedPos = straightXZDragAndFrictionPath.getPositionAtTime(t);
                //float d = Vector3.Distance(calculatedPos, ballRigidbody.position);
                //print("Distance=" + d);
                //targetPosition += targetDirection * v0 * Time.deltaTime;
                /*while (t2 < optimalTime)
                {

                    
                    //chaserTransform.position += chaserDirection * chaserSpeed * Time.deltaTime;
                    targetPosition += targetDirection * v0 * Time.deltaTime;
                    t2 += Time.deltaTime;
                    chaserDirection = (optimalPoint - chaserPublicPlayerData.position).normalized;
                    movementCtrl.ForwardDesiredDirection = chaserDirection;
                    movementCtrl.DesiredLookDirection = chaserDirection;

                }
                float d = Vector3.Distance(chaserPublicPlayerData.position, targetPosition);
                print("Distance=" + d);*/
            }

            yield return null;
            t += Time.deltaTime;
        }
        //print("Fin");
    }

    
    StraightXZDragAndFrictionPath getStraightXZDragAndFrictionPath()
    {
        float radius = MatchComponents.ballComponents.radio;
        Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        Vector3 ballPosition = MatchComponents.ballComponents.transBall.position;
        float drag = ballRigidbody.drag;
        PhysicMaterial ballPhysicsMaterial = MatchComponents.ballComponents.physicMaterial;
        PhysicMaterial footballFieldPhysicMaterial = MatchComponents.footballField.footballFieldPhysicMaterial;
        float bounciness = GetPhysicMaterialCombine.getCombination(ballPhysicsMaterial.bounciness, footballFieldPhysicMaterial.bounciness, GetPhysicMaterialCombine.getPhysicMaterialCombine(ballPhysicsMaterial.bounceCombine, footballFieldPhysicMaterial.bounceCombine));
        float dynamicFriction = GetPhysicMaterialCombine.getCombination(ballPhysicsMaterial.dynamicFriction, footballFieldPhysicMaterial.dynamicFriction, GetPhysicMaterialCombine.getPhysicMaterialCombine(ballPhysicsMaterial.frictionCombine, footballFieldPhysicMaterial.frictionCombine));
        float staticFriction = GetPhysicMaterialCombine.getCombination(ballPhysicsMaterial.staticFriction, footballFieldPhysicMaterial.staticFriction, GetPhysicMaterialCombine.getPhysicMaterialCombine(ballPhysicsMaterial.frictionCombine, footballFieldPhysicMaterial.frictionCombine));
        float friction = staticFriction > dynamicFriction ? staticFriction : dynamicFriction;
        
        StraightXZDragAndFrictionPath straightXZDragAndFrictionPath = new StraightXZDragAndFrictionPath(ballPosition, Vector3.positiveInfinity, ballRigidbody.velocity, ballRigidbody.velocity, 0, Mathf.Infinity, drag, radius, friction, ballRigidbody.mass);
        return straightXZDragAndFrictionPath;
    }
    List<Path> list;
    Vector3 optimalPoint, optimalPoint2,optimalPointWithoutAcceleration;
    float optimalTime, optimalTimeWithoutAcceleration;
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && drawGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(optimalPoint, 0.2f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(optimalPoint2, 0.2f);
            Gizmos.color = Color.gray;
            Gizmos.DrawSphere(optimalPointWithoutAcceleration, 0.2f);
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.normal.textColor = Color.red;
            Handles.color = Color.green;
            string info = "optimalTime=" + optimalTime.ToString();
            Handles.Label(optimalPoint + Vector3.up * 0.5f, info, style);
#endif

            if (list != null)
            {
                int i = 1;
                Gizmos.color = Color.blue;
                foreach (var path in list)
                {
                    Gizmos.DrawSphere(path.Pos0, 0.1f);

#if UNITY_EDITOR
                    style = new GUIStyle();
                    style.fontSize = 14;
                    style.normal.textColor = Color.yellow;
                    Handles.color = Color.green;
                    info = i.ToString();
                    Handles.Label(path.Pos0 + Vector3.up * 0.5f, info, style);
#endif
                    i++;
                }
            }
            
        }
        DrawArrow.ForGizmo(targetTransform.position, targetTransform.forward * 20);
    }











    IEnumerator withAccelerationTest4()
    {
        /*Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        ballRigidbody.velocity = testKickDirection.forward * force;
        
        yield return new WaitForSeconds(1);*/
        if (true)
        {
            movementCtrl.DesiredDirection = chaserPublicPlayerData.bodyTransform.forward;
            movementCtrl.ForwardDesiredSpeed = chaserPublicPlayerData.movimentValues.maxForwardSpeed;
            movementCtrl.Velocity = chaserPublicPlayerData.bodyTransform.forward * v0;
            movementCtrl.EndForwardSpeed = v0;
            movementCtrl.MinForwardSpeed = 0;
            yield return new WaitForSeconds(0.25f);
        }
        scope = chaserPublicPlayerData.playerComponents.scope;
        //scope = 0;
        //path.getOptimalPointForReachTargetWhitAcceleration(chaserPublicPlayerData, -0.0f,scope, 0.01f, out results);
        Rigidbody ballRigidbody = MatchComponents.ballRigidbody;
        MatchComponents.ballComponents.transBall.position = MyFunctions.setYToVector3(targetPosition, MatchComponents.ballRadio);
        //ballRigidbody.position = MyFunctions.setYToVector3(targetPosition, MatchComponents.ballRadio);
        ballRigidbody.velocity = targetTransform.forward * v0;
        //list = getPathList();
        //getTofPathList(list, out optimalTime, out optimalPoint2);
        getTofStraightXZDragAndFrictionPath(out optimalTime, out optimalPoint2);
        float t3 = optimalTime;
        float t = 0;
        StraightXZDragAndFrictionPath straightXZDragAndFrictionPath = getStraightXZDragAndFrictionPath();
        while (t < t3)
        {
            //list = getPathList();
            //getTofPathList(list, out optimalTime, out optimalPoint);
            //
            //t3 = optimalTime;
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
            if (true)
            {
                //optimalPoint = MyFunctions.setYToVector3(ballRigidbody.position, 0) + targetTransform.forward * v0 * optimalTime;
                if (true)
                {
                    getTofStraightXZDragAndFrictionPath(out optimalTime, out optimalPoint);
                }
                else
                {

                    optimalPoint = optimalPoint2;
                }
                Vector3 targetDirection = (optimalPoint - targetPosition).normalized;
                Vector3 chaserDirection = (optimalPoint - chaserPublicPlayerData.position).normalized;
                movementCtrl.DesiredDirection = chaserDirection;
                movementCtrl.DesiredLookDirection = chaserDirection;
                movementCtrl.ForwardDesiredSpeed = chaserPublicPlayerData.movimentValues.maxForwardSpeed;
                movementCtrl.MinForwardSpeed = 0;
                movementCtrl.TargetPosition = optimalPoint;
                chaserPublicPlayerData.movimentValues.stopOffset = scope;
                if (false)
                {
                    yield return new WaitForSeconds(optimalTime);
                    break;
                }
                //Vector3 calculatedPos = straightXZDragAndFrictionPath.getPositionAtTime(t);
                //float d = Vector3.Distance(calculatedPos, ballRigidbody.position);
                //print("Distance=" + d);
                //targetPosition += targetDirection * v0 * Time.deltaTime;
                /*while (t2 < optimalTime)
                {

                    
                    //chaserTransform.position += chaserDirection * chaserSpeed * Time.deltaTime;
                    targetPosition += targetDirection * v0 * Time.deltaTime;
                    t2 += Time.deltaTime;
                    chaserDirection = (optimalPoint - chaserPublicPlayerData.position).normalized;
                    movementCtrl.ForwardDesiredDirection = chaserDirection;
                    movementCtrl.DesiredLookDirection = chaserDirection;

                }*/

            }


        }
        Vector3 ballPosition = MatchComponents.ballComponents.transBall.position;
        float d = Vector3.Distance(chaserPublicPlayerData.position, ballPosition);
        print("Fin Distance = " + d);
    }

}
