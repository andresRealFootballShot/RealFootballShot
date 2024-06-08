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
        public NativeArray<PlayerPositionType> PlayerPositionTypes;
        public NativeList<Point2> points;
        public NativeList<NextPlayerPosition> NextPlayerPositions;
        public PressureFieldPositionDatas(string name)
        {
            this.name = name;
        }
    }
    public FootballPositionCtrl FootballPositionCtrl;
    List<LineupFieldPositionDatas> lineupFieldPositionDatas=new List<LineupFieldPositionDatas>();
    public NativeArray<Vector2> normalizedPositions;
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
        normalizedPositions = new NativeArray<Vector2>(JobSize, Allocator.Persistent);
        offsideLinePosYs= new NativeArray<float>(JobSize, Allocator.Persistent);
        weightOffsideLines = new NativeArray<float>(JobSize, Allocator.Persistent);
        normalNextPosition = new NativeArray<NextPositionData2>(JobSize, Allocator.Persistent);
    }
    public void SetCalculateNextPositionParameters(int index,Vector2 normalizedPosition,float offsideLinePosY,float weightOffsideLine)
    {
        normalizedPositions[index] = normalizedPosition;
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
        jobData.playerPositionTypes = pressureFieldPositionDatas.PlayerPositionTypes;
        jobData.normalNextPosition = normalNextPosition;
        jobData.NextPlayerPositions = pressureFieldPositionDatas.NextPlayerPositions;
        jobData.playerSize = playerSize;

        JobHandle handle = jobData.Schedule(jobSize, 1);

        handle.Complete();
    }
    private void LateUpdate()
    {
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
                NativeArray<PlayerPositionType> PlayerPositionType = new NativeArray<PlayerPositionType>(PressureFieldPositionDatas.FieldPositionDatas.Count, Allocator.Persistent);
                NativeList<NextPlayerPosition> NextPlayerPositions = new NativeList<NextPlayerPosition>(Allocator.Persistent);
                FieldPositionsData FieldPositionsData = PressureFieldPositionDatas.FieldPositionDatas[0];

                for (int i = 0; i < PressureFieldPositionDatas.FieldPositionDatas.Count; i++)
                {
                    PlayerPositionType[i] = PressureFieldPositionDatas.FieldPositionDatas[i].playerPositionType;

                }
                foreach (var point in FieldPositionsData.points)
                {
                    Points.Add(new Point2(point.point));
                }
                foreach (var item in PressureFieldPositionDatas.FieldPositionDatas)
                {
                    foreach (var point in item.points)
                    {
                        NextPlayerPositions.Add(new NextPlayerPosition(point));
                    }
                }
                pressureFieldPositionDatas.PlayerPositionTypes = PlayerPositionType;
                pressureFieldPositionDatas.points = Points;
                pressureFieldPositionDatas.NextPlayerPositions = NextPlayerPositions;
            }
        }
    }
    private void OnDestroy()
    {
        foreach (var lineupFieldPositionData in lineupFieldPositionDatas)
        {
            foreach (var PressureFieldPositionData in lineupFieldPositionData.PressureFieldPositionDatas)
            {
                PressureFieldPositionData.PlayerPositionTypes.Dispose();
                PressureFieldPositionData.points.Dispose();
                PressureFieldPositionData.NextPlayerPositions.Dispose();
            }
        }
        normalizedPositions.Dispose();
        offsideLinePosYs.Dispose();
        weightOffsideLines.Dispose();
        normalNextPosition.Dispose();
    }
}
