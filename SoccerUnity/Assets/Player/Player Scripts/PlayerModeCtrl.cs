using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModeCtrl : MonoBehaviour
{
    PlayerState playerMode;
    public GameObject eventsGObj;
    public ComponentsKeys keys;
    public ComponentsPlayer componentsPlayer;
    public float distanceMaxPossession;
    public TransparencyCtrl transparency;
    bool computerDistance;
    private void Awake()
    {
        transparency.GetListMaterials(componentsPlayer.transModelo);
        //transparency.SetOpaque(componentsPlayer.transModelo);
    }
    void Start()
    {
        MatchEvents.kick.AddListener(Kick);
        playerMode = PlayerState.LookingBall;
        MatchEvents.losePossession.AddListener(LosePossession);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(ComponentsKeys.keyFreeCamera) || false && Input.GetKeyDown(ComponentsKeys.joyRotateAround))
        {
            if (playerMode == PlayerState.LookingBall)
            {
                playerMode = PlayerState.Free;
            }
            else if(playerMode==PlayerState.WithPossession)
            {
                playerMode = PlayerState.Free;
                
            }
            else
            {
                playerMode = PlayerState.Free;
            }
            transparency.SetOpaque(componentsPlayer.transModelo);
        }
        if (Input.GetKeyDown(ComponentsKeys.keyLookingBall) || false && Input.GetKeyDown(ComponentsKeys.joyRotateAround))
        {
            if (playerMode == PlayerState.LookingBall)
            {
                playerMode = PlayerState.LookingBall;
            }
            else if (playerMode == PlayerState.WithPossession)
            {
                playerMode = PlayerState.LookingBall;
                //transparency.SetOpaque(componentsPlayer.transModelo);
            }
            else
            {
                playerMode = PlayerState.LookingBall;
            }
        }
        DistanceWithPossession();
    }
    void DistanceWithPossession()
    {
        Vector3 ballPos = componentsPlayer.componentsBall.transCenterBall.position;
        Vector3 bodyPos = componentsPlayer.transBody.position;
        float distance = Vector3.Distance(bodyPos, ballPos);
        if (distance > distanceMaxPossession)
        {
           
            transparency.SetOpaque(componentsPlayer.transModelo);
             
            if (playerMode == PlayerState.WithPossession)
            {
                playerMode = PlayerState.LookingBall;
            }
        }
        else
        {
          transparency.SetTransparency(componentsPlayer.transModelo);    
        }
    }
    public void LosePossession()
    {
        playerMode = PlayerState.LookingBall;
        transparency.SetTransparency(componentsPlayer.transModelo);
    }
    void Kick(KickEventArgs args)
    {
        if (args.playerID.Equals(ComponentsPlayer.myMonoPlayerID.playerIDStr))
        {
            Vector3 kickDirection = args.kickDirection;
            if (playerMode == PlayerState.LookingBall || playerMode == PlayerState.WithPossession)
            {
                if (kickDirection.magnitude < PuppetParameters.maxKickForceToExitWithPossessionState)
                {
                    playerMode = PlayerState.WithPossession;
                    transparency.SetTransparency(componentsPlayer.transModelo);
                }
                else
                {
                    playerMode = PlayerState.LookingBall;
                    transparency.SetOpaque(componentsPlayer.transModelo);
                }
            }
        }
    }
}
