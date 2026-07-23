using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PossessionCtrl : PlayerComponent
{
    bool ownerBall;
    private void Start()
    {
        MatchEvents.kick.AddListener(Kick);
        MatchEvents.losePossession.AddListener(()=> ownerBall = false);
        MatchEvents.getPossession.AddListener(() => ownerBall = true);
    }
    void Kick(KickEventArgs args)
    {
        if (!args.playerID.Equals(ComponentsPlayer.myMonoPlayerID.playerID.ToString()))
        {
            if (ownerBall)
            {
                DebugsList.rules.print("LosePossession");
                MatchEvents.losePossession.Invoke();
            }
        }
        else
        {
            if (!ownerBall)
            {
                DebugsList.rules.print("GetPossession");
                MatchEvents.getPossession.Invoke();
            }
            
        }
    }
}
