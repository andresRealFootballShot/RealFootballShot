using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideOfFieldCtrl : MonoBehaviour,ILoad
{
    /*
    public class SideOfFieldData
    {
        public Transform transformSideOfField;
        public GameObject goal;
        public SideOfFieldData(Transform transformSideOfField,GameObject goal)
        {
            this.transformSideOfField = transformSideOfField;
            this.goal = goal;
        }
    }*/
    static Dictionary<string, SideOfFieldID> sideOfFieldTeams = new Dictionary<string, SideOfFieldID>();
    static Dictionary<SizeFootballFieldID, Dictionary<SideOfFieldID, SideOfField>> sideOfFields = new Dictionary<SizeFootballFieldID, Dictionary<SideOfFieldID, SideOfField>>();
    public Transform parent;

    public static int _loadLevel = 0;
    public int loadLevel { get => _loadLevel; set => _loadLevel = value; }
    private void Awake()
    {
        //sideOfFields.Add(SideOfFieldID.One,new SideOfFieldData(sideOfFieldOne));
        //sideOfFields.Add(SideOfFieldID.Two, new SideOfFieldData(sideOfFieldOne));

    }
    public void Load(int level)
    {
        if(level == loadLevel)
        {
            sideOfFieldTeams.Clear();
            sideOfFields.Clear();

            getData();
        }
    }
    void getData()
    {
        List<SizeFootballField> sizeFootballFields = MyFunctions.GetComponentsInChilds<SizeFootballField>(parent.gameObject,true,true);
        foreach (var sizeFootballField in sizeFootballFields)
        {
            Dictionary<SideOfFieldID, SideOfField> sideOfFieldDictionary = new Dictionary<SideOfFieldID, SideOfField>();
            List<SideOfField> sideOfFields = MyFunctions.GetComponentsInChilds<SideOfField>(sizeFootballField.gameObject, true,true);
            foreach (var sideOfField in sideOfFields)
            {
                sideOfFieldDictionary.Add(sideOfField.Value, sideOfField);
            }
            SideOfFieldCtrl.sideOfFields.Add(sizeFootballField.Value, sideOfFieldDictionary);
        }
    }
    public static bool getTeamOfSideOfField(SideOfFieldID sideOfFieldID, out string teamName)
    {
        if (MyFunctions.GetKeyByValue(sideOfFieldTeams, sideOfFieldID, out teamName))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static void alternateSideOfFieldsOfTeams()
    {
        string teamName1 = Teams.teamsList[0].TeamName;
        string teamName2 = Teams.teamsList[1].TeamName;
        SideOfFieldID sideOfFieldID1 = sideOfFieldTeams[teamName1];
        SideOfFieldID sideOfFieldID2 = sideOfFieldTeams[teamName2];
        SideOfField sideOfField1, sideOfField2;
        SideOfFieldCtrl.getSideOfField(sideOfFieldID1, out sideOfField1);
        SideOfFieldCtrl.getSideOfField(sideOfFieldID2, out sideOfField2);
        GameObject goalkeeper1 = sideOfField1.goalComponents.goalkeeper;
        GameObject goalkeeper2 = sideOfField2.goalComponents.goalkeeper;
        sideOfField1.goalComponents.goalkeeper = goalkeeper2;
        sideOfField2.goalComponents.goalkeeper = goalkeeper1;
        goalkeeper1.transform.parent = sideOfField2.goalComponents.transform;
        goalkeeper2.transform.parent = sideOfField1.goalComponents.transform;
        setTeamSide(teamName1, sideOfFieldID2);
        setTeamSide(teamName2, sideOfFieldID1);
        foreach (var publicPlayerData in PublicPlayerDataList.fieldPlayers.Values)
        {
            InitialPosition.setInitPositionAndRotationInPublicPlayerData(publicPlayerData);
        }
        foreach (var publicGoalkeeperData in PublicPlayerDataList.goalkeepers.Values)
        {
            //SetupGoalkeeper.changeSideOfField(publicGoalkeeperData);
        }
    }
    public static bool getGoalComponentsOfPlayer(string playerID,out GoalComponents goalComponents)
    {
        Team team;
        if(Teams.getTeamFromPlayer(playerID,out team))
        {
            SideOfField sideOfField;
            if (getSideOfFieldOfTeam(team.TeamName, out sideOfField))
            {
                goalComponents = sideOfField.goalComponents;
                return true;
            }
        }
        goalComponents = null;
        return false;
    }
    public static List<SideOfField> getSideOfFieldsOfSizeFootballField(SizeFootballFieldID sizeFootballFieldID)
    {
        List<SideOfField> list = MyFunctions.DictionaryValuesToList(sideOfFields[sizeFootballFieldID].Values);
        return list;
    }
    public static void setTeamSide(string teamName,SideOfFieldID sideOfFieldID)
    {
        if (!sideOfFieldTeams.ContainsKey(teamName))
        {
            sideOfFieldTeams.Add(teamName, sideOfFieldID);
        }
        else
        {
            sideOfFieldTeams[teamName] = sideOfFieldID;
        }
        Teams.teamsDictionary[teamName].setSideOfField(sideOfFieldID);
        //addGoalKeeperToTeam(teamName, sideOfFieldID);
    }
    public static void setTeamSide(string teamName, string sideOfFieldID)
    {
        setTeamSide(teamName, MyFunctions.parseEnum<SideOfFieldID>(sideOfFieldID));
    }
    public static bool getSideOfFieldOfTeam(string teamName,out SideOfField sideOfField){
        SideOfFieldID sideOfFieldID;
        if (getSideOfFieldIDOfTeam(teamName, out sideOfFieldID))
        {
            sideOfField = sideOfFields[TypeMatch.SizeFootballField][sideOfFieldID];
            return true;
        }
        else
        {
            sideOfField = null;
            return false;
        }
    }
    public static bool getSideOfFieldIDOfTeam(string teamName, out SideOfFieldID sideOfFieldID)
    {
        if (sideOfFieldTeams.ContainsKey(teamName))
        {
            sideOfFieldID = sideOfFieldTeams[teamName];
            return true;
        }
        else
        {
            sideOfFieldID = SideOfFieldID.One;
            return false;
        }
    }
    public static bool getSideOfField(string sideOfFieldID, out SideOfField sideOfField)
    {
        return getSideOfField(MyFunctions.parseEnum<SideOfFieldID>(sideOfFieldID), out sideOfField);
    }
    public static bool getSideOfField(SideOfFieldID sideOfFieldID, out SideOfField sideOfField)
    {
        sideOfField = null;
        if (sideOfFields.ContainsKey(TypeMatch.SizeFootballField))
        {
            if (sideOfFields[TypeMatch.SizeFootballField].ContainsKey(sideOfFieldID))
            {
                sideOfField = sideOfFields[TypeMatch.SizeFootballField][sideOfFieldID];
                return true;
            }
        }
        return false;
    }
    static void addGoalKeeperToTeam(string teamName, SideOfFieldID sideOfFieldID)
    {
        /*
        GameObject goal = sideOfFields[TypeMatch.SizeFootballField][sideOfFieldID].goal;
        if (goal != null)
        {
            PlayerIDMonoBehaviour playerIDMonoBehaviour = goal.GetComponentInChildren<PlayerIDMonoBehaviour>();
            Team team = Teams.getTeamByName(teamName);
            team.addPlayer(playerIDMonoBehaviour.getStringID());
            team.assignFieldPositionToPlayer(playerIDMonoBehaviour.getStringID(), TypeFieldPosition.Type.GoalKeeper.ToString());
        }*/
    }
    public static void SetRandomSideOfField(List<string> teamNames)
    {
        foreach (var teamName in teamNames)
        {
            List<SideOfFieldID> sideOfFieldIDs = MyFunctions.EnumToList<SideOfFieldID>();
            foreach (var item in sideOfFieldTeams)
            {
                sideOfFieldIDs.Remove(item.Value);
            }
            if (sideOfFieldIDs.Count > 0)
            {
                int index = Random.Range(0, sideOfFieldIDs.Count);
                setTeamSide(teamName, sideOfFieldIDs[index]);
            }
        }
    }
    public static List<SideOfFieldID> GetAvailableSideOfFields()
    {
        List<SideOfFieldID> sideOfFieldIDs = MyFunctions.EnumToList<SideOfFieldID>();
        foreach (var item in sideOfFieldTeams)
        {
            sideOfFieldIDs.Remove(item.Value);
        }
        return sideOfFieldIDs;
    }
    public static SideOfFieldID sideOfFieldOfTeam(string teamName)
    {
        return sideOfFieldTeams[teamName];
    }
    public static Vector3 TransformPointSideOfField(string teamName,Vector3 vector3)
    {
        SideOfFieldID sideOfFieldID = sideOfFieldTeams[teamName];

        SideOfField sideOfField = sideOfFields[TypeMatch.SizeFootballField][sideOfFieldID];
        return sideOfField.transform.TransformPoint(vector3);
    }
    public static Vector3 TransformDirectionSideOfField(string teamName, Vector3 vector3)
    {
        SideOfFieldID sideOfFieldID = sideOfFieldTeams[teamName];
        SideOfField sideOfField = sideOfFields[TypeMatch.SizeFootballField][sideOfFieldID];
        return sideOfField.transform.TransformDirection(vector3);
    }
    public static Quaternion TransformRotationSideOfField(string teamName, Vector3 vector3)
    {
        SideOfFieldID sideOfFieldID = sideOfFieldTeams[teamName];
        return sideOfFields[TypeMatch.SizeFootballField][sideOfFieldID].transform.rotation;
    }
}
