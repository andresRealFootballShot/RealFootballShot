using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipementCtrl : MonoBehaviour,ILoad
{
    public static int staticLoadLevel = MatchEvents.staticLoadLevel + 1;
    public int loadLevel { get => staticLoadLevel; set => staticLoadLevel = value; }
    public void Load(int level)
    {
        if (loadLevel == level)
        {
            MatchEvents.publicPlayerDataOfAddedPlayerToTeamIsAvailable.AddListener(playerAddedToTeam);
        }
    }
    void playerAddedToTeam(PlayerAddedToTeamEventArgs args)
    {
        
        StartCoroutine(waitUntilPublicPlayerDataIsAvailable(args));
    }
    IEnumerator waitUntilPublicPlayerDataIsAvailable(PlayerAddedToTeamEventArgs args)
    {
        PublicPlayerData publicPlayerData = PublicPlayerDataList.all[args.PlayerID];
        SetupModel setupModel = publicPlayerData.setupModel;
        if (setupModel != null)
        {
            yield return new WaitUntil(() => setupModel.isLoaded);
            Team team = Teams.getTeamByName(args.TeamName);
            setupModel.setEquipementColor(team.equipament);
            if (publicPlayerData.nickNameHUDCtrl != null)
            {
                publicPlayerData.nickNameHUDCtrl.setColor(team.equipament);
            }
        }

    }
}
