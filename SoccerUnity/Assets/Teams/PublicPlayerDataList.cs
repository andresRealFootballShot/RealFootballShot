using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublicPlayerDataList : MonoBehaviour,ILoad,IClearBeforeLoadScene
{
    public static Dictionary<string,PublicPlayerData> all = new Dictionary<string, PublicPlayerData>();
    public static Dictionary<string, PublicFieldPlayerData> fieldPlayers = new Dictionary<string, PublicFieldPlayerData>();
    public static Dictionary<string, PublicGoalkeeperData> goalkeepers = new Dictionary<string, PublicGoalkeeperData>();
    public static Dictionary<string, PublicPlayerData> myPublicPlayerDatas = new Dictionary<string, PublicPlayerData>();
    public static int _loadLevel = MatchEvents.staticLoadLevel + 1;
    public int loadLevel { get => _loadLevel; set => _loadLevel = value; }


    public void Load(int level)
    {
        if (loadLevel == level)
        {
            Load();
        }
    }
    public void Load()
    {
        all.Clear();
        fieldPlayers.Clear();
        myPublicPlayerDatas.Clear();
        goalkeepers.Clear();
        MatchEvents.otherPlayerLeftRoom.AddListener(removePlayer);
    }
    void Update()
    {

    }
    public static bool getPlayerID(int onlineActor,int localActor,out string playerID)
    {
        foreach (var item in all.Keys)
        {
            if (PlayerID.Equals(item, onlineActor, localActor))
            {
                playerID = item;
                return true;
            }
        }
        playerID = "";
        return false;
    }
    public static void addPublicFieldPlayerData(GameObject newPlayer)
    {
        PublicFieldPlayerData publicPlayerData = MyFunctions.GetComponentInChilds<PublicFieldPlayerData>(newPlayer, true);
        if (!fieldPlayers.ContainsKey(publicPlayerData.playerIDMono.getStringID()) && !all.ContainsKey(publicPlayerData.playerIDMono.getStringID()))
        {
            fieldPlayers.Add(publicPlayerData.playerIDMono.getStringID(), publicPlayerData);
            all.Add(publicPlayerData.playerIDMono.getStringID(), publicPlayerData);
            MatchEvents.addedPublicPlayerDataToList.Invoke(publicPlayerData);
            if(publicPlayerData.playerIDMono.playerID.onlineActor == MatchData.myActor && !myPublicPlayerDatas.ContainsKey(publicPlayerData.playerID))
            {
                myPublicPlayerDatas.Add(publicPlayerData.playerID, publicPlayerData);
            }
        }
    }
    public static void addPublicFieldPlayerData(PublicFieldPlayerData publicPlayerData)
    {
        if (!fieldPlayers.ContainsKey(publicPlayerData.playerIDMono.getStringID()) && !all.ContainsKey(publicPlayerData.playerIDMono.getStringID()))
        {
            fieldPlayers.Add(publicPlayerData.playerIDMono.getStringID(), publicPlayerData);
            all.Add(publicPlayerData.playerIDMono.getStringID(), publicPlayerData);
            MatchEvents.addedPublicPlayerDataToList.Invoke(publicPlayerData);
            if (publicPlayerData.playerIDMono.playerID.onlineActor == MatchData.myActor && !myPublicPlayerDatas.ContainsKey(publicPlayerData.playerID))
            {
                myPublicPlayerDatas.Add(publicPlayerData.playerID, publicPlayerData);
            }
            
        }
    }
    public static void addPublicGoalkeeperData(PublicGoalkeeperData publicPlayerData)
    {
        if (!goalkeepers.ContainsKey(publicPlayerData.playerIDMono.getStringID()) && !all.ContainsKey(publicPlayerData.playerIDMono.getStringID()))
        {
            goalkeepers.Add(publicPlayerData.playerIDMono.getStringID(), publicPlayerData);
            all.Add(publicPlayerData.playerIDMono.getStringID(), publicPlayerData);
            MatchEvents.addedPublicPlayerDataToList.Invoke(publicPlayerData);
            if (publicPlayerData.playerIDMono.playerID.onlineActor == MatchData.myActor && !myPublicPlayerDatas.ContainsKey(publicPlayerData.playerID))
            {
                myPublicPlayerDatas.Add(publicPlayerData.playerID, publicPlayerData);
            }
        }
    }
    public static void addPublicPlayerData(GameObject newPlayer)
    {
        PublicPlayerData publicPlayerData = MyFunctions.GetComponentInChilds<PublicPlayerData>(newPlayer, true);
        if (!all.ContainsKey(publicPlayerData.playerIDMono.getStringID()))
        {
            all.Add(publicPlayerData.playerIDMono.getStringID(), publicPlayerData);
            MatchEvents.addedPublicPlayerDataToList.Invoke(publicPlayerData);
            if (publicPlayerData.playerIDMono.playerID.onlineActor == MatchData.myActor && !myPublicPlayerDatas.ContainsKey(publicPlayerData.playerID))
            {
                myPublicPlayerDatas.Add(publicPlayerData.playerID, publicPlayerData);
            }
            if (publicPlayerData.GetType().Name == typeof(PublicGoalkeeperData).Name && !goalkeepers.ContainsKey(publicPlayerData.playerID))
            {
                goalkeepers.Add(publicPlayerData.playerID, (PublicGoalkeeperData)publicPlayerData);
            }
            else if (publicPlayerData.GetType().Name == typeof(PublicFieldPlayerData).Name && !fieldPlayers.ContainsKey(publicPlayerData.playerID))
            {
                fieldPlayers.Add(publicPlayerData.playerID, (PublicFieldPlayerData)publicPlayerData);
            }
        }
    }
    public static void addPublicPlayerData(PublicPlayerData publicPlayerData)
    {
        
        if (!all.ContainsKey(publicPlayerData.playerIDMono.getStringID()))
        {
            all.Add(publicPlayerData.playerIDMono.getStringID(), publicPlayerData);
            MatchEvents.addedPublicPlayerDataToList.Invoke(publicPlayerData);
            if (publicPlayerData.playerIDMono.playerID.onlineActor == MatchData.myActor && !myPublicPlayerDatas.ContainsKey(publicPlayerData.playerID))
            {
                myPublicPlayerDatas.Add(publicPlayerData.playerID, publicPlayerData);
            }
            if (publicPlayerData.GetType().Name == typeof(PublicGoalkeeperData).Name && !goalkeepers.ContainsKey(publicPlayerData.playerID))
            {
                goalkeepers.Add(publicPlayerData.playerID, (PublicGoalkeeperData)publicPlayerData);
            }else if (publicPlayerData.GetType().Name == typeof(PublicFieldPlayerData).Name && !fieldPlayers.ContainsKey(publicPlayerData.playerID))
            {
                fieldPlayers.Add(publicPlayerData.playerID, (PublicFieldPlayerData)publicPlayerData);
            }
        }
    }
    public static void addToMyPublicPlayerDataList(PublicPlayerData publicPlayerData)
    {
        if (!myPublicPlayerDatas.ContainsKey(publicPlayerData.playerIDMono.getStringID()))
        {
            myPublicPlayerDatas.Add(publicPlayerData.playerIDMono.getStringID(), publicPlayerData);
            MatchEvents.addedPublicPlayerDataToList.Invoke(publicPlayerData);
            if (publicPlayerData.playerIDMono.playerID.onlineActor == MatchData.myActor && !myPublicPlayerDatas.ContainsKey(publicPlayerData.playerID))
            {
                myPublicPlayerDatas.Add(publicPlayerData.playerID, publicPlayerData);
            }
            if (publicPlayerData.GetType().Name == typeof(PublicGoalkeeperData).Name && !goalkeepers.ContainsKey(publicPlayerData.playerID))
            {
                goalkeepers.Add(publicPlayerData.playerID, (PublicGoalkeeperData)publicPlayerData);
            }
        }
    }
    static void removePlayer(string playerID)
    {
        if (all.ContainsKey(playerID))
        {
            all.Remove(playerID);
        }
    }

    public void Clear()
    {
        all.Clear();
        fieldPlayers.Clear();
        myPublicPlayerDatas.Clear();
        goalkeepers.Clear();
    }
}
