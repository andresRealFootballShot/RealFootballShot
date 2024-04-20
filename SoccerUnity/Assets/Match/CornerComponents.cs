using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CornerID
{
    Right,Left
}
public class CornerComponents : MonoBehaviour,ILoad
{
    public CornerID cornerID;
    [HideInInspector]
    public List<PlaneWithLimits> planes;
    [HideInInspector]
    public Transform cornerPoint;
    public static int _loadLevel = MatchEvents.staticLoadLevel + 1;
    public int loadLevel { get => _loadLevel; set => _loadLevel = value; }
    public MyEvent cornerLoaded = new MyEvent(nameof(CornerComponents)+" "+nameof(cornerLoaded));
    public void Load(int level)
    {
        if (loadLevel == level)
        {
            Load();
        }
    }
    void Load()
    {
        
    }
    public void BuildPlanes(Vector3 offset)
    {
        if (planes == null || true)
        {
            planes = new List<PlaneWithLimits>();
            planes.AddRange(transform.GetComponentsInChildren<PlaneWithLimits>());
            cornerPoint = transform.Find("CornerPoint");
        }
        foreach (var plane in planes)
        {
            plane.buildPlanes(offset);
        }
        cornerLoaded.Invoke();
    }
}
