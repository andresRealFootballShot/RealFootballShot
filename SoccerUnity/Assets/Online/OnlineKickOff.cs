using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class OnlineKickOff :MonoBehaviourPunCallbacks, IKickOff
{
    public string teamName { get ; set; }

    void Start()
    {
        MatchComponents.kickOff = this;
    }
    public void startProcess(string teamName)
    {
        if (MatchData.ImReferee)
        {
            
            Team team = Teams.getTeamByName(teamName);
            SideOfField sideOfField;
            SideOfFieldCtrl.getSideOfFieldOfTeam(team.TeamName, out sideOfField);
            float force = MatchComponents.rulesComponents.settings.forceKickOff;
            Vector3 dirKick = SideOfFieldCtrl.TransformDirectionSideOfField(team.TeamName, -Vector3.forward* force);
            Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
            Transform ballTransform = MatchComponents.ballComponents.transBall;
            string rpcName = nameof(MatchComponents.ballComponents.kickRPCs.AddForceRPC);
            MatchComponents.ballComponents.photonViewBall.RPC(rpcName,RpcTarget.All,ballRigidbody.position, ballTransform.eulerAngles, dirKick, Vector3.zero, Vector3.zero, -1, -1);
           
            //DebugsList.testing.print("sendTeamServe=" + teamName);
            
            
        }
    }
    public void startProcess()
    {
        startProcess(teamName);
    }
    public void notifyTeamServe(string teamName)
    {
        if (MatchData.ImReferee)
        {
            int index;
            Teams.getIndexOfTeam(teamName, out index);
            photonView.RPC(nameof(sendTeamServe), RpcTarget.Others, index);
            sendTeamServe(index);
        }
    }
    [PunRPC]
    public void sendTeamServe(int teamIndex)
    {
        Team team;
        Teams.getTeamByIndex(teamIndex, out team);
        MatchData.teamNameOfServe = team.TeamName;

        //DebugsList.testing.print("receiveTeamServe=" + team.TeamName);
    }

    
}
