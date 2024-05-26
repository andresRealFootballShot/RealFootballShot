using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;




public class CalculateNextPositionShedule : MonoBehaviour
{
    public class LineupFieldPositionDatas
    {
        public List<PressureFieldPositionDatas> PressureFieldPositionDatas = new List<PressureFieldPositionDatas>();
    }
    public class PressureFieldPositionDatas
    {
        public List<FieldPositionDataList> FieldPositionDataList = new List<FieldPositionDataList>();
        public List<PointsArray> PressurePoints = new List<PointsArray>();
    }
    public class FieldPositionDataList
    {
        public NativeArray<FieldPositionData> FieldPositionDatas;
    }
    public class PointsArray
    {
        public NativeList<Point2> points;
    }
    public FootballPositionCtrl FootballPositionCtrl;
    List<LineupFieldPositionDatas> lineupFieldPositionDatas=new List<LineupFieldPositionDatas>();
    public NativeArray<Vector2> normalizedPositions;
    public NativeArray<float> offsideLinePosYs;
    public NativeArray<float> weightOffsideLines;
    public int JobSize = 10;
    private void Start()
    {
        LoadParameters();
        LoadPoints();
    }
    void LoadParameters()
    {
        normalizedPositions = new NativeArray<Vector2>(JobSize, Allocator.Persistent);
        offsideLinePosYs= new NativeArray<float>(JobSize, Allocator.Persistent);
        weightOffsideLines = new NativeArray<float>(JobSize, Allocator.Persistent);
    }
    public void SetCalculateNextPositionParameters(int index,Vector2 normalizedPosition,float offsideLinePosY,float weightOffsideLine)
    {
        normalizedPositions[index] = normalizedPosition;
        offsideLinePosYs[index] = offsideLinePosY;
        weightOffsideLines[index] = weightOffsideLine;
    }
    void Update()
    {

        CalculateNextPositionJob jobData = new CalculateNextPositionJob();
        
        // Schedule the job
        JobHandle handle = jobData.Schedule(result.Length, 1);
        handle.Complete();
    }
    void LoadPoints()
    {
        foreach (var LineupFieldPositionData in FootballPositionCtrl.LineupFieldPositionList.LineupFieldPositionDatas)
        {
            LineupFieldPositionDatas lineupFieldPositionData = new LineupFieldPositionDatas();
            lineupFieldPositionDatas.Add(lineupFieldPositionData);
            
            
            foreach (var PressureFieldPositionDatas in LineupFieldPositionData.PressureFieldPositionDatasList)
            {
                PressureFieldPositionDatas pressureFieldPositionDatas = new PressureFieldPositionDatas();
                lineupFieldPositionData.PressureFieldPositionDatas.Add(pressureFieldPositionDatas);
                PointsArray PointsArray = new PointsArray();
                pressureFieldPositionDatas.PressurePoints.Add(PointsArray);
                NativeList<Point2> Points = new NativeList<Point2>(Allocator.Persistent);
                FieldPositionDataList fieldPositionDataList = new FieldPositionDataList();
                pressureFieldPositionDatas.FieldPositionDataList.Add(fieldPositionDataList);
                int startIndex = 0;
                NativeArray<FieldPositionData> FieldPositionDatas = new NativeArray<FieldPositionData>(PressureFieldPositionDatas.FieldPositionDatas.Count, Allocator.Persistent);
                
               
                int i = 0;
                foreach (var FieldPositionData in PressureFieldPositionDatas.FieldPositionDatas)
                {
                    FieldPositionData fieldPositionsData = new FieldPositionData(startIndex,startIndex+ FieldPositionData.points.Count,FieldPositionData.playerPositionType);
                    startIndex = startIndex + FieldPositionData.points.Count;
                    FieldPositionDatas[i] = fieldPositionsData;
                    foreach (var point in FieldPositionData.points)
                    {
                        Points.Add(new Point2(point));
                    }
                    i++;
                }
                fieldPositionDataList.FieldPositionDatas = FieldPositionDatas;
                PointsArray.points = Points;
            }
        }
    }
    private void OnDestroy()
    {
        foreach (var lineupFieldPositionData in lineupFieldPositionDatas)
        {
            foreach (var PressureFieldPositionData in lineupFieldPositionData.PressureFieldPositionDatas)
            {
                foreach (var PressurePoint in PressureFieldPositionData.PressurePoints)
                {
                    PressurePoint.points.Dispose();
                }
                foreach (var FieldPositionData in PressureFieldPositionData.FieldPositionDataList)
                {
                    FieldPositionData.FieldPositionDatas.Dispose();
                }
            }
        }
        normalizedPosition.Dispose();
        offsideLinePosY.Dispose();
        weightOffsideLine.Dispose();
    }
}
