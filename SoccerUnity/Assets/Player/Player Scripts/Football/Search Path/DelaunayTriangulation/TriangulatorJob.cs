using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using andywiecko.BurstTriangulator;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using System;

public class TriangulatorJob : MonoBehaviour
{
    public Transform pointsRoot;
    public int jobSize = 10;
    public bool debug = false;
    public int debugIndex;
    List<Transform> points;
    List<NativeArray<float2>> positions = new List<NativeArray<float2>>();
    List<int> positionsSizes = new List<int>();
    List<Triangulator> triangulators=new List<Triangulator>();
    float fieldOffset = 2;
    void Start() {
        points = MyFunctions.GetComponentsInChilds<Transform>(pointsRoot.gameObject, false, true);
        for (int i = 0; i < jobSize; i++)
        {

            positions.Add(new NativeArray<float2>(15, Allocator.Persistent));
        }
        for (int i = 0; i < jobSize; i++)
        {
            triangulators.Add(new Triangulator(Allocator.Persistent));
        }
        
        MatchEvents.footballFieldLoaded.AddListenerConsiderInvoked(footballFieldLoaded);
    }

    private void footballFieldLoaded()
    {
        for (int i = 0; i < jobSize; i++)
        {
            NativeArray<float2> array = positions[i];
            for (int j = 0; j < MatchComponents.footballField.cornersComponents.Count; j++)
            {
                Transform cornerTransform = MatchComponents.footballField.cornersComponents[j].cornerPoint;
                Vector3 pos = cornerTransform.position + cornerTransform.TransformDirection(new Vector3(fieldOffset, 0, fieldOffset));
                array[j] = new Vector2(pos.x, pos.z);
                
            }
            positions[i] = array;
        }
    }

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
    }

    public NativeList<int> GetTriangles(int index)
    {
        var triangles = triangulators[index].Output.Triangles;

        return triangles;
    }
    void DrawTriangles(NativeList<int> points, NativeArray<float2> positions)
    {
        Color color = Color.red;
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
        }
    }
    // Update is called once per frame
    void OnDrawGizmos()
    {
        if (Application.isPlaying && debug)
        {
            var triangles = GetTriangles(debugIndex);
            DrawTriangles(triangles, positions[debugIndex]);
        }
    }
    private void OnDestroy()
    {
        triangulators.ForEach(x=>x.Dispose());
        positions.ForEach(x => x.Dispose());
    }
}
