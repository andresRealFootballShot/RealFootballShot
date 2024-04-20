using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class EndMatchAnimation : MonoBehaviour
{
    public Canvas endMatchMenu;
    public AudioSource audioSource;
    public Text redGoals,blueGoals;
    void Start()
    {
        
    }
    public void play()
    {
        if (audioSource != null)
        {
            audioSource.Play();
            Invoke(nameof(showMenu), MatchComponents.rulesSettings.timeWaitToShowEndMenu);
        }
        
    }
    void showMenu()
    {
        CursorCtrl.notifyShowMenu();
        endMatchMenu.enabled = true;
        Team redTeam = Teams.getTeamByName("Red");
        Team blueTeam = Teams.getTeamByName("Blue");
        redGoals.text = redTeam.goals.Count.ToString();
        blueGoals.text = blueTeam.goals.Count.ToString();
    }
}
