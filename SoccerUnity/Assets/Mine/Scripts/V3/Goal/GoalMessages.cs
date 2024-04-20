using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class GoalMessages : MonoBehaviourPunCallbacks
{
    MatchDataObsolete matchData;
    public bool activateGoal=true;
    void Start()
    {
        GameObject[] goalTriggers = GameObject.FindGameObjectsWithTag("Goal");
        foreach(GameObject g in goalTriggers)
        {
            //g.GetComponent<GoalEventDeprecated>().goalEvent+= GoalLocal;
        }
        matchData = GameObject.FindGameObjectWithTag("MatchData").GetComponent<MatchDataObsolete>();
       
    }

    // Update is called once per frame
    void GoalLocal(string team)
    {
        if (PhotonNetwork.IsMasterClient && matchData.matchStarted && activateGoal && matchData.enableGoals)
        {
            TimerObsolete timer = GameObject.FindGameObjectWithTag("TimerMatch").GetComponent<TimerObsolete>();
            timer.pause = true;
            GameObject.FindGameObjectWithTag("MatchSequence").GetComponent<GoalSetInitPos>().teamGoal = team;
            activateGoal = false;
            photonView.RPC("SendGoal", RpcTarget.All, team, matchData.actorWithPosession);
            
        }
    }
    [PunRPC]
    public void SendGoal(string team,int actor)
    {
        GameObject teamsGObj = GameObject.FindGameObjectWithTag("Teams");
        teamsGObj.GetComponent<Teams>().Goal(team, actor);
    }
}
