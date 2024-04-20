using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PruebasKickBall : MonoBehaviour
{
    public float scaleTime;
    public float pruebaY;
    public float pruebaForce;
    public Transform dirPos;
    public Rigidbody ballRig { get => MatchComponents.ballComponents.rigBall; }
    public GetChaserToReachTheTargetOrder GetTheFirstChaserToReachTheTarget;
    public PhysicMaterial footballFieldPhysicMaterial;
    public int resultsSize=1;
    public float maxJumpForceFieldPlayer = 5;
    public float maxGoalkeeperHeight=5;
    public Area area;
    SegmentedPath segmentedPath;
    Vector3 maxHeightV3;
    public bool drawGizmos;
    public bool drawText;
    public bool setBallPositionInDirPos;
    public Transform transPruebaTimeToReachY;
    public Transform handTransform;
    public Transform areaLine1, areaLine2;
    Vector3 initHandPosition;
    void Start()
    {
#if UNITY_EDITOR
        SceneView.duringSceneGui += view =>
        {
            var e = Event.current;
            if (e != null && e.keyCode == KeyCode.K && e.type == EventType.KeyDown)
            {
                kick();
            }
        };
#endif
    }
    void prueba()
    {
        Vector3 v1 = dirPos.forward;
        Path path = new Path(transPruebaTimeToReachY.position, transPruebaTimeToReachY.position+ transPruebaTimeToReachY.forward*100, transPruebaTimeToReachY.forward* pruebaForce, 0);
        List<float> ts = path.timeToReachY(pruebaY, 1);
        if (ts.Count > 0)
        {
            Debug.DrawLine(transPruebaTimeToReachY.position, transPruebaTimeToReachY.position+ transPruebaTimeToReachY.forward  * ts[0]);
            Vector3 pos = transPruebaTimeToReachY.position + transPruebaTimeToReachY.forward* pruebaForce * ts[0];
            /*ballRig.position = transPruebaTimeToReachY.position;
            ballRig.velocity = transPruebaTimeToReachY.forward * pruebaForce;
            Invoke(nameof(printY), ts[0]);*/
        }
    }
    void printY()
    {
        print(ballRig.position.y);
    }
    void kick()
    {
        //PruebasTimeScale.setScale(KeyCode.U);
        //buildSegmentedPath();
        StartCoroutine(segmentedPath.tracingPath());
        Time.timeScale = scaleTime;
        if(setBallPositionInDirPos)
            ballRig.position = dirPos.position;
        //ballRig.velocity = dirPos.forward * pruebaForce;
        ballRig.AddForce(dirPos.forward * pruebaForce, ForceMode.VelocityChange);
        ballRig.angularVelocity = Vector3.zero;
        ballRig.isKinematic = false;
        if (initHandPosition == Vector3.zero)
        {
            initHandPosition = handTransform.position;
        }
        handTransform.position = initHandPosition;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            kick();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            ballRig.position = dirPos.position;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            prueba();
        }
        //print(area.PointIsInside(ballRig.position));
        //lineAreaPrueba();
    }
    void lineAreaPrueba()
    {
        MyLine myLine = new MyLine(areaLine1.position, areaLine2.position);
        MyLine myLine2;
        MatchComponents.footballField.fullFieldArea.GetLine(myLine,out myLine2);
        Debug.DrawLine(myLine2.pos0,myLine2.posf);
        Vector3 result;
        Ray ray1 = new Ray(areaLine2.position, areaLine1.position - areaLine2.position );
        Ray ray2 = new Ray(areaLine2.position, areaLine2.forward);
        MatchComponents.footballField.fullFieldArea.GetPoint(areaLine1.position, ray1, out result);
        //MatchComponents.footballField.fullFieldArea.GetIntercession(ray, out result);
        DrawArrow.ForDebug(result + Vector3.up, -Vector3.up, Color.green);
    }
    private void OnDrawGizmos()
    {
        if (drawGizmos && MatchComponents.ballComponents!=null)
        {
            return;
            SphereCollider sphereCollider;
            ballRig.TryGetComponent(out sphereCollider);
            float radius = sphereCollider.radius * ballRig.transform.localScale.x;
            DrawArrow.ForGizmo(MyFunctions.setYToVector3(dirPos.position, radius), dirPos.forward, Color.blue, 0.25f, 20);
            buildSegmentedPath();
            drawReachTargetResults();
        }
    }

    void buildSegmentedPath()
    {
        SphereCollider sphereCollider;
        ballRig.TryGetComponent(out sphereCollider);
        float radius = sphereCollider.radius * ballRig.transform.localScale.x;
        float drag = ballRig.drag;
        PhysicMaterial ballPhysicsMaterial = sphereCollider.material;
        //PhysicMaterial footballFieldPhysicMaterial = MatchComponents.footballFieldPhysicMaterial;
        float bounciness = GetPhysicMaterialCombine.getCombination(ballPhysicsMaterial.bounciness, footballFieldPhysicMaterial.bounciness, GetPhysicMaterialCombine.getPhysicMaterialCombine(ballPhysicsMaterial.bounceCombine, footballFieldPhysicMaterial.bounceCombine));
        float dynamicFriction = GetPhysicMaterialCombine.getCombination(ballPhysicsMaterial.dynamicFriction, footballFieldPhysicMaterial.dynamicFriction, GetPhysicMaterialCombine.getPhysicMaterialCombine(ballPhysicsMaterial.frictionCombine, footballFieldPhysicMaterial.frictionCombine));
        ParabolaWithDrag trajectory;
        Vector3 adjustedPoint;
        if (ballRig.velocity.magnitude == 0)
        {
            if(dirPos.position.y< radius)
            {
                adjustedPoint = MyFunctions.setYToVector3(dirPos.position, radius);
            }
            else
            {
                adjustedPoint = dirPos.position;
            }
            trajectory = new ParabolaWithDrag(adjustedPoint, dirPos.forward * pruebaForce, 0, 9.81f, drag);
        }
        else
        {
            trajectory = new ParabolaWithDrag(ballRig.position, ballRig.velocity, 0, 9.81f, drag);
            adjustedPoint = ballRig.position;
        }
        //StraightXZDragPath straightXZDragPath = new StraightXZDragPath(adjustedPoint, adjustedPoint + dirPos.forward * 3, dirPos.forward * pruebaForce, dirPos.forward * pruebaForce, 0, 3, drag);
        StraightXZDragPath straightXZDragPath = new StraightXZDragPath(drag);
        BouncyPath bouncyPath = new BouncyPath(trajectory, straightXZDragPath, radius,0.1f, bounciness, dynamicFriction);
        segmentedPath = new SegmentedPath(bouncyPath);


        //segmentedPath = new SegmentedPath(straightXZDragPath);
        //print("aaa");
        segmentedPath.buildPath(0, GetTheFirstChaserToReachTheTarget.timeRange, GetTheFirstChaserToReachTheTarget.timeIncrement, GetTheFirstChaserToReachTheTarget.minAngle, GetTheFirstChaserToReachTheTarget.minVelocity, GetTheFirstChaserToReachTheTarget.maxAngle, GetTheFirstChaserToReachTheTarget.maxVelocity);
        //print("bbb");
    }

    void drawReachTargetResults()
    {
        if (segmentedPath == null)
        {
            return;
        }
        segmentedPath.DrawPath("", pruebaForce, 0.05f, drawText);
        PublicFieldPlayerData[] list2 = FindObjectsOfType<PublicFieldPlayerData>();
        foreach (var item in list2)
        {
            break;
            item.maximumJumpForce = maxJumpForceFieldPlayer;
            item.maxSpeedVar = new Variable<float>(10.5f);
            //print(item.name+" | "+ item.maximumJumpHeight);
        }
        PublicGoalkeeperData[] list3 = FindObjectsOfType<PublicGoalkeeperData>();
        foreach (var item in list3)
        {
            break;
            GoalkeeperCtrl goalkeeperCtrl = item.GetComponent<GoalkeeperCtrl>();

            item.addMaximumJumpHeight(maxGoalkeeperHeight, goalkeeperCtrl.Area);
            item.maxSpeedVar = new Variable<float>(5);
        }
        PublicPlayerData[] list = FindObjectsOfType<PublicPlayerData>();
        
        foreach (var item in list)
        {
            SortedList<float, Vector3> results;
            int count = 0;
            foreach (var maximumJumpHeight in item.maximumJumpHeights)
            {
                segmentedPath.getOptimalPointForReachTargetWithTimeSegmented(item.bodyTransform.position, item.maxSpeed, maximumJumpHeight.Key, out results);

                foreach (var item2 in results)
                {
                    if (item.maximumJumpHeightIsInArea(maximumJumpHeight.Key, item2.Value))
                    {
                        if (count < resultsSize)
                        {
                            count++;
                            Color color = Color.green;
                            if (item2.Value.y <= maximumJumpHeight.Key)
                            {
                                color = Color.green;

                            }
                            else
                            {
                                color = Color.red;
                            }
                            //Debug.DrawLine(item.bodyTransform.position, item2.Value, color);
                        }
                    }
                }
            }
           
        }
    }
    void drawReachTargetResultsConsideringTheMaxHeight()
    {

    }
}
