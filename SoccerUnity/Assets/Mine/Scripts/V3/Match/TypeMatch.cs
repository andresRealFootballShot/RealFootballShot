using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public interface INotifyTypeMatchInstantiated
{
    void typeMatchInstantiated();
}
public enum TypeMatchID
{
    NormalMatch,
    Playtime
}
public enum TypeNormalMatch
{
    OnlyOne,
    OneVSOne,
    TwoVSTwo,
    ThreeVSThree,
    FourVsFour,
    FiveVSFive,
    TenVSTen
}
public class TypeMatch : MonoBehaviour, IOnEventCallback
{
    public static Dictionary<TypeNormalMatch, string> levels = new Dictionary<TypeNormalMatch, string>()
    {
    { TypeNormalMatch.OnlyOne, "LittleMap" },
    { TypeNormalMatch.OneVSOne, "LittleMap" },
    { TypeNormalMatch.TwoVSTwo, "LittleMap" },
    { TypeNormalMatch.ThreeVSThree, "LittleMap" },
    { TypeNormalMatch.FourVsFour, "LittleMap" },
    { TypeNormalMatch.FiveVSFive, "LittleMap" },
    { TypeNormalMatch.TenVSTen, "LittleMap" }
    };
    public static Dictionary<TypeNormalMatch, byte> maxFieldPlayersDictionary = new Dictionary<TypeNormalMatch, byte>()
    {
    { TypeNormalMatch.OnlyOne, 1 },
    { TypeNormalMatch.OneVSOne, 2 },
    { TypeNormalMatch.TwoVSTwo, 4 },
    { TypeNormalMatch.ThreeVSThree, 6},
    { TypeNormalMatch.FourVsFour, 8},
    { TypeNormalMatch.FiveVSFive, 10},
    { TypeNormalMatch.TenVSTen, 20}
    };
    public static Dictionary<TypeNormalMatch, byte> maxPlayersDictionary = new Dictionary<TypeNormalMatch, byte>()
    {
    { TypeNormalMatch.OnlyOne, 3 },
    { TypeNormalMatch.OneVSOne, 4 },
    { TypeNormalMatch.TwoVSTwo, 6 },
    { TypeNormalMatch.ThreeVSThree, 8},
    { TypeNormalMatch.FourVsFour, 10},
    { TypeNormalMatch.FiveVSFive, 12},
    { TypeNormalMatch.TenVSTen, 22}
    };
    public static Dictionary<TypeNormalMatch, byte> numberOfTeams = new Dictionary<TypeNormalMatch, byte>()
    {
    { TypeNormalMatch.OnlyOne, 1 },
    { TypeNormalMatch.OneVSOne, 2 },
    { TypeNormalMatch.TwoVSTwo, 2 },
    { TypeNormalMatch.ThreeVSThree, 2},
    { TypeNormalMatch.FourVsFour, 2},
    { TypeNormalMatch.FiveVSFive, 2},
    { TypeNormalMatch.TenVSTen, 2}
    };
    public static Dictionary<TypeNormalMatch, SizeFootballFieldID> sizeFootballFieldDictionary = new Dictionary<TypeNormalMatch, SizeFootballFieldID>()
    {
    { TypeNormalMatch.OnlyOne, SizeFootballFieldID.ElevenVSEleven },
    { TypeNormalMatch.OneVSOne, SizeFootballFieldID.ElevenVSEleven },
    { TypeNormalMatch.TwoVSTwo, SizeFootballFieldID.ElevenVSEleven },
    { TypeNormalMatch.ThreeVSThree, SizeFootballFieldID.ElevenVSEleven},
    { TypeNormalMatch.FourVsFour, SizeFootballFieldID.ElevenVSEleven},
    { TypeNormalMatch.FiveVSFive, SizeFootballFieldID.ElevenVSEleven},
    { TypeNormalMatch.TenVSTen, SizeFootballFieldID.ElevenVSEleven}
    };
    public static Dictionary<TypeNormalMatch, List<TypeFieldPosition.Type>> fieldPositioinsInTypeMatch = new Dictionary<TypeNormalMatch, List<TypeFieldPosition.Type>>()
    {
    { TypeNormalMatch.TenVSTen, new List<TypeFieldPosition.Type>(){ TypeFieldPosition.Type.RightForward, TypeFieldPosition.Type.LeftForward, TypeFieldPosition.Type.LeftCentreMidfield, TypeFieldPosition.Type.RightCentreMidfield, TypeFieldPosition.Type.LeftOutsideMidfield, TypeFieldPosition.Type.RightOutsideMidfield, TypeFieldPosition.Type.LeftOutsideDefense, TypeFieldPosition.Type.RightOutsideDefense, TypeFieldPosition.Type.CentreLeftBack, TypeFieldPosition.Type.CentreRightBack, TypeFieldPosition.Type.GoalKeeper } },
        { TypeNormalMatch.FourVsFour, new List<TypeFieldPosition.Type>(){ TypeFieldPosition.Type.RightForward, TypeFieldPosition.Type.LeftForward, TypeFieldPosition.Type.CentreLeftBack, TypeFieldPosition.Type.CentreRightBack, TypeFieldPosition.Type.GoalKeeper } }
    };
    public static string getNameScene(string typeMatchString)
    {
        TypeNormalMatch typeMatch = parseString(typeMatchString);
        return levels[typeMatch];
    }
    public static byte getMaxPlayers(string typeMatchString)
    {
        TypeNormalMatch typeMatch = parseString(typeMatchString);
        return maxFieldPlayersDictionary[typeMatch];
    }
    public static int getGlobalMaxPlayersWithGoalkeepers()
    {
        TypeNormalMatch typeMatch = typeNormalMatch;
        int maxPlayers = maxFieldPlayersDictionary[typeMatch];
        int teamsSize = numberOfTeams[typeMatch];
        //print("a " + maxPlayers + " " + teamsSize+ " "+ typeNormalMatch);
        return maxPlayers + teamsSize;
    }
    public static int getTeamMaxPlayersWithGoalkeepers()
    {
        TypeNormalMatch typeMatch = typeNormalMatch;
        int teamsSize = numberOfTeams[typeMatch];
        int maxPlayers = (maxFieldPlayersDictionary[typeMatch] / teamsSize)+1;
        //print("a " + maxPlayers + " " + teamsSize+ " "+ typeNormalMatch);
        return maxPlayers;
    }
    public static int getTemsSize()
    {
        TypeNormalMatch typeMatch = typeNormalMatch;
        int teamsSize = numberOfTeams[typeMatch];
        return teamsSize;
    }
    [SerializeField]
    TypeNormalMatch NormalMatchType;
    public static SceneModeID sceneMode;
    public static TypeMatchID typeMatch { get; set; }
    public static TypeNormalMatch typeNormalMatch { get; set; }
    public static int maxFieldPlayers { get; set; }
    public static int maxPlayers { get; set; }
    public static int maxTeamPlayers { get; set; }
    public static SizeFootballFieldID SizeFootballField { get => sizeFootballField; set => setSizeFootballField(value); }

    public static bool isPublic;
    private static SizeFootballFieldID sizeFootballField;
    public EmptyEventSC typeMatchInstantiatedEvent;
    public static string getLevel()
    {
        switch (typeMatch)
        {
            case TypeMatchID.NormalMatch:
                return levels[typeNormalMatch];
            case TypeMatchID.Playtime:
                return "LittleMap";
        }
        return "";
    }
    void Start()
    {
        setup(NormalMatchType, false);
    }
    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    public static void SendTypeMatchData(string typeNormalMatch,bool isPublic, SizeFootballFieldID sizeFootballField)
    {
        object[] content = new object[] { typeNormalMatch , isPublic , sizeFootballField.ToString()}; // Array contains the target position and the IDs of the selected units
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCacheGlobal }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent(CodeEventsNet.SendTypeMatchData, content, raiseEventOptions, SendOptions.SendReliable);
    }
    public static void setup(string typeMatchString,bool isPublic)
    {
        typeMatch = TypeMatchID.NormalMatch;
        typeNormalMatch = parseString(typeMatchString);
        maxFieldPlayers = maxFieldPlayersDictionary[typeNormalMatch];
        maxPlayers = maxPlayersDictionary[typeNormalMatch];
        maxTeamPlayers = maxPlayersDictionary[typeNormalMatch]/ numberOfTeams[typeNormalMatch];
        TypeMatch.isPublic = isPublic;
        //SizeFootballField = sizeFootballField;
        SizeFootballField = sizeFootballFieldDictionary[typeNormalMatch];
        MatchEvents.typeMatchSetuped.Invoke();
    }
    public static void setup(TypeNormalMatch TypeNormalMatch, bool isPublic)
    {
        typeMatch = TypeMatchID.NormalMatch;
        typeNormalMatch = TypeNormalMatch;
        maxFieldPlayers = maxFieldPlayersDictionary[typeNormalMatch];
        maxPlayers = maxPlayersDictionary[typeNormalMatch];
        maxTeamPlayers = maxPlayersDictionary[typeNormalMatch] / numberOfTeams[typeNormalMatch];
        TypeMatch.isPublic = isPublic;
        //SizeFootballField = sizeFootballField;
        SizeFootballField = sizeFootballFieldDictionary[typeNormalMatch];
        MatchEvents.typeMatchSetuped.Invoke();
    }
    static void setSizeFootballField(SizeFootballFieldID sizeFootballField)
    {
        TypeMatch.sizeFootballField = sizeFootballField;
        
        MatchEvents.sizeFootballFieldChanged.Invoke();
    }
    public static TypeNormalMatch parseString(string typeMatchName)
    {
        return (TypeNormalMatch)System.Enum.Parse(typeof(TypeNormalMatch), typeMatchName);
    }
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == CodeEventsNet.SendTypeMatchData)
        {
            object[] data = (object[])photonEvent.CustomData;
            string typeNormalMatch = (string)data[0];
            bool isPublic = (bool)data[1];
            SizeFootballFieldID sizeFootballField = MyFunctions.parseEnum<SizeFootballFieldID>((string)data[2]);
            setup(typeNormalMatch, isPublic);
            var list = FindObjectsOfType<MonoBehaviour>().OfType<INotifyTypeMatchInstantiated>();
            foreach (INotifyTypeMatchInstantiated item in list)
            {
                item.typeMatchInstantiated();
            }
            typeMatchInstantiatedEvent.Raise();
            MatchEvents.typeMatchSetuped.Invoke();
        }
    }
}
