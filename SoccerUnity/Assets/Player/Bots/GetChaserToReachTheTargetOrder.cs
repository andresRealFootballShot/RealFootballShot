using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class ChaserCalculationData
{
    public bool ReachTheTarget = false,thereIsIntercession = false,thereIsClosestPoint = false;
    public void Clear()
    {
        ReachTheTarget = false;
        thereIsIntercession = false;
        thereIsClosestPoint = false;
    }
}
public class GetChaserToReachTheTargetOrder : MonoBehaviour,ILoad
{
    public new bool enabled;
    public float timeRange, timeIncrement, minAngle, minVelocity, maxAngle, maxVelocity;
    public static int _loadLevel = MatchEvents.staticLoadLevel + 1;
    public int loadLevel { get => _loadLevel; set => _loadLevel = value; }
    public List<ChaserData> chaserDataList = new List<ChaserData>();
    SegmentedPath segmentedPath;
    public bool drawGizmos;
    public bool drawChaserData;
    public bool drawClosestPoint;
    public bool drawOptimalPoint;
    public bool useAcceleration;
    public bool useFrameRate;
    public float frames;
    float period;
    float startTime;
    public float getSegmentCalculationPeriod = 0.1f;
    Area area { get => MatchComponents.footballField.fullFieldArea; }
    EventTrigger trigger = new EventTrigger();

    List<PublicPlayerData> _allPublicPlayerDataChasers = new List<PublicPlayerData>();
    List<ChaserData> chaserDataListNoChecked = new List<ChaserData>();
    Dictionary<ChaserData, ChaserCalculationData> chaserCalculationDatas = new Dictionary<ChaserData, ChaserCalculationData>();
    public void Load(int level)
    {
       
        if (loadLevel == level)
        {
            Load();
        }
    }
    void Load()
    {
        if (enabled)
        {
            period = 1 / frames;
            MatchEvents.matchLoaded.AddListener(() => enabled = true);
            MatchComponents.chaserList = chaserDataList;
            enabled = false;
            trigger.addTrigger(MatchEvents.setMainBall,false,1,true);
            trigger.addFunction(()=>StartCoroutine(getChaserDatasCoroutine()));
            //trigger.addFunction(()=> getAsyncChaserDatas());
            trigger.endLoadTrigger();
        }
    }
    void updateStartTime()
    {
        startTime = Time.realtimeSinceStartup;
    }
    bool checkInRate()
    {
       float t = Time.realtimeSinceStartup - startTime;
        print(t + " "+period);
       bool result = t <= period;
        if (!result)
        {
            print("out rate");
        }
        return result;
    }
    IEnumerator getChaserDatasCoroutine()
    {
        while (true)
        {
            //enabled = false;

            buildSegmentedPath();
            float t = 0;
            int count = 0;
            _allPublicPlayerDataChasers.Clear();
            chaserDataListNoChecked.Clear();
            chaserCalculationDatas.Clear();
            chaserDataList.Clear();
            foreach (var chaser in PublicPlayerDataList.all)
            {
                _allPublicPlayerDataChasers.Add(chaser.Value);
                foreach (var chaserData in chaser.Value.ChaserDataList)
                {
                    chaserDataList.Add(chaserData);
                    chaserCalculationDatas.Add(chaserData,new ChaserCalculationData());
                    chaserDataListNoChecked.Add(chaserData);
                    //chaserData.Clear();
                    //print(chaserData.publicPlayerData.name + " "+chaserData.info + " " + chaserData.position);
                }
            }
            //ChaserList.Clear();
            float radius = MatchComponents.ballComponents.radio;
            bool thereIsInterseccionEnd = false;
            while (t < timeRange && chaserDataListNoChecked.Count > 0)
            {
                float startTime = Time.realtimeSinceStartup;
                
                count++;
                if (count > 500)
                {
                    Debug.LogError("GetChaserToReachTheTargetOrder count>100");
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
                    foreach (var chaserData in chaserDataList)
                    {
                        if (thereIsIntercession)
                        {
                            //print("b " + thereIsIntercession);
                            chaserData.Intercession = intercession;
                            chaserData.thereIsIntercession = thereIsIntercession;
                            chaserData.TargetIntercessionTime = targetIntercessionTime;
                        }
                        bool calculateClosestPointResult = calculateClosestPoint(chaserData, newPath);
                        chaserCalculationDatas[chaserData].thereIsClosestPoint = calculateClosestPointResult ? true : chaserCalculationDatas[chaserData].thereIsClosestPoint;
                    }
                    foreach (var chaserData in chaserDataListNoChecked)
                    {
                        if (useFrameRate)
                        {
                            if (!checkInRate())
                            {
                                updateStartTime();
                                //yield return null;
                            }
                        }
                        PublicPlayerData publicChaserData = chaserData.publicPlayerData;

                        Vector3 chaserPosition = chaserData.position;
                        float chaserSpeed = publicChaserData.maxSpeed;
                        //print("chaser=" + chaser.Value.name + " | position=" + chaserPosition + " | speed=" +chaserSpeed);

                        Vector3 optimalPoint = newPath.Pos0;
                        //float optimalTime = Vector3.Distance(chaserPosition, newPath.Pos0) / chaserSpeed;
                        float optimalTime = chaserData.publicPlayerData.getTimeToReachPosition(newPath.Pos0, chaserData.scope);
                        bool thereIsOptimalPoint = false;
                        if (optimalTime <= t0)
                        {
                            //El chaser alcanza el objetivo antes de t0 (antes de que el bal�n llegue al inicio del segmento)

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
                if(!useFrameRate)
                {
                    //yield return new WaitForSeconds(getSegmentCalculationPeriod);
                }
            }
            foreach (var chaserData in chaserDataList)
            {
                chaserData.ReachTheTarget = chaserCalculationDatas[chaserData].ReachTheTarget;
                chaserData.thereIsClosestPoint = chaserCalculationDatas[chaserData].thereIsClosestPoint;
                chaserData.thereIsIntercession = thereIsInterseccionEnd;
            }
            chaserDataListNoChecked.ForEach(x => x.ReachTheTarget = false);
            
            SortChaserDatas();
            updateStartTime();
            yield return null;
        }
    }
    bool checkHeight(ChaserData chaserData, Path path, out float timeResult, out Vector3 pointResult,out float timeTargetResult)
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
        float scope = chaserData.scope;
        //scope = 0;
        if(useAcceleration)
        {
            path.getOptimalPointForReachTargetWhitAcceleration(chaserData, t0, scope, 0.01f, out times);
            if (times.Count > 0 && publicPlayerData.name.Equals("Behaviour"))
            {

                Vector3 point = path.Pos0 + path.V0 * times[0];
                //Debug.Log(publicPlayerData.name + " optimalPoint=" + point.ToString("f2"));
            }
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
                //La m�xima altura esta en este path. Necesitamos saber si el chaser llega hasta el punto con la m�xima altura ya que el teorema de cosenos no resuelve este problema.
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
            //Buscamos el closestPoint teniendo en cuenta las alturas m�ximas
            if (closestPoint.y > maximumJumpHeight.Key)
            {
                Plane plane = new Plane(MatchComponents.footballField.normal, MatchComponents.footballField.position + MatchComponents.footballField.normal * maximumJumpHeight.Key);
                Vector3 direction = closestPoint - chaserData.position;
                Ray ray = new Ray(chaserData.position, direction);
                float lenght;
                if (plane.Raycast(ray, out lenght))
                {
                    //Punto del closestPoint en la altura m�xima
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
            return thereIsClosestPoint;
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
                    thereIsClosestPoint = true;
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
            thereIsClosestPoint = true;
        }
        return thereIsClosestPoint;
    }
    bool checkIntersecctionArea(Path path,out Vector3 intercession,out float targetIntercessionTime)
    {
        intercession = Vector3.zero;
        targetIntercessionTime = 0;
        //print("c " + path.Pos0 +" "+ path.Posf);
        if (area.PointIsInside(path.Pos0) && !area.PointIsInside(path.Posf))
        {
            
            Ray ray = new Ray(path.Pos0,path.V0);
            Vector3 aux;
            bool thereIsIntercession = area.GetIntercession(ray, out aux);
            //print("b "+ thereIsIntercession);
            intercession = aux;
            targetIntercessionTime = path.t0 + Vector3.Distance(aux, path.Pos0) / path.V0.magnitude;
            return thereIsIntercession;
        }
        return false;
    }
    void changeChaserData(ChaserData chaserData,bool reachTheTarget,Vector3 optimalPoint,float optimalTime,float optimalTargetTime)
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
    void OnDrawGizmos()
    {
        if (Application.isPlaying && drawGizmos && segmentedPath!=null)
        {
            printChaserListResult();
            segmentedPath.DrawPath("",20,0.1f,false);
        }
    }
    void printChaserListResult()
    {
        //print("printChaserListResult");
        if (drawGizmos)
        {
            foreach (var chaser in PublicPlayerDataList.all)
            {
                //print(chaser.ToString());
                
                ChaserData chaserData;
                if(!chaser.Value.getFirstChaserData(out chaserData))
                {
                    continue;
                }
                if (chaserData.ReachTheTarget || true)
                {
                        if (drawChaserData)
                        {

#if UNITY_EDITOR
            Handles.Label(chaser.Value.bodyTransform.position + Vector3.up * 0.75f, chaserData.ToString());
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
    void SortChaserDatas()
    {
        chaserDataList.Sort(ChaserData.CompareBySelectedOptimalTime);
        foreach (var item in PublicPlayerDataList.all)
        {
            item.Value.ChaserDataList.Sort(ChaserData.CompareBySelectedOptimalTime);
        }
        int index = 0;
        foreach (var item in chaserDataList)
        {
            item.Order = index;
            index++;
        }
    }
    void buildSegmentedPath()
    {
        float radius = MatchComponents.ballComponents.radio;
        //print("a "+radius);
        Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        float drag = ballRigidbody.drag;
        PhysicMaterial ballPhysicsMaterial = MatchComponents.ballComponents.physicMaterial;
        PhysicMaterial footballFieldPhysicMaterial = MatchComponents.footballField.footballFieldPhysicMaterial;
        float bounciness = GetPhysicMaterialCombine.getCombination(ballPhysicsMaterial.bounciness, footballFieldPhysicMaterial.bounciness, GetPhysicMaterialCombine.getPhysicMaterialCombine(ballPhysicsMaterial.bounceCombine, footballFieldPhysicMaterial.bounceCombine));
        float dynamicFriction = GetPhysicMaterialCombine.getCombination(ballPhysicsMaterial.dynamicFriction, footballFieldPhysicMaterial.dynamicFriction, GetPhysicMaterialCombine.getPhysicMaterialCombine(ballPhysicsMaterial.frictionCombine, footballFieldPhysicMaterial.frictionCombine));
        float staticFriction = GetPhysicMaterialCombine.getCombination(ballPhysicsMaterial.staticFriction, footballFieldPhysicMaterial.staticFriction, GetPhysicMaterialCombine.getPhysicMaterialCombine(ballPhysicsMaterial.frictionCombine, footballFieldPhysicMaterial.frictionCombine));
        float friction = staticFriction > dynamicFriction ? staticFriction : dynamicFriction;
        StraightXZDragAndFrictionPath straightXZDragAndFrictionPath = new StraightXZDragAndFrictionPath(drag, radius, friction, ballRigidbody.mass);
        ParabolaWithDrag trajectory = new ParabolaWithDrag(ballRigidbody.position, ballRigidbody.velocity, 0, 9.81f, drag);
        
        //BouncyPath bouncyPath = new BouncyPath(trajectory,new StraightXZDragPath(drag), radius,0.1f, bounciness, dynamicFriction);
        BouncyPath bouncyPath = new BouncyPath(trajectory, straightXZDragAndFrictionPath, radius, 0.1f, bounciness, dynamicFriction);
        segmentedPath = new SegmentedPath(bouncyPath);
        
    }
    void printHeightsOfChasers()
    {
        foreach (var chaser in PublicPlayerDataList.all)
        {
            print(chaser.Value.name);
            foreach (var item in chaser.Value.maximumJumpHeights)
            {
                print(item.Key + " | " + item.Value);
            }
        }
    }

}
