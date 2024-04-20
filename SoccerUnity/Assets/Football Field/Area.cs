using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Area : MonoBehaviour
{
    public List<Plane> planes { get; set; } = new List<Plane>();
    List<Transform> planesTransform = new List<Transform>();
    public MyEvent isBuildedEvent = new MyEvent(nameof(Area) + " "+nameof(isBuildedEvent));
    private void Start()
    {
        //loadPlanes();
    }
    public void buildArea(Vector3 offset)
    {
        planes.Clear();
        planesTransform = new List<Transform>();
        string[] planeNames = new string[] { "Forward", "Back", "Right", "Left", "Up", "Down" };
        float[] offsets = new float[] { offset.z, offset.z, offset.x, offset.x, offset.y, offset.y };
        for (int i = 0; i < planeNames.Length; i++)
        {
            Transform plane = transform.Find(planeNames[i]);
            if (plane != null)
            {
                planesTransform.Add(plane);
                planes.Add(new Plane(plane.forward, plane.TransformPoint(Vector3.forward * offsets[i])));
            }
        }
        isBuildedEvent.Invoke();
    }
    void loadPlanes()
    {
        planes = new List<Plane>();
        planesTransform = new List<Transform>();
        string[] planeNames = new string[] { "Forward", "Back", "Right", "Left", "Up", "Down" };
        
        foreach (var planeName in planeNames)
        {
            Transform plane = transform.Find(planeName);
            if (plane != null)
            {
                planesTransform.Add(plane);
                planes.Add(new Plane(plane.forward, plane.position));
            }
        }
    }
    public bool checkAreaLoaded()
    {
        if (planes.Count == 0)
        {
            //Debug.LogError("Area " + name + " isn't loaded");
            return false;
        }
        else
        {
            return true;
        }
    }
    public bool PointIsInside(Vector3 point)
    {
        bool isInside = true;
        checkAreaLoaded();
        foreach (var plane in planes)
        {
            //print("a " + plane.GetSide(point));
            if (!plane.GetSide(point))
            {
                isInside = false;
            }
        }
        return isInside;
    }
    public bool GetPoint(Vector3 point, out Vector3 result)
    {
        bool isInside = true;
        result = point;
        checkAreaLoaded();
        foreach (var plane in planes)
        {
            //print("a " + plane.GetSide(point));
            if (!plane.GetSide(point))
            {
                result = plane.ClosestPointOnPlane(result);
                isInside = false;
            }
        }
        return isInside;
    }
    public bool GetIntercession(Ray ray,out Vector3 intercession)
    {
        intercession = Vector3.zero;
        foreach (var plane in planes)
        {
            float lenght;
            if (plane.Raycast(ray,out lenght))
            {
                Vector3 intercessionAux = ray.origin + ray.direction * lenght;
                bool intercessionIsInside = true;
                foreach (var plane2 in planes)
                {
                    if (!plane2.Equals(plane))
                    {
                        if (!plane2.GetSide(intercessionAux))
                        {
                            intercessionIsInside = false;
                        }
                    }
                }
                if (intercessionIsInside)
                {
                    intercession = intercessionAux;
                    return true;
                }
            }
        }
        return false;
    }
    public bool GetPoint(Vector3 point,Ray ray, out Vector3 result)
    {
        bool isInside = true;
        result = point;
        
        checkAreaLoaded();
        foreach (var plane in planes)
        {
            //print("a " + plane.GetSide(point));
            if (!plane.GetSide(result))
            {
                //result = plane.ClosestPointOnPlane(result);
                isInside = false;
            }
            float lenghtRayOfInstersection = 0;
            if(plane.Raycast(ray,out lenghtRayOfInstersection))
            {
                Vector3 intersection = ray.origin + ray.direction * lenghtRayOfInstersection;
                bool isInsideIntersection=true;
                foreach (var plane2 in planes)
                {
                    if (!plane2.Equals(plane))
                    {
                        if (!plane2.GetSide(intersection))
                        {
                            isInsideIntersection = false;
                        }
                    }
                }
                if (isInsideIntersection)
                {
                    result = intersection;
                }
            }
        }
        if (isInside)
        {
            result = point;
        }
        return isInside;
    }
    public bool GetLine(MyLine line, out MyLine result)
    {
        //Si hay un punto dentro entonces el resultado es la linea entre el punto y la intersección y se devuelve verdadero
        //Si los dos están dentro el resultado el la line y se devuelve verdadero
        //Si los dos están fuera se devuelve falso
        bool pos0isInside = true;
        bool posfIsInside = true;
        result = new MyLine(line.pos0,line.posf);
        checkAreaLoaded();
        Ray ray = new Ray(line.posf,line.pos0-line.posf);
        Vector3 intercession;
        if(!GetPoint(line.pos0,ray,out intercession)){
            result.pos0 = intercession;
            pos0isInside = false;
        }
        ray = new Ray(line.pos0, line.posf - line.pos0);
        if (!GetPoint(line.posf, ray, out intercession))
        {
            result.posf = intercession;
            posfIsInside = false;
        }
        return pos0isInside || posfIsInside;
    }
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (Selection.activeGameObject != null && Selection.activeGameObject.transform.IsChildOf(transform))
        {
            if (Application.isEditor)
            {
                loadPlanes();
            }

            drawGizmos();
        }
#endif
    }

    private void drawGizmos()
    {
        foreach (var plane in planesTransform)
        {
#if UNITY_EDITOR
            Handles.Label(plane.position + Vector3.up*0.6f,plane.name);
#endif
            Gizmos.color = Color.red;
            Gizmos.DrawCube(plane.position, Vector3.one * 0.1f);
            DrawArrow.ForGizmo(plane.position, plane.forward,Color.blue, 0.25f, 20);
        }
    }
        }
