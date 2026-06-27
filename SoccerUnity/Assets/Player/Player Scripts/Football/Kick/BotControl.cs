using UnityEditor;
using UnityEngine;
using CullPositionPoint;
using DOTS_ChaserDataCalculation;
using FieldTriangleV2;
using TMPro;
public class BotControl : PlayerComponent
{
    
    public bool CheckBallControl(Brains brains,Vector3 targetPosition,float precisionRadio)
    {
        return false;
        LonelyPointElement2 lonelyPointElement = brains.GetReachableLonelyPoint(0);
        
        PublicPlayerData passer = brains.passerPublicPlayerData;
        if (!passer.IsBot||passer.IsGoalkeeper||!passer.kickAvailable) return false;
        if (passer.BotKick.ReachBall()&&CheckBallControl(targetPosition,passer,ballPosition,precisionRadio,out Vector3 kickVelocity))
        {
            //EditorApplication.isPaused = true;
            passer.BotKick.Kick(kickVelocity);
            return true;
        }
        return false;
    }
    public static bool CheckBallControl(Vector3 targetPosition,PublicPlayerData passer,Vector3 ballPosition,float precisionRadio,out Vector3 result)
    {


        PlayerSkills playerSkills = passer.playerComponents.playerSkills;
        Vector3 velocity = MatchComponents.ballVelocity;
        Vector3 playerPosition = passer.position;
        Vector3 dir1 = targetPosition - ballPosition;
        dir1.y = 0;
        Vector3 dir2 = ballPosition - playerPosition;
        dir2.y = 0;
        Vector3 pos1 = playerPosition + passer.bodyTransform.forward * passer.playerComponents.ballScope;
        

        Vector3 dir3 = playerPosition - pos1;
        dir3.y = 0;
        Vector3 dir4 = targetPosition - pos1;
        dir4.y = 0;
        Vector3 pos2 = playerPosition - Vector3.Cross(dir3, dir4).y * passer.bodyTransform.right * (passer.playerComponents.bodyBallRadio + 0.1f);
        float controlAngle = Vector3.Angle(dir3, pos2 - pos1);
        Vector3 dir5 = pos2 - pos1;
        dir5.y = 0;
        float angle = Vector3.Angle(dir1, dir2);
        float precisionLerp = precisionRadio / 10;

        float maxVelocity = Mathf.Lerp(playerSkills.MaxVelocityControl, playerSkills.MinVelocityControl, dir1.magnitude / playerSkills.MaxVelocityDistanceControl);
        maxVelocity = Mathf.Lerp(maxVelocity, playerSkills.MaxVelocityControl, precisionLerp);
        
        if ((angle > playerSkills.MaxAngleControl&&true) || MatchComponents.ballSpeed >= maxVelocity)
        {
            Vector3 axis = Vector3.Cross(dir3, dir4).normalized;

            // Si son paralelos
            if (axis == Vector3.zero)
                axis = Vector3.up;
            
            float force = Mathf.Min(ParabolicPassDOTS.ParabolaWithDrag_GetV0(passer.BotKick.kickPeriod,ballPosition, pos2,MatchComponents.ballRigidbody.drag,9.81f).magnitude,5);
            // Rota a exactamente 60° hacia el lado donde está b
             result = dir5.normalized * force*0.7f;
            //if (ballPosition.y > 0.7f) result.y = -1;
            
            return true;
        }
        result = Vector3.zero;
        return false;
    }
    public static float GetMaxVelocityControl(Vector3 targetPosition,Vector3 ballPosition, PlayerSkills playerSkills,float precisionRadio)
    {
        Vector3 dir1 = targetPosition - ballPosition;
        dir1.y = 0;
        float maxVelocity = Mathf.Lerp(playerSkills.MaxVelocityControl, playerSkills.MinVelocityControl, dir1.magnitude / playerSkills.MaxVelocityDistanceControl);
        float precisionLerp = precisionRadio / 10;
        maxVelocity = Mathf.Lerp(maxVelocity, playerSkills.MaxVelocityControl, precisionLerp);
        return maxVelocity;
    }
    public static bool CheckBallControl(ShotCandidate shotCandidate)
    {
        Vector3 targetPosition = shotCandidate.target;
        Vector3 ballPosition = shotCandidate.ballPos;

        Vector3 playerPosition = shotCandidate.passerPos;
        Vector3 dir1 = targetPosition - ballPosition;
        dir1.y = 0;
        Vector3 dir2 = ballPosition - playerPosition;
        dir2.y = 0;


        float angle = Vector3.Angle(dir1, dir2);


        if (angle > shotCandidate.maxAngleControl || shotCandidate.ballSpeed >= shotCandidate.maxVelocityControl)
        {
            return true;
        }
        return false;
    }
}