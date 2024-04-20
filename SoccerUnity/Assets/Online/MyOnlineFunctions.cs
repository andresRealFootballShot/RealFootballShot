using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
public class MyOnlineFunctions : MonoBehaviour
{
    public static bool getPlayer(int actor,out Player player)
    {
        foreach (var item in PhotonNetwork.PlayerList)
        {
            if (item.ActorNumber == actor)
            {
                player = item;
                return true;
            }
        }
        player = null;
        return false;
    }
}
