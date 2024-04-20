using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinToTeam : MonoBehaviour,IJoinToTeam
{
    public event JoiningProcessFinishedDelegate joininProcessFinishedEvent;
    //[HideInInspector]
    //public List<ChooseFieldPositionCtrl> chooseFieldPositionCtrls;
    public ChooseFieldPositionCtrl chooseFieldPositionCtrl;
    /*public void joinToTeam(string teamName)
    {
        
        foreach (ChooseFieldPositionCtrl chooseFieldPositionCtrl in chooseFieldPositionCtrls)
        {
            if (chooseFieldPositionCtrl.teamNameVar.Value.Equals(teamName))
            {
                
                chooseFieldPositionCtrl.ShowMenu();
            }
            else
            {
                chooseFieldPositionCtrl.HideMenu();
            }
        }
        chooseFieldPositionCtrl.ShowMenu(team);
    }*/
    public void joinToTeam()
    {
        chooseFieldPositionCtrl.ShowMenu();
    }
}
