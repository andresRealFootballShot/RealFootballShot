using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerTypeID
{
    Bot,Puppet
}
public class PlayerType : MonoVariable<PlayerTypeID>
{
    public PlayerTypeID GetOtherPlayerType()
    {

        if (Value == PlayerTypeID.Bot) return PlayerTypeID.Puppet;
        else return PlayerTypeID.Bot;
    }
}
