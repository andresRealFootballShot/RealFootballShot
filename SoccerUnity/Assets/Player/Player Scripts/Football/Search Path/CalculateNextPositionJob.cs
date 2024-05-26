using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

public struct TestCalculateNextPosition
{
    public float a;
    public Vector3 b;
}
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
[BurstCompile]
public struct CalculateNextPositionJob : IJobParallelFor
{
    [ReadOnly]
    public float fieldLenght,fieldWidth;
    [ReadOnly]
    public PlayerPositionType playerPositionType;
    [ReadOnly]
    public NativeArray<Vector2> normalizedPosition;
    [ReadOnly]
    public NativeArray<float> offsideLinePosY;
    [ReadOnly]
    public NativeArray<float> weightOffsideLine;
    [ReadOnly]
    public NativeArray<Point2> points;
    [ReadOnly]
    public NativeArray<FieldPositionData> FieldPositionDatas;
    
    public NativeArray<Vector2> normalNextPosition;
    public void Execute(int i)
    {
        getWeightyValue4(normalizedPosition[i], ref points,offsideLinePosY[i],playerPositionType, weightOffsideLine[i], out Vector2 value);
        normalNextPosition[i] = value;
    }
    public void getWeightyValue4(Vector2 normalizedPosition,ref NativeArray<Point2> points, float offsideLinePosY, PlayerPositionType playerPositionType, float weightOffsideLine, out Vector2 value)
    {
        float totalH = 0;
        float[] hs = new float[points.Length];

        float[] weights = new float[points.Length];
        Vector2 p = getNormalPoint2(normalizedPosition);
        for (int i = 0; i < points.Length; i++)
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
            for (int j = 0; j < points.Length; j++)
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
                float h_2 = Mathf.Clamp01(1 - (p1 / (p2 * p2)) * points[j].weight) * points[i].weight;

                if (h_2 < hs[i]) hs[i] = h_2;

            }
            totalH += hs[i];
        }
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = hs[i] / totalH;
        }
        value = getValue(weights,ref points, normalizedPosition);
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
    Vector2 getValue(float[] weights,ref NativeArray<Point2> points, Vector2 p)
    {
        Vector2 result = Vector2.zero;
        float totalweight = 0;
        for (int i = 0; i < points.Length; i++)
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
        }
        return result;
    }
    bool isDefensePlayer(PlayerPositionType playerPositionType)
    {
        return playerPositionType.Equals(PlayerPositionType.CenterBack) || playerPositionType.Equals(PlayerPositionType.LateralBack);
    }
}
