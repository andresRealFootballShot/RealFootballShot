using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoiningProcessFinishedEventArgs
{
    public bool finishedSuccessfully
    {
        get;set;
    }
    string name;
    public JoiningProcessFinishedEventArgs(bool finishedSuccessfully)
    {
        this.finishedSuccessfully = finishedSuccessfully;
    }
}
public class TeamsInfoEventArgs
{
    public bool finishedSuccessfully
    {
        get; set;
    }
    string name;
    public TeamsInfoEventArgs(bool finishedSuccessfully)
    {
        this.finishedSuccessfully = finishedSuccessfully;
    }
}
public delegate void JoiningProcessFinishedDelegate(JoiningProcessFinishedEventArgs args);
public delegate void TeamsInfoDelegate(TeamsInfoEventArgs args);
public interface IJoinToTeam
{
    event JoiningProcessFinishedDelegate joininProcessFinishedEvent;
    void joinToTeam();
}
public interface IGetData
{
    object[] getData();
}
public interface IDeliverData
{
    void deliverData(object[] data);
}
public interface INotifyNewOnlinePlayer
{
    void newOnlinePlayer(GameObject newPlayer);
}
public interface IRequestFieldPosition
{
   void RequestFieldPosition(string playerID,string teamName,string typeFieldPosition);
}
public interface ISetupTeams
{
    void SetupTeams();
}
public interface ILoad
{
    int loadLevel { get; set; }
    void Load(int level);
}
public interface IClearBeforeLoadScene
{
    void Clear();
}
public interface IKickOff
{
    string teamName { get; set; }
    void startProcess(string teamName);
    void startProcess();
    void notifyTeamServe(string teamName);
}
public interface IKickNotifier
{
    void notifyAddForce(KickEventArgs args);
    void notifySetData(BallData args);
}
public interface IRulesSettingsType<T>
{
    T settingsType { get; set; }
}
public interface ITransition
{
    IEnumerator Coroutine();
    bool isInterruptible { get; set; }
    emptyDelegate endTransition { get; set; }
}
public interface IBehaviour
{
    void setCurrentState(State currentState);
    void startTransition(ITransition coroutine);
    void stopCoroutines();
    void UpdateRun();
    void FixedUpdateRun();
}

