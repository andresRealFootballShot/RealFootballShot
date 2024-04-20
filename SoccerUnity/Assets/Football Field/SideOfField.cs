using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum SideOfFieldID
{
    One,
    Two
}

public class SideOfField : MonoVariable<SideOfFieldID>
{
    public GoalComponents goalComponents;
    [HideInInspector]
    public List<CornerComponents> corners = new List<CornerComponents>();
    public Area bigArea;
    public Transform goalKickPoint,limit;
    Plane limitPlane;
    public static int _loadLevel = MatchEvents.staticLoadLevel+1;
    public MyEvent isLoadedEvent = new MyEvent(nameof(SideOfField)+ " "+nameof(isLoadedEvent));
    public Transform backTransform, forwardTransform, rightTransform, leftTransform;

    public Plane backPlane, rightPlane, leftPlane, forwardPlane;
    public float sideOfFieldLenght { get; set; }
    public float sideOfFieldWidth { get; set; }
    private void Start()
    {
        MatchEvents.setMainBall.AddListenerConsiderInvoked(setup);
        
    }
    void setupPlane(Transform transform,ref Plane plane)
    {
        plane = new Plane(transform.forward, transform.position);
    }
    public bool pointIsInside(Vector3 point)
    {
        return limitPlane.GetSide(point);
    }
    bool checkEnable()
    {
        if(bigArea != null && goalComponents!=null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void loadPlanes()
    {
        setupPlane(backTransform, ref backPlane);
        setupPlane(forwardTransform, ref forwardPlane);
        setupPlane(rightTransform, ref rightPlane);
        setupPlane(leftTransform, ref leftPlane);
        sideOfFieldLenght = backPlane.GetDistanceToPoint(forwardTransform.position);
        sideOfFieldWidth = rightPlane.GetDistanceToPoint(leftTransform.position);
    }
    void setup()
    {
        if (checkEnable())
        {
            float ballRadio = MatchComponents.ballComponents.radio;
            Vector3 offset = new Vector3(-ballRadio, 0, -ballRadio);

            bigArea.buildArea(offset);
            List<CornerComponents> _corners = MyFunctions.GetComponentsInChilds<CornerComponents>(gameObject, true, true);
            Vector3 offset2 = new Vector3(0, 0, -ballRadio);
            foreach (var corner in _corners)
            {
                corner.BuildPlanes(offset2);
            }
            goalComponents.ballGoesInsidePlane.buildPlanes(offset2);
            goalComponents.goalPlane.buildPlanes(offset2);
            corners.AddRange(_corners);
            if (limit != null)
            {
                limitPlane = new Plane(limit.forward, limit.TransformPoint(Vector3.forward * ballRadio));
            }
            loadPlanes();
            isLoadedEvent.Invoke();
        }
    }
}
