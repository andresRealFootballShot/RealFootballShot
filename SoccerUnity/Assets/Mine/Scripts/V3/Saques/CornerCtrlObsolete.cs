using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class CornerCtrlObsolete : MonoBehaviour
{
    public List<CornerObsolete> corners;
    public GameObject eventsGObj;
    public BallComponents componentsBall;
    public ComponentsPlayer componentsPlayer;
    public PhotonView photonView;
    public Teams teams;
    public float distance;
    Vector3 position;
    public MatchDataObsolete matchData;
    public Transform transSaquePuertaRed, transSaquePuertaBlue;
    void Start()
    {
        componentsPlayer = FindObjectOfType<ComponentsPlayer>();
        foreach (CornerObsolete corner in corners)
        {
            corner.CornerEvent += BallOut;
        }
        Vector3Event[] events = eventsGObj.GetComponents<Vector3Event>();
        foreach (Vector3Event emptyEvent in events){
            if (emptyEvent.typeEvent == Vector3Event.TypeEvent.Kick)
            {
                emptyEvent.Event += Kick;
            }
        }
    }

    void BallOut(CornerObsolete corner)
    {
        if (!matchData.saqueCorner)
        {
            string teamSaque = teams.getTeamFromActor(matchData.actorWithPosession).TeamName;
            if (corner.teamName == teamSaque)
            {
                position = corner.transPosCornerBall.position;
            }
            else
            {
                switch (corner.teamName)
                {
                    case "Red":
                        position = transSaquePuertaRed.position;
                        break;
                    case "Blue":
                        position = transSaquePuertaBlue.position;
                        break;
                }
            }
            //componentsBall.rigBall.isKinematic = true;
            componentsBall.rigBall.angularVelocity = Vector3.zero;
            componentsBall.rigBall.velocity = Vector3.zero;
            
            componentsBall.transBall.position = position;
            componentsBall.transBall.eulerAngles = Vector3.zero;
            

            if (teams.getTeamFromActor(PhotonNetwork.LocalPlayer.ActorNumber).name == teamSaque&&matchData.matchStarted && !matchData.endMatch && matchData.enableGoals)
            {
                eventsGObj.SetActive(false);
            }
            matchData.saqueCorner = true;
        }
    }
    public void Kick(Vector3 dir)
    {
        if(matchData.saqueCorner)
            photonView.RPC("EnableKickCorner", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }
    private void Update()
    {
        if (matchData.saqueCorner&&matchData.matchStarted && !matchData.endMatch && matchData.enableGoals)
        {
            string teamSaque = teams.getTeamFromActor(matchData.actorWithPosession).TeamName;
            if (teams.getTeamFromActor(PhotonNetwork.LocalPlayer.ActorNumber).name == teamSaque)
            {
                Vector3 dir =componentsPlayer.transBody.position - new Vector3(componentsBall.transBall.position.x, componentsPlayer.transBody.position.y, componentsBall.transBall.position.z);
                if (dir.magnitude < distance)
                {
                    componentsPlayer.transBody.position = new Vector3(componentsBall.transBall.position.x, componentsPlayer.transBody.position.y, componentsBall.transBall.position.z) + dir.normalized * distance;
                }
            }
        }
    }
}
