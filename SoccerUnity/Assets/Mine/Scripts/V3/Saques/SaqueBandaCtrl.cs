using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class SaqueBandaCtrl : MonoBehaviour
{
    public MatchDataObsolete matchData;
    public BallComponents componentsBall;
    public ComponentsPlayer componentsPlayer;
    public Teams teams;
    public GameObject eventsGObj;
    public PhotonView photonView;
    public Vector3 position;
    public Vector3 posOffset;
    public int actorSaque;
    public float distance;
    bool blockKick;
    bool resetKick;
    void Start()
    {
        Vector3Event[] events = eventsGObj.GetComponents<Vector3Event>();
        foreach (Vector3Event emptyEvent in events)
        {
            if (emptyEvent.typeEvent == Vector3Event.TypeEvent.Kick)
            {
                emptyEvent.Event += Kick;
            }
        }
    }

    public void BallOut(Transform transBanda)
    {
        if (!matchData.saqueDeBanda)
        {
            //componentsBall.rigBall.isKinematic = true;
            componentsBall.rigBall.angularVelocity = Vector3.zero;
            componentsBall.rigBall.velocity = Vector3.zero;
            position = new Vector3(componentsBall.transBall.position.x, 0.01f, componentsBall.transBall.position.z) + transBanda.TransformDirection(posOffset);
            componentsBall.transBall.position = position;
            componentsBall.transBall.eulerAngles = Vector3.zero;
            string teamSaque = teams.getTeamFromActor(matchData.actorWithPosession).TeamName;
            
            if (teams.getTeamFromActor(PhotonNetwork.LocalPlayer.ActorNumber).name == teamSaque && matchData.matchStarted && !matchData.endMatch && matchData.enableGoals)
            {
                eventsGObj.SetActive(false);
            }
            matchData.saqueDeBanda = true;
        }
    }

    public void Kick(Vector3 dir)
    {
        if (matchData.saqueDeBanda)
            photonView.RPC("EnableKick", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }
    private void Update()
    {
        if (matchData.saqueDeBanda&&matchData.matchStarted && !matchData.endMatch && matchData.enableGoals)
        {
            string teamSaque = teams.getTeamFromActor(matchData.actorWithPosession).TeamName;
            if (teams.getTeamFromActor(PhotonNetwork.LocalPlayer.ActorNumber).name == teamSaque)
            {
                Vector3 dir = componentsPlayer.transBody.position - new Vector3(componentsBall.transBall.position.x, componentsPlayer.transBody.position.y, componentsBall.transBall.position.z);
                if (dir.magnitude < distance)
                {
                    componentsPlayer.transBody.position = new Vector3(componentsBall.transBall.position.x, componentsPlayer.transBody.position.y, componentsBall.transBall.position.z) + dir.normalized * distance;
                }
            }
        }
    }
}
