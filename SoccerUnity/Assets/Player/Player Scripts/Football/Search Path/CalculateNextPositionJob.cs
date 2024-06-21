using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

public struct Point2
{
    public Vector2 point;
    public Point2(Vector2 point)
    {
        this.point = point;
    }
}
public struct NextPlayerPosition
{
    public bool enabled;
    public Vector2 value;
    public float radio, weight;
    public bool useRadio;
    public bool snap;
    public NextPlayerPosition(FieldPositionsData.Point point)
    {
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
    public NativeArray<Vector2> normalizedPosition;
    [ReadOnly]
    public NativeArray<float> offsideLinePosY;
    [ReadOnly]
    public NativeArray<float> weightOffsideLine;
    [ReadOnly]
    public int playerSize;
    [ReadOnly]
    public NativeArray<Point2> points;
    [ReadOnly]
    public NativeArray<NextPlayerPosition> NextPlayerPositions;
    [ReadOnly]
    public NativeArray<PlayerPositionType> playerPositionTypes;

    public NativeArray<NextPositionData2> normalNextPosition;
    public void Execute(int i)
    {
        NextPositionData NextPositionData1 = new NextPositionData();
        NextPositionData NextPositionData2 = new NextPositionData();
        getNextPosition(normalizedPosition[i], ref points, ref NextPlayerPositions,ref NextPositionData1,offsideLinePosY[i], ref playerPositionTypes, weightOffsideLine[i],playerSize);
        Vector2 symetricNormalBallPosition = normalizedPosition[i];
        symetricNormalBallPosition.x = 1 - normalizedPosition[i].x;
        getNextPosition(symetricNormalBallPosition, ref points, ref NextPlayerPositions, ref NextPositionData2, offsideLinePosY[i], ref playerPositionTypes, weightOffsideLine[i], playerSize);
        NextPositionData2 nextPositionData = normalNextPosition[i];
        
        nextPositionData.NextPositionData = NextPositionData1;
        nextPositionData.symetricNextPositionData = NextPositionData2;
        normalNextPosition[i] = nextPositionData;
    }
    public void getNextPosition(Vector2 normalizedPosition,ref NativeArray<Point2> points,ref NativeArray<NextPlayerPosition> NextPlayerPositions,ref NextPositionData NextPositionData, float offsideLinePosY,ref NativeArray<PlayerPositionType> playerPositionTypes, float weightOffsideLine,int playerSize)
    {
        float totalH = 0;
        Vector2 p = getNormalPoint(normalizedPosition);
        int size = points.Length;
        for (int i = 0; i < size; i++)
        {
            //if (!NextPlayerPositions[i].enabled) continue;
            Vector2 dir = p - getNormalPoint(points[i].point);
            //dir = dir.normalized * Mathf.Clamp(NextPlayerPositions[i].radio, 0, dir.magnitude);
            dir = dir.normalized ;
            Vector2 pi = getNormalPoint(points[i].point);
            /*if (NextPlayerPositions[i].useRadio)
            {
                pi += dir;
            }*/
            float hs = 1;
            for (int j = 0; j < size; j++)
            {
                if (i == j) continue;
                //if (!NextPlayerPositions[j].enabled) continue;
                Vector2 dir2 = p - getNormalPoint(points[j].point);
                //dir2 = dir2.normalized * Mathf.Clamp(NextPlayerPositions[j].radio, 0, dir2.magnitude);
                dir2 = dir2.normalized;
                Vector2 pj = getNormalPoint(points[j].point);
                /*if (NextPlayerPositions[j].useRadio)
                {
                    pj += dir2;
                }*/
                float p1 = Vector2.Dot(p - pi, pj - pi);
                float p2 = Vector2.Distance(pi, pj);
                //float h_2 = Mathf.Clamp01(1 - (p1 / (p2 * p2)) * NextPlayerPositions[j].weight) * NextPlayerPositions[i].weight;
                float h_2 = Mathf.Clamp01(1 - (p1 / (p2 * p2)));

                if (h_2 < hs) hs = h_2;

            }
            totalH += hs;
            for (int k = 0; k < playerSize; k++)
            {
                NextPlayerPosition nextPlayerPosition = NextPlayerPositions[i + k*size];
                Vector2 value = NextPositionData.Get(k);
                value += getValue(hs, ref nextPlayerPosition, normalizedPosition);
                NextPositionData.Set(k, value);
            } 
        }
        for (int k = 0; k < playerSize; k++)
        {
            Vector2 value = NextPositionData.Get(k);
            value /= totalH;
           
            if (isDefensePlayer(playerPositionTypes[k]))
            {
                value.y = Mathf.Lerp(value.y, offsideLinePosY, weightOffsideLine);
            }
            else
            {
                value.y = Mathf.Lerp(value.y, Mathf.Clamp(value.y, offsideLinePosY, 1), weightOffsideLine);
            }
            NextPositionData.Set(k, value);
        }
        
        //value = Vector2.zero;
        return;
    }
    Vector2 getNormalPoint(Vector2 p)
    {
        p.y = p.y * fieldLenght / fieldWidth;
        return p;
    }
    Vector2 getValue(float weight,ref NextPlayerPosition NextPlayerPosition, Vector2 p)
    {
        Vector2 result = Vector2.zero;
        if (NextPlayerPosition.snap)
        {
            result += p * weight;
        }
        else
        {
            result += NextPlayerPosition.value * weight;

        }
        
        return result;
    }
    bool isDefensePlayer(PlayerPositionType playerPositionType)
    {
        return playerPositionType==PlayerPositionType.CenterBack || playerPositionType == PlayerPositionType.LateralBack;
    }
}
