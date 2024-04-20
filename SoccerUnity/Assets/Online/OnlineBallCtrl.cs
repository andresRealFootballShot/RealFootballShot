using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class OnlineBallCtrl : MonoBehaviour
{
    public BallComponents componentsBall;
    public float maxDistance = 1;
    public float maxTime = 0.1f;
    float time;
    bool activeTime;
    static float sendBallDataPeriod = 1;
    Coroutine sendBallDataCoroutine;
    static bool _getRoutineData;
    public static bool getRoutineData { get => _getRoutineData; set => setGetRoutineData(value); }
    void Start()
    {
        MatchEvents.kick.AddListener(KickEvent);
        MatchEvents.losePossession.AddListener(losePossession);
        MatchEvents.getPossession.AddListener(getPossession);
    }
    static void setGetRoutineData(bool value)
    {
        _getRoutineData = value;
        OnlineGoalkeeperCtrl.getRoutineData = value;
    }
    void losePossession()
    {
        if (sendBallDataCoroutine != null)
        {
            StopCoroutine(sendBallDataCoroutine);
            sendBallDataCoroutine = null;
        }
    }
    void getPossession()
    {
        if (sendBallDataCoroutine == null)
        {
            sendBallDataCoroutine = StartCoroutine(sendBallData());
        }
    }
    void KickEvent(KickEventArgs args)
    {
        getRoutineData = !args.playerID.Equals("");
    }
    [PunRPC]
    public void AddForceAtPositionRPC(Vector3 position, Vector3 eulerAngles, Vector3 point, Vector3 kickDirection, Vector3 velocity, Vector3 angularVelocity, int onlineActor, int localActor)
    {
        componentsBall.transBall.position = position;
        componentsBall.transBall.eulerAngles = eulerAngles;
        string playerID;
        PublicPlayerDataList.getPlayerID(onlineActor, localActor, out playerID);
        KickEventArgs args = new KickEventArgs(kickDirection, velocity, angularVelocity,point, playerID);
        Kick.AddForceAtPosition(ForceMode.VelocityChange, args);
    }
    [PunRPC]
    public void AddForceRPC(Vector3 position, Vector3 eulerAngles, Vector3 kickDirection, Vector3 velocity, Vector3 angularVelocity, int onlineActor,int localActor)
    {
        Rigidbody ballRigidbody = componentsBall.rigBall;
        componentsBall.transBall.position = position;
        componentsBall.transBall.eulerAngles = eulerAngles;
        //componentsBall.rigBall.AddForce(kickDirection, ForceMode.VelocityChange);
        string playerID;
        PublicPlayerDataList.getPlayerID(onlineActor, localActor, out playerID);
        KickEventArgs args = new KickEventArgs(kickDirection, velocity, angularVelocity,ballRigidbody.position, playerID);
        Kick.AddForce(ForceMode.VelocityChange, args);
        //componentsBall.controllerKickSound.ApplySoundKick(volume);
    }
    [PunRPC]
    public void SendRoutineData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, PhotonMessageInfo info)
    {
        //DebugsList.testing.print("SendRoutineData");
        if (getRoutineData)
        {
            Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
            Transform ballTransform = MatchComponents.ballComponents.transBall;
            float distance = Vector3.Distance(ballTransform.position, position);
            //DebugsList.testing.print("Receive Ball Data");
            if (distance > maxDistance)
            {
                //print("Update Ball Data");
                //DebugsList.testing.print("Update Ball Data");
                ballTransform.position = position;
                ballTransform.rotation = rotation;
                ballRigidbody.velocity = velocity;
                ballRigidbody.angularVelocity = angularVelocity;
            }
        }
    }
    [PunRPC]
    public void SetData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, PhotonMessageInfo info)
    {
        Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        Transform ballTransform = MatchComponents.ballComponents.transBall;
        float distance = Vector3.Distance(ballTransform.position, position);
        //DebugsList.testing.print("Update Ball Data");
        ballTransform.position = position;
        ballTransform.rotation = rotation;
        ballRigidbody.velocity = velocity;
        ballRigidbody.angularVelocity = angularVelocity;
    }
    IEnumerator sendBallData()
    {
        Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        Transform ballTransform = MatchComponents.ballComponents.transBall;
        while (true)
        {
            yield return new WaitForSeconds(sendBallDataPeriod);
            if (!Kick.ballLocked)
            {
                //DebugsList.testing.print("sendBallData");
                MatchComponents.ballComponents.photonViewBall.RPC(nameof(SendRoutineData), RpcTarget.Others, ballTransform.position, ballTransform.rotation, ballRigidbody.velocity, ballRigidbody.angularVelocity);
            }
        }
    }
    /*
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(componentsBall.transBall.position);
            stream.SendNext(componentsBall.transBall.eulerAngles);
            stream.SendNext(componentsBall.rigBall.velocity);
            stream.SendNext(componentsBall.rigBall.angularVelocity);
        }
        else
        {
            Vector3 position = (Vector3)stream.ReceiveNext();
            float distance = Vector3.Distance(componentsBall.transBall.position, position);
            if (distance > maxDistance)
            {
                //print("distance > maxDistance time="+ time);
                activeTime = true;
                //print("aa "+ time);
                if (time > maxTime)
                {
                    //print("time > maxTime");
                    //print("bb");
                    componentsBall.transBall.position = position;
                    componentsBall.transBall.eulerAngles = (Vector3)stream.ReceiveNext();
                    componentsBall.rigBall.velocity = (Vector3)stream.ReceiveNext();
                    componentsBall.rigBall.angularVelocity = (Vector3)stream.ReceiveNext();
                }
            }
            else
            {
                //print("distance < maxDistance");
                time = 0;
                activeTime = false;
            }
        }
    }*/
    void Update()
    {
        if (activeTime)
        {
            time += Time.deltaTime;
        }
        //print("deltaTime2=" + deltaTime2);
    }
}
