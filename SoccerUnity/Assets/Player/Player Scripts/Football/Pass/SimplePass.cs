using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePass : Pass
{
    public string info;
    public AnimationCurve distanceCurve;
    public float maxDistance = 40;
    public float minXAngle,maxXAngle;
    public float minForce, maxForce;
    public float maxYAngle;

    Vector3 ballPosition { get => MatchComponents.ballComponents.transBall.position; }
    
    public void randomPass(Vector3 direction,string playerID)
    {
        Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        Vector3 velocity = getRandomPassVelocity(direction);
        KickEventArgs args = new KickEventArgs(velocity, ballRigidbody.velocity, ballRigidbody.angularVelocity,ballRigidbody.position, playerID);
        MatchComponents.kickNotifier.notifyAddForce(args);
    }
    public Vector3 getRandomPassVelocity(Vector3 direction)
    {
        direction = MyFunctions.setYToVector3(direction, 0);
        Vector3 rightDirection = Vector3.Cross(Vector3.up, direction);
        float xAngle = Random.Range(minXAngle, maxXAngle);
        float yAngle = Random.Range(-maxYAngle, maxYAngle);
        //direction = Quaternion.AngleAxis(-xAngle, MatchComponents.footballField.transform.right) * direction;
        direction = Quaternion.AngleAxis(-xAngle, rightDirection) * direction;
        direction = Quaternion.AngleAxis(yAngle, Vector3.up) * direction;
        float force = Random.Range(minForce, maxForce);
        return direction.normalized * force;
    }
    public void pass(List<PublicPlayerData> enemies, List<PublicPlayerData> partners,PublicPlayerData me)
    {
        //randomPass();
    }
    bool enemyIsForwardOfPartner(PublicPlayerData enemy, PublicPlayerData partner)
    {
        Vector3 point;
        if(MyFunctions.GetClosestPointOnFiniteLine(enemy.position,ballPosition,partner.position,out point))
        {
            float distance = Vector3.Distance(point, enemy.position);
            float distanceBallPartner = Vector3.Distance(ballPosition, partner.position);
            float timeBallPartner = Mathf.Lerp(distanceBallPartner / 50, 0, 2);
            float time = distance / enemy.maxSpeed;
            if(time> timeBallPartner)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        return false;
    }
    void passToPartner(PublicPlayerData partner,PublicPlayerData me)
    {
        float distance = Vector3.Distance(partner.position, ballPosition);
        Vector3 direction = partner.position - ballPosition;
        MatchComponents.ballComponents.rigBall.AddForce(direction.normalized*distanceCurve.Evaluate(distance/maxDistance), ForceMode.VelocityChange);
    }

    public override PassResult getPassResult(PassParameters passData)
    {
        throw new System.NotImplementedException();
    }
}
