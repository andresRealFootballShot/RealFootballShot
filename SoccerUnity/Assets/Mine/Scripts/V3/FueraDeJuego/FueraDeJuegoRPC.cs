using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class FueraDeJuegoRPC : MonoBehaviourPunCallbacks
{
    MatchDataObsolete matchData;
    public BallComponents componentsBall;
    public FueraDeJuego fueraDeJuego;
    void Start()
    {
        matchData = GameObject.FindGameObjectWithTag("MatchData").GetComponent<MatchDataObsolete>();
        fueraDeJuego = GameObject.FindGameObjectWithTag("FueraDeJuego").GetComponent<FueraDeJuego>();
        fueraDeJuego.pVPlayer = photonView;
        componentsBall = GameObject.FindGameObjectWithTag("ObjectGoal").GetComponent<BallComponents>();
    }

    [PunRPC]
    public void SetFueraDeJuego(bool value)
    {
        fueraDeJuego.Animation();
        componentsBall.rigBall.velocity = Vector3.zero;
        componentsBall.rigBall.angularVelocity = Vector3.zero;
        matchData.fueraDeJuego = value;
    }
}
