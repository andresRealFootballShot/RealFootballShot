using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldPositionsCtrl : MonoBehaviour,ILoad
{
    [SerializeField]
    GameObject _UIFieldPositions;
    [SerializeField]
    GameObject _fieldPositions;
    public static GameObject UIFieldPositions;
    public static GameObject fieldPositions;
    public static int _loadLevel = 0;
    public int loadLevel { get => _loadLevel; set => _loadLevel=value; }

    //public static Dictionary<string, TypeFieldPosition.Type> fieldPositionOfPlayers = new Dictionary<string, TypeFieldPosition.Type>();
    public void Load(int level)
    {
        if(level == loadLevel)
        {
            Load();
        }
    }
    private void Load()
    {
        UIFieldPositions = _UIFieldPositions;
        fieldPositions = _fieldPositions;
    }
    public static GameObject getLineup(Lineup.TypeLineup typeLineup)
    {
        List<SizeFootballField> footballSizes = MyFunctions.GetComponentsInChilds<SizeFootballField>(fieldPositions, false, true);
        SizeFootballField sizeFootballFieldParent = footballSizes.Find(x => x.Value == TypeMatch.SizeFootballField);
        List<TypeNormalMatchClass> typeMatchNameList = MyFunctions.GetComponentsInChilds<TypeNormalMatchClass>(sizeFootballFieldParent.gameObject, false, true);
        List<Lineup> lineupList = new List<Lineup>();
        //print(typeLineup.ToString()+" "+ TypeMatch.typeNormalMatch.ToString());
        foreach (var item in typeMatchNameList)
        {
            if (item.Value == TypeMatch.typeNormalMatch)
            {
                lineupList.Add(item.GetComponent<Lineup>());
            }
        }
        GameObject lineupGObj = lineupList.Find(item => item.typeLineup.Equals(typeLineup)).gameObject;
        return lineupGObj;
    }
    public static GameObject getUILineup(Lineup.TypeLineup typeLineup)
    {
        List<TypeNormalMatchClass> typeMatchNameList = MyFunctions.GetComponentsInChilds<TypeNormalMatchClass>(UIFieldPositions, false, true);
        List<Lineup> lineupList = new List<Lineup>();
        //print(typeLineup.ToString()+" "+ TypeMatch.typeNormalMatch.ToString());
        foreach (var item in typeMatchNameList)
        {
            if (item.Value == TypeMatch.typeNormalMatch)
            {
                lineupList.Add(item.GetComponent<Lineup>());
            }
        }
        GameObject lineupGObj = lineupList.Find(item => item.typeLineup.Equals(typeLineup)).gameObject;
        return lineupGObj;
    }
    public static GameObject getTypeFieldPosition(Lineup.TypeLineup typeLineup, TypeFieldPosition.Type typeFieldPosition)
    {
        List<TypeFieldPosition> typeFieldPositions = MyFunctions.GetComponentsInChilds<TypeFieldPosition>(getLineup(typeLineup), false, true);
        TypeFieldPosition typeFieldPositionFinded = typeFieldPositions.Find(item => item.Value == typeFieldPosition);
        if (typeFieldPositionFinded != null)
        {
            GameObject typeFieldPositionGObj = typeFieldPositions.Find(item => item.Value == typeFieldPosition).gameObject;
            return typeFieldPositionGObj;
        }
        else
        {
            return null;
        }
    }
    public static GameObject getUITypeFieldPosition(Lineup.TypeLineup typeLineup, TypeFieldPosition.Type typeFieldPosition)
    {

        List<TypeFieldPosition> typeFieldPositions = MyFunctions.GetComponentsInChilds<TypeFieldPosition>(getUILineup(typeLineup), false, true);
        TypeFieldPosition typeFieldPositionFinded = typeFieldPositions.Find(item => item.Value == typeFieldPosition);
        if (typeFieldPositionFinded != null)
        {
            GameObject typeFieldPositionGObj = typeFieldPositions.Find(item => item.Value == typeFieldPosition).gameObject;
            return typeFieldPositionGObj;
        }
        else
        {
            return null;
        }
    }
    public static List<TypeFieldPosition.Type> getFieldPositions(Lineup.TypeLineup typeLineup)
    {
        GameObject lineup = getLineup(typeLineup);
        List<TypeFieldPosition> typeFieldPositions = MyFunctions.GetComponentsInChilds<TypeFieldPosition>(lineup, false, true);
        List<TypeFieldPosition.Type> list = new List<TypeFieldPosition.Type>();
        foreach (var item in typeFieldPositions)
        {
            list.Add(item.Value);
        }
        return list;
    }
    public static List<TypeFieldPosition.Type> getUIFieldPositions(Lineup.TypeLineup typeLineup)
    {
        GameObject lineup = getUILineup(typeLineup);
        List<TypeFieldPosition> typeFieldPositions = MyFunctions.GetComponentsInChilds<TypeFieldPosition>(lineup, false, true);
        List<TypeFieldPosition.Type> list = new List<TypeFieldPosition.Type>();
        foreach (var item in typeFieldPositions)
        {
            list.Add(item.Value);
        }
        return list;
    }
    /*
public static void addFieldPositionOfPlayer(string playerID, string typeFieldPositionString)
{
   if (!fieldPositionOfPlayers.ContainsKey(playerID))
   {
       TypeFieldPosition.Type typeFieldPosition = (TypeFieldPosition.Type)System.Enum.Parse(typeof(TypeFieldPosition.Type), typeFieldPositionString, true);
       fieldPositionOfPlayers.Add(playerID, typeFieldPosition);
   }
   else
   {
       fieldPositionOfPlayers.Remove(playerID);
       TypeFieldPosition.Type typeFieldPosition = (TypeFieldPosition.Type)System.Enum.Parse(typeof(TypeFieldPosition.Type), typeFieldPositionString, true);
       fieldPositionOfPlayers.Add(playerID, typeFieldPosition);
       //myDebug.print("You want add playerID " + playerID.getIDToPrint() + " in team "+TeamName+ " that already exist | Teams.addPlayer()", MyColor.Red);
   }
}
public static void removeFieldPositionOfPlayer(PlayerID playerID)
{
   if (fieldPositionOfPlayers.ContainsKey(playerID.getStringID()))
   {
       fieldPositionOfPlayers.Remove(playerID.getStringID());
   }
   //players.Remove(actor);
   //spawns.Remove(actor);
}
public static string getKeyByValue(TypeFieldPosition.Type value)
{
   return MyFunctions.KeyByValue(fieldPositionOfPlayers, value);
}
public void onlinePlayerLeft(int onlineActor)
{
   List<string> listToDelete = new List<string>();
   foreach (var item in fieldPositionOfPlayers)
   {
       if (new PlayerID(item.Key).onlineActor == onlineActor)
       {
           listToDelete.Add(item.Key);
       }
   }
   foreach (var item in listToDelete)
   {
       fieldPositionOfPlayers.Remove(item);
   }
}*/
}
public class FieldPositionEventArgs
{
    public string teamName;
    public string typeFieldPosition;
    public string playerID;
    public FieldPositionEventArgs(string teamName,string typeFieldPosition,string playerID)
    {
        this.teamName = teamName;
        this.typeFieldPosition = typeFieldPosition;
        this.playerID = playerID;
    }
}
