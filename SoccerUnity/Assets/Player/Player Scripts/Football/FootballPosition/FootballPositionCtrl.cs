using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public enum PlayerPositionType
{
    Forward, CenterMidfield, EdgeMidfield, CenterBack, LateralBack
}
[System.Serializable]
public class FieldPositionsData
{
    public enum HorizontalPositionType
    {
        Right,Left
    }
    
    [System.Serializable]
    public class Point2
    {

        public bool enabled;
        public string info;
        public Vector2 point;
        public Vector2 value;
        public float radio,weight=1;
        public bool useRadio=false;
        public bool snap;
        public Point2(Vector2 point,Vector2 value)
        {
            this.point = point;
            enabled = true;
            this.value = value;
            info = ";";
        }
        public override string ToString()
        {
            return info;
        }
        public Point2 Clone()
        {
            Point2 clone = new Point2(point,value);
            clone.enabled = enabled;
            clone.info = info;
            clone.radio = radio;
            clone.useRadio = useRadio;
            clone.snap = snap;
            return clone;
        }
    }
    public PlayerPositionType playerPositionType;
    public List<Point2> points;
    
    [HideInInspector] public FieldPositionsData.Point2 selectedPoint;

    public FieldPositionsData Clone()
    {
        FieldPositionsData clone = new FieldPositionsData();
        clone.points = new List<Point2>();
        foreach (var point in points)
        {
            clone.points.Add(point.Clone());
        }
        return clone;
    }
}
[System.Serializable]
public class PressureFieldPositionDatas
{
    public string name = "Default";
    public List<FieldPositionsData> FieldPositionDatas = new List<FieldPositionsData>();
    public List<OffsideLine> offsideLines=new List<OffsideLine>();
    public List<OffsideStop> offsideStops = new List<OffsideStop>();
}
[System.Serializable]
public class LineupFieldPositionDatas
{
    public string name = "Default";
    public List<PressureFieldPositionDatas> PressureFieldPositionDatasList = new List<PressureFieldPositionDatas>();
}
[System.Serializable]
public class LineupFieldPositionDatasList
{
    public List<LineupFieldPositionDatas> LineupFieldPositionDatas = new List<LineupFieldPositionDatas>();
}
[System.Serializable]
public class OffsideLine
{
    public float yPos, yValue;
    public bool stop,enabled=true;

    public OffsideLine(float yPos, float yValue)
    {
        this.yPos = yPos;
        this.yValue = yValue;
    }
}
[System.Serializable]
public class OffsideStop
{
    public Vector2 point;
    public float radio,lerpRadio;
    public bool enabled=true;

    public OffsideStop(Vector2 point, float radio)
    {
        this.point = point;
        this.radio = radio;
        this.lerpRadio = radio;
    }
}
public class FootballPositionCtrl : MonoBehaviour
{
    public bool debug;
    public bool debugRadios,debugAllPositions,debugAllValues, debugSymmetricalPositions,debugText=true,debugOnlyPlayerPositions;
    //Vector3 ballPosition { get => MatchComponents.ballPosition; }
    public SideOfField mySideOfField,rivalSideOfField;
    public SetupFootballField setupFootballField;
    public LineupFieldPositionDatasList LineupFieldPositionList;
    public float buttonSize = 0.1f;
    public float fieldLenght { get => MatchComponents.footballField.fieldLenght; }
    public float fieldWidth{ get => MatchComponents.footballField.fieldWidth; }
    public Vector2 newPointPosition = new Vector2(0.5f,0.5f);
    public FieldPositionsData.HorizontalPositionType horizontalPositionType;
    Vector2 normalizedPosition,normalizedBallPosition;
    float horizontalAdjustInfo;
    [HideInInspector]
    public Vector2 normailizedBallPosition;
    [HideInInspector]
    public FieldPositionsData selectedFieldPositionParameters;
    [HideInInspector]
    public FieldPositionsData.Point2 selectedPoint;
    [HideInInspector] public PlayerPositionType playerPositionType;
    [HideInInspector] public string lineupName = "Default";
    [HideInInspector] public string pressureName = "Default";
    public int playerSize = 5;
    /* void Start()
    {
       string text = File.ReadAllText(Application.dataPath + "/Player/Player Scripts/Football/FootballPosition/FieldPoints.json");
        FieldPositionParameters fieldPositionParameters = JsonUtility.FromJson<FieldPositionParameters>(text);
        FieldPositionList fieldPositionList = new FieldPositionList();
        fieldPositionList.FieldPositionParametersList.Add(fieldPositionParameters);
        LineupFieldPositionList.FieldPositionList.Add(fieldPositionList);
}*/
    public void Load()
    {
        mySideOfField.loadPlanes();
        rivalSideOfField.loadPlanes();
        List<SideOfField> sideOfFields = new List<SideOfField>();
        sideOfFields.Add(mySideOfField);
        sideOfFields.Add(rivalSideOfField);
        setupFootballField.loadFieldDimensions(sideOfFields);
    }
    public void AddOffsideLine(PressureFieldPositionDatas pressureFieldPositionDatas,OffsideLine offsideLine)
    {
        int index = 0;
        foreach (var offsideLine1 in pressureFieldPositionDatas.offsideLines)
        {
            if (offsideLine.yPos < offsideLine1.yPos)
            {
                break;
            }
            index++;
        }
        pressureFieldPositionDatas.offsideLines.Insert(index, offsideLine);
    }
    public float GetOffsideLineGetValue(PressureFieldPositionDatas pressureFieldPositionDatas,Vector2 normalizedPosition,out float weight)
    {
        float d1 = 1, d2 = 1;
        int index1 = -1, index2 = -1;
        int i = 0;
        
        foreach (var offsideLine in pressureFieldPositionDatas.offsideLines)
        {
            if (!offsideLine.enabled) continue;
            float distance = offsideLine.yPos - normalizedPosition.y;
            if (distance < 0)
            {
                if (Mathf.Abs(distance) < Mathf.Abs(d1))
                {
                    d1 = distance;
                    index1 = i;
                }
            }else if (distance > 0)
            {
                if (Mathf.Abs(distance) < Mathf.Abs(d2))
                {
                    d2 = distance;
                    index2 = i;
                }
            }
            else
            {
                weight = offsideLine.stop ? 0 : 1;
                return offsideLine.yValue;
            }
            i++;
        }
        if (index1 != -1 && index2 == -1)
        {
            weight = pressureFieldPositionDatas.offsideLines[index1].stop ? 0 : 1;
            return pressureFieldPositionDatas.offsideLines[index1].yValue;
        }else if(index1 == -1 && index2 != -1)
        {
            weight = pressureFieldPositionDatas.offsideLines[index2].stop ? 0 : 1;
            return pressureFieldPositionDatas.offsideLines[index2].yValue;
        }
        else
        {
            float w2 = Mathf.Abs(d1) / (Mathf.Abs(d1) + Mathf.Abs(d2));
            float w1 = 1 - w2;
            if (pressureFieldPositionDatas.offsideLines[index1].stop&& !pressureFieldPositionDatas.offsideLines[index2].stop)
            {
                weight = w2;
                return pressureFieldPositionDatas.offsideLines[index2].yValue;
            }
            else if(!pressureFieldPositionDatas.offsideLines[index1].stop && pressureFieldPositionDatas.offsideLines[index2].stop)
            {
                weight = w1;
                return pressureFieldPositionDatas.offsideLines[index1].yValue;
            }else if(pressureFieldPositionDatas.offsideLines[index1].stop && pressureFieldPositionDatas.offsideLines[index2].stop)
            {
                weight = 0;
                float result = pressureFieldPositionDatas.offsideLines[index1].yValue * w1 + pressureFieldPositionDatas.offsideLines[index2].yValue * w2;
                return result;
            }
            else
            {
                weight = 1;
                float result = pressureFieldPositionDatas.offsideLines[index1].yValue * w1 + pressureFieldPositionDatas.offsideLines[index2].yValue * w2;
                return result;
            }
        }
    }
    
    void getWeightyValue(Vector2 normalizedPosition, List<FieldPositionsData.Point2> points, out Vector2 value)
    {
        //Vector2 normalizedPosition = getNormalizedPosition(position);
        Vector2 a=Vector2.zero;
        float b = 0;
        foreach (var point in points)
        {
            float d = Vector2.Distance(point.point, normalizedPosition);
            //a += point.value / d;
            b += 1 / d;
        }
        value = a / b;
    }
    Vector2 getNormalPoint2(Vector2 p)
    {
        p.y = p.y * fieldLenght / fieldWidth;
        return p;
    }
    bool isDefensePlayer(PlayerPositionType playerPositionType)
    {
        return playerPositionType.Equals(PlayerPositionType.CenterBack) || playerPositionType.Equals(PlayerPositionType.LateralBack);
    }
    public void getWeightyValue4(Vector2 normalizedPosition, List<FieldPositionsData.Point2> points,float offsideLinePosY, PlayerPositionType playerPositionType,float weightOffsideLine, out Vector2 value)
    {
        float totalH = 0;
        float[] hs = new float[points.Count];

        float[] weights = new float[points.Count];
        Vector2 p = getNormalPoint2(normalizedPosition);
        for (int i = 0; i < points.Count; i++)
        {
            if (!points[i].enabled) continue;
            Vector2 dir = p - getNormalPoint2(points[i].point);
            //dir.y *= fieldWidth / fieldLenght;
            dir = dir.normalized * Mathf.Clamp(points[i].radio, 0, dir.magnitude);
            //Vector2 pi = points[i].point + dir;
            Vector2 pi = getNormalPoint2(points[i].point);
            if (points[i].useRadio)
            {
                pi += dir;
            }
            
            //hs[i] = Mathf.Infinity;
            hs[i] = 1;
            for (int j = 0; j < points.Count; j++)
            {
                if (i == j) continue;
                if (!points[j].enabled) continue;
                Vector2 dir2 = p - getNormalPoint2(points[j].point);
                //dir2.y *= fieldWidth / fieldLenght;
                dir2 = dir2.normalized * Mathf.Clamp(points[j].radio, 0, dir2.magnitude);
                Vector2 pj = getNormalPoint2(points[j].point);
                if (points[j].useRadio)
                {
                    pj += dir2;
                }
                float p1 = Vector2.Dot(p - pi, pj - pi);
                float p2 = Vector2.Distance(pi, pj);
                float h_2 = Mathf.Clamp01(1 - (p1 / (p2 * p2))* points[j].weight) * points[i].weight;

                if (h_2 < hs[i]) hs[i] = h_2;

            }
            totalH += hs[i];
        }
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = hs[i] / totalH;
        }
        value = getValue(weights, points, normalizedPosition);
        if (isDefensePlayer(playerPositionType))
        {
            value.y = Mathf.Lerp(value.y,offsideLinePosY,weightOffsideLine);
        }
        else
        {
            value.y = Mathf.Lerp(value.y, Mathf.Clamp(value.y, offsideLinePosY,1), weightOffsideLine);
        }
        //value = Vector2.zero;
        return;
    }
   
    Vector2 getValue(float[] weights, List<FieldPositionsData.Point2> points,Vector2 p)
    {
        Vector2 result = Vector2.zero;
        float totalweight = 0;
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].snap)
            {
            result += p * weights[i];
            }
            else
            {
            result += points[i].value * weights[i];

            }
            totalweight += weights[i];
            
            /*GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.red;
            Handles.Label(getGlobalPosition(points[i].point)+Vector3.up*2, "weight= " + weights[i].ToString("f3"), style);*/
        }
        return result;
    }
    public Vector2 getNormalizedPosition(FieldPositionsData.HorizontalPositionType horizontalPositionType, Vector3 position, SideOfField sideOfField)
    {
        float verticalBallDistance = sideOfField.backPlane.GetDistanceToPoint(position) / fieldLenght;
        verticalBallDistance = Mathf.Clamp01(verticalBallDistance);
        float horizontalBallDistance;
        if (horizontalPositionType.Equals(FieldPositionsData.HorizontalPositionType.Right))
        {

            horizontalBallDistance = sideOfField.rightPlane.GetDistanceToPoint(position) / fieldWidth;
        }
        else
        {
            horizontalBallDistance = sideOfField.leftPlane.GetDistanceToPoint(position) / fieldWidth;
        }
        horizontalBallDistance = Mathf.Clamp01(horizontalBallDistance);

        normalizedBallPosition = new Vector2(horizontalBallDistance, verticalBallDistance);
        return normalizedBallPosition;
    }
    public Vector2 getNormalizedPosition(FieldPositionsData.HorizontalPositionType horizontalPositionType, Vector3 position)
    {
        float verticalBallDistance = mySideOfField.backPlane.GetDistanceToPoint(position)/fieldLenght;
        verticalBallDistance = Mathf.Clamp01(verticalBallDistance);
        float horizontalBallDistance;
        if (horizontalPositionType.Equals(FieldPositionsData.HorizontalPositionType.Right)){

            horizontalBallDistance = mySideOfField.rightPlane.GetDistanceToPoint(position)/fieldWidth;
        }
        else
        {
            horizontalBallDistance = mySideOfField.leftPlane.GetDistanceToPoint(position) / fieldWidth;
        }
        horizontalBallDistance = Mathf.Clamp01(horizontalBallDistance);

        normalizedBallPosition = new Vector2(horizontalBallDistance, verticalBallDistance);
        return normalizedBallPosition;
    }
    public Vector3 getGlobalPosition(FieldPositionsData.HorizontalPositionType horizontalPositionType, Vector2 normalizedPosition, SideOfField sideOfField)
    {
        Vector3 globalPosition = sideOfField.backTransform.TransformPoint(Vector3.forward * normalizedPosition.y * fieldLenght);
        Vector3 globalHorizontalPosition;
        if (horizontalPositionType.Equals(FieldPositionsData.HorizontalPositionType.Right))
        {
            globalHorizontalPosition = sideOfField.rightTransform.TransformDirection(Vector3.forward * normalizedPosition.x * fieldWidth - Vector3.forward * (fieldWidth / 2));

            //Debug.DrawRay(mySideOfField.rightTransform.position, globalHorizontalPosition, Color.blue);
        }
        else
        {
            globalHorizontalPosition = sideOfField.leftTransform.TransformDirection(Vector3.forward * normalizedPosition.x * fieldWidth - Vector3.forward * (fieldWidth / 2));
            //Debug.DrawRay(mySideOfField.leftTransform.position, globalHorizontalPosition, Color.blue);
        }
        globalPosition += globalHorizontalPosition;
        //Debug.DrawLine(mySideOfField.backTransform.position, globalPosition,Color.red);
        return globalPosition;
    }
    public Vector3 getGlobalPosition(FieldPositionsData.HorizontalPositionType horizontalPositionType, Vector2 normalizedPosition)
    {
        Vector3 globalPosition= mySideOfField.backTransform.TransformPoint(Vector3.forward*normalizedPosition.y*fieldLenght);
        Vector3 globalHorizontalPosition;
        if (horizontalPositionType.Equals(FieldPositionsData.HorizontalPositionType.Right))
        {
            globalHorizontalPosition = mySideOfField.rightTransform.TransformDirection(Vector3.forward * normalizedPosition.x * fieldWidth - Vector3.forward * (fieldWidth/2));

            //Debug.DrawRay(mySideOfField.rightTransform.position, globalHorizontalPosition, Color.blue);
        }
        else
        {
            globalHorizontalPosition = mySideOfField.leftTransform.TransformDirection(Vector3.forward * normalizedPosition.x * fieldWidth - Vector3.forward * (fieldWidth / 2));
            //Debug.DrawRay(mySideOfField.leftTransform.position, globalHorizontalPosition, Color.blue);
        }
        globalPosition += globalHorizontalPosition;
        //Debug.DrawLine(mySideOfField.backTransform.position, globalPosition,Color.red);
        return globalPosition;
    }
    public bool GetSelectedFieldPositionParameters(out FieldPositionsData fieldPositionDatas)
    {
        //t.selectedFieldPositionParameters = null;
        fieldPositionDatas = null;
        LineupFieldPositionDatas LineupFieldPositionDatas = LineupFieldPositionList.LineupFieldPositionDatas.Find(x => x.name.Equals(lineupName));
        if (LineupFieldPositionDatas == null) return false;
        
        PressureFieldPositionDatas fieldPositionList = LineupFieldPositionDatas.PressureFieldPositionDatasList.Find(x => x.name.Equals(pressureName));
        if (fieldPositionList == null) return false;
        fieldPositionDatas = fieldPositionList.FieldPositionDatas.Find(x => x.playerPositionType.Equals(playerPositionType));

        return fieldPositionDatas != null;
    }
    public bool getCurrentPressureFieldPositions(out PressureFieldPositionDatas lineupFieldPositionData)
    {
        lineupFieldPositionData = null;
        LineupFieldPositionDatas LineupFieldPositionDatas = LineupFieldPositionList.LineupFieldPositionDatas.Find(x => x.name.Equals(lineupName));
        if (LineupFieldPositionDatas == null) return false;
        lineupFieldPositionData = LineupFieldPositionDatas.PressureFieldPositionDatasList.Find(x => x.name.Equals(lineupName));
        if (lineupFieldPositionData == null) return false;
        return true;
    }
    public bool getCurrentLineup(out LineupFieldPositionDatas lineupFieldPositionData)
    {
        lineupFieldPositionData = null;
        LineupFieldPositionDatas LineupFieldPositionDatas = LineupFieldPositionList.LineupFieldPositionDatas.Find(x => x.name.Equals(lineupName));
        return LineupFieldPositionDatas != null;
    }
    public FieldPositionsData.HorizontalPositionType getOtherHorizontalPositionType(FieldPositionsData.HorizontalPositionType horizontalPositionType)
    {
        return horizontalPositionType == FieldPositionsData.HorizontalPositionType.Left ? FieldPositionsData.HorizontalPositionType.Right : FieldPositionsData.HorizontalPositionType.Left;
    }
    /*
   public void getWeightyValue3(Vector2 p, List<FieldPositionParameters.Point> points, out Vector2 value)
   {
       float totalH = 0;
       float[] hs = new float[points.Count];

       float[] weights = new float[points.Count];
       for (int i = 0; i < points.Count; i++)
       {
           if (!points[i].enabled) continue;
           Vector2 pi = points[i].point;
           hs[i] = Mathf.Infinity;
           for (int j = 0; j < points.Count; j++)
           {
               if (i == j) continue;
               Vector2 pj = points[j].point;
               float p1 = Vector2.Dot(p-pi, pj-pi);
               float p2 = Vector2.Distance(pi,pj);
               float h_2 = Mathf.Clamp01(1 - (p1 / (p2 * p2)));
               if (h_2 < hs[i]) hs[i] = h_2;

           }
           totalH += hs[i];
       }
       for (int i = 0; i < weights.Length; i++)
       {
           weights[i] = hs[i] / totalH;
       }
       value = getValue(weights, points,p);
       //value = Vector2.zero;
       return;
   }
   void getWeightyValue2(Vector2 normalizedPosition, List<FieldPositionParameters.Point> points, out Vector2 value)
   {
       float total_sqrd_distance = 0;
       float total_angular_distance = 0;
       float[] weights = new float[points.Count];
       float[] sqrd_distances = new float[points.Count];
       float[] angular_distances = new float[points.Count];
       int i = 0;
       foreach (var point in points)
       {
           float sqr_distance = (normalizedPosition - point.point).sqrMagnitude;
           if (sqr_distance > 0)
           {
               //float angular_distance = -(Mathf.Clamp(Vector2.Dot(normalizedPosition.normalized, point.point.normalized), -1, 1)-1) * 0.5f;
               float angular_distance = -(Mathf.Clamp(Vector2.Dot(normalizedPosition.normalized, point.point.normalized), -1, 1) - 1) * 0.5f;
               //float angular_distance = Vector2.Angle(normalizedPosition, point.point);
               total_sqrd_distance += 1 / sqr_distance;
               if (angular_distance > 0) total_angular_distance += 1 / angular_distance;
               sqrd_distances[i]=sqr_distance;
               angular_distances[i] = angular_distance;
           }
           else
           {
               weights[i] = 1;
               value = getValue(weights, points, normalizedPosition);
               return;
           }
           i++;
       }
       for (int j = 0; j < points.Count; j++)
       {
           float sqrd_distance = total_sqrd_distance * sqrd_distances[j];
           float angular_distance = total_angular_distance * angular_distances[j];
           if(sqrd_distance>0 && angular_distance > 0)
           {
               //weights[j] = (1 / sqrd_distance) * 0.5f + (1 / angular_distance) * 0.5f;
               weights[j] = (1 / sqrd_distance) * 0.5f + (1 / angular_distance) * 0.5f;
           }else if (sqrd_distance > 0)
           {
               //weights[j] = (1 / sqrd_distance) * 0.5f + 0.5f;
               weights[j] = (1 / sqrd_distance) * 0.5f + 0.5f;
           }
           else
           {
               weights[j] = 0;
           }
       }
       value = getValue(weights, points, normalizedPosition);
       return;
   }*/
}
