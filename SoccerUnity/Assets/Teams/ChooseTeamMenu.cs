using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ChooseTeamMenu : MonoBehaviour,ILoad
{
    public Canvas canvasMenu;
    public List<ToggleCtrl> toggleCtrls = new List<ToggleCtrl>();
    Dictionary<string, ToggleCtrl> dictionaryNameToggles = new Dictionary<string, ToggleCtrl>();
    public static int staticLoadLevel=Teams.staticLoadLevel+1;
    public int loadLevel { get => staticLoadLevel; set => staticLoadLevel = value; }

    public void Load(int level)
    {
        if (loadLevel == level)
        {
            load();
        }
    }
    private void load()
    {
        List<string> teamNames = Teams.getTeamNames();
        if (toggleCtrls.Count != teamNames.Count)
        {
            Debug.LogError("Toggles.Count and teamNames.Count are different");
        }
        else
        {
            for (int i = 0; i < toggleCtrls.Count; i++)
            {
                dictionaryNameToggles.Add(teamNames[i], toggleCtrls[i]);
            }
        }
    }
    public void Select(Variable<string> nameTeam)
    {
        dictionaryNameToggles[nameTeam.Value].SelectFunction();
    }
    public void Deselect(Variable<string> nameTeam)
    {
        dictionaryNameToggles[nameTeam.Value].Deselect();
    }
    public void ShowMenu()
    {
        canvasMenu.enabled = true;
    }
    public void HideMenu()
    {
        canvasMenu.enabled = false;
    }
}
