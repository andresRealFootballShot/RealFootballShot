using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class InstantiateBall : MonoBehaviour
{
    public InstantiatePhotonGameObject ballInstantiator;
    int countError, maxError = 5;
    public void Instantiate()
    {
        ballInstantiator.gameObjectIsInstantiatedEvent += setupOnlineBall;
        ballInstantiator.errorEvent += instantiationError;
        ballInstantiator.setup(CodeEventsNet.BallInstantiate, SizeFootballFieldCtrl.getMidField(TypeMatch.SizeFootballField).position, SizeFootballFieldCtrl.getMidField(TypeMatch.SizeFootballField).eulerAngles);
        if (PhotonNetwork.IsMasterClient)
        {
            ballInstantiator.Instantiate();
        }
    }
    public void setupOnlineBall(GameObject ball)
    {
        BallComponents componentsBall = ball.GetComponent<BallComponents>();
        ComponentsPlayer componentsPlayer = FindObjectOfType<ComponentsPlayer>();
        componentsPlayer.componentsBall = componentsBall;
    }
    void instantiationError()
    {
        countError++;
        if (countError >= maxError)
        {
            OnlineErrorHandler.OnlineError("Instantiate ball");
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ballInstantiator.Instantiate();
            }
        }
    }
}
