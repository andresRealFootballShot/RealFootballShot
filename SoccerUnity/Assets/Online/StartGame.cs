using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class StartGame : MonoBehaviour,IOnEventCallback
{
    byte evCode = 2;
    public GameFeatures gameFeatures;
    void Start()
    {
        
    }
    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    public void startGame()
    {

    }


    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == evCode)
        {
            gameFeatures.AddPlayer();
            if (gameFeatures.isFull())
            {
                startGame();
            }
        }
    }
}
