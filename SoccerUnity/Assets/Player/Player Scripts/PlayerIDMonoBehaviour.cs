using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Realtime;
using Photon.Pun;

public enum TypePlayer
{
    PlayerNet,
    GoalKeeper
}
public class PlayerID
{
    public TypePlayer typePlayer;
    public int onlineActor;
    public int localActor;
    static string separator = "----";
    public static int mainLocalActor=0;
    public static string None = "None";
    public static int numFields = 3;
    public PlayerID(TypePlayer typePlayer,int localActor)
    {
        this.typePlayer = typePlayer;
        this.localActor = localActor;
    }
    public PlayerID(TypePlayer typePlayer,int onlineActor, int localActor)
    {
        this.typePlayer = typePlayer;
        this.onlineActor = onlineActor;
        this.localActor = localActor;
    }
    public PlayerID(string id)
    {
        string[] parts = id.Split(new string[] { separator }, 3, StringSplitOptions.None);
        if(parts.Length == numFields)
        {
            typePlayer = (TypePlayer)Enum.Parse(typeof(TypePlayer), parts[0], true);
            onlineActor = int.Parse(parts[1]);
            localActor = int.Parse(parts[2]);
        }
    }
    public static int getOnlineActor(string playerID)
    {
        return new PlayerID(playerID).onlineActor;
    }
    public static TypePlayer getTypePlayer(string playerID)
    {
        return new PlayerID(playerID).typePlayer;
    }
    public void setMainLocalActor()
    {
        localActor = mainLocalActor;
    }
    public static string getMultipleIDsToPrint(string[] ids)
    {
        string result = "";
        foreach (var item in ids)
        {
            result += getIDToPrint(item) + " / ";
        }
        if (result != "")
        {
            result.Remove(result.Length - 3);
        }
        return result;
    }
    public static int[] getOnlineActorsOfMultipleIDs(List<PlayerID> ids)
    {
        List<int> result = new List<int>();
        foreach (var item in ids)
        {
           result.Add(item.onlineActor);
        }
        return result.ToArray();
    }
    public static int[] getLocalActorsOfMultipleIDs(List<PlayerID> ids)
    {
        List<int> result = new List<int>();
        foreach (var item in ids)
        {
            result.Add(item.localActor);
        }
        return result.ToArray();
    }
    public string getIDToPrint()
    {
        return typePlayer.ToString() + "-" + onlineActor + "-" + localActor;
    }
    public static string getIDToPrint(string id)
    {
        if (id.Equals(None))
        {
            return None;
        }
        else
        {
            string[] parts = id.Split(new string[] { separator }, 3, StringSplitOptions.None);
            return parts[0] + "-" + parts[1] + "-" + parts[2];
        }
    }
    public string getStringID()
    {
        return typePlayer.ToString() + separator + onlineActor + separator + localActor;
    }
    public static string getStringID(PlayerID playerID)
    {
        return playerID.typePlayer.ToString() + separator + playerID.onlineActor + separator + playerID.localActor;
    }
    public bool Equals(PlayerID other)
    {
        return (typePlayer == other.typePlayer) && (onlineActor == other.onlineActor) && (localActor == other.localActor);
    }
    public static bool Equals(string playerIDStr,int onlineActor,int localActor)
    {
        PlayerID playerID = new PlayerID(playerIDStr);
        return (playerID.onlineActor == onlineActor) && (playerID.localActor == localActor);
    }
    public bool Equals(string otherStr)
    {
        if (otherStr.Equals(None))
        {
            return false;
        }
        else
        {
            PlayerID other = new PlayerID(otherStr);
            return (typePlayer == other.typePlayer) && (onlineActor == other.onlineActor) && (localActor == other.localActor);
        }
    }
    public void setOnlneActor(int onlineActor)
    {
        this.onlineActor = onlineActor;
    }
    public void setLocalActor(int localActor)
    {
        this.localActor = localActor;
    }
    public override string ToString()
    {
        return typePlayer.ToString() + " " + onlineActor + " " + localActor;
    }
}
public class PlayerIDMonoBehaviour : MonoBehaviour
{
    [SerializeField]
    TypePlayer typePlayer;
    [SerializeField]
    int onlineActor;
    [SerializeField]
    int localActor;
    public PlayerID playerID;
    public string playerIDStr { get => playerID.getStringID(); }
    public static int _loadLevel = 0;
    public int loadLevel { get => _loadLevel; set => _loadLevel = value; }
    public MyEvent<GameObject> playerIDisLoadedEvent = new MyEvent<GameObject>(nameof(playerIDisLoadedEvent));
    public static int localActorCount = 0;
    public void Awake()
    {
        /*if (playerID ==null)
            playerID = new PlayerID(typePlayer,localActor);*/
    }
    /*
   public void awake()
   {
       if (playerID == null)
           playerID = new PlayerID(typePlayer, localActor);
   }*/
   /*
    public void Load(int level)
    {
        if (loadLevel == level)
        {
            //Load();
        }
    }*/
    void Load()
    {
        if (playerID == null)
            playerID = new PlayerID(typePlayer, localActor);
    }
    public void LocalLoad(TypePlayer typePlayer,int onlineActor)
    {
        PlayerID newPlayerID = new PlayerID(typePlayer, onlineActor, localActorCount);
        localActorCount++;
        playerID = newPlayerID;
        playerIDisLoadedEvent.Invoke(gameObject);
    }
    public void LocalLoad(int onlineActor)
    {
        PlayerID newPlayerID = new PlayerID(typePlayer, onlineActor, localActorCount);
        localActorCount++;
        playerID = newPlayerID;
        playerIDisLoadedEvent.Invoke(gameObject);
    }
    public void RemoteLoad(string playerIDStr)
    {
        PlayerID newPlayerID = new PlayerID(playerIDStr);
        playerID = newPlayerID;
        playerIDisLoadedEvent.Invoke(gameObject);
    }
    /*
    public void setOnlineActor(int onlineActor)
    {
        this.onlineActor = onlineActor;
        if(playerID!=null)
            playerID.setOnlneActor(onlineActor);
    }
    public void setLocalActor(int localActor)
    {
        this.localActor = localActor;
        if (playerID != null)
            playerID.setLocalActor(localActor);
    }*/
    public bool Equals(PlayerIDMonoBehaviour other)
    {
       return (typePlayer == other.typePlayer) && (onlineActor == other.onlineActor) && (localActor == other.localActor);
    }
    public string getStringID()
    {
        return playerID.getStringID();
    }
    public string getIDToPrint()
    {
        return playerID.getIDToPrint();
    }
}
