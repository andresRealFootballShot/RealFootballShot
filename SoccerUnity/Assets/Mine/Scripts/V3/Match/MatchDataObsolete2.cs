using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MatchStateObsolete
{
    WaitingForWarmUp,
    WarmUp,
    Running,
    Stoped,
    Ended
}
public class MatchDataObsolete2 : MonoBehaviour,IClearBeforeLoadScene
{
    public static bool isStarted;
    static Variable<MatchStateObsolete> matchStateVar = new Variable<MatchStateObsolete>();
    public static MatchStateObsolete matchState { get=> matchStateVar.Value; set => setMatchState(value); }
    public static string teamNameOfServe="";
    public static int currentPart;
    static emptyDelegate changeStateListeners;
    public static int myActor = 0;
    public static string referee;
    public static string myReferee;
    public static bool ImReferee;
    public static string lastPlayerIDPossession="";
    public static string lastTeamPossession = "";
    public static int responsibleForTheBall;
    public static bool ImResponsibleForTheBall;
    public static int responsibleForTheGoalkeepers;
    public static bool ImResponsibleForTheGoalkeepers;
    public static bool isLoaded;
    void Start()
    {
        LoadListeners();
    }
    public static void setReferee(string _referee, string _myReferee)
    {
        referee = _referee;
        myReferee = _myReferee;
        if (referee == myReferee)
        {
            ImReferee = true;
        }
        else
        {
            ImReferee = false;
        }
        if (!referee.Equals(""))
        {
            DebugsList.rules.print("MatchData.setReferee() refereeIsAssigned");
            RulesEvents.refereeIsAssigned.Invoke();
        }
    }
    public static void setResponsibleForTheBall(int _responsibleForTheBall)
    {
        responsibleForTheBall = _responsibleForTheBall;
        if (responsibleForTheBall == myActor)
        {
            ImResponsibleForTheBall = true;
        }
        else
        {
            ImResponsibleForTheBall = false;
        }
    }
    public static void setResponsibleForTheGoalkeepers(int _responsibleForTheGoalkeepers)
    {
        responsibleForTheGoalkeepers = _responsibleForTheGoalkeepers;
        if (responsibleForTheGoalkeepers == myActor)
        {
            ImResponsibleForTheBall = true;
        }
        else
        {
            ImResponsibleForTheBall = false;
        }
    }
    public static object[] getData()
    {
        List<object> data = new List<object>();
        data.Add(matchState.ToString());
        data.Add(teamNameOfServe);
        data.Add(referee);
        data.Add(currentPart);
        data.Add(lastPlayerIDPossession);
        return data.ToArray();
    }
    public static void setData(object[] data)
    {
        int index = 0;
        string matchStateStr = (string)data[index++];
        teamNameOfServe = (string)data[index++];
        string referee = (string)data[index++];
        currentPart = (int)data[index++];
        string _lastPlayerIDPossession = (string)data[index++];
        setReferee(referee,ComponentsPlayer.myMonoPlayerID.playerID.onlineActor.ToString());
        matchState = MyFunctions.parseEnum<MatchStateObsolete>(matchStateStr);
        lastPlayerIDPossession = _lastPlayerIDPossession;
    }
    public void Clear()
    {
        isStarted = false;
        //matchState = MatchState.WaitingForWarmUp;
        lastPlayerIDPossession = "";
        currentPart = 0;
        myActor = 0;
        matchStateVar = new Variable<MatchStateObsolete>();
        changeStateListeners = null;
    }
    public static void addChangeStateListener(emptyDelegate listener)
    {
        changeStateListeners += listener;
        //matchStateVar.addObserverAndExecuteIfValueNotIsNull(observer);
    }
    public static void removeChangeStateListener(emptyDelegate listener)
    {
        MyFunctions.RemoveListener(changeStateListeners, listener);
        //matchStateVar.addObserverAndExecuteIfValueNotIsNull(observer);
    }
    static void setMatchState(MatchStateObsolete value)
    {
        MatchStateObsolete previousState = matchStateVar.Value;
        //matchStateVar.Value = value;
        
        switch (value)
        {
            case MatchStateObsolete.WaitingForWarmUp:
                MatchEvents.waitingWarmUp.Invoke();
                break;
            case MatchStateObsolete.WarmUp:
                MatchEvents.warmUp.Invoke();
                break;
            case MatchStateObsolete.Running:
                if(previousState == MatchStateObsolete.WarmUp || previousState == MatchStateObsolete.WaitingForWarmUp)
                {
                    MatchEvents.initialMatch.Invoke();
                }
                break;
            case MatchStateObsolete.Stoped:
                MatchEvents.stopMatch.Invoke();
                break;
            case MatchStateObsolete.Ended:
                MatchEvents.endMatch.Invoke();
                break;
        }
    }
    private void LoadListeners()
    {
        MatchEvents.initialMatch.AddListener(startMatchEvent);
        MatchEvents.waitingWarmUp.AddListenerConsiderInvoked(waitingWarmUpEvent);
        MatchEvents.warmUp.AddListenerConsiderInvoked(warmUpEvent);
        MatchEvents.continueMatch.AddListener(continueMatchEvent);
        MatchEvents.endPart.AddListener(stopMatchEvent);
        MatchEvents.endMatch.AddListener(endMatchEvent);
        MatchEvents.kick.AddListener(setPlayerIDPossession);
    }
    void setPlayerIDPossession(KickEventArgs args)
    {
        lastPlayerIDPossession = args.playerID;
        Team team;
        if(Teams.getTeamFromPlayer(args.playerID,out team))
        {
            lastTeamPossession = team.TeamName;
        }
        else
        {
            lastTeamPossession = "";
        }
    }
    void waitingWarmUpEvent()
    {
        matchStateVar.Value = MatchStateObsolete.WaitingForWarmUp;
        isStarted = false;
        changeStateListeners?.Invoke();
    }
    void warmUpEvent()
    {
        matchStateVar.Value = MatchStateObsolete.WarmUp;
        isStarted = false;
        changeStateListeners?.Invoke();
    }
    void startMatchEvent()
    {
        matchStateVar.Value = MatchStateObsolete.Running;
        isStarted = true;
        changeStateListeners?.Invoke();
    }
    void continueMatchEvent()
    {
        matchStateVar.Value = MatchStateObsolete.Running;
        changeStateListeners?.Invoke();
    }
    void stopMatchEvent()
    {
        matchStateVar.Value = MatchStateObsolete.Stoped;
        changeStateListeners?.Invoke();
    }
    void endMatchEvent()
    {
        matchStateVar.Value = MatchStateObsolete.Ended;
        changeStateListeners?.Invoke();
    }
}
