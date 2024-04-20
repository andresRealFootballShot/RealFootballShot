using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel
{
    public Player player;
    public int goals { get; set; }
    public int goalsDefeat { get; set; }
    public PlayerModel(Player player)
    {
        this.player = player;
    }
}
