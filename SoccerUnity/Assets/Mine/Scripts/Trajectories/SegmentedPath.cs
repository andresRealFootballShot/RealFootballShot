using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SegmentedPath : Path
{
    Path mainPath;
    public List<Path> paths = new List<Path>();
    
    public SegmentedPath(Path path) : base(path.Pos0,path.V0,path.t0)
    {
        mainPath = path;
    }
    public void getOptimalPointForReachTargetWithTimeSegmented(Vector3 chaserPosition, float chaserSpeed, float timeRange, float timeIncrement, float minAngle, float minVelocity, float maxAngle, float maxVelocity,out SortedList<float,Vector3> results)
    {
        float t=0;
        Path newPath;
        results = new SortedList<float, Vector3>();
        if (minAngle >= maxAngle || minVelocity >= maxVelocity || timeIncrement <= 0 || timeIncrement <= 0)
        {
            return;
        }
        while (t < timeRange)
        {
            if (calculateSegmentedPath(t,timeRange,timeIncrement, minAngle, minVelocity, maxAngle, maxVelocity, out newPath))
            {
                paths.Add(newPath);
                float t0 = t;
                t = newPath.tf;
                List<float> times;
                float chaserT = Vector3.Distance(chaserPosition, newPath.Pos0) / chaserSpeed;
                newPath.getOptimalPointForReachTarget(chaserPosition, chaserSpeed, t0, out times);
                if (chaserT <= t0)
                {
                    if (!results.ContainsKey(t0))
                    {
                        results.Add(t0, newPath.Pos0);
                    }
                }
                else
                {
                    foreach (var time in times)
                    {
                        if (!results.ContainsKey(time + t0))
                        {
                            results.Add(time + t0, newPath.Pos0 + newPath.V0 * time);
                        }
                        Debug.DrawLine(newPath.Pos0, newPath.Pos0 + newPath.V0 * time, Color.cyan);
                    }
                }
            }
            else
            {
                break;
            }
        }
        //Debug.Log("count =" + count3/count+" | count3="+count3+" | count="+count);
    }
    public void buildPath(float t0, float timeRange, float timeIncrement, float minAngle, float minVelocity, float maxAngle, float maxVelocity)
    {
        float t = t0;
        Path newPath;
        int count = 0;
        while (t < timeRange)
        {
            if (calculateSegmentedPath(t, timeRange, timeIncrement, minAngle, minVelocity, maxAngle, maxVelocity, out newPath))
            {
                paths.Add(newPath);
                t = newPath.tf;
            }
            else
            {
                t += timeIncrement;
            }
            count++;
            if (count > 100)
            {
                break;
            }
        }
    }
    public void getOptimalPointForReachTargetWithTimeSegmented(Vector3 chaserPosition,float chaserSpeed,float maxHeight,out SortedList<float, Vector3> results)
    {
        results = new SortedList<float, Vector3>();
        foreach (var path in paths)
        {
            List<float> times;
            float t0 = path.t0;
            float chaserT = Vector3.Distance(chaserPosition, path.Pos0) / chaserSpeed;
            path.getOptimalPointForReachTarget(chaserPosition, chaserSpeed, t0, out times);
            drawMaxHeightPoint(path, maxHeight);
            if (chaserT <= t0)
            {
                if (!results.ContainsKey(t0))
                {
                    results.Add(t0, path.Pos0);
                }
            }
            else
            {
                foreach (var time in times)
                {
                    if(!results.ContainsKey(time + t0))
                    {
                        results.Add(time + t0, path.Pos0 + path.V0 * time);
                    }
                    //Debug.DrawLine(path.Pos0, path.Pos0 + path.V0 * time, Color.cyan);
                }
            }
        }
    }

    void drawMaxHeightPoint(Path path,float maxHeight)
    {
        if ((path.Pos0.y < maxHeight && path.Posf.y > maxHeight) || (path.Pos0.y > maxHeight && path.Posf.y < maxHeight))
        {
            ParabolaWithDrag parabolaWithDrag = new ParabolaWithDrag(path.Pos0, path.Posf, path.V0, 0, 9.81f, 0.1f);

            //List<float> list = parabolaWithDrag.timeToReachY2(maxHeight-path.Pos0.y, 0.0001f);
            List<float> list = path.timeToReachY(maxHeight,1);
            if (list.Count > 0)
            {
                //Vector3 maxHeightV3 = parabolaWithDrag.getPositionAtTime(list[0]);
                Vector3 maxHeightV3 = path.Pos0 + path.V0* list[0];
                //Debug.Log("b " + maxHeightV3);
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(maxHeightV3, 0.07f);
#if UNITY_EDITOR
                //Handles.Label(maxHeightV3 + Vector3.up * 0.2f, "maxHeightPoint="+ maxHeightV3.ToString("F2")+" | maxHeight="+maxHeight);
                
#endif
            }
        }
    }
    public bool calculateSegmentedPath(float t0,float timeRange,float timeIncrement, float minAngle, float minVelocity, float maxAngle, float maxVelocity,out Path newPath)
    {
        float tf = t0;
        Path path1, path2;
        Vector3 v0 = mainPath.getVelocityAtTime(t0,out path1);
        //Debug.Log("a " + path1.ToString());
        Vector3 v;
        int count = 0;
        do
        {
            tf = Mathf.Clamp(tf+timeIncrement, 0,timeRange);
            v = mainPath.getVelocityAtTime(tf,out path2);
            if (count > 100)
            {
                break;
            }
            count++;
        } while (Vector3.Angle(v0, v) < minAngle && Mathf.Abs(v.magnitude - v0.magnitude) < minVelocity && tf<timeRange && path1==path2);
        if (Vector3.Angle(v0, v)==0 && Mathf.Abs(v.magnitude - v0.magnitude)==0)
        {
            Vector3 pos0 = mainPath.getPositionAtTime(t0);
            
            newPath = new Path(pos0,pos0, v0,v, t0,tf);
            return true;
        }
        while (t0 <= tf && tf < timeRange && Mathf.Abs(v.magnitude - v0.magnitude)>0)
        {
            count++;
            
            if (count > 100)
            {
                break;
            }
            timeIncrement /= 2;
            if(path1 == path2)
            {
                if (Vector3.Angle(v0, v) > maxAngle || Mathf.Abs(v.magnitude - v0.magnitude) > maxVelocity)
                {
                    //El segmento no sigue de forma adecuada la trayectoria ó la velocidad varía mucho durante el segmento
                    tf -= timeIncrement;
                    v = mainPath.getVelocityAtTime(tf);
                }
                else if (Vector3.Angle(v0, v) < minAngle && Mathf.Abs(v.magnitude - v0.magnitude) < minVelocity)
                {
                    //El segmento es casi identico a la trayectoria y la velocidad no varía lo suficiente así que buscaremos en un instante posterior para intentar tener menos segmentos de la trayectoria
                    
                    tf += timeIncrement;
                    v = mainPath.getVelocityAtTime(tf);
                }
                else
                {
                    //Debug.Log("a");
                    Vector3 pos0 = mainPath.getPositionAtTime(t0);
                    
                    Vector3 posf = mainPath.getPositionAtTime(tf);
                    Vector3 dir = posf - pos0;
                    Vector3 newV0 = dir / (tf - t0);
                    newPath = new Path(pos0, posf, newV0, v, t0, tf);
                    //Debug.Log("tf =" + tf + " | angle=" + Vector3.Angle(v0, v) + " | magnitude=" + Mathf.Abs(v.magnitude - v0.magnitude) + " | pos0=" + pos0);
                    return true;
                }
            }
            else
            {
                //Debug.Log("b");
                Vector3 pos0 = mainPath.getPositionAtTime(t0);
                Vector3 posf = mainPath.getPositionAtTime(tf);
                Vector3 dir = posf - pos0;
                Vector3 newV0 = dir / (tf - t0);
                newPath = new Path(pos0, posf, newV0, v, t0, tf);
                return true;
            }
        }
        //Debug.Log("a=" + Mathf.Abs(v.magnitude - v0.magnitude) + " | " + (t0 <= tf) + " |" + (tf < timeRange));
        //Debug.Log("tf =" + tf + " | angle=" + Vector3.Angle(v0, v) + " | magnitude=" + Mathf.Abs(v.magnitude - v0.magnitude) + " | pos0=" + path2.Pos0 + " | posf=" + path2.Posf);
        // Terminamos de buscar y no encontramos el elemento
        newPath = null;
        return false;

    }
    public IEnumerator tracingPath()
    {
        foreach (var path in paths)
        {
            float t=0;
            float tf = path.tf - path.t0;
            while (t < tf)
            {
                DrawArrow.ForDebug(path.getPositionAtTime(t)+Vector3.up*0.5f,-Vector3.up*0.5f);
                yield return null;
                t += Time.deltaTime;
            }

        }
    }
    public override void DrawPath(string info,float maxVelocity,float sphereSize,bool printText)
    {
        int i = 0;
        string info2 = info.Equals("") ? info : info + " | ";
        Path lastPath = null;
        foreach (var path in paths)
        {
            //Debug.Log("c " + path.t0);
            if(lastPath != null)
            {
                float angle = Vector3.Angle(path.V0, lastPath.V0);
                float magnitude = path.V0.magnitude;
                path.DrawPath(info2 + i.ToString() + " | t=" + path.t0 + " | m=" + magnitude.ToString("F2"), mainPath.V0.magnitude, sphereSize, printText);
                //path.DrawPath(info2 + i.ToString() + " | t=" + path.t0+ " | t0-tf="+(path.t0 - lastPath.t0) +" | Angle="+angle+" | magnitude="+magnitude, mainPath.V0.magnitude, sphereSize);
            }
            else
            {
                float magnitude = path.V0.magnitude;
                path.DrawPath(info2 + i.ToString() + " | t=" + path.t0 + " | m=" + magnitude.ToString("F2"), mainPath.V0.magnitude, sphereSize, printText);
                //path.DrawPath(info2 + i.ToString() + " | t=" + path.t0, mainPath.V0.magnitude, sphereSize);
            }
            i++;
            lastPath = path;
        }
        DrawLastPoint(info2, maxVelocity,printText);
    }
    void DrawLastPoint(string info,float maxVelocity,bool printText)
    {
        if (paths.Count > 0)
        {
            string info2 = info.Equals("") ? info : info + " | ";
            Path path = paths[paths.Count - 1];
            
            GUIStyle style = new GUIStyle();
            style.fontSize = 10;
            style.normal.textColor = Color.yellow;
            //Handles.Label(Pos0 + Vector3.up * 0.5f, info + " | Position=" + Pos0.ToString() + " | Velocity=" + V0.magnitude.ToString(), style);
#if UNITY_EDITOR
            if(printText){
            Handles.color = Color.green;
            Handles.Label(path.Posf + Vector3.up *0.15f, info2 + paths.Count + " | t=" + path.tf + " | Velocity=" + path.Vf.magnitude.ToString("f2"), style);
            }
#endif
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(path.Posf, 0.05f);
            Debug.DrawLine(path.Pos0, path.Posf, Color.red);
            //Debug.DrawRay(path.Posf, path.Vf * 2 / maxVelocity, Color.green);
        }
    }
    public override List<float> timeToReachY(float y, int count)
    {
        return mainPath.timeToReachY(y, count);
    }
    /*public bool getOptimalPointForReachTargetWithTimeSegmented(Vector3 chaserPosition, float chaserSpeed, float timeRange, float timeIncrement, float minAngle, float minVelocity, float maxAngle, float maxVelocity, out Vector3 result)
    {
        result = Vector3.zero;
        float t0 = 0f;
        Vector3 posf;
        Vector3 pos0 = mainPath.getPositionAtTime(t0);
        Vector3 v0 = mainPath.getVelocityAtTime(t0);
        float tf = t0 + timeIncrement;
        Vector3 v = mainPath.getVelocityAtTime(tf);
        List<trajectoryPointData> trajectoryPoints = new List<trajectoryPointData>();
        while (tf <= timeRange)
        {
            //Debug.Log(t + " | " + v.magnitude);
            if (Vector3.Angle(v0, v) > maxAngle || Mathf.Abs(v.magnitude - v0.magnitude) > maxVelocity)
            {
                trajectoryPoints.Insert(0, new trajectoryPointData(tf, v));
                tf = tf / 2;

            }
            else if (Vector3.Angle(v0, v) < minAngle && Mathf.Abs(v.magnitude - v0.magnitude) < minVelocity)
            {

                if (trajectoryPoints.Count == 0)
                {
                    tf += timeIncrement;
                }
                else
                {
                    tf += (trajectoryPoints[0].t - tf) / 2;
                }
            }
            else
            {
                posf = mainPath.getPositionAtTime(tf);
                Path newPath = new Path(pos0, posf, v0, v, t0, tf);
                paths.Add(newPath);
                pos0 = posf;
                v0 = v;
                t0 = tf;
                trajectoryPoints.RemoveAt(0);
                if (trajectoryPoints.Count == 0)
                {
                    tf += timeIncrement;
                }
                else
                {
                    tf = trajectoryPoints[0].t;
                    trajectoryPoints.RemoveAt(0);
                }
            }
        }
        return false;
    }
    public bool getOptimalPointForReachTargetWithSegmentedVelocity(Vector3 chaserPosition, float chaserSpeed,float timeRange, out Vector3 result)
    {
        result = Vector3.zero;
        float t0 = 0f;
        Vector3 pos0 = mainPath.getPositionAtTime(t0);
        Vector3 v0 = mainPath.getVelocityAtTime(t0);
        float vtMagnitude = v0.magnitude;
        velocities.Add(v0);
        List<float> times = new List<float>();
        vtMagnitude = Mathf.Clamp(vtMagnitude - derivativeDecrement, mainPath.minV, Mathf.Infinity);
        times.AddRange(mainPath.getTimeOfVelocity(vtMagnitude));
        times.Sort();
        while (times.Count > 0 && vtMagnitude > mainPath.minV)
        {
            float t = times[0];
            Vector3 posf = mainPath.getPositionAtTime(t);
            Vector3 v = mainPath.getVelocityAtTime(t);
            //Debug.Log("getOptimalPointForReachTargetWithSegmentedVelocity " + t+" | " + v.magnitude + " | "+ vtMagnitude);
            velocities.Add(v);
            Path newPath = new Path(pos0, posf, v0,v, t0,t);
            paths.Add(newPath);
            pos0 = posf;
            v0 = v;
            t0 = t; 
            vtMagnitude = Mathf.Clamp(vtMagnitude - derivativeDecrement,mainPath.minV,Mathf.Infinity);

            times.AddRange(mainPath.getTimeOfVelocity(vtMagnitude));
            if (vtMagnitude == 0)
            {
                List<float> aux = mainPath.getTimeOfVelocity(0.1f);
                if (aux.Count>0)
                times.Add(aux[0]);
            }
            else
            {
                
            }
            times.Sort();
            times.RemoveAt(0);
            } 
            
            foreach (var item in times)
            {
            float t = item;
            Vector3 posf = mainPath.getPositionAtTime(t);
            Vector3 v = mainPath.getVelocityAtTime(t);

            Debug.Log("vcb t="+t+" | v="+v.magnitude);
            //Debug.Log("getOptimalPointForReachTargetWithSegmentedVelocity " + t+" | " + v.magnitude + " | "+ vtMagnitude);
            velocities.Add(v);
            Path newPath = new Path(pos0, posf, v0, v, t0, t);
            paths.Add(newPath);
            pos0 = posf;
            v0 = v;
            t0 = t;
            }
        return false;
    }
     */
}
