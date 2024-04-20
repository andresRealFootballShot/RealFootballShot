using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class pruebasTrajectoria : MonoBehaviour
{
    public Transform chaser;
    public float chaserSpeed;
    public Rigidbody ballRigidbody;
    public Transform dir,impactPointTrans;
    PhysicMaterial physicMaterial;
    public float force;
    public float height;
    public float time;
    public float drag;
    List<Vector3> list = new List<Vector3>();
    List<BouncyPathData> list2 = new List<BouncyPathData>();
    SegmentedPath segmentedPath;
    ParabolaWithDrag trajectory;
    BouncyPath bouncyPath;
    Vector3 initBallPosition;
    public bool computer;
    public float timeRange,timeIncrement,minAngle, maxAngle, minVelocity, maxVelocity;
    public float a;
    Vector3 optimalPoint;
    float optimalPointTime;
    Vector3 firstOptimalPoint;
    float firstOptimalPointTime;
    Vector3 initChaserPosition;
    SortedList<float,Vector3> results;
    public bool buildSegmentedPathContinuous;
    void Start()
    {

        drag = ballRigidbody.drag;
        //velocidadAlturaWithDrag2();
        physicMaterial = ballRigidbody.GetComponent<SphereCollider>().material;

        SortedList<float,string> p = new SortedList<float, string>();
        p.Add(1.1f, "a");
        p.Add(0.5f, "b");
        p.Add(2.2f, "c");
        ballRigidbody.useGravity = false;
        ballRigidbody.position = dir.position;
        initBallPosition = ballRigidbody.position;
        initChaserPosition = chaser.position;
        //print(p[0.5f]);
        //buildSegmentedPath();
    }
    void parabola()
    {
        //buildSegmentedPath();
        if (Input.GetKeyDown(KeyCode.G))
        {
            chaser.position = initChaserPosition;
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            ballRigidbody.useGravity = true;
            ballRigidbody.position = dir.position;
            
            ballRigidbody.velocity = dir.forward * force;
            buildSegmentedPath(ref firstOptimalPoint,ref firstOptimalPointTime);
            StartCoroutine(optimalPointCoroutine(firstOptimalPointTime));
        }
        for (int i = 0; i < Mathf.Clamp(list.Count-1,0, list.Count); i++)
        {
            //Debug.DrawLine(list[i], list[i+1]);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {

            //velocidadAlturaWithDrag2();
        }
    }
    void buildParabolaWithDrag()
    {
        trajectory = new ParabolaWithDrag(initBallPosition, dir.forward * force, 0, 9.81f, drag);
        for (int i = 0; i < 10; i++)
        {
            float t = (float)(i * 3) / 10;
            trajectory.getPositionAtTime(t);
            trajectory.getVelocityAtTime(t);
        }
        
    }
    void buildSegmentedPath(ref Vector3 optimalPoint,ref float optimalPointTime)
    {
        SphereCollider sphereCollider = ballRigidbody.GetComponent<SphereCollider>();
        float radius = sphereCollider.radius * ballRigidbody.transform.localScale.magnitude;
        if (ballRigidbody.velocity.magnitude == 0)
        {
            trajectory = new ParabolaWithDrag(dir.position, dir.forward * force, 0, 9.81f, drag);
        }
        else
        {
            trajectory = new ParabolaWithDrag(ballRigidbody.position, ballRigidbody.velocity, 0, 9.81f, drag);
        }
       
        bouncyPath = new BouncyPath(trajectory, new StraightXZDragPath(drag), radius,0.1f, physicMaterial.bounciness, physicMaterial.dynamicFriction);
        segmentedPath = new SegmentedPath(bouncyPath);
        segmentedPath.getOptimalPointForReachTargetWithTimeSegmented(chaser.position, chaserSpeed, timeRange, timeIncrement, minAngle,minVelocity,maxAngle,maxVelocity,out results);
        if (results.Count > 0)
        {
            optimalPoint = results.Values[0];
            optimalPointTime =results.Keys[0];
        }
        else
        {
            optimalPoint = Vector3.zero;
            optimalPointTime = 0;
        }
    }
    IEnumerator optimalPointCoroutine(float duration)
    {
        float time = 0;
        while (time < duration)
        {
            chaser.position += (optimalPoint - chaser.position).normalized * chaserSpeed * Time.deltaTime;
            yield return null;
            time += Time.deltaTime;
        }
        chaser.position = initChaserPosition;
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying && segmentedPath!=null)
        {
            if (buildSegmentedPathContinuous)
            {
                buildSegmentedPath(ref optimalPoint, ref optimalPointTime);
            }
            foreach (Vector3 result in results.Values)
            {
                Debug.DrawLine(chaser.position, result, Color.magenta);
            }
            drawFirstOptimalPoint();
            segmentedPath.DrawPath("", force, 0.05f,true);
            //Debug.DrawLine(chaser.position, optimalPoint);
        }
    }
    void drawFirstOptimalPoint()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 10;
        style.normal.textColor = Color.red;
#if UNITY_EDITOR
        //Handles.Label(Pos0 + Vector3.up * 0.5f, info + " | Position=" + Pos0.ToString() + " | Velocity=" + V0.magnitude.ToString(), style);
        Handles.Label(firstOptimalPoint + Vector3.up * 0.4f, "First optimal point", style);
    
#endif
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(firstOptimalPoint, 0.1f);
    }
    IEnumerator coroutineBounceData(float duration,BouncyPathData data)
    {
        yield return new WaitForSeconds(duration);
        print("time="+ duration + " | BouncyPathData ball position =" + data.position + " | ball velocity=" + data.velocity + " | velocity magnitude=" + data.velocity.magnitude);
        print("ball position =" + ballRigidbody.position + " | ball velocity=" + ballRigidbody.velocity + " | velocity magnitude=" + ballRigidbody.velocity.magnitude);
    }
    void pruebasParabolaWithDrag()
    {
        //trajectory = new Trajectory(Vector3.up * -9.8f, dir.normalized * force, ballRigidbody.position);
        //trajectory = new Trajectory(Vector3.up * -9.8f, dir.forward * force, ballRigidbody.position);
        //ballRigidbody.AddForce(dir.forward * force, ForceMode.VelocityChange);
        ballRigidbody.position = MyFunctions.setYToVector3(ballRigidbody.position, height);
        ballRigidbody.velocity = dir.forward * force;
        //Vector3 pos = dir.forward * 2;
        ParabolaWithDrag trajectory = new ParabolaWithDrag(ballRigidbody.position, dir.forward * force, 0, 9.81f, drag);

        //float t = trajectory.getTimeOfVelocity(0);
        float t = 0;
        float maximumYTime = trajectory.getMaximumYTime();
        float yd = trajectory.getVelocityAtTime(t).magnitude;
        float maximumY = trajectory.getMaximumY();
        Vector3 pos1 = trajectory.getPositionAtTime(t);
        Vector3 pos2 = trajectory.getPositionAtTime(t + 0.01f);
        float m = (pos2.y - pos1.y) / 0.01f;
        print("maximumYTime=" + maximumYTime);
        print("maximum=" + maximumY);
        print("t=" + t + " | yd=" + yd + " | pos1=" + pos1 + " | m=" + m);
        Invoke(nameof(printBallData), t);
    }
    void printBallVelocity()
    {
        SphereCollider sphereCollider = ballRigidbody.GetComponent<SphereCollider>();
        float radius = sphereCollider.radius * ballRigidbody.transform.localScale.magnitude;
        if (ballRigidbody.position.y < radius + 0.01f)
        {
            float vy = ballRigidbody.velocity.y*physicMaterial.bounciness;
            print("velocity=" + ballRigidbody.velocity + " | velocity2="+vy);
        }
    }
    void printBallData()
    {
        print("ball position =" + ballRigidbody.position + " | ball velocity=" + ballRigidbody.velocity + " | velocity magnitude=" + ballRigidbody.velocity.magnitude);
    }
    private void FixedUpdate()
    {

        //printBallVelocity();
        //disparoParabolicoForce3();
        //print(ballRigidbody.velocity.magnitude);
    }
    void disparoParabolicoForce3()
    {
        var direction = ballRigidbody.velocity;
        Vector3 dirVelocityY = -Vector3.up * ballRigidbody.velocity.y;
        Vector3 dragDirection = MyFunctions.setYToVector3(ballRigidbody.velocity.normalized, 0);
        float vt = (ballRigidbody.mass * 9.81f) / drag;
        Vector3 velocityV2 = new Vector3(ballRigidbody.velocity.x,0, ballRigidbody.velocity.z);
        //Vector2 vxz = -velocityV2 * (9.81f / vt);
        //float v = ballRigidbody.velocity.magnitude;
        float v = velocityV2.magnitude;
        //var forceAmount = v* drag*ballRigidbody.mass;
        var forceAmount = v * drag * ballRigidbody.mass;
        //ballRigidbody.AddForce((-direction.normalized * forceAmount) - (Vector3.up * 9.81f * ballRigidbody.mass));
        ballRigidbody.AddForce((-velocityV2.normalized * forceAmount) - (Vector3.up * 9.81f * ballRigidbody.mass));
    }
    public bool timeToReachHeightSolution1(float height,out float solution)
    {
        float a, b, c;
        a = -9.81f / 2;
        b = (dir.forward * force).y;
        c = ballRigidbody.position.y - height;
        float solution1, solution2;
        bool result = MyFunctions.SolveQuadratic(a, b, c, out solution1, out solution2);
        Debug.Log("timeToReachHeight | solution1=" + solution1 + " | " + solution2);
        solution = solution1;
        return result;
    }
    public bool timeToReachHeightSolution2(float height, out float solution)
    {
        float a, b, c;
        a = -9.81f / 2;
        b = (dir.forward * force).y;
        c = ballRigidbody.position.y - height;
        float solution1, solution2;
        bool result = MyFunctions.SolveQuadratic(a, b, c, out solution1, out solution2);
        Debug.Log("timeToReachHeight | solution1=" + solution1 + " | solution2=" + solution2);
        solution = solution2;
        return result;
    }
    void disparoParabolicoForce2()
    {
        var direction = ballRigidbody.velocity;
        Vector3 dirVelocityY = -Vector3.up * ballRigidbody.velocity.y;
        Vector3 dragDirection = MyFunctions.setYToVector3(-ballRigidbody.velocity.normalized, 0);
        float vt = (ballRigidbody.mass * 9.81f) / drag;
        Vector2 velocityV2 = new Vector2(ballRigidbody.velocity.x, ballRigidbody.velocity.z);
        //Vector2 vxz = -velocityV2 * (9.81f / vt);
        //float v = ballRigidbody.velocity.magnitude;
        float v = direction.magnitude;
        //var forceAmount = v* drag*ballRigidbody.mass;
        var forceAmount = v * drag * ballRigidbody.mass;
        //ballRigidbody.AddForce((-direction.normalized * forceAmount) - (Vector3.up * 9.81f * ballRigidbody.mass));
        ballRigidbody.AddForce((-direction.normalized * forceAmount) - (Vector3.up * 9.81f * ballRigidbody.mass));
    }
    void disparoParabolicoForce()
    {
        var direction = ballRigidbody.velocity;
        Vector3 dirVelocityY = -Vector3.up * ballRigidbody.velocity.y;
        Vector3 dragDirection = MyFunctions.setYToVector3(-ballRigidbody.velocity.normalized, 0);
        float vt = (ballRigidbody.mass * 9.81f) / drag;
        Vector2 velocityV2 = new Vector2(ballRigidbody.velocity.x, ballRigidbody.velocity.z);
        //Vector2 vxz = -velocityV2 * (9.81f / vt);
        //float v = ballRigidbody.velocity.magnitude;
        float v = direction.magnitude;
        //var forceAmount = v* drag*ballRigidbody.mass;
        var forceAmount = v * drag ;
        //ballRigidbody.AddForce((-direction.normalized * forceAmount) - (Vector3.up * 9.81f * ballRigidbody.mass));
        ballRigidbody.AddForce((-direction.normalized * forceAmount) - (Vector3.up * 9.81f * ballRigidbody.mass));
    }
    void pruebaTrajectoryWithXZDrag()
    {
        //trajectory = new Trajectory(Vector3.up * -9.8f, dir.normalized * force, ballRigidbody.position);
        //trajectory = new Trajectory(Vector3.up * -9.8f, dir.forward * force, ballRigidbody.position);
        //ballRigidbody.AddForce(dir.forward * force, ForceMode.VelocityChange);
        //ballRigidbody.position = MyFunctions.setYToVector3(ballRigidbody.position, height);
        ballRigidbody.velocity = dir.forward * force;
        Vector3 pos = dir.forward * 2;
        TrajectoryWithXZDrag trajectory = new TrajectoryWithXZDrag(9.81f, dir.forward * force, ballRigidbody.position, drag);

        float t1;
        trajectory.timeToReachPositionXZ(pos, out t1);
        print("t1=" + t1 + " | position=" + (pos + ballRigidbody.position));
        Invoke(nameof(printBallData), t1);
        //float timeOfFlight = getTimeOfFlight();
        float timeOfFlight = trajectory.getImpactTime();
        float timeToReachHight;
        SphereCollider sphereCollider = ballRigidbody.GetComponent<SphereCollider>();
        float radius = sphereCollider.radius * ballRigidbody.transform.localScale.magnitude;
        //print("a " + trajectory.getMaximumHeight());
        trajectory.timeToReachHeight(trajectory.getMaximumHeight(), out timeToReachHight, false);
        //timeToReachHeightSolution2(0,out timeToReachHight);
        Vector3 endPosition = trajectory.getPositionAtTime(timeToReachHight);
        Vector3 endVelocity = trajectory.getVelocity(timeToReachHight);
        impactPointTrans.position = endPosition;
        //print("timeOfFlight=" + timeOfFlight + " | " + "timeToReachHeight=" + timeToReachHight + " | " + "endPosition=" + endPosition + " | " + "endVelocity=" + endVelocity.magnitude);

        //Invoke(nameof(printBallData), timeToReachHight);
    }
    Vector3 getParabolicShotPosition(float t)
    {
        //float angle = Vector3.Angle(dir.forward,MyFunctions.setYToVector3(dir.forward,0));
        Vector3 velocity = dir.forward * force;
        Vector2 vx0 = new Vector2(velocity.x, velocity.z);
        //Vector2 horizontalPosition = (initVelocity / drag) * (1 - Mathf.Exp(-drag*time));
        
        float g = 9.81f;
        float vy0 = (dir.forward * force).y;
        float vf = g / drag;
        float ekt = Mathf.Exp(-drag * t);
        
        Vector2 x = (vx0 / drag) * (1 - ekt);
        //float y = (-vf * t) + (((vy0 + vf) / drag) * (1 - ekt));
        float y = (-g/ 2) * (t * t) + vy0 * t ;
        Vector3 endPosition = new Vector3(x.x, y, x.y) + ballRigidbody.position;
        
        print("calculated end position=" + endPosition);
        return endPosition;
    }
    Vector3 getParabolicShotPosition2(float t)
    {
        //float angle = Vector3.Angle(dir.forward,MyFunctions.setYToVector3(dir.forward,0));
        Vector3 velocity = dir.forward * force;
        Vector2 vx0 = new Vector2(velocity.x, velocity.z);
        //Vector2 horizontalPosition = (initVelocity / drag) * (1 - Mathf.Exp(-drag*time));

        float g = 9.81f;
        float vy0 = (dir.forward * force).y;
        float vt = (ballRigidbody.mass * g) / drag;
        float ekt = Mathf.Exp((-g * t) / vt);
        Vector2 x = (vx0 * vt / g) * (1 - ekt);
        float y = ((vt / g) * (vy0 + vt) * (1 - Mathf.Exp((-g * t) / vt))) - (vt * t);
        //print("vt/g="+(vt / g));
        Vector3 endPosition = new Vector3(x.x, y, x.y) + ballRigidbody.position;

        print("calculated end position=" + endPosition + " | vt="+(vt));
        return endPosition;
    }
    Vector3 getParabolicShotVelocity(float t)
    {
        Vector3 velocity = dir.forward * force;
        float g = 9.81f;
        float ekt = Mathf.Exp(-drag * t);
        Vector2 vx0 = new Vector2(velocity.x, velocity.z);
        float vy0 = (dir.forward * force).y;
        float vf = g / drag;
        Vector2 vx = vx0 * Mathf.Exp(-drag * t);
        float vy = vy0 - g*t;
        Vector3 endVelocity = new Vector3(vx.x, vy, vx.y);
        print("v(t)=" + endVelocity.magnitude);
        return endVelocity;
    }
    Vector3 getParabolicShotVelocity2(float t)
    {
        Vector3 velocity = dir.forward * force;
        float g = 9.81f;
        float ekt = Mathf.Exp(-drag * t);
        Vector2 vx0 = new Vector2(velocity.x, velocity.z);
        float vy0 = (dir.forward * force).y;
        float vf = g / drag;
        Vector2 vx = vx0 * Mathf.Exp(-drag * t);
        float vy = -vf + (vy0 + vf) * ekt;
        Vector3 endVelocity = new Vector3(vx.x, vy, vx.y);
        print("v(t)=" + endVelocity.magnitude);
        return endVelocity;
    }
    float getTimeOfFlight()
    {
        Vector3 velocity = dir.forward * force;
        Vector2 vx0 = new Vector2(velocity.x, velocity.z);
        
        float g = 9.81f;
        float tf = (2 * velocity.y) / g;
        print("time of flight=" + tf);
        return tf;
    }
    Vector3 calculateParabolicShotPosition2()
    {
        //float angle = Vector3.Angle(dir.forward,MyFunctions.setYToVector3(dir.forward,0));
        Vector3 velocity = dir.forward * force;
        Vector2 vx0 = new Vector2(velocity.x, velocity.z);
        //Vector2 horizontalPosition = (initVelocity / drag) * (1 - Mathf.Exp(-drag*time));
        Vector2 vx = vx0 * Mathf.Exp(-drag * time);
        float g = 9.81f;
        float vy0 = (dir.forward * force).y;
        float vt = (ballRigidbody.mass* g) / drag;
        float ekt = Mathf.Exp((-g * time)/vt);
        Vector2 x = (vx0*vt / g) * (1 - ekt);
        //Vector2 x = (vx0 / drag) * (1 - Mathf.Exp(-drag * time));
        float y = ((vt / g) * (vy0 + vt) * (1 - Mathf.Exp((-g * time) / vt))) - (vt * time);
        Vector3 endPosition = new Vector3(x.x, y, x.y) + ballRigidbody.position;
        //Vector3 endVelocity = new Vector3(vx.x, vy, vx.y);
        print("calculated end position=" + endPosition);
        //print("calculated end position=" + endPosition + " | " + endVelocity.magnitude);
        return endPosition;
    }
    void calculateHorintalVelocityWithAirResistance()
    {
        float drag = ballRigidbody.drag;
        Vector2 initVelocity = new Vector2(dir.forward.x, dir.forward.z) * force;
        Vector2 endVelocity = initVelocity * Mathf.Exp(-drag * time);
        print("calculateVelocityDrag=" + endVelocity.magnitude);
    }

    float calculateYVelocityWithDrag()
    {
        //float angle = Vector3.Angle(dir.forward, MyFunctions.setYToVector3(dir.forward, 0)) * Mathf.Deg2Rad;
        float v0 = (dir.forward * force).y;

        float g = 9.81f;
        //float y = ((1 / drag) * ((g / drag) + v0) * (1 - Mathf.Exp(-drag * time))) - ((g / drag) * time);
        float y = (((g / drag) + v0) * Mathf.Exp(-drag * time)) - (g / drag);
        return y;
    }
    Vector3 calculateParabolicShotPosition3()
    {
        float angle = Vector3.Angle(dir.forward, MyFunctions.setYToVector3(dir.forward, 0)) * Mathf.Deg2Rad;
        Vector3 velocity = dir.forward * force;
        //Vector2 initVelocity = new Vector2(velocity.x, velocity.z) * Mathf.Cos(angle);
        Vector2 initVelocity = new Vector2(velocity.x, velocity.z);
        //float vy0 = velocity.y * Mathf.Sin(angle);
        float vy0 = velocity.y;
        float g = 9.81f;
        float vt = g/drag;
        //Vector2 horizontalPosition = (initVelocity / drag) * (1 - Mathf.Exp(-drag*time));
        Vector2 vxz = initVelocity * Mathf.Exp(-drag*time);
        float vy = -vt + ((vy0 + vt) * Mathf.Exp(-drag * time));
        Vector3 vxyz = new Vector3(vxz.x,vy,vxz.y);
        print("vxyz =" + vxyz);
        Vector2 horizontalPosition = (initVelocity / drag) * (1-Mathf.Exp(-drag*time));
        float y = (-vt*time)+ ((vy0+vt)/drag)*(1-Mathf.Exp(-drag/time));
        Vector3 endPosition = new Vector3(horizontalPosition.x, y, horizontalPosition.y) + ballRigidbody.position;
        print("calculated end position=" + endPosition + " | y="+y);
        return endPosition;
    }
    Vector3 calculateParabolicShotPosition22()
    {
        //float angle = Vector3.Angle(dir.forward, MyFunctions.setYToVector3(dir.forward, 0));
        Vector3 velocity = dir.forward * force;
        Vector2 initVelocity = new Vector2(velocity.x, velocity.z);
        float vy = velocity.y;
        float g = 9.81f;
        float vt = (ballRigidbody.mass * g)/ drag;
        //Vector2 horizontalPosition = (initVelocity / drag) * (1 - Mathf.Exp(-drag*time));
        Vector2 horizontalPosition = ((vt* initVelocity)/g) *(1- Mathf.Exp((-time*g) * vt));
        float y = ((vt / g) * (vy + vt) * (1 - Mathf.Exp((-g * time) / vt))) - (vt * time);
        Vector3 endPosition = new Vector3(horizontalPosition.x, y, horizontalPosition.y) + ballRigidbody.position;
        print("calculated end position=" + endPosition);
        return endPosition;
    }
    void caidaLibre()
    {
        var direction = -ballRigidbody.velocity.normalized;
        float vt = ballRigidbody.mass * 9.81f / drag;
        Vector2 vxz = -ballRigidbody.velocity * 9.81f / vt;
        float vy = -9.81f * (1 + (ballRigidbody.velocity.y / vt));
        float v = ballRigidbody.velocity.magnitude;
        //Vector3 v = new Vector3(vxz.x, vy, vxz.y);
        var forceAmount = v * v * drag;
        //var forceAmount = v * drag;
        //var forceAmount = v * drag*ballRigidbody.mass;
        ballRigidbody.AddForce(direction * forceAmount - (Vector3.up * 9.81f * ballRigidbody.mass));
    }
    void velocidadAlturaWithDrag2()
    {
        ballRigidbody.position = MyFunctions.setYToVector3(ballRigidbody.position, height);
        SphereCollider sphereCollider = ballRigidbody.GetComponent<SphereCollider>();
        float _height = ballRigidbody.position.y - sphereCollider.radius;
        float g = 9.81f;
        float v0 = 0;
        float vt = Mathf.Sqrt((ballRigidbody.mass * 9.81f) / drag);
        float vtPow2 = vt * vt;
        float c = (g * time) / vt;
        float a = ((v0 / vt) * sinh(c)) + cosh(c);
        float h = (vtPow2 / g) * Mathf.Log(a);
        float velocity = vt * tanh((g * time) / vt);
        print("velocity=" + velocity + " | h="+h);
        //print("vt=" + vt);
        StartCoroutine(coroutine(time));
    }
    IEnumerator coroutine(float duration)
    {
        float time = 0;
        float h0 = ballRigidbody.position.y;
        while (time < duration)
        {
            if (Mathf.Abs(height - ballRigidbody.position.y) < 0.1f)
            {
                //print("ball reach height | time =" + time);
            }
            yield return null;
            time += Time.deltaTime;
        }
        float h = h0 - ballRigidbody.position.y;
        //print("ball velocity =" + ballRigidbody.velocity.magnitude + " | time=" + time + " | h=" + h);
        Vector2 vxz = new Vector2(ballRigidbody.velocity.x,ballRigidbody.velocity.z);
        print("ball position =" + ballRigidbody.position + " | ball velocity="+ballRigidbody.velocity +" | velocity magnitude"+ballRigidbody.velocity.magnitude + " | velocity xz = "+ vxz.magnitude);
        //print("h="+ (h0 - ballRigidbody.position.y));
    }
    void velocidadAltura()
    {
        ballRigidbody.position = MyFunctions.setYToVector3(ballRigidbody.position, height);
        SphereCollider sphereCollider = ballRigidbody.GetComponent<SphereCollider>();
        float _height = ballRigidbody.position.y - sphereCollider.radius;
        float heightTime = Mathf.Sqrt((2* _height) /9.81f);
        float velocity = Mathf.Sqrt(2 * 9.81f * _height);
        float velocity2 = 9.81f * heightTime;
        print("time=" + heightTime + " | velocity=" + velocity + " | velocity2=" + velocity2);
        StartCoroutine(coroutine2());
    }
    float sinh(float x)
    {
        return (Mathf.Exp(x) - Mathf.Exp(-x)) / 2;
    }
    float cosh(float x)
    {
        return (Mathf.Exp(x) + Mathf.Exp(-x)) / 2;
    }
    float tanh(float x)
    {
        return sinh(x) / cosh(x);
    }
    void velocidadAlturaWithDrag()
    {
        ballRigidbody.position = MyFunctions.setYToVector3(ballRigidbody.position, height);
        SphereCollider sphereCollider = ballRigidbody.GetComponent<SphereCollider>();
        float drag = ballRigidbody.drag;
        float _height = ballRigidbody.position.y - sphereCollider.radius;
        float heightTime = Mathf.Sqrt((2 * _height) / 9.81f);
        float vl = Mathf.Sqrt((ballRigidbody.mass * 9.81f) / drag);
        float vlPow2 = vl * vl;
        float v0=0;
        float hf = 0;
        float velocity = vlPow2 + (((v0*v0)+ vlPow2) *Mathf.Exp(((2*9.81f)/ vlPow2)*(hf-_height)));
        velocity = Mathf.Sqrt(velocity);
        //velocity = velocity * Mathf.Exp(-drag * heightTime);
        print("velocity=" + velocity);
        StartCoroutine(coroutine(heightTime));
    }
    void velocidadAltura2()
    {
        SphereCollider sphereCollider = ballRigidbody.GetComponent<SphereCollider>();
        float _height = ballRigidbody.position.y - sphereCollider.radius;
        float heightTime = Mathf.Sqrt((2 * _height) / 9.81f);
        float velocity = Mathf.Sqrt(2 * 9.81f * _height);
        float velocity2 = 9.81f * heightTime;
        print("time=" + heightTime + " | velocity=" + velocity + " | velocity2=" + velocity2);
        StartCoroutine(coroutine2());
    }
    IEnumerator coroutine2()
    {
        float time = 0;
        SphereCollider sphereCollider = ballRigidbody.GetComponent<SphereCollider>();
        while (true)
        {
            if(ballRigidbody.position.y < sphereCollider.radius + 0.01f)
            {
                print("time="+time);
            }
            yield return null;
            time += Time.deltaTime;
        }
        print("ball velocity =" + ballRigidbody.velocity.magnitude + " | time=" + time);
    }
    void Update()
    {
        //calculatePoints();
        parabola();
        //if (ballRigidbody.position.y < 0.15f)
        //print(_time+" "+ballRigidbody.velocity.y);
        //_time += Time.fixedDeltaTime;
        /*print("time="+_time+" | velocity="+ballRigidbody.velocity.magnitude);
        */
    }
}

