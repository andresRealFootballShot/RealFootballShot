using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class CalculateChaserDatas
{
    float timeRange, timeIncrement, minAngle, minVelocity, maxAngle, maxVelocity;
    //public List<ChaserData> chaserDataList = new List<ChaserData>();
    Area area { get => MatchComponents.footballField.fullFieldArea; }
    public CalculateChaserDatas(float timeRange, float timeIncrement, float minAngle, float minVelocity, float maxAngle, float maxVelocity)
    {
        this.timeRange = timeRange;
        this.timeIncrement = timeIncrement;
        this.minAngle = minAngle;
        this.minVelocity = minVelocity;
        this.maxAngle = maxAngle;
        this.maxVelocity = maxVelocity;
    }

    public IEnumerator getChaserDatas(List<ChaserData> chaserDatas, SegmentedPath segmentedPath,bool instantCalculation, float period, chaserDataResultDelegate chaserDataResultDelegate)
    {

        float t = 0;
        int count = 0,maxCount = 500;
        List<ChaserData> chaserDataListNoChecked = new List<ChaserData>();
        Dictionary<ChaserData, ChaserCalculationData> chaserCalculationDatas = new Dictionary<ChaserData, ChaserCalculationData>();
        foreach (var chaserData in chaserDatas)
        {
            chaserDataListNoChecked.Add(chaserData);
            chaserCalculationDatas.Add(chaserData, new ChaserCalculationData());
            //chaserData.Clear();
            //print(chaserData.publicPlayerData.name + " "+chaserData.info + " " + chaserData.position);
        }
        bool thereIsInterseccionEnd = false;
        while (t < timeRange && chaserDataListNoChecked.Count > 0)
        {
            count++;
            
            if (count > maxCount)
            {
                Debug.Log("CalculateChaserDatas count>" + maxCount);
                //Debug.LogError("CalculateChaserDatas count>"+ maxCount);
                break;
            }
            Path newPath;
            if (segmentedPath.calculateSegmentedPath(t, timeRange, timeIncrement, minAngle, minVelocity, maxAngle, maxVelocity, out newPath))
            {
                
                segmentedPath.paths.Add(newPath);
                Vector3 intercession;
                float targetIntercessionTime;
                bool thereIsIntercession = checkIntersecctionArea(newPath, out intercession, out targetIntercessionTime);
                thereIsInterseccionEnd = thereIsIntercession ? true : thereIsInterseccionEnd;
                List<ChaserData> removeChaserList = new List<ChaserData>();
                float t0 = t;
                t = newPath.tf;
                foreach (var chaserData in chaserDatas)
                {
                    
                    if (thereIsIntercession)
                    {
                        //print("b " + thereIsIntercession);
                        chaserData.Intercession = intercession;
                        chaserData.thereIsIntercession = thereIsIntercession;
                        chaserData.TargetIntercessionTime = targetIntercessionTime;
                    }
                    bool calculateClosestPointResult =calculateClosestPoint(chaserData, newPath);
                    chaserCalculationDatas[chaserData].thereIsClosestPoint = calculateClosestPointResult ? true : chaserCalculationDatas[chaserData].thereIsClosestPoint;
                }
                foreach (var chaserData in chaserDataListNoChecked)
                {
                    PublicPlayerData publicChaserData = chaserData.publicPlayerData;

                    Vector3 chaserPosition = chaserData.position;
                    float chaserSpeed = publicChaserData.maxSpeed;
                    //print("chaser=" + chaser.Value.name + " | position=" + chaserPosition + " | speed=" +chaserSpeed);

                    Vector3 optimalPoint = newPath.Pos0;
                    float optimalTime = chaserData.getTimeToReachPoint(newPath.Pos0, chaserData.scope);

                    bool thereIsOptimalPoint = false;
                    if (optimalTime <= t0)
                    {
                        //El chaser alcanza el objetivo antes de t0 (antes de que el balón llegue al inicio del segmento)

                        foreach (var maximumHeight in publicChaserData.maximumJumpHeights)
                        {
                            if (checkOptimalPointIsInsideTheField(optimalPoint) && optimalPoint.y <= maximumHeight.Key && publicChaserData.maximumJumpHeightIsInArea(maximumHeight.Key, optimalPoint))
                            {
                                thereIsOptimalPoint = true;
                            }
                        }
                        if (thereIsOptimalPoint)
                        {
                            removeChaserList.Add(chaserData);
                            chaserCalculationDatas[chaserData].ReachTheTarget = true;
                            changeChaserData(chaserData, true, optimalPoint, optimalTime + newPath.t0, newPath.t0);
                        }
                    }
                    if (!thereIsOptimalPoint)
                    {
                        float timeTargetResult;
                        if (checkHeight(chaserData, newPath, out optimalTime, out optimalPoint, out timeTargetResult))
                        {
                            if (checkOptimalPointIsInsideTheField(optimalPoint))
                            {
                                removeChaserList.Add(chaserData);
                                chaserCalculationDatas[chaserData].ReachTheTarget = true;
                                changeChaserData(chaserData, true, optimalPoint, optimalTime, timeTargetResult);

                            }
                        }
                    }
                }
                chaserDataListNoChecked.RemoveAll(x => removeChaserList.Contains(x));
            }
            else
            {
                t += timeIncrement;
                //break;
            }
            if (!instantCalculation)
            {
                yield return new WaitForSeconds(period);
            }
        }
        foreach (var chaserData in chaserDatas)
        {
            chaserData.ReachTheTarget = chaserCalculationDatas[chaserData].ReachTheTarget;
            chaserData.thereIsClosestPoint = chaserCalculationDatas[chaserData].thereIsClosestPoint;
            chaserData.thereIsIntercession = thereIsInterseccionEnd;
            
        }
        chaserDataListNoChecked.ForEach(x => x.ReachTheTarget = false);
        SortChaserDatas(ref chaserDatas);
        chaserDataResultDelegate?.Invoke(chaserDatas);
    }
    bool checkHeight(ChaserData chaserData, Path path, out float timeResult, out Vector3 pointResult, out float timeTargetResult)
    {
        //Comprueba que el chaser llega hasta optimalPoint.y
        timeResult = Mathf.Infinity;
        pointResult = Vector3.zero;
        PublicPlayerData publicPlayerData = chaserData.publicPlayerData;
        List<float> times;
        Vector3 chaserPosition = chaserData.position;
        float chaserSpeed = publicPlayerData.maxSpeed;
        float t0 = path.t0;
        //print("t0=" + t0);
        float heightTime = -1;
        float lastHeightTime = Mathf.Infinity;
        float maxHeight = Mathf.Infinity;
        Vector3 heightPoint = Vector3.positiveInfinity;
        timeTargetResult = Mathf.Infinity;
        //path.getOptimalPointForReachTarget(chaserPosition, chaserSpeed, t0,chaserData.scope, out times);
        if (chaserData.useAcceleration)
        {
            path.getOptimalPointForReachTargetWhitAcceleration(chaserData, t0, chaserData.scope, 0.1f, out times);
        }
        else
        {
            path.getOptimalPointForReachTarget(chaserPosition, chaserSpeed, t0, out times);
        }
        for (int i = 0; i < times.Count; i++)
        {
            //print(times[i]);
            times[i] += path.t0;
        }
        foreach (var maximumHeight in publicPlayerData.maximumJumpHeights)
        {
            if ((path.Pos0.y < maximumHeight.Key && path.Posf.y > maximumHeight.Key) || (path.Pos0.y > maximumHeight.Key && path.Posf.y < maximumHeight.Key))
            {
                //La máxima altura esta en este path. Necesitamos saber si el chaser llega hasta el punto con la máxima altura ya que el teorema de cosenos no resuelve este problema.
                Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
                float drag = ballRigidbody.drag;
                /*ParabolaWithDrag parabolaWithDrag = new ParabolaWithDrag(path.Pos0, path.V0, path.Vf, 0, 9.81f, drag);
                parabolaWithDrag.getPositionAtTime(0);*/
                List<float> list = path.timeToReachY(maximumHeight.Key, 1);

                if (list.Count > 0)
                {
                    heightTime = list[0];
                    Vector3 point = path.Pos0 + path.V0 * heightTime;
                    float timeToReachPoint = Vector3.Distance(point, chaserPosition) / chaserSpeed;
                    if (publicPlayerData.maximumJumpHeightIsInArea(maximumHeight.Key, point) && point.y <= maximumHeight.Key && heightTime + path.t0 < timeTargetResult && timeToReachPoint <= heightTime + path.t0)
                    {
                        lastHeightTime = timeToReachPoint;
                        timeTargetResult = heightTime + path.t0;
                        maxHeight = maximumHeight.Key;
                        heightPoint = point;
                        times.Add(timeToReachPoint);
                        times.Sort();
                    }
                    /*if (publicPlayerData.maximumJumpHeightIsInArea(maximumHeight.Key, point) && point.y <= maximumHeight.Key && timeToReachPoint < lastHeightTime && timeToReachPoint <= heightTime + path.t0)
                    {
                        lastHeightTime = timeToReachPoint;
                        timeTargetResult = heightTime + path.t0;
                        maxHeight = maximumHeight.Key;
                        heightPoint = point;
                        times.Add(timeToReachPoint);
                        times.Sort();
                    }*/
                }
            }
        }
        foreach (var time in times)
        {
            if (time != lastHeightTime)
            {
                Vector3 optimalPoint = path.Pos0 + path.V0 * (time - path.t0);
                //Vector3 optimalPoint = path.Pos0 + (path.Posf - path.Pos0).normalized * time;
                foreach (var maximumHeight in publicPlayerData.maximumJumpHeights)
                {
                    if (publicPlayerData.maximumJumpHeightIsInArea(maximumHeight.Key, optimalPoint) && optimalPoint.y <= maximumHeight.Key)
                    {
                        timeTargetResult = time;
                        pointResult = optimalPoint;
                        timeResult = time;
                        return true;
                    }
                }
            }
            else
            {
                pointResult = heightPoint;
                timeResult = lastHeightTime;
                return true;
            }
        }
        return false;
    }
    bool calculateClosestPoint(ChaserData chaserData, Path path)
    {
        MyLine line1 = new MyLine(path.Pos0, path.Posf);
        MyLine line2;
        //print(chaserData.name + " | " + chaserData.closestArea);
        if (!chaserData.closestArea.GetLine(line1, out line2))
        {
            return false;
        }
        Vector3 closestPoint = MyFunctions.GetClosestPointOnFiniteLine(chaserData.position, line2.pos0, line2.posf);
        float minDistanceOfMaximumHeight = Mathf.Infinity;
        Vector3 closestPoint2 = closestPoint;
        bool thereIsClosestPoint = false;
        foreach (var maximumJumpHeight in chaserData.publicPlayerData.maximumJumpHeights)
        {
            //Buscamos el closestPoint teniendo en cuenta las alturas máximas
            if (closestPoint.y > maximumJumpHeight.Key)
            {
                Plane plane = new Plane(MatchComponents.footballField.normal, MatchComponents.footballField.position + MatchComponents.footballField.normal * maximumJumpHeight.Key);
                Vector3 direction = closestPoint - chaserData.position;
                Ray ray = new Ray(chaserData.position, direction);
                float lenght;
                if (plane.Raycast(ray, out lenght))
                {
                    //Punto del closestPoint en la altura máxima
                    Vector3 point = chaserData.position + direction * lenght;

                    if (chaserData.publicPlayerData.maximumJumpHeightIsInArea(maximumJumpHeight.Key, point))
                    {

                        float distanceOfMaximumHeight = Vector3.Distance(point, closestPoint);
                        if (distanceOfMaximumHeight < minDistanceOfMaximumHeight && Vector3.Distance(closestPoint, point) < 0.3f)
                        {
                            closestPoint2 = point;
                            minDistanceOfMaximumHeight = distanceOfMaximumHeight;
                            thereIsClosestPoint = true;
                        }
                        else
                        {

                        }
                    }
                }
            }
            else
            {
                thereIsClosestPoint = true;
                break;
            }
        }
        if (!thereIsClosestPoint)
        {
            return false;
        }
        float closestTime = Vector3.Distance(closestPoint2, chaserData.position) / chaserData.publicPlayerData.maxSpeed;
        float chaserTimeFromPathT0 = closestTime - path.t0;
        Vector3 targetPositionInClosestTime = path.Pos0 + path.AverageV * chaserTimeFromPathT0;
        float timeTargetToReachClosestPoint;
        if (path.V0.magnitude != 0)
        {
            timeTargetToReachClosestPoint = Vector3.Distance(closestPoint, path.Pos0) / path.V0.magnitude;
            float timeDifference = chaserTimeFromPathT0 - timeTargetToReachClosestPoint;
            float closestPointDistance = Vector3.Distance(closestPoint, targetPositionInClosestTime);
            //print(targetTimeWithOffset + " | " + timeTargetToReachClosestPoint + " | " + timeDifference);
            //print(closestPointDistance + " "+chaserData.ClosestPointDistance+" "+ chaserData.TargetPositionInClosestTime + " " + chaserData.ClosestPoint);
            Ray ray = new Ray(chaserData.position, closestPoint2 - chaserData.position);
            Vector3 closestPoint3;
            if (chaserData.closestArea.GetPoint(closestPoint2, ray, out closestPoint3))
            {
                if (!chaserData.thereIsClosestPoint || (timeDifference < chaserData.differenceClosestTime))
                {

                    chaserData.ClosestPoint = closestPoint3;
                    chaserData.ClosestChaserTime = closestTime;
                    chaserData.ClosestTargetTime = timeTargetToReachClosestPoint + path.t0;
                    chaserData.TargetPositionInClosestTime = targetPositionInClosestTime;
                    chaserData.differenceClosestTime = timeDifference;
                    chaserData.thereIsClosestPoint = true;
                }
            }
        }
        else
        {
            chaserData.ClosestPoint = path.Pos0;
            chaserData.ClosestChaserTime = closestTime;
            chaserData.ClosestTargetTime = 0;
            chaserData.TargetPositionInClosestTime = path.Pos0;
            chaserData.differenceClosestTime = closestTime;
            chaserData.thereIsClosestPoint = true;
        }
        return thereIsClosestPoint;
    }
    bool checkIntersecctionArea(Path path, out Vector3 intercession, out float targetIntercessionTime)
    {
        intercession = Vector3.zero;
        targetIntercessionTime = 0;
        //print("c " + path.Pos0 +" "+ path.Posf);
        if (area.PointIsInside(path.Pos0) && !area.PointIsInside(path.Posf))
        {

            Ray ray = new Ray(path.Pos0, path.V0);
            Vector3 aux;
            bool thereIsIntercession = area.GetIntercession(ray, out aux);
            //print("b "+ thereIsIntercession);
            intercession = aux;
            targetIntercessionTime = path.t0 + Vector3.Distance(aux, path.Pos0) / path.V0.magnitude;
            return thereIsIntercession;
        }
        return false;
    }
    void changeChaserData(ChaserData chaserData, bool reachTheTarget, Vector3 optimalPoint, float optimalTime, float optimalTargetTime)
    {
        chaserData.ReachTheTarget = reachTheTarget;
        chaserData.OptimalPoint = optimalPoint;
        chaserData.OptimalTime = optimalTime;
        chaserData.OptimalTargetTime = optimalTargetTime;
    }
    
    void draw()
    {
        //Debug.DrawLine(optimalPoint, chaserPosition, Color.cyan);
    }
    /*
    void OnDrawGizmos()
    {
        if (Application.isPlaying && drawGizmos)
        {
            printChaserListResult();
        }
    }*/
    public void printChaserListResult(List<ChaserData> chaserDatas, bool drawGizmos,bool drawChaserData, bool drawOptimalPoint,bool drawClosestPoint)
    {
        //Debug.Log("printChaserListResult");
        if (drawGizmos)
        {
            foreach (var chaserData in chaserDatas)
            {
                if (chaserData.ReachTheTarget || true)
                {
                    if (drawChaserData)
                    {

#if UNITY_EDITOR
                        
                        Handles.Label(chaserData.publicPlayerData.bodyTransform.position + Vector3.up * 0.2f, chaserData.ToString());
#endif
                    }
                    if (drawOptimalPoint)
                    {
                        Debug.DrawLine(chaserData.position, chaserData.OptimalPoint, Color.blue);
                        //print(chaser.Value.bodyTransform.position+" | "+chaser.Value.ChaserData.ClosestPoint);
                    }
                }
                if (drawClosestPoint && chaserData.thereIsClosestPoint)
                {
                    Debug.DrawLine(chaserData.position, chaserData.ClosestPoint, Color.green);
                }
            }
        }
    }
    bool checkOptimalPointIsInsideTheField(Vector3 optimalPoint)
    {

        return MatchComponents.footballField.fullFieldArea.PointIsInside(optimalPoint);
    }
    void SortChaserDatas(ref List<ChaserData> chaserDatas)
    {
        chaserDatas.Sort(ChaserData.CompareBySelectedOptimalTime);
        int index = 0;
        foreach (var item in chaserDatas)
        {
            item.Order = index;
            index++;
        }
    }
    
}
