using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Team : MonoBehaviour
{
    public string initName;
    public Variable<string> nameTeamVar=new Variable<string>();
    public List<string> players = new List<string>();
    public List<PublicPlayerData> publicPlayerDatas = new List<PublicPlayerData>();
    public List<ChaserData> firstChaserDatas = new List<ChaserData>();
    public Dictionary<TypeFieldPosition.Type, string> fieldPositionOfPlayers = new Dictionary<TypeFieldPosition.Type, string>();
    public SortedDictionary<int, int> spawns = new SortedDictionary<int, int>();
    public Lineup choosedLineup;
    public List<Transform> spawnsTransform;
    public Transform posStartBall;
    public int actorPosStartMatch=0;
    public MatchDataObsolete matchData;
    public bool IsMine { get => isMine(); }
    public Animation goalMineAnim, goalOtherAnim;
    List<GoalData> _goals = new List<GoalData>();
    public List<GoalData> goals { get => _goals; }
    public MyEvent<GoalData> goalAddedEvent = new MyEvent<GoalData>();
    public ColorVar colorVar;
    public Color Color { get => colorVar.Value; set => colorVar.Value = value; }
    public string TeamName { get => nameTeamVar.Value; set => nameTeamVar.Value = value; }
    public Equipement equipament;
    public MyEvent<SideOfField> sideOfFieldChanged;
    public MyEvent lineupChanged;
    public int teamMaxPlayers { get => TypeMatch.maxPlayers / TypeMatch.getTemsSize() + 1; }
    public int teamMaxFieldPlayers { get => TypeMatch.maxPlayers / TypeMatch.getTemsSize(); }
    public int playersNoGoalkeeperCount { get; set; }
    public TeamHUD teamHUD;
    public SideOfField SideOfField { get; set; }
    public void Load()
    {
        nameTeamVar.Value = initName;
        MatchEvents.otherPlayerLeftRoom.AddListener(removePlayer);
        equipament.Load();
        lineupChanged = new MyEvent(nameof(lineupChanged)+" "+this.GetInstanceID());
        sideOfFieldChanged = new MyEvent<SideOfField>(nameof(sideOfFieldChanged) + " " + this.GetInstanceID());
        if (teamHUD != null)
        {
            goalAddedEvent.AddListener(teamHUD.goalAdded);
        }
    }
   bool isMine()
    {
        return fieldPositionOfPlayers.ContainsValue(ComponentsPlayer.myMonoPlayerID.playerIDStr);
    }
    public PublicPlayerData getPublicPlayerData(string playerID)
    {
        if (publicPlayerDatas.Exists(x => x.playerID.Equals(playerID)))
        return publicPlayerDatas.Find(x => x.playerID.Equals(playerID));
        else return null;
    }
    public void addGoal(GoalData goal)
    {
        _goals.Add(goal);
        goalAddedEvent.Invoke(goal);
    }
    public void setLineup(Lineup.TypeLineup typeLineup)
    {
        
        choosedLineup.setLineup(typeLineup);
        fieldPositionOfPlayers.Clear();
        List<TypeFieldPosition.Type> list = FieldPositionsCtrl.getFieldPositions(choosedLineup.typeLineup);
        foreach (var item in list)
        {
            fieldPositionOfPlayers.Add(item, "None");
            //print(TeamName+" setLineup " + item.ToString());
        }
        fieldPositionOfPlayers.Add(TypeFieldPosition.Type.GoalKeeper, "None");
        DebugsList.testing.print("setLineup team=" + TeamName + " | typeLineup=" + typeLineup.ToString());
        lineupChanged.Invoke();
       
    }
    public string getGoalkeeper()
    {
        return fieldPositionOfPlayers[TypeFieldPosition.Type.GoalKeeper];
    }
    public PublicPlayerData getGoalkeeperPublicPlayerData()
    {
        PublicPlayerData publicPlayerData = getPublicPlayerData(getGoalkeeper());
        return publicPlayerData;
    }
    public bool setSideOfField(SideOfFieldID sideOfFieldID)
    {
        //SideOfFieldCtrl.setTeamSide(TeamName, sideOfFieldID);
        SideOfField sideOfField;
        if(SideOfFieldCtrl.getSideOfField(sideOfFieldID,out sideOfField))
        {
            DebugsList.testing.print("setSideOfField team=" + TeamName + " | sideOfField=" + sideOfFieldID.ToString());
            sideOfFieldChanged.Invoke(sideOfField);
            SideOfField = sideOfField;

        }
        return true;
    }
    public List<TypeFieldPosition.Type> getAvailableFieldPositions()
    {
        List<TypeFieldPosition.Type> list = new List<TypeFieldPosition.Type>();
        foreach (var item in fieldPositionOfPlayers)
        {
            if (item.Value.Equals("None") && item.Key != TypeFieldPosition.Type.GoalKeeper)
            {
                list.Add(item.Key);
            }
        }
        return list;
    }
    public bool fieldPositionIsAvailable(TypeFieldPosition.Type type)
    {
        return fieldPositionOfPlayers[type] == "None";
        
    }
    public TypeFieldPosition.Type getAvailableRandomFieldPosition()
    {
        List<TypeFieldPosition.Type> list = getAvailableFieldPositions();
        if (list.Count > 0)
        {
            TypeFieldPosition.Type typeFieldPosition = list[Random.Range(0, list.Count)];
            return typeFieldPosition;
        }
        else
        {
            Debug.LogError("Team.getAvailableRandomFieldPosition()");
            DebugsList.errors.print("Team.getAvailableRandomFieldPosition()", Color.red);
            return TypeFieldPosition.Type.None;
        }
    }
    public List<string> getPlayersWithAssignedFieldPosition()
    {
        List<string> list = new List<string>();
        foreach (var item in fieldPositionOfPlayers)
        {
            if (!item.Value.Equals("None"))
            {
                list.Add(item.Value);
            }
        }
        return list;
    }
    public List<PublicPlayerData> getPublicPlayerDatasWithAssignedFieldPosition()
    {
        List<string> playerIDList = getPlayersWithAssignedFieldPosition();
        List<PublicPlayerData> list = new List<PublicPlayerData>();
        foreach (var playerID in playerIDList)
        {
            list.Add(PublicPlayerDataList.all[playerID]);
        }
        return list;
    }
    public bool ContainsPlayer(string playerID)
    {
        return players.Contains(playerID);
    }
    public void SetActorPosStartMatch(int actor)
    {
        if(actorPosStartMatch==0)
            actorPosStartMatch = actor;
    }
    bool containsFieldPosition(TypeFieldPosition.Type typeFieldPosition)
    {
        return fieldPositionOfPlayers.ContainsKey(typeFieldPosition);
    }
    public bool isFull()
    {
        if (teamMaxPlayers == players.Count)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool fieldPositionsIsFull()
    {
        if (getAvailableFieldPositions().Count == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public List<int> FindRandomSpawn()
    {
        List<int> spawnsAvailables=new List<int>();
        for(int i=0;i<PhotonNetwork.CurrentRoom.MaxPlayers/2;i++)
        {
            spawnsAvailables.Add(i);
        }
        foreach(int i in spawns.Values)
        {
            spawnsAvailables.Remove(i);
        }
        return spawnsAvailables;
    }
    public bool addPlayer(string playerID)
    {
        //El + 1 es por el portero
        if (teamMaxPlayers > players.Count)
        {
            if (!playerID.Equals("None"))
            {
                players.Add(playerID);
                MatchEvents.playerAddedToTeam.Invoke(new PlayerAddedToTeamEventArgs(playerID,TeamName,null));
                StartCoroutine(waitUntilPublicPlayerDataIsAvailable(playerID));
                return true;
            }
        }
        return false;
    }
    public bool addPlayer(string playerID, string typeFieldPositionString)
    {
        int teamMaxPlayers = TypeMatch.maxPlayers / 2 + 1;
        TypeFieldPosition.Type typeFieldPosition = MyFunctions.parseEnum<TypeFieldPosition.Type>(typeFieldPositionString);
        //El + 1 es por el portero
        if (teamMaxPlayers > players.Count)
        {
            if (!playerID.Equals("None") && (typeFieldPosition == TypeFieldPosition.Type.None || containsFieldPosition(typeFieldPosition)))
            {
                players.Add(playerID);
                assignFieldPositionToPlayer(playerID, typeFieldPositionString);
                MatchEvents.playerAddedToTeam.Invoke(new PlayerAddedToTeamEventArgs(playerID, TeamName,null));
                StartCoroutine(waitUntilPublicPlayerDataIsAvailable(playerID));
                return true;
            }
        }
        return false;
    }
    public void addPublicPlayerData(PublicPlayerData publicPlayerData)
    {
        publicPlayerDatas.Add(publicPlayerData);
    }
    public void addFirstChaserData(ChaserData chaserData)
    {
        firstChaserDatas.Add(chaserData);
    }
    IEnumerator waitUntilPublicPlayerDataIsAvailable(string playerID)
    {
        yield return new WaitUntil(() => PublicPlayerDataList.all.ContainsKey(playerID));
        PublicPlayerData publicPlayerData = PublicPlayerDataList.all[playerID];
        MatchEvents.publicPlayerDataOfAddedPlayerToTeamIsAvailable.Invoke(new PlayerAddedToTeamEventArgs(playerID, TeamName, publicPlayerData));
    }
    public bool assignFieldPositionToPlayer(string playerID, string typeFieldPositionString)
    {
        TypeFieldPosition.Type typeFieldPosition = MyFunctions.parseEnum<TypeFieldPosition.Type>(typeFieldPositionString);
        if (players.Contains(playerID) || playerID.Equals("None"))
        {
            if (fieldPositionOfPlayers.ContainsKey(typeFieldPosition))
            {
                if (!fieldPositionOfPlayers[typeFieldPosition].Equals(playerID))
                {
                    fieldPositionOfPlayers[typeFieldPosition] = playerID;
                    if (!typeFieldPosition.Equals(TypeFieldPosition.Type.GoalKeeper))
                    {
                        playersNoGoalkeeperCount++;
                    }
                    DebugsList.testing.print("assignFieldPositionToPlayer Team=" + TeamName + " playerID=" + playerID + " typeFieldPosition=" + typeFieldPositionString, Color.green);
                    MatchEvents.fieldPositionsChanged.Invoke(new FieldPositionEventArgs(TeamName, typeFieldPositionString, playerID));
                    return true;
                }
            }else if (typeFieldPosition != TypeFieldPosition.Type.None)
            {
                Debug.LogError("Team " + TeamName + " dont contains the field position " + typeFieldPositionString);
            }
        }
        else
        {
            Debug.LogError("Team " + TeamName + " dont contains the player " + playerID);
        }
        return false;
    }
    public bool addPlayerToRandomFieldPosition(string playerID)
    {
        List<TypeFieldPosition.Type> list = getAvailableFieldPositions();
        TypeFieldPosition.Type typeFieldPosition = list[Random.Range(0, list.Count)];
        if (addPlayer(playerID))
        {
            assignFieldPositionToPlayer(playerID, typeFieldPosition.ToString());
            return true;
        }
        return false;
    }
    public void removePlayer(string playerID)
    {
        if (players.Contains(playerID))
        {
            players.Remove(playerID);
        }
        TypeFieldPosition.Type typeFieldPositionOfPlayer;
        if(MyFunctions.GetKeyByValue(fieldPositionOfPlayers,playerID, out typeFieldPositionOfPlayer))
        {
            assignFieldPositionToPlayer("None", typeFieldPositionOfPlayer.ToString());
        }
        //spawns.Remove(actor);
    }
    public bool getTypeFieldPositionOfPlayer(string playerID,out TypeFieldPosition.Type typeFieldPositionOfPlayer)
    {
       return MyFunctions.GetKeyByValue(fieldPositionOfPlayers, playerID, out typeFieldPositionOfPlayer);
    }
    public void addGoal(int actor)
    {
        /*
        goals++;
        goalsString.Value = goals.ToString();
        if (players.Contains(actor))
        {
            matchData.players[actor].goals++;
        }
        else
        {
            matchData.players[actor].goalsDefeat++;
        }
        if (isMine)
        {
            goalMineAnim.Play();
        }
        else
        {
            goalOtherAnim.Play();
        }*/
    }
    public int setRandomSpawn(int actor)
    {
        List<int> list = FindRandomSpawn();
        if (!spawns.ContainsKey(actor))
        {
            spawns.Add(actor, list[Random.Range(0, list.Count - 1)]);
        }
        return spawns[actor];
    }
    public void setSpawn(int actor,int spawn)
    {
        if (!spawns.ContainsKey(actor))
        {
            spawns.Add(actor, spawn);
        }
    }
    public Transform getSpawn(int actor)
    {
        return spawnsTransform[spawns[actor]];
    }
    
    public int[] GetPlayersToInt()
    {
        //return players.ToArray();
        return null;
    }
    public void addPlayers(int[] players)
    {
        /*
        foreach(int player in players)
        {
            if (!this.players.Contains(player))
            {
                this.players.Add(player);
            }
        }*/
    }
}
public struct PlayerAddedToTeamEventArgs
{
    string playerID;
    public string PlayerID { get => playerID; set => playerID = value; }
    public PublicPlayerData publicPlayerData;
    string teamName;
    public string TeamName { get => teamName; set => teamName = value; }
    public PlayerAddedToTeamEventArgs(string playerID,string teamName, PublicPlayerData publicPlayerData)
    {
        this.playerID = playerID;
        this.teamName = teamName;
        this.publicPlayerData = publicPlayerData;
    }

}


