using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldTriangleSpace;
using UnityEditor;

public class FootballPointWeightCalculator : MonoBehaviour
{
    public Transform goalStick1Transform, goalStick2Transform,goalCenterTransform;
    public List<PublicPlayerData> partners;
    public List<PublicPlayerData> rivals;
    public StraightPass straightPass;
    public ParabolicPass parabolicPass;
    public ChaserDataCalculationParameters ChaserDataCalculationParameters;
    public Transform pointTransform;
    public float angleMagnitude=1;
    public float maxDistance=1;
    public float distanceMagnitude=1;
    public AnimationCurve distanceAnimationCurve;
    Vector3 goalStick1position { get =>MyFunctions.setY0ToVector3( goalStick1Transform.position); }
    Vector3 goalStick2position { get => MyFunctions.setY0ToVector3(goalStick2Transform.position); }
    Vector3 ballPosition { get => MatchComponents.ballComponents.position; }
    float ballRadio { get => MatchComponents.ballComponents.radio; }
    public bool debug;
    void Start()
    {
        
    }
    private void OnDrawGizmos()
    {
        if (Application.isPlaying&&enabled&&debug)
        {
            Point point = new Point(pointTransform.position, "point");
            Vector3 goalCenterDir = getGoalCenterDir(point);
            DrawArrow.ForGizmo(point.position, goalCenterDir);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(point.position, 0.1f);
            Vector3 dir1 = goalStick1position - point.position;
            Vector3 dir2 = goalStick2position - point.position;
            DrawArrow.ForGizmo(point.position, dir1,Color.blue);
            DrawArrow.ForGizmo(point.position, dir2,Color.blue);

            float pointWeight = calculatePointWeight(point);

        }
    }
    float getGoalAngle(Point point)
    {
        Vector3 dir1 = goalStick1position - point.position;
        Vector3 dir2 = goalStick2position - point.position;
        return Vector3.Angle(dir1, dir2);
    }
    Vector3 getGoalCenterDir(Point point)
    {
        Vector3 pointPosition = MyFunctions.setY0ToVector3(point.position);
        Vector3 dir1 = goalStick1position - pointPosition;
        Vector3 dir2 = goalStick2position - pointPosition;
        float a = Vector3.Angle(dir1, dir2);
        float b = Mathf.Sign(Vector3.Cross(dir1, dir2).y);
        Vector3 dir = Quaternion.AngleAxis( a/2, Vector3.up * b) * dir1;
        return dir;
    }
    float getRivalsCoverAdjust_Angle(Point point,float angleGoal)
    {
        float coverAngle = 0;
        Vector3 goalCenterDir = getGoalCenterDir(point);
        foreach (var rival in rivals)
        {

            Vector3 dir = MyFunctions.setY0ToVector3(rival.position) - MyFunctions.setY0ToVector3(point.position);
            
            float radio = rival.bodyRadio + ballRadio;
            float a = Mathf.Atan2(radio, dir.magnitude) * Mathf.Rad2Deg;
            /*Vector3 bodyRadioDir = Quaternion.AngleAxis(a, Vector3.up) * dir;
            Vector3 bodyRadioDir2 = Quaternion.AngleAxis(a, -Vector3.up) * dir;
            DrawArrow.ForGizmo(point.position, bodyRadioDir, Color.cyan);
            DrawArrow.ForGizmo(point.position, bodyRadioDir2, Color.cyan);*/
            float a2 = a * 2;
            float b2 = angleGoal;
            float b = b2 / 2;
            float c = Vector3.Angle(dir, goalCenterDir);
            float max = a2 >= b2 ? a2 : b2;
            float d = max + Mathf.Clamp(c - Mathf.Abs(a-b),0,Mathf.Infinity);
            float x = Mathf.Clamp(a2 - Mathf.Clamp(d - b2,0,Mathf.Infinity),0,Mathf.Infinity);
            coverAngle+= x;
        }
        return coverAngle;
    }



    float calculatePointWeight(Point point)
    {
        float angleGoal = getGoalAngle(point);
        float distance = Vector3.Distance(point.position, goalCenterTransform.position);
        float coveredAngle = getRivalsCoverAdjust_Angle(point, angleGoal);
        float angleWeight= Mathf.Clamp01(1-((angleGoal - coveredAngle)/180));
        angleWeight *= angleMagnitude;
        float weight = angleWeight;
        float distanceWeight = Mathf.Clamp01((distance / maxDistance));
        distanceWeight = distanceAnimationCurve.Evaluate(distanceWeight)*distanceMagnitude;
        weight += distanceWeight;
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.yellow;
        string text = "weight=" + weight.ToString("f2")+ " distance=" + distance.ToString("f2")+ " distanceWeight="+ distanceWeight.ToString("f2") + " distanceCurveWeight=" + distanceAnimationCurve.Evaluate(Mathf.Clamp01((distance / maxDistance))).ToString("f2") + " angle=" + angleGoal.ToString("f2") + " angleWeight=" + angleWeight.ToString("f2");
        Handles.Label(point.position + Vector3.up * 0.5f, text, style);
#endif
        return weight;
    }
    
}
