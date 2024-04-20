using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    public ComponentsPlayer componentsPlayer;
    public ControllerDistance ctrlDistance;
    public ControllerAim ctrlAim;
    public Transform arrowTrans;
    public GameObject arrowGObj;
    MyRaycastHit myRaycast;
    float radioBall;
    public float decimals;
    Vector2 position,positionMouse;
    Vector3 lastPoint,lastRotation;
    LayerMask layerMask;
    void Start()
    {
        radioBall = Vector3.Distance(componentsPlayer.componentsBall.transBall.position, componentsPlayer.componentsBall.transCenterBall.position);
        position = new Vector2(Screen.width / 2, Screen.height / 2);
        layerMask = LayerMask.GetMask("ObjectGoal");
        myRaycast = componentsPlayer.scriptsPlayer.raycastAim;
        arrowGObj.transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        prueba(Time.deltaTime);
    }
    RaycastHit rayCastHit()
    {
        Ray ray = componentsPlayer.camera.ScreenPointToRay(componentsPlayer.scriptsPlayer.controllerAim.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10 + 0.5f, layerMask))
        {
            
        }
        return hit;
    }
    Vector3 lastRayDirection;
    public void prueba3(float delta)
    {
        
        if (ctrlDistance.isClose())
        {
            float mouseX = Input.GetAxis("Mouse X") * delta * 300;
            float mouseY = Input.GetAxis("Mouse Y") * delta * 300;
            positionMouse += new Vector2(mouseX, mouseY);
            Vector3 ballPosScreen = componentsPlayer.camera.WorldToScreenPoint(componentsPlayer.componentsBall.rigBall.position);
            position = positionMouse + new Vector2(ballPosScreen.x, ballPosScreen.y);

            //Ray ray = componentsPlayer.camera.ScreenPointToRay(position);
            Ray ray = new Ray(componentsPlayer.transCamera.position, componentsPlayer.componentsBall.rigBall.position- componentsPlayer.transCamera.position-Vector3.up*0.05f);
            if (ray.direction != lastRayDirection)
            {
                print(ray.direction * 1000);
            }
            lastRayDirection = ray.direction;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10 + 0.5f))
            {

                arrowGObj.SetActive(true);

                arrowTrans.position = hit.point;
                arrowTrans.rotation = Quaternion.LookRotation(hit.normal);
                //print(componentsPlayer.componentsBall.transBall.InverseTransformPoint(myRaycast.hit.point));
                if (lastPoint != myRaycast.hit.point)
                {

                    //print("Point-" + myRaycast.hit.point * decimals);
                }
                lastPoint = myRaycast.hit.point;

                if (lastRotation != myRaycast.hit.normal)
                {

                    //print("Rotation-" + myRaycast.hit.normal * decimals);
                }
                lastRotation = myRaycast.hit.normal;
            }
            else
            {
                arrowGObj.SetActive(false);
            }
        }
        else
        {
            arrowGObj.SetActive(false);
        }
    }
    public void prueba4(float delta)
    {
        float mouseX = Input.GetAxis("Mouse X") * delta;
        float mouseY = Input.GetAxis("Mouse Y") * delta;
        positionMouse += new Vector2(mouseX, mouseY);
        if (ctrlDistance.isClose())
        {
            if (myRaycast.isHitting)
            {

                arrowGObj.SetActive(true);

                float x = Mathf.Floor(myRaycast.hit.point.x * decimals) / decimals;
                float y = Mathf.Floor(myRaycast.hit.point.y * decimals) / decimals;
                float z = Mathf.Floor(myRaycast.hit.point.z * decimals) / decimals;
                arrowTrans.position = new Vector3(x, y, z);
                x = Mathf.Floor(myRaycast.hit.normal.x * decimals) / decimals;
                y = Mathf.Floor(myRaycast.hit.normal.y * decimals) / decimals;
                z = Mathf.Floor(myRaycast.hit.normal.z * decimals) / decimals;
                arrowTrans.rotation = Quaternion.LookRotation(new Vector3(x,y,z));

                if (lastPoint != myRaycast.hit.point)
                {

                    print("Point-"+myRaycast.hit.point * decimals);
                }
                lastPoint = myRaycast.hit.point;

                if (lastRotation!= myRaycast.hit.normal)
                {

                   print("Rotation-"+myRaycast.hit.normal * decimals);
                }
                lastRotation = myRaycast.hit.normal;
            }
            else
            {
                arrowGObj.SetActive(false);
            }
        }
        else
        {
            arrowGObj.SetActive(false);
        }
    }
    public void prueba2(float delta)
    {
        float mouseX = Input.GetAxis("Mouse X") * delta;
        float mouseY = Input.GetAxis("Mouse Y") * delta;
        positionMouse += new Vector2(mouseX, mouseY);
        if (ctrlDistance.isClose())
        {
            if (myRaycast.isHitting)
            {

                arrowGObj.SetActive(true);

                arrowTrans.position = myRaycast.hit.point;
                arrowTrans.rotation = Quaternion.LookRotation(myRaycast.hit.normal);

                if (lastPoint != myRaycast.hit.point)
                {

                    print(myRaycast.hit.point * 10000);
                }
                lastPoint = myRaycast.hit.point;

                if (lastRotation != myRaycast.hit.normal)
                {

                    //print(myRaycast.hit.normal * 10000);
                }
                lastRotation = myRaycast.hit.normal;
            }
            else
            {
                arrowGObj.SetActive(false);
            }
        }
        else
        {
            arrowGObj.SetActive(false);
        }
    }
    public void prueba(float deltaTime)
    {
        float mouseX = Input.GetAxis("Mouse X") * deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * deltaTime;
        positionMouse += new Vector2(mouseX, mouseY);
        if (ctrlDistance.isClose())
        {
            if (mouseX != 0 || mouseY != 0)
            {
                if (myRaycast.isHitting)
                {
                    
                    arrowGObj.SetActive(true);
                    arrowTrans.position = myRaycast.hit.point;
                    arrowTrans.rotation = Quaternion.LookRotation(myRaycast.hit.normal);
                    lastPoint = myRaycast.hit.point- componentsPlayer.componentsBall.rigBall.position;
                    
                }
                else
                {
                    arrowGObj.SetActive(false);
                }
            }
            else
            {
                if (myRaycast.isHitting)
                {
                    if(lastPoint == Vector3.zero)
                    {
                        arrowTrans.position = myRaycast.hit.point;
                        arrowTrans.rotation = Quaternion.LookRotation(myRaycast.hit.normal);
                        lastPoint = myRaycast.hit.point - componentsPlayer.componentsBall.rigBall.position;
                    }
                    arrowGObj.SetActive(true);
                }
                arrowTrans.position = lastPoint + componentsPlayer.componentsBall.rigBall.position;
            }
        }
        else
        {
            arrowGObj.SetActive(false);
        }
    }
   
}
