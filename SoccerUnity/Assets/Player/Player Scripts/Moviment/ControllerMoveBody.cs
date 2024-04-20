using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerMoveBody : MonoBehaviour
{
    public LookingBallMoviment moveFreeBody;
    public FreeCameraMoviment normalMoviment;
    public PossessionBallMoviment movimentWithPossession;
    public GameObject eventsGObj;
    public ComponentsKeys keys;
    public ComponentsPlayer componentsPlayer;
    public PlayerState playerMode;
    public EmptyEvent2 losePossessionEvent;
    public float distanceMaxPossession;
    public Sprint sprintState;
    void Start()
    {

        //playerMode = PlayerState.LookingBall;
        MatchEvents.kick.AddListener(Kick);
        MatchEvents.losePossession.AddListener(LosePossession);
    }
    public void LosePossession()
    {
        if (playerMode == PlayerState.WithPossession)
        {
            playerMode = PlayerState.LookingBall;
            normalMoviment.DisableScript();
            moveFreeBody.EnableScript();
            movimentWithPossession.DisableScript();
            sprintState.state = true;
            //enabled = false;
        }
    }
    void Kick(KickEventArgs args)
    {
        if (args.playerID.Equals(ComponentsPlayer.myMonoPlayerID.playerIDStr))
        {

            Vector3 kickDirection = args.kickDirection;
            if (playerMode == PlayerState.LookingBall || playerMode == PlayerState.WithPossession)
            {
                if (kickDirection.magnitude < PuppetParameters.maxKickForceToExitWithPossessionState)
                    playerMode = PlayerState.WithPossession;
                else
                {
                    playerMode = PlayerState.LookingBall;
                    sprintState.state = true;
                }

                switch (playerMode)
                {
                    case PlayerState.Free:
                        normalMoviment.EnableScript();
                        moveFreeBody.DisableScript();
                        movimentWithPossession.DisableScript();
                        break;
                    case PlayerState.LookingBall:
                        normalMoviment.DisableScript();
                        moveFreeBody.EnableScript();
                        movimentWithPossession.DisableScript();
                        break;
                    case PlayerState.WithPossession:
                        normalMoviment.DisableScript();
                        moveFreeBody.DisableScript();
                        movimentWithPossession.EnableScript();
                        break;
                }
            }
        }

    }
    void DistanceWithPossession()
    {
        Vector3 ballPos = componentsPlayer.componentsBall.transCenterBall.position;
        Vector3 bodyPos = componentsPlayer.transBody.position;
        float distance = Vector3.Distance(bodyPos, ballPos);
        if (distance > distanceMaxPossession)
        {
            playerMode = PlayerState.LookingBall;
            normalMoviment.DisableScript();
            moveFreeBody.EnableScript();
            movimentWithPossession.DisableScript();
            sprintState.state = true;
        }
    }
    void Update()
    {
        if (playerMode == PlayerState.WithPossession)
        {
            DistanceWithPossession();
        }
        if (Input.GetKeyDown(ComponentsKeys.keyFreeCamera) || false && Input.GetKeyDown(ComponentsKeys.joyRotateAround))
        {
            if (playerMode == PlayerState.LookingBall)
            {
                playerMode = PlayerState.Free;
            }
            else if (playerMode == PlayerState.Free || playerMode == PlayerState.WithPossession)
            {
                playerMode = PlayerState.Free;
            }
            switch (playerMode)
            {
                case PlayerState.Free:
                    normalMoviment.EnableScript();
                    moveFreeBody.DisableScript();
                    movimentWithPossession.DisableScript();
                    break;
                case PlayerState.LookingBall:
                    normalMoviment.DisableScript();
                    moveFreeBody.EnableScript();
                    movimentWithPossession.DisableScript();
                    break;
                case PlayerState.WithPossession:
                    normalMoviment.DisableScript();
                    moveFreeBody.DisableScript();
                    movimentWithPossession.EnableScript();
                    break;
            }
        }
        if (Input.GetKeyDown(ComponentsKeys.keyLookingBall) || false && Input.GetKeyDown(ComponentsKeys.joyRotateAround))
        {
            if (playerMode == PlayerState.LookingBall)
            {
                playerMode = PlayerState.LookingBall;
            }
            else if (playerMode == PlayerState.Free || playerMode == PlayerState.WithPossession)
            {
                playerMode = PlayerState.LookingBall;
            }
            switch (playerMode)
            {
                case PlayerState.Free:
                    normalMoviment.EnableScript();
                    moveFreeBody.DisableScript();
                    movimentWithPossession.DisableScript();
                    break;
                case PlayerState.LookingBall:
                    normalMoviment.DisableScript();
                    moveFreeBody.EnableScript();
                    movimentWithPossession.DisableScript();
                    break;
                case PlayerState.WithPossession:
                    normalMoviment.DisableScript();
                    moveFreeBody.DisableScript();
                    movimentWithPossession.EnableScript();
                    break;
            }
        }
    }
}

     /*
        if (playerMode == PlayerState.WithPossession)
        {
            DistanceWithPossession();
        }
        if (Input.GetKeyDown(ComponentsKeys.keyFreeCamera) || false && Input.GetKeyDown(ComponentsKeys.joyRotateAround))
        {
            if (playerMode == PlayerState.LookingBall)
            {
                playerMode = PlayerState.Free;
            }
            else if(playerMode == PlayerState.Free|| playerMode == PlayerState.WithPossession)
            {
                playerMode = PlayerState.LookingBall;
            }
            switch (playerMode)
            {
                case PlayerState.Free:
                    normalMoviment.EnableScript();
                    moveFreeBody.DisableScript();
                    movimentWithPossession.DisableScript();
                    break;
                case PlayerState.LookingBall:
                    normalMoviment.DisableScript();
                    moveFreeBody.EnableScript();
                    movimentWithPossession.DisableScript();
                    break;
                case PlayerState.WithPossession:
                    normalMoviment.DisableScript();
                    moveFreeBody.DisableScript();
                    movimentWithPossession.EnableScript();
                    break;
            }
        }*/
