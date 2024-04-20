using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PathScopeTest : MonoBehaviour
{
    public Transform chaserTransform, targetTransform;
    public PublicPlayerData chaserPublicPlayerData;
    public MovementCtrl movementCtrl;
    public float v0,chaserSpeed,scope;
    public bool withAcceleration = true;
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
            if (withAcceleration)
            {
                StartCoroutine(withAccelerationTest2());
            }
            else
            {

                StartCoroutine(test());
            }
        }
    }
    
    List<Path> list;
    Vector3 optimalPoint,optimalPoint2;
    float optimalTime;
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && list!=null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(optimalPoint, 0.2f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(optimalPoint2, 0.2f);
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.normal.textColor = Color.red;
            Handles.color = Color.green;
            string info = "optimalTime="+ optimalTime.ToString();
            Handles.Label(optimalPoint + Vector3.up * 0.5f, info, style);
#endif


            int i = 1;
            Gizmos.color = Color.blue;
            foreach (var path in list)
            {
                Gizmos.DrawSphere(path.Pos0,0.2f);

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
    List<Path> getPathList()
    {
        List<Path> list = new List<Path>();
        float t0 = 0;
        float increase = 1f;
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
    bool getTofPathList(List<Path> list,out float t)
    {
        t = 0;
        //scope = chaserPublicPlayerData.playerComponents.playerData.scope;
        int i = 0;
        foreach (var path in list)
        {
            //scope = 0;
            List<float> results;
            print("path " + i);
            ChaserData chaserData;
            chaserPublicPlayerData.getFirstChaserData(out chaserData);
            path.getOptimalPointForReachTargetWhitAcceleration(chaserData, path.t0, scope, 0.05f, out results);
            if (results.Count > 0)
            {
                t = results[0] + path.t0;
                return true;
            }
            i++;
        }
        return false;
    }
    IEnumerator withAccelerationTest2()
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
            yield return new WaitForSeconds(0.5f);
        }
        Path path = new Path(targetPosition, targetTransform.forward * v0, 0);
        List<float> results;
        scope = chaserPublicPlayerData.playerComponents.scope;
        //scope = 2;
        //path.getOptimalPointForReachTargetWhitAcceleration(chaserPublicPlayerData, -0.0f,scope, 0.01f, out results);
        list = getPathList();
        getTofPathList(list, out optimalTime);
        optimalPoint2 = targetPosition + targetTransform.forward * v0 * optimalTime;
        float t3 = optimalTime;
        float t=0;
         Vector3 targetDirection = (optimalPoint2 - targetPosition).normalized;
        while (t<t3)
        {
            
            list = getPathList();
            targetPosition += targetDirection * v0 * Time.deltaTime;
            if (getTofPathList(list, out optimalTime))
            {
                
                optimalPoint = targetPosition + targetTransform.forward * v0 * optimalTime;

               
                
                Vector3 chaserDirection = (optimalPoint - chaserPublicPlayerData.position).normalized;
                movementCtrl.DesiredDirection = chaserDirection;
                movementCtrl.DesiredLookDirection = chaserDirection;
                movementCtrl.ForwardDesiredSpeed = chaserPublicPlayerData.movimentValues.maxForwardSpeed;
                movementCtrl.MinForwardSpeed = 0;
                movementCtrl.TargetPosition = optimalPoint;
                chaserPublicPlayerData.movimentValues.stopOffset = scope;
                float t2 = 0;

               

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
        
    }
    IEnumerator withAccelerationTest()
    {
        /*Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        ballRigidbody.velocity = testKickDirection.forward * force;
        
        yield return new WaitForSeconds(1);*/
        movementCtrl.DesiredDirection = chaserPublicPlayerData.bodyTransform.forward;
        movementCtrl.ForwardDesiredSpeed = chaserPublicPlayerData.movimentValues.maxForwardSpeed;
        movementCtrl.MinForwardSpeed = 0;
        yield return new WaitForSeconds(1);
        Path path = new Path(targetPosition, targetTransform.forward * v0, 0);
        List<float> results;
        scope = chaserPublicPlayerData.playerComponents.scope;
        //scope = 2;
        //path.getOptimalPointForReachTargetWhitAcceleration(chaserPublicPlayerData, -0.0f,scope, 0.01f, out results);
        
        list = getPathList();
        if (getTofPathList(list, out optimalTime))
        {
            optimalPoint = targetPosition + targetTransform.forward * v0 * optimalTime;
            
            Vector3 targetDirection = (optimalPoint - targetPosition).normalized;
            Vector3 chaserDirection = (optimalPoint - chaserPublicPlayerData.position).normalized;
            movementCtrl.DesiredDirection = chaserDirection;
            movementCtrl.DesiredLookDirection = chaserDirection;
            movementCtrl.ForwardDesiredSpeed = chaserPublicPlayerData.movimentValues.maxForwardSpeed;
            movementCtrl.MinForwardSpeed = 0;
            movementCtrl.TargetPosition = optimalPoint;
            chaserPublicPlayerData.movimentValues.stopOffset = scope;
            float t2 = 0;
            while (t2 < optimalTime)
            {

                yield return null;
                //chaserTransform.position += chaserDirection * chaserSpeed * Time.deltaTime;
                targetPosition += targetDirection * v0 * Time.deltaTime;
                t2 += Time.deltaTime;
                chaserDirection = (optimalPoint - chaserPublicPlayerData.position).normalized;
                movementCtrl.DesiredDirection = chaserDirection;
                movementCtrl.DesiredLookDirection = chaserDirection;

            }
            float d = Vector3.Distance(chaserPublicPlayerData.position, targetPosition);
            print("Distance=" + d);
        }
    }
    IEnumerator test()
    {
        /*Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        ballRigidbody.velocity = testKickDirection.forward * force;
        
        yield return new WaitForSeconds(1);*/
        Path path = new Path(targetPosition, targetTransform.forward * v0, 0);
        List<float> results;
        path.getOptimalPointForReachTarget(chaserPosition, chaserSpeed, 0, scope, out results);
        if (results.Count > 0)
        {
            float t = results[0];
            Vector3 optimalPoint = targetPosition + targetTransform.forward * v0 * t;
            Vector3 chaserDirection = (optimalPoint - chaserPosition).normalized;
            Vector3 targetDirection = (optimalPoint - targetPosition).normalized;
            float t2 = 0;
            while (t2 < t)
            {
                
                yield return null;
                chaserTransform.position += chaserDirection * chaserSpeed * Time.deltaTime;
                targetPosition += targetDirection * v0 * Time.deltaTime;
                t2 += Time.deltaTime;
            }
            float d = Vector3.Distance(chaserPosition, targetPosition);
            print("Distance=" + d);
        }
    }
}
