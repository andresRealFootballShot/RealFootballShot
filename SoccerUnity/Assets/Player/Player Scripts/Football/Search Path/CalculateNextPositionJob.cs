using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

public struct Point2
{
    public bool enabled;
    public Vector2 point;
    public Vector2 value;
    public float radio, weight;
    public bool useRadio;
    public bool snap;
    public Point2(Vector2 point, Vector2 value)
    {
        this.point = point;
        enabled = true;
        this.value = value;
        weight = 1;
        useRadio = false;
        radio = 0;
        snap = false;
    }
    public Point2(FieldPositionsData.Point point)
    {
        this.point = point.point;
        enabled = point.enabled;
        value = point.value;
        weight = point.weight;
        useRadio = point.useRadio;
        radio = point.radio;
        snap = point.snap;
    }
}
public struct NextPositionData
{
    public Vector2 pos1,pos2,pos3,pos4,pos5;
    public void Set(int index,Vector2 pos)
    {
        switch (index)
        {
            case 0: 
                pos1 = pos;
                break;
            case 1:
                pos2 = pos;
                break;
            case 2:
                pos3 = pos;
                break;
            case 3:
                pos4 = pos;
                break;
            case 4:
                pos5 = pos;
                break;
            default:
                break;
        }
    }
    public Vector2 Get(int index)
    {
        switch (index)
        {
            case 0:
                return pos1;
                ;
            case 1:
                return pos2;
                ;
            case 2:
                return pos3;
                ;
            case 3:
                return pos4;
                ;
            case 4:
                return pos5;
                ;
            default:
                return pos1;
        }
    }
}
public struct FieldPositionData
{
    public int startIndex, endIndex;
    public PlayerPositionType PlayerPositionType;
    public FieldPositionData(int startIndex, int endIndex, PlayerPositionType playerPositionType)
    {
        this.startIndex = startIndex;
        this.endIndex = endIndex;
        PlayerPositionType = playerPositionType;
    }
}
public struct TwoPositions
{
    public Vector2 normalizedPosition1, normalizedPosition2;

    public TwoPositions(Vector2 normalizedPosition1, Vector2 normalizedPosition2)
    {
        this.normalizedPosition1 = normalizedPosition1;
        this.normalizedPosition2 = normalizedPosition2;
    }
}
public struct NextPositionData2
{
    public NextPositionData NextPositionData, symetricNextPositionData;

    public NextPositionData2(NextPositionData NextPositionData, NextPositionData symetricNextPositionData)
    {
        this.NextPositionData = NextPositionData;
        this.symetricNextPositionData = symetricNextPositionData;
    }
}
[BurstCompile]
public struct CalculateNextPositionJob : IJobParallelFor
{
    [ReadOnly]
    public float fieldLenght,fieldWidth;
    [ReadOnly]
    public NativeArray<TwoPositions> normalizedPosition;
    [ReadOnly]
    public NativeArray<float> offsideLinePosY;
    [ReadOnly]
    public NativeArray<float> weightOffsideLine;
    [ReadOnly]
    public float playerSize;
    [ReadOnly]
    public NativeArray<Point2> points;
    [ReadOnly]
    public NativeArray<FieldPositionData> FieldPositionDatas;
    
    public NativeArray<NextPositionData2> normalNextPosition;
    public void Execute(int i)
    {
        for (int j = 0; j < playerSize; j++)
        {
            getNextPosition(normalizedPosition[i].normalizedPosition1, ref points, FieldPositionDatas[j].startIndex, FieldPositionDatas[j].endIndex, offsideLinePosY[i], FieldPositionDatas[j].PlayerPositionType, weightOffsideLine[i], out Vector2 value);
            getNextPosition(normalizedPosition[i].normalizedPosition2, ref points, FieldPositionDatas[j].startIndex, FieldPositionDatas[j].endIndex, offsideLinePosY[i], FieldPositionDatas[j].PlayerPositionType, weightOffsideLine[i], out Vector2 value2);
            NextPositionData2 nextPositionData = normalNextPosition[i];
            nextPositionData.NextPositionData.Set(j,value);
            nextPositionData.symetricNextPositionData.Set(j, value2);
            normalNextPosition[i] = nextPositionData;
        }
    }
    public void getNextPosition(Vector2 normalizedPosition,ref NativeArray<Point2> points,int start,int end, float offsideLinePosY, PlayerPositionType playerPositionType, float weightOffsideLine, out Vector2 value)
    {
        float totalH = 0;
        value = Vector2.zero;
        Vector2 p = getNormalPoint2(normalizedPosition);
        for (int i = start; i < end; i++)
        {
            if (!points[i].enabled) continue;
            Vector2 dir = p - getNormalPoint2(points[i].point);
            dir = dir.normalized * Mathf.Clamp(points[i].radio, 0, dir.magnitude);
            Vector2 pi = getNormalPoint2(points[i].point);
            if (points[i].useRadio)
            {
                pi += dir;
            }
            float hs = 1;
            for (int j = start; j < end; j++)
            {
                if (i == j) continue;
                if (!points[j].enabled) continue;
                Vector2 dir2 = p - getNormalPoint2(points[j].point);
                dir2 = dir2.normalized * Mathf.Clamp(points[j].radio, 0, dir2.magnitude);
                Vector2 pj = getNormalPoint2(points[j].point);
                if (points[j].useRadio)
                {
                    pj += dir2;
                }
                float p1 = Vector2.Dot(p - pi, pj - pi);
                float p2 = Vector2.Distance(pi, pj);
                float h_2 = Mathf.Clamp01(1 - (p1 / (p2 * p2)) * points[j].weight) * points[i].weight;

                if (h_2 < hs) hs = h_2;

            }
            totalH += hs;
            value += getValue(hs, points[i], normalizedPosition);

        }
        value = value / totalH;
        if (isDefensePlayer(playerPositionType))
        {
            value.y = Mathf.Lerp(value.y, offsideLinePosY, weightOffsideLine);
        }
        else
        {
            value.y = Mathf.Lerp(value.y, Mathf.Clamp(value.y, offsideLinePosY, 1), weightOffsideLine);
        }
        //value = Vector2.zero;
        return;
    }
    public void getNextPosition2(Vector2 normalizedPosition, ref NativeArray<Point2> points, int start, int end, float offsideLinePosY, PlayerPositionType playerPositionType, float weightOffsideLine, out Vector2 value)
    {
        float totalH = 0;
        value = Vector2.zero;
        Vector2 p = getNormalPoint2(normalizedPosition);
        float[] weights = new float[end-start];
        float[] hs = new float[end-start];
        for (int i = start; i < end; i++)
        {
            if (!points[i].enabled) continue;
            Vector2 dir = p - getNormalPoint2(points[i].point);
            dir = dir.normalized * Mathf.Clamp(points[i].radio, 0, dir.magnitude);
            Vector2 pi = getNormalPoint2(points[i].point);
            if (points[i].useRadio)
            {
                pi += dir;
            }
            hs[i-start] = 1;
            for (int j = start; j < end; j++)
            {
                if (i == j) continue;
                if (!points[j].enabled) continue;
                Vector2 dir2 = p - getNormalPoint2(points[j].point);
                dir2 = dir2.normalized * Mathf.Clamp(points[j].radio, 0, dir2.magnitude);
                Vector2 pj = getNormalPoint2(points[j].point);
                if (points[j].useRadio)
                {
                    pj += dir2;
                }
                float p1 = Vector2.Dot(p - pi, pj - pi);
                float p2 = Vector2.Distance(pi, pj);
                float h_2 = Mathf.Clamp01(1 - (p1 / (p2 * p2)) * points[j].weight) * points[i].weight;

                if (h_2 < hs[i - start]) hs[i - start] = h_2;

            }
            totalH += hs[i - start];

        }
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = hs[i - start] / totalH;
        }
        value = getValue2(weights, ref points,start,end, normalizedPosition);
        if (isDefensePlayer(playerPositionType))
        {
            value.y = Mathf.Lerp(value.y, offsideLinePosY, weightOffsideLine);
        }
        else
        {
            value.y = Mathf.Lerp(value.y, Mathf.Clamp(value.y, offsideLinePosY, 1), weightOffsideLine);
        }
        //value = Vector2.zero;
        return;
    }
    Vector2 getNormalPoint2(Vector2 p)
    {
        p.y = p.y * fieldLenght / fieldWidth;
        return p;
    }
    Vector2 getValue2(float[] weights, ref NativeArray<Point2> points,int start,int end, Vector2 p)
    {
        Vector2 result = Vector2.zero;
        float totalweight = 0;
        for (int i = start; i < end; i++)
        {
            if (points[i].snap)
            {
                result += p * weights[i - start];
            }
            else
            {
                result += points[i].value * weights[i - start];

            }
            totalweight += weights[i - start];
        }

        return result;
    }
    Vector2 getValue(float weight,Point2 point, Vector2 p)
    {
        Vector2 result = Vector2.zero;
        if (point.snap)
        {
            result += p * weight;
        }
        else
        {
            result += point.value * weight;

        }
        
        return result;
    }
    bool isDefensePlayer(PlayerPositionType playerPositionType)
    {
        return playerPositionType==PlayerPositionType.CenterBack || playerPositionType == PlayerPositionType.LateralBack;
    }
}
