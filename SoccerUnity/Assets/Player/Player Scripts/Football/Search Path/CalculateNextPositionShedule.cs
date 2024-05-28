using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;




public class CalculateNextPositionShedule : MonoBehaviour
{
    public class LineupFieldPositionDatas
    {
        public string name;
        public List<PressureFieldPositionDatas> PressureFieldPositionDatas = new List<PressureFieldPositionDatas>();

        public LineupFieldPositionDatas(string name)
        {
            this.name = name;
        }
    }
    public class PressureFieldPositionDatas
    {
        public string name;
        public NativeArray<FieldPositionData> FieldPositionDatas;
        public NativeList<Point2> points;

        public PressureFieldPositionDatas(string name)
        {
            this.name = name;
        }
    }
    public FootballPositionCtrl FootballPositionCtrl;
    List<LineupFieldPositionDatas> lineupFieldPositionDatas=new List<LineupFieldPositionDatas>();
    public NativeArray<TwoPositions> normalizedPositions;
    public NativeArray<float> offsideLinePosYs;
    public NativeArray<float> weightOffsideLines;
    public NativeArray<NextPositionData2> normalNextPosition;
    public int JobSize = 10;
    private void Start()
    {
        LoadParameters();
        LoadPoints();
    }
    void LoadParameters()
    {
        normalizedPositions = new NativeArray<TwoPositions>(JobSize, Allocator.Persistent);
        offsideLinePosYs= new NativeArray<float>(JobSize, Allocator.Persistent);
        weightOffsideLines = new NativeArray<float>(JobSize, Allocator.Persistent);
        normalNextPosition = new NativeArray<NextPositionData2>(JobSize, Allocator.Persistent);
    }
    public void SetCalculateNextPositionParameters(int index,Vector2 normalizedPosition,float offsideLinePosY,float weightOffsideLine)
    {
        Vector2 symetricNormalBallPosition = normalizedPosition;
        symetricNormalBallPosition.x = 1 - normalizedPosition.x;
        normalizedPositions[index] = new TwoPositions(normalizedPosition, symetricNormalBallPosition);
        offsideLinePosYs[index] = offsideLinePosY;
        weightOffsideLines[index] = weightOffsideLine;
    }
    PressureFieldPositionDatas GetFieldPositionsData(string lineupName, string pressureName)
    {
        LineupFieldPositionDatas LineupFieldPositionDatas = lineupFieldPositionDatas.Find(x => x.name == lineupName);
        PressureFieldPositionDatas PressureFieldPositionDatas = LineupFieldPositionDatas.PressureFieldPositionDatas.Find(x => x.name == pressureName);
        return PressureFieldPositionDatas;
    }
    public void SheduleJobs(int jobSize,int playerSize,string lineupName,string pressureName)
    {

        float fieldLenght = MatchComponents.footballField.fieldLenght;
        float fieldWidth = MatchComponents.footballField.fieldWidth;

        PressureFieldPositionDatas pressureFieldPositionDatas = GetFieldPositionsData(lineupName,pressureName);

        CalculateNextPositionJob jobData = new CalculateNextPositionJob();
        jobData.fieldLenght = fieldLenght;
        jobData.fieldWidth = fieldWidth;
        jobData.normalizedPosition = normalizedPositions;
        jobData.offsideLinePosY = offsideLinePosYs;
        jobData.weightOffsideLine = weightOffsideLines;
        jobData.points = pressureFieldPositionDatas.points;
        jobData.FieldPositionDatas = pressureFieldPositionDatas.FieldPositionDatas;
        jobData.normalNextPosition = normalNextPosition;
        jobData.playerSize = playerSize;
        
        JobHandle handle = jobData.Schedule(jobSize, 1);
        handle.Complete();
    }
    void LoadPoints()
    {
        foreach (var LineupFieldPositionData in FootballPositionCtrl.LineupFieldPositionList.LineupFieldPositionDatas)
        {
            LineupFieldPositionDatas lineupFieldPositionData = new LineupFieldPositionDatas(LineupFieldPositionData.name);
            lineupFieldPositionDatas.Add(lineupFieldPositionData);
            
            
            foreach (var PressureFieldPositionDatas in LineupFieldPositionData.PressureFieldPositionDatasList)
            {
                PressureFieldPositionDatas pressureFieldPositionDatas = new PressureFieldPositionDatas(PressureFieldPositionDatas.name);
                lineupFieldPositionData.PressureFieldPositionDatas.Add(pressureFieldPositionDatas);
                NativeList<Point2> Points = new NativeList<Point2>(Allocator.Persistent);
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
                pressureFieldPositionDatas.FieldPositionDatas = FieldPositionDatas;
                pressureFieldPositionDatas.points = Points;
            }
        }
    }
    private void OnDestroy()
    {
        foreach (var lineupFieldPositionData in lineupFieldPositionDatas)
        {
            foreach (var PressureFieldPositionData in lineupFieldPositionData.PressureFieldPositionDatas)
            {
                PressureFieldPositionData.FieldPositionDatas.Dispose();
                PressureFieldPositionData.points.Dispose();
            }
        }
        normalizedPositions.Dispose();
        offsideLinePosYs.Dispose();
        weightOffsideLines.Dispose();
        normalNextPosition.Dispose();
    }
}
