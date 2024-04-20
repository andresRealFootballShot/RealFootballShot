using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneWithLimits : MonoBehaviour
{
    
    Plane pointNormal;
    List<Plane> limits = new List<Plane>();
    public MyEvent isBuildedEvent = new MyEvent(nameof(PlaneWithLimits) + " " + nameof(isBuildedEvent));
    struct planeData
    {
        public string name;
        public float offset;
        public planeData(string _name,float _offset)
        {
            name = _name;
            offset = _offset;
        }
    }
    public void buildPlanes(Vector3 offset)
    {
        limits = new List<Plane>();
        Transform pointNormalTrans = transform.Find("PointNormal");
        pointNormal = new Plane(pointNormalTrans.forward, pointNormalTrans.TransformPoint(Vector3.forward*offset.z));
        string[] planeNames = new string[] { "Right", "Left", "Up", "Down" };
        float[] offsets = new float[] { offset.x, offset.x, offset.y, offset.y };
        List<planeData> planeDatas = new List<planeData>();
        for (int i = 0; i < planeNames.Length; i++)
        {
            planeDatas.Add(new planeData(planeNames[i],offsets[i]));
        }
        foreach (var planeData in planeDatas)
        {
            Transform limit = transform.Find(planeData.name);
            
            if (limit != null)
            {
                limits.Add(new Plane(limit.forward, limit.TransformPoint(Vector3.forward * planeData.offset)));
            }
        }
        isBuildedEvent.Invoke();
    }
    public bool checkAreaLoaded()
    {
        if (limits.Count == 0)
        {
            Debug.LogError("PlaneWithLimits " + name + " isn't loaded");
            return false;
        }
        else
        {
            return true;
        }
    }
    public void PointIsForward(Vector3 point,out bool isInside,out bool isForward)
    {
        isInside = true;
        isForward = pointNormal.GetSide(point);
        
        foreach (var limit in limits)
        {
            if (!limit.GetSide(point))
            {
                isInside = false;
            }
        }
        //Debug.Log("isForward=" + isForward + " | isInside="+isInside);
    }
    public void PointIsForward(Vector3 point, out bool isInside, out bool isForward,string name)
    {
        isInside = true;
        isForward = pointNormal.GetSide(point);

        foreach (var limit in limits)
        {
            if (!limit.GetSide(point))
            {
                isInside = false;
            }
        }
        //Debug.Log(name+ " | "+gameObject.name+ " | isForward=" + isForward + " | isInside=" + isInside);
    }
    public bool GetPoint(Vector3 point, out Vector3 closestPoint,out bool isForward)
    {
        bool isInside = true;
        isForward = pointNormal.GetSide(point);
        closestPoint = pointNormal.ClosestPointOnPlane(point);
        foreach (var limit in limits)
        {
            if (!limit.GetSide(point))
            {
                closestPoint = limit.ClosestPointOnPlane(closestPoint);
                isInside = false;
            }
        }
        return isInside;
    }
    public void Raycast(Ray ray, out Vector3 result,out bool goesToThePlane,out bool intersectionIsInside)
    {
        float lenghtRayIntersection;
        intersectionIsInside = true;
        goesToThePlane = pointNormal.Raycast(ray, out lenghtRayIntersection);
        result = ray.origin + ray.direction * lenghtRayIntersection;
        foreach (var limit in limits)
        {
           if (!limit.GetSide(result))
            {
                result = limit.ClosestPointOnPlane(result);
                intersectionIsInside = false;
            }
        }
    }
    
}
