using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Photon.Pun;
public class PruebaMemoria : MonoBehaviour, IPunObservable
{
    public Transform ballTransform, playerTransform,cameraTransform;
    public float maxSpeed,rotationSpeed;
    public float maxDistance,minDistance;
    Vector3 _forwardPlayerBallDirection, _rightPlayerBallDirection;
    Vector2 _axesDirection;
    public AnimationCurve animationCurve;
    public Rigidbody playerRigidbody;
    public Transform angle1, angle2;
    Vector3 forwardDirection,rightDirection;
    Vector3 direction;
    void Start()
    {

    }
    void pruebaAngle()
    {
        Vector3 scale = Vector3.Scale(angle1.forward, angle2.forward);
        float a = scale.x + scale.y + scale.z;
        float b = angle1.forward.magnitude * angle2.forward.magnitude;
        float cos = a / b;
        float angle = Mathf.Acos(cos)*Mathf.Rad2Deg;
        print(Vector3.Angle(angle1.forward, angle2.forward)+" | myAngle="+ angle);
    }
    void pruebaAngle(int q)
    {
        Vector3 scale = Vector3.Scale(angle1.forward, angle2.forward);
        float a = scale.x + scale.y + scale.z;
        float b = angle1.forward.magnitude * angle2.forward.magnitude;
        float cos = a / b;
        float angle = Mathf.Acos(cos) * Mathf.Rad2Deg;
        print(Vector3.Angle(angle1.forward, angle2.forward) + " | myAngle=" + angle);
    }
    // Update is called once per frame
    /*void Update()
    {
        //possessionBall();
        freeCamera();
        
    }*/


    Vector2 axesDirection;
    private void Update()
    {
        lookingBallUpdate();
    }
    private void FixedUpdate()
    {
        lookingBallFixedUpdate();
        rotation();
    }
    void freeCamerUpdate()
    {
        float verticalAxes = Input.GetAxis("Vertical");
        float horizontalAxes = Input.GetAxis("Horizontal");
        axesDirection = new Vector2(horizontalAxes, verticalAxes);
    }
    void freeCamera()
    {
        Vector3 forwardDirection = cameraTransform.forward;
        Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection);
        forwardDirection.Normalize();
        rightDirection.Normalize();
        Vector3 forwardAxesDirection = forwardDirection * axesDirection.y;
        Vector3 rightAxesDirection = rightDirection * axesDirection.x;
        direction = forwardAxesDirection + rightAxesDirection;
        Vector3 velocity = (forwardAxesDirection + rightAxesDirection) * maxSpeed;
        playerRigidbody.MovePosition(playerRigidbody.position + velocity * Time.fixedDeltaTime);
    }
    void rotation()
    {
        float angle = FindAngle(playerTransform.forward,direction);
        Vector3 lerpDirection = Quaternion.AngleAxis(angle * rotationSpeed * Time.fixedDeltaTime, Vector3.up) * playerTransform.forward;
        Quaternion lerpRotation = Quaternion.LookRotation(lerpDirection);
        playerRigidbody.MoveRotation(lerpRotation);
    }
    protected float FindAngle(Vector3 fromVector, Vector3 toVector)
    {
        if (toVector == Vector3.zero)
            return 0;

        float angle = Vector3.Angle(fromVector, toVector);
        Vector3 signo = Vector3.Cross(fromVector, toVector);
        angle *= Mathf.Sign(signo.y);
        return angle;
    }

    void possessionBallFixedUpdate()
    {
        Vector3 forwardPlayerBallDirection = ballTransform.position - playerTransform.position;
        forwardPlayerBallDirection.Normalize();
        float playerBallDistance = Vector3.Distance(ballTransform.position,playerTransform.position);
        float adjustDistance = (playerBallDistance-minDistance)/(maxDistance- minDistance);
        adjustDistance = Mathf.Clamp01(adjustDistance);
        adjustDistance = animationCurve.Evaluate(adjustDistance);
        //Vector3 forwardDirection = forwardPlayerBallDirection * axesDirection.y * Time.deltaTime;
        Vector3 forwardDirection = forwardPlayerBallDirection * adjustDistance;
        playerRigidbody.MovePosition(playerRigidbody.position + forwardDirection * maxSpeed * Time.fixedDeltaTime);
        Quaternion rotation = Quaternion.LookRotation(playerTransform.forward + forwardDirection * rotationSpeed);
        playerRigidbody.MoveRotation(rotation);
    }

    //playerTransform.rotation = Quaternion.LookRotation(playerTransform.forward + forwardDirection* rotationSpeed * Time.deltaTime + rightDirection * rotationSpeed * Time.deltaTime);
    //playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, Quaternion.LookRotation(playerTransform.forward + forwardDirection + rightDirection), rotationSpeed * Time.deltaTime);

    void lookingBallUpdate()
    {

        
        float verticalAxes = Input.GetAxis("Vertical");
        float horizontalAxes = Input.GetAxis("Horizontal");
        axesDirection = new Vector2(horizontalAxes, verticalAxes);
        
        
    }
    private void lookingBallFixedUpdate()
    {
        Vector3 forwardPlayerBallDirection = ballTransform.position - playerTransform.position;
        forwardPlayerBallDirection = new Vector3(forwardPlayerBallDirection.x, 0, forwardPlayerBallDirection.z);
        Vector3 rightPlayerBallDirection = Vector3.Cross(Vector3.up, forwardPlayerBallDirection);
        forwardPlayerBallDirection.Normalize();
        rightPlayerBallDirection.Normalize();
        forwardDirection = forwardPlayerBallDirection * axesDirection.y;
        rightDirection = rightPlayerBallDirection * axesDirection.x;
        direction = forwardDirection + rightDirection;
        Vector3 velocity = forwardDirection * maxSpeed;
        velocity += rightDirection * maxSpeed;
        playerRigidbody.MovePosition(playerRigidbody.position + velocity*Time.fixedDeltaTime);

    }

    /*
       _forwardPlayerBallDirection = forwardPlayerBallDirection;
       _rightPlayerBallDirection = rightPlayerBallDirection;
       _axesDirection = axesDirection;
       DrawArrow.ForDebug(playerTransform.position, forwardPlayerBallDirection, Color.blue);
       DrawArrow.ForDebug(playerTransform.position, rightPlayerBallDirection, Color.red);*/
    private void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.red;
        return;
#if UNITY_EDITOR
        Handles.Label(playerTransform.position+ _rightPlayerBallDirection + Vector3.up * 0.15f, "rightPlayerBallDirection", style);

        style.normal.textColor = Color.blue;
        Handles.Label(playerTransform.position + _forwardPlayerBallDirection + Vector3.up * 0.15f, "forwardPlayerBallDirection", style);
#endif
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.eulerAngles);
        }
        else
        {
            Vector3 position = (Vector3)stream.ReceiveNext();
            Vector3 eulerAngles = (Vector3)stream.ReceiveNext();
        }
    }
}
