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
    { TypeNormalMatch.FiveVSFive, "LittleMap" },
    { TypeNormalMatch.TenVSTen, "LittleMap" }
    };
    public static Dictionary<TypeNormalMatch, byte> maxPlayersDictionary = new Dictionary<TypeNormalMatch, byte>()
    {
    { TypeNormalMatch.OnlyOne, 1 },
    { TypeNormalMatch.OneVSOne, 2 },
    { TypeNormalMatch.TwoVSTwo, 4 },
    { TypeNormalMatch.ThreeVSThree, 6},
    { TypeNormalMatch.FiveVSFive, 10},
    { TypeNormalMatch.TenVSTen, 20}
    };
    public static Dictionary<TypeNormalMatch, byte> numberOfTeams = new Dictionary<TypeNormalMatch, byte>()
    {
    { TypeNormalMatch.OnlyOne, 1 },
    { TypeNormalMatch.OneVSOne, 2 },
    { TypeNormalMatch.TwoVSTwo, 2 },
    { TypeNormalMatch.ThreeVSThree, 2},
    { TypeNormalMatch.FiveVSFive, 2},
    { TypeNormalMatch.TenVSTen, 2}
    };
    public static Dictionary<TypeNormalMatch, SizeFootballFieldID> sizeFootballFieldDictionary = new Dictionary<TypeNormalMatch, SizeFootballFieldID>()
    {
        { TypeNormalMatch.OnlyOne, SizeFootballFieldID.ElevenVSEleven },
    { TypeNormalMatch.OneVSOne, SizeFootballFieldID.ElevenVSEleven },
    { TypeNormalMatch.TwoVSTwo, SizeFootballFieldID.ElevenVSEleven },
    { TypeNormalMatch.ThreeVSThree, SizeFootballFieldID.FiveVSFive},
    { TypeNormalMatch.FiveVSFive, SizeFootballFieldID.SevenVSSeven},
    { TypeNormalMatch.TenVSTen, SizeFootballFieldID.ElevenVSEleven}
    };
    public static string getNameScene(string typeMatchString)
    {
        TypeNormalMatch typeMatch = parseString(typeMatchString);
        return levels[typeMatch];
    }
    public static byte getMaxPlayers(string typeMatchString)
    {
        TypeNormalMatch typeMatch = parseString(typeMatchString);
        return maxPlayersDictionary[typeMatch];
    }
    public static int getGlobalMaxPlayersWithGoalkeepers()
    {
        TypeNormalMatch typeMatch = typeNormalMatch;
        int maxPlayers = maxPlayersDictionary[typeMatch];
        int teamsSize = numberOfTeams[typeMatch];
        //print("a " + maxPlayers + " " + teamsSize+ " "+ typeNormalMatch);
        return maxPlayers + teamsSize;
    }
    public static int getTeamMaxPlayersWithGoalkeepers()
    {
        TypeNormalMatch typeMatch = typeNormalMatch;
        int teamsSize = numberOfTeams[typeMatch];
        int maxPlayers = (maxPlayersDictionary[typeMatch] / teamsSize)+1;
        //print("a " + maxPlayers + " " + teamsSize+ " "+ typeNormalMatch);
        return maxPlayers;
    }
    public static int getTemsSize()
    {
        TypeNormalMatch typeMatch = typeNormalMatch;
        int teamsSize = numberOfTeams[typeMatch];
        return teamsSize;
    }
    public static SceneModeID sceneMode;
    public static TypeMatchID typeMatch { get; set; }
    public static TypeNormalMatch typeNormalMatch { get; set; }
    public static int maxPlayers { get; set; }
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
        maxPlayers = maxPlayersDictionary[typeNormalMatch];
        TypeMatch.isPublic = isPublic;
        //SizeFootballField = sizeFootballField;
        SizeFootballField = sizeFootballFieldDictionary[typeNormalMatch];
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
