using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using andywiecko.BurstTriangulator;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using System;
using static UnityEditor.PlayerSettings;
using UnityEditor;

public class TriangulatorJob : MonoBehaviour
{
    
    public Transform pointsRoot;
    public int jobSize = 10;
    public bool debug = false;
    public int debugIndex;
    List<Transform> points;
    //List<NativeArray<float2>> positions = new List<NativeArray<float2>>();
    List<int> positionsSizes = new List<int>();
    //List<Triangulator> triangulators=new List<Triangulator>();
    float fieldOffset = 2;
    public SearchPlayData searchPlayData { get; set; }
    void Start() {
        points = MyFunctions.GetComponentsInChilds<Transform>(pointsRoot.gameObject, false, true);
        
        //for (int i = 0; i < jobSize; i++)
        //{
        //    triangulators.Add(new Triangulator(Allocator.Persistent,GetLonelyPointParameters));
        //}
        
        
    }

    
    /*
    NativeList<int> getTriangles(List<Transform> points,int pointsSize)
    {
        NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(jobSize, Allocator.Temp);
        for (int i = 0; i < jobSize; i++)
        {
            NativeArray<float2> array = positions[i];
            for (int j = 0; j < points.Count; j++)
            {
                array[j] = new Vector2(points[j].position.x, points[j].position.z);
                positions[i] = array;
            }
            triangulators[i].Input.Positions = array;
            jobHandles[i] = triangulators[i].Schedule();
        }
        JobHandle.CompleteAll(jobHandles);
        var triangles = triangulators[0].Output.Triangles;
        
        return triangles;
    }
    public void SetPoint(int jobIndex,int index,Vector3 position)
    {
        NativeArray<float2> array = positions[jobIndex];
        array[index+4] = new Vector2(position.x, position.z);
        positions[jobIndex] = array;
    }
    public void UpdatePoints(int jobSize)
    {
        NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(jobSize, Allocator.Temp);
        for (int i = 0; i < jobSize; i++)
        {
            NativeArray<float2> array = positions[i];
            triangulators[i].Input.Positions = array;
            jobHandles[i] = triangulators[i].Schedule();
        }
        JobHandle.CompleteAll(jobHandles);
    }*/

    public NativeList<int> GetTriangles(int index)
    {
        var triangles = searchPlayData.searchPlayNodes[index].triangulator.Output.Triangles;

        return triangles;
    }
    public NativeList<Point> GetPoints(int index)
    {
        var lonelyPoints = searchPlayData.GetLonelyPoints(index);

        return lonelyPoints;
    }
    void DrawTriangles(NativeList<int> points, NativeArray<float2> positions)
    {
        Color color = new Color(0,0.9f,0.7f);
        for (int i = 0; i < points.Length; i+=3)
        {
            int index1 = points[i];
            int index2 = points[i+1];
            int index3 = points[i+2];
            Vector3 point1 = new Vector3(positions[index1].x, 0, positions[index1].y);
            Vector3 point2 = new Vector3(positions[index2].x, 0, positions[index2].y);
            Vector3 point3 = new Vector3(positions[index3].x, 0, positions[index3].y);
            Debug.DrawLine(point1, point2, color);
            Debug.DrawLine(point2, point3, color);
            Debug.DrawLine(point3, point1, color);

            
            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.normal.textColor = color;
            Handles.Label(point1 + Vector3.up * 0.5f, point1.ToString("f2"), style);
            Handles.Label(point2 + Vector3.up * 0.5f, point2.ToString("f2"), style);
            Handles.Label(point3 + Vector3.up * 0.5f, point3.ToString("f2"), style);

        }
    }
    void DrawLonelyPoints(NativeList<Point> lonelyPoints)
    {
        Color color = Color.red;
        for (int i = 0; i < lonelyPoints.Length; i ++)
        {
            Vector2 pos = lonelyPoints[i].position;
            Vector3 point = new Vector3(pos.x, 0.1f, pos.y);
            Gizmos.color = color;
            Gizmos.DrawSphere(point, 0.2f);
        }
    }
    // Update is called once per frame
    void OnDrawGizmos()
    {
        if (Application.isPlaying && debug)
        {
            var triangles = GetTriangles(debugIndex);
            var lonelyPoints = GetPoints(debugIndex);
            DrawTriangles(triangles, searchPlayData.GetPlayerPositions(debugIndex));
            DrawLonelyPoints(lonelyPoints);
        }
    }
    
}
