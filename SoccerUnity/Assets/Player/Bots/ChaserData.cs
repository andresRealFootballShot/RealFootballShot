using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaserData
{
    public Vector3 offsetPosition { get; set; }
    public string name { get => info; }
    public Vector3 position { get=>publicPlayerData.bodyTransform.TransformPoint(offsetPosition);}
    Vector3 optimalPoint;
    public float height { get => publicPlayerData.playerData.height; }
    public float bodyBallRadio { get => publicPlayerData.playerComponents.bodyBallRadio; }
    float optimalTime;
    bool reachToTarget;
    int order;
    public string info;
    public PublicPlayerData publicPlayerData;
    public bool useAcceleration { get; set; }
    public GetTimeToReachPoint getTimeToReachPointClass;
    public float getTimeToReachPoint(Vector3 value, float scope)
    {
        return getTimeToReachPointClass.getTimeToReachPointDelegate(value, scope);
    }
    public Vector3 OptimalPoint { get => optimalPoint; set => optimalPoint = value; }
    public float OptimalTime { get => optimalTime; set => optimalTime = value; }
    public float OptimalTargetTime { get; set; }
    public bool ReachTheTarget { get => reachToTarget; set => reachToTarget = value; }
    public int Order { get => order; set => order = value; }
    public bool thereIsClosestPoint { get; set; }
    public Vector3 ClosestPoint { get; set; }
    public float differenceClosestTime { get; set; } = Mathf.Infinity;
    public Vector3 TargetPositionInClosestTime { get; set; }
    public float ClosestPointDistance { get => getClosestPointDistance(); }
    public float ClosestChaserTime { get; set; }
    public float ClosestTargetTime { get; set; }
    public bool thereIsIntercession { get; set; }
    public Vector3 Intercession { get; set; }
    public float TargetIntercessionTime { get; set; }
    public Area closestArea;
    public float scope;
    public ChaserData(Vector3 optimalPoint, float optimalPointTime, int order,float scope, string info)
    {
        OptimalPoint = optimalPoint;
        OptimalTime = optimalPointTime;
        this.info = info;
        ReachTheTarget = false;
        Order = order;
        this.scope = scope;
        createGetTimeToReachPoint();
    }
    public ChaserData()
    {
        OptimalPoint = Vector3.zero;
        OptimalTime = -1;
        ReachTheTarget = false;
        ClosestPoint = Vector3.positiveInfinity;
        ClosestChaserTime = float.PositiveInfinity;
        thereIsIntercession = false;
        Intercession = Vector3.zero;
        scope = 0;
        createGetTimeToReachPoint();
    }
    public ChaserData(Vector3 offsetPosition,PublicPlayerData publicPlayerData, Area closestArea,float scope, string info)
    {
        this.offsetPosition = offsetPosition;
        OptimalPoint = Vector3.zero;
        OptimalTime = -1;
        ReachTheTarget = false;
        ClosestPoint = Vector3.positiveInfinity;
        ClosestChaserTime = float.PositiveInfinity;
        thereIsIntercession = false;
        Intercession = Vector3.zero;
        this.publicPlayerData = publicPlayerData;
        this.info = info;
        this.closestArea = closestArea;
        this.scope = scope;
        createGetTimeToReachPoint();
    }
    public ChaserData(Vector3 offsetPosition, PublicPlayerData publicPlayerData, Area closestArea, float scope,bool useAcceleration, string info)
    {
        this.offsetPosition = offsetPosition;
        OptimalPoint = Vector3.zero;
        OptimalTime = -1;
        ReachTheTarget = false;
        ClosestPoint = Vector3.positiveInfinity;
        ClosestChaserTime = float.PositiveInfinity;
        thereIsIntercession = false;
        Intercession = Vector3.zero;
        this.publicPlayerData = publicPlayerData;
        this.info = info;
        this.closestArea = closestArea;
        this.scope = scope;
        this.useAcceleration = useAcceleration;
        createGetTimeToReachPoint();
    }
    public void Clear()
    {
        OptimalPoint = Vector3.positiveInfinity;
        OptimalTime = float.PositiveInfinity;
        ReachTheTarget = false;
        ClosestPoint = Vector3.positiveInfinity;
        ClosestChaserTime = float.PositiveInfinity;
        thereIsIntercession = false;
        Intercession = Vector3.zero;
        thereIsClosestPoint = false;
        TargetIntercessionTime = Mathf.Infinity;
        TargetPositionInClosestTime = Vector3.positiveInfinity;
        differenceClosestTime = Mathf.Infinity;
        OptimalTargetTime = Mathf.Infinity;
    }
    void createGetTimeToReachPoint()
    {
        getTimeToReachPointClass = new GetTimeToReachPoint(publicPlayerData);
        getTimeToReachPointClass.getTimeToReachPointDelegate = getTimeToReachPointDelegate(getTimeToReachPointClass);
    }
    getTimeToReachPointDelegate getTimeToReachPointDelegate(GetTimeToReachPoint getTimeToReachPoint)
    {
        if (useAcceleration)
        {
            return getTimeToReachPoint.accelerationGetTimeToReachPosition;
        }
        else
        {
            return getTimeToReachPoint.linearGetTimeToReachPosition;
        }
    }
    float getClosestPointDistance()
    {

        if (ClosestPoint == Vector3.positiveInfinity || TargetPositionInClosestTime == Vector3.positiveInfinity)
        {
            return Mathf.Infinity;
        }
        else
        {
            return Vector3.Distance(ClosestPoint, TargetPositionInClosestTime);
        }
    }
    public float getGlobalOptimalTime()
    {
        if (ReachTheTarget)
        {
            return OptimalTime;
        }else if (thereIsClosestPoint)
        {
            return differenceClosestTime;
        }
        else
        {
            return Mathf.Infinity;
        }
    }
    public bool getGlobalOptimalPoint(out Vector3 result)
    {
        if (ReachTheTarget)
        {
            result = OptimalPoint;
            return true;
        }
        else if (thereIsClosestPoint)
        {
            result = ClosestPoint;
            return true;
        }
        else
        {
            result = Vector3.positiveInfinity;
            return false;
        }
    }
    float selectOptimalTime()
    {
        if (OptimalTime <= OptimalTargetTime)
        {
            //El chaser llega antes que el target al optimalPoint,por lo tanto, no importa el tiempo que tarde en llegar al optimalPoint ya que tendrá que esperar hasta el OptimalTargetTime
            return OptimalTargetTime;
        }
        else
        {
            return OptimalTime;
        }
    }
    float selectClosestTime()
    {
        if (ClosestChaserTime <= ClosestTargetTime)
        {
            //El chaser llega antes que el target al closestPoint,por lo tanto, no importa el tiempo que tarde en llegar al closestPoint ya que tendrá que esperar hasta el ClosestTargetTime
            return ClosestTargetTime;
        }
        else
        {
            return ClosestChaserTime;
        }
    }
    public Vector3 getOptimalPointWithOffset()
    {
        return OptimalPoint - offsetPosition;
    }
    public static int CompareBySelectedOptimalTime(ChaserData chaser1, ChaserData chaser2)
    {
        if (!chaser1.ReachTheTarget && !chaser2.ReachTheTarget)
        {
            return chaser1.getGlobalOptimalTime().CompareTo(chaser2.getGlobalOptimalTime());
        }else if (!chaser2.ReachTheTarget)
        {
            return -1;
        }else if (!chaser1.ReachTheTarget)
        {
            return 1;
        }
        else
        {
            return chaser1.getGlobalOptimalTime().CompareTo(chaser2.getGlobalOptimalTime());
        }
    }
    public static int CompareByChaserTime(ChaserData chaser1, ChaserData chaser2)
    {
        if (!chaser2.ReachTheTarget && !chaser1.ReachTheTarget)
        {
            return chaser1.differenceClosestTime.CompareTo(chaser2.differenceClosestTime);
        }
        else if (!chaser2.ReachTheTarget)
        {
            return -1;
        }
        else if (!chaser1.ReachTheTarget)
        {
            return 1;
        }
        else
        {
            return chaser1.OptimalTime.CompareTo(chaser2.OptimalTime);
        }
    }
    public override string ToString()
    {
        return "Order=" + Order + " | ReachTheTarget=" + ReachTheTarget + " | optimalPointTime=" + getGlobalOptimalTime()+" | closestArea="+ closestArea + " | info=" + info;
    }
    public static List<ChaserData> getFirstChaserDatas(List<ChaserData> chaserDatas,float range)
    {
        List<ChaserData> result = new List<ChaserData>();
        if (chaserDatas.Count > 0)
        {
            chaserDatas.Sort(ChaserData.CompareBySelectedOptimalTime);

            List<ChaserData> chaserDatasReachTarget = new List<ChaserData>();
            chaserDatasReachTarget.AddRange(chaserDatas.FindAll(x=>x.ReachTheTarget));
            if (chaserDatasReachTarget.Count > 0)
            {
                float firstOptimalTime = chaserDatasReachTarget[0].OptimalTargetTime;
                foreach (var chaserData in chaserDatasReachTarget)
                {
                    if (chaserData.OptimalTargetTime - firstOptimalTime <= range)
                    {
                        result.Add(chaserData);
                    }
                    else
                    {
                        break;
                    }
                }
                return result;
            }
            List<ChaserData> chaserDatasClosestPoint = new List<ChaserData>();
            chaserDatasClosestPoint.AddRange(chaserDatas.FindAll(x => !x.ReachTheTarget));
            if (chaserDatasClosestPoint.Count > 0)
            {
                float firstOptimalTime = chaserDatasClosestPoint[0].getGlobalOptimalTime();
                foreach (var chaserData in chaserDatasClosestPoint)
                {
                    if (chaserData.getGlobalOptimalTime() - firstOptimalTime <= range)
                    {
                        result.Add(chaserData);
                    }
                    else
                    {
                        break;
                    }
                }
                return result;
            }
        }
        return result;
    }
    public static bool checkFirstChaserDatas(ChaserData chaserData,List<ChaserData> chaserDatas, float range)
    {

        List<ChaserData> result = getFirstChaserDatas(chaserDatas, range);
        return result.Contains(chaserData);
    }
    public static bool checkOnlyFirstChaserDatas(ChaserData chaserData, List<ChaserData> chaserDatas, float range)
    {
        List<ChaserData> result = getFirstChaserDatas(chaserDatas, range);
        return result.Contains(chaserData) && result.Count==1;
    }
    public static List<ChaserData> getChaserDataOfPublicPlayerDatas(List<PublicPlayerData> publicPlayerDatas)
    {
        List<ChaserData> result = new List<ChaserData>();
        foreach (var item in publicPlayerDatas)
        {
            ChaserData chaserData;
            if(item.getFirstChaserData(out chaserData))
            {
                result.Add(chaserData);
            }
        }
        return result;
    }
}
