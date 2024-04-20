using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class OnlineGoalkeeperCtrl : MonoBehaviourPunCallbacks
{
    bool activeTime;
    static float sendBallDataPeriod = 1;
    Coroutine sendBallDataCoroutine;
    public static bool getRoutineData;

    void Start()
    {
        MatchEvents.kick.AddListener(KickEvent);
        MatchEvents.losePossession.AddListener(losePossession);
        MatchEvents.getPossession.AddListener(getPossession);
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
            sendBallDataCoroutine = StartCoroutine(sendGoalkeeperData());
        }
    }
    void KickEvent(KickEventArgs args)
    {
        getRoutineData = !args.playerID.Equals("");
    }
    IEnumerator sendGoalkeeperData()
    {
        Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
        Transform ballTransform = MatchComponents.ballComponents.transBall;
        while (true)
        {
            yield return new WaitForSeconds(sendBallDataPeriod);
            photonView.RPC(nameof(SendGoalkeepersData), RpcTarget.Others, getGoalkeeperDatas() as object);
        }
    }
    object[] getGoalkeeperDatas()
    {
        List<object> datas = new List<object>();
        foreach (var publicGoalkeeperData in PublicPlayerDataList.goalkeepers.Values)
        {
            List<object> data = new List<object>();
            data.Add(publicGoalkeeperData.playerID);
            data.Add(publicGoalkeeperData.bodyTransform.position);
            data.Add(publicGoalkeeperData.bodyTransform.rotation);
            datas.Add(data.ToArray());
        }
        return datas.ToArray();
    }
    void setGoalkeeperDatas(object[] datas)
    {
        foreach (object[] data in datas)
        {
            int index = 0;
            string playerID = (string) data[index++];
            Vector3 position = (Vector3)data[index++];
            Quaternion rotation = (Quaternion)data[index++];
            PublicPlayerDataList.goalkeepers[playerID].bodyTransform.position = position;
            PublicPlayerDataList.goalkeepers[playerID].bodyTransform.rotation = rotation;
        }
    }
    [PunRPC]
    void SendGoalkeepersData(object datas, PhotonMessageInfo info)
    {
        if (getRoutineData || true)
        {
            setGoalkeeperDatas((object[])datas);
        }
    }
    class GoalkeeperData
    {
        public string playerID;
        public Vector3 position;
        public Vector3 rotation;

        public GoalkeeperData(string playerID, Vector3 position, Vector3 rotation)
        {
            this.playerID = playerID;
            this.position = position;
            this.rotation = rotation;
        }
    }
}
