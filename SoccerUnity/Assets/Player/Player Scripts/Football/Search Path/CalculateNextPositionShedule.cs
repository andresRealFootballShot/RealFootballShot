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
    public Dictionary<PlayerPositionType, List<TypeFieldPosition.Type>> RightPlayerPosition_TypeFieldPosition = new Dictionary<PlayerPositionType, List<TypeFieldPosition.Type>>() {
             { PlayerPositionType.Forward, new List<TypeFieldPosition.Type>(){ TypeFieldPosition.Type.RightForward,} },
             { PlayerPositionType.CenterBack, new List<TypeFieldPosition.Type>(){ TypeFieldPosition.Type.CentreRightBack,} },
             { PlayerPositionType.LateralBack, new List<TypeFieldPosition.Type>(){ TypeFieldPosition.Type.RighttOutsideDefense,} },
             { PlayerPositionType.CenterMidfield, new List<TypeFieldPosition.Type>(){ TypeFieldPosition.Type.RightCentreMidfield,} },
             { PlayerPositionType.EdgeMidfield, new List<TypeFieldPosition.Type>(){ TypeFieldPosition.Type.RightOutsideMidfield,} },
        };
    public Dictionary<PlayerPositionType, List<TypeFieldPosition.Type>> LeftPlayerPosition_TypeFieldPosition = new Dictionary<PlayerPositionType, List<TypeFieldPosition.Type>>() {
             { PlayerPositionType.Forward, new List<TypeFieldPosition.Type>(){ TypeFieldPosition.Type.LeftForward } },
             { PlayerPositionType.CenterBack, new List<TypeFieldPosition.Type>(){ TypeFieldPosition.Type.CentreLeftBack } },
             { PlayerPositionType.LateralBack, new List<TypeFieldPosition.Type>(){TypeFieldPosition.Type.LeftOutsideDefense } },
             { PlayerPositionType.CenterMidfield, new List<TypeFieldPosition.Type>(){TypeFieldPosition.Type.LeftCentreMidfield } },
             { PlayerPositionType.EdgeMidfield, new List<TypeFieldPosition.Type>(){TypeFieldPosition.Type.LeftOutsideMidfield } },
        };
    public FootballPositionCtrl FootballPositionCtrl;
    List<LineupFieldPositionDatas> lineupFieldPositionDatas=new List<LineupFieldPositionDatas>();
    public NativeArray<Vector2> normalizedPositions;
    public NativeArray<float> offsideLinePosYs;
    public NativeArray<float> weightOffsideLines;
    public NativeArray<NextPositionData2> normalNextPosition;
    public int JobSize = 10;
    public List<PlayerPositionType> playerPositionTypeOrder;
    private void Start()
    {
        //LoadParameters();
        LoadPoints();
    }
    void LoadParameters()
    {
        normalizedPositions = new NativeArray<Vector2>(JobSize, Allocator.Persistent);
        offsideLinePosYs= new NativeArray<float>(JobSize, Allocator.Persistent);
        weightOffsideLines = new NativeArray<float>(JobSize, Allocator.Persistent);
        normalNextPosition = new NativeArray<NextPositionData2>(JobSize, Allocator.Persistent);
    }
    /*public void SetCalculateNextPositionParameters(int index,Vector2 normalizedPosition,float offsideLinePosY,float weightOffsideLine)
    {
        normalizedPositions[index] = normalizedPosition;
        offsideLinePosYs[index] = offsideLinePosY;
        weightOffsideLines[index] = weightOffsideLine;
    }*/
    PressureFieldPositionDatas GetFieldPositionsData(string lineupName, string pressureName)
    {
        LineupFieldPositionDatas LineupFieldPositionDatas = lineupFieldPositionDatas.Find(x => x.name == lineupName);
        PressureFieldPositionDatas PressureFieldPositionDatas = LineupFieldPositionDatas.PressureFieldPositionDatas.Find(x => x.name == pressureName);
        return PressureFieldPositionDatas;
    }
    public void SheduleJobs(List<int> nodes, int nodeSize,int nextNodeSize,SearchPlayData searchPlayData,int playerSize,string lineupName,string pressureName)
    {

        float fieldLenght = MatchComponents.footballField.fieldLenght;
        float fieldWidth = MatchComponents.footballField.fieldWidth;
        NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(nodeSize, Allocator.Temp);
        PressureFieldPositionDatas pressureFieldPositionDatas = GetFieldPositionsData(lineupName,pressureName);
        for (int i = 0; i < nodeSize; i++)
        {
            int node = nodes[i];
            CalculateNextPositionComponents CalculateNextPositionComponents = searchPlayData.GetCalculateNextPositionComponents(node);
            CalculateNextPositionJob jobData = new CalculateNextPositionJob();
            jobData.fieldLenght = fieldLenght;
            jobData.fieldWidth = fieldWidth;
            jobData.normalizedPosition = CalculateNextPositionComponents.normalizedPositions;
            jobData.offsideLinePosY = CalculateNextPositionComponents.offsideLinePosYs;
            jobData.weightOffsideLine = CalculateNextPositionComponents.weightOffsideLines;
            jobData.points = pressureFieldPositionDatas.points;
            jobData.playerPositionTypes = pressureFieldPositionDatas.PlayerPositionTypes;
            jobData.normalNextPosition = CalculateNextPositionComponents.normalNextPosition;
            jobData.NextPlayerPositions = pressureFieldPositionDatas.NextPlayerPositions;
            jobData.playerSize = playerSize;
            jobHandles[i] = jobData.Schedule(nextNodeSize, 1);
        }
        JobHandle.CompleteAll(jobHandles);
    }
    FieldPositionsData getFieldPositionData(PlayerPositionType playerPositionType,List<FieldPositionsData> FieldPositionDatas)
    {
        foreach (var FieldPositionData in FieldPositionDatas)
        {
            if (FieldPositionData.playerPositionType.Equals(playerPositionType))
            {
                return FieldPositionData;
            }
        }
        return FieldPositionDatas[0];
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
                //FieldPositionsData FieldPositionsData = PressureFieldPositionDatas.FieldPositionDatas[0];
                FieldPositionsData FieldPositionsData = PressureFieldPositionDatas.FieldPositionDatas[0];

                for (int i = 0; i < PressureFieldPositionDatas.FieldPositionDatas.Count; i++)
                {
                    //PlayerPositionType[i] = PressureFieldPositionDatas.FieldPositionDatas[i].playerPositionType;
                    PlayerPositionType[i] = playerPositionTypeOrder[i];

                }
                foreach (var point in FieldPositionsData.points)
                {
                    Points.Add(new Point2(point.point));
                }
                foreach (var playerPositionType in playerPositionTypeOrder)
                {
                    FieldPositionsData FieldPositionsData2 = getFieldPositionData(playerPositionType, PressureFieldPositionDatas.FieldPositionDatas);
                    foreach (var point in FieldPositionsData2.points)
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
