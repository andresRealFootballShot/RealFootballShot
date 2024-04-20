using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvokeMatchEvents : MonoBehaviour
{
    public Transform dirPass,bot;
    public Transform me;
    public Transform positionGoal;
    public GameObject goalkeeper;
    public SimplePass pass;
    public SideOfField sideOfField;
    public CornerComponents cornerComponents;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            RulesEvents.notifyStartPart.Invoke();
            //MatchEvents.startMatch.Invoke();
            //MatchEvents.continueMatch.Invoke();
            //MatchData.matchState = MatchState.Running;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Rigidbody ballRigidbody = MatchComponents.ballComponents.rigBall;
            MatchComponents.ballComponents.rigBall.velocity = Vector3.zero;
            MatchComponents.ballComponents.rigBall.angularVelocity = Vector3.zero;
            ballRigidbody.position = ComponentsPlayer.currentComponentsPlayer.transBody.position + Vector3.up* MatchComponents.ballComponents.radio + ComponentsPlayer.currentComponentsPlayer.transBody.forward;
            MatchComponents.ballComponents.photonViewBall.RPC(nameof(MatchComponents.ballComponents.kickRPCs.AddForceRPC),Photon.Pun.RpcTarget.Others,
                ballRigidbody.position,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,ComponentsPlayer.myMonoPlayerID.playerID.onlineActor, ComponentsPlayer.myMonoPlayerID.playerID.localActor);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            //directionPass();
            //randomKick();
            kickUp();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            passToMe();
            //directionPass();
            //passToBot();
        }
        checkOffside();
    }
    void kickUp()
    {
        MatchComponents.ballComponents.rigBall.velocity = Vector3.up*4;
    }
    void randomKick()
    {
        Vector3 kick = new Vector3(Random.Range(-5,5), Random.Range(0, 5), Random.Range(-5, 5));
        MatchComponents.ballComponents.rigBall.velocity = kick;
        Vector3 position = new Vector3(Random.Range(-2, 2), Random.Range(0, 4), Random.Range(-2, 2));
        MatchComponents.ballComponents.rigBall.position += position;
    }
    void passToMe()
    {
        Vector3 dir = me.position - MatchComponents.ballComponents.rigBall.position;
        dir.y = 0;
        MatchComponents.ballComponents.rigBall.velocity = dir*2;
    }
    void passToBot()
    {
        Vector3 dir = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * Vector3.forward;
        
        float distance = Random.Range(5.0f, 15.0f);
        MatchComponents.ballComponents.rigBall.position = bot.position + dir* distance;
        Vector3 kick = -dir * distance;
        kick = MyFunctions.setY0ToVector3(kick);
        MatchComponents.ballComponents.rigBall.angularVelocity = Vector3.zero;
        Vector3 velocity = kick * Random.Range(1.0f, 2.0f);
        MatchComponents.ballComponents.rigBall.velocity = Vector3.zero;
        MatchComponents.ballComponents.rigBall.velocity = velocity;
    }
    void directionPass()
    {
        MatchComponents.ballComponents.rigBall.position = MyFunctions.setYToVector3(dirPass.position, MatchComponents.ballRadio);
        Vector3 direction = dirPass.forward;
        MatchComponents.ballComponents.rigBall.velocity = direction.normalized * 15;
    }
    void checkOffside()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            //MatchData.lastPlayerIDPossession = ComponentsPlayer.myMonoPlayerID.playerIDStr;
            MatchData.lastPlayerIDPossession = "";
            MatchData.lastTeamPossession = Teams.MyTeam.TeamName;
            Vector3 velocity = ComponentsPlayer.currentComponentsPlayer.transBody.position- MatchComponents.ballRigidbody.position;
            velocity.Normalize();
            velocity *= 10;
            MatchComponents.ballComponents.rigBall.velocity = velocity;
            MatchComponents.ballComponents.rigBall.angularVelocity = Vector3.zero;
        }
    }
    void checkGoal()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            goalkeeper.SetActive(false);
            MatchData.lastTeamPossession = "Blue";
            MatchComponents.ballComponents.rigBall.velocity = positionGoal.forward*30;
            MatchComponents.ballComponents.rigBall.angularVelocity = Vector3.zero;
            MatchComponents.ballComponents.position = positionGoal.position + Vector3.up * MatchComponents.ballComponents.radio;
        }
    }
}
