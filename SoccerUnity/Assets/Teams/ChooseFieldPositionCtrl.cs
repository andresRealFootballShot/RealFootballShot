using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseFieldPositionCtrl : MonoBehaviour
{
    public ChooseFieldPositionMenu chooseFieldPositionMenu;
    public static TypeFieldPosition.Type typeFieldSelected;
    public ButtonCtrl okButton;
    public PublicPlayerDataList publicPlayerDataList;
    public DataFieldPositionCtrl dataFieldPositionCtrl;
    private void Start()
    {
        if (FieldPositionsCtrl.UIFieldPositions != null)
        {
            ToggleCtrl[] toggleCtrls = FieldPositionsCtrl.UIFieldPositions.transform.GetComponentsInChildren<ToggleCtrl>();
            foreach (ToggleCtrl item in toggleCtrls)
            {
                item.Enable();
                TypeFieldPosition typeFieldPosition = item.GetComponent<TypeFieldPosition>();
                item.selectUnityEvent.AddListener(delegate { SelectFieldPosition(typeFieldPosition); });
            }
        }
        MatchEvents.fieldPositionsChanged.AddListener(updateData);
        okButton.selectedEvent.AddListener(requestFieldPosition);
        okButton.selectedEvent.AddListener(okFieldPositionClicked);
    }
    void requestFieldPosition()
    {
        MatchComponents.requestFieldPosition.RequestFieldPosition(ComponentsPlayer.myMonoPlayerID.getStringID(), ChooseTeamCtrl.teamSelected.TeamName, typeFieldSelected.ToString());
    }
    private void SelectFieldPosition(TypeFieldPosition typeFieldPosition)
    {
        okButton.Enable();
        typeFieldSelected = typeFieldPosition.Value;
    }

    public void okFieldPositionClicked()
    {
        okButton.Disable();
    }
    public void ShowMenu()
    {
        DebugsList.RPCRequestFieldPosition.print("ChooseFieldPositionCtrl.ShowMenu()");
        okButton.Disable();
        Team team = ChooseTeamCtrl.teamSelected;
        Lineup teamLineup = team.choosedLineup;
        List<GameObject> list = MyFunctions.GetChildsWithComponent<Lineup>(FieldPositionsCtrl.UIFieldPositions, false,true);
        foreach (GameObject lineupUI in list)
        {
            Lineup lineup = lineupUI.GetComponent<Lineup>();
            TypeNormalMatchClass typeMatchName = lineupUI.GetComponent<TypeNormalMatchClass>();
            Canvas canvas = lineupUI.GetComponent<Canvas>();
            canvas.enabled = typeMatchName.Value == TypeMatch.typeNormalMatch && lineup.typeLineup == teamLineup.typeLineup;
        }
        //GameObject lineupUIGObj = list.Find(item => item.GetComponent<Lineup>().typeMatch == teamLineup.typeMatch && item.GetComponent<Lineup>().typeLineup.Equals(teamLineup.typeLineup));
        GameObject lineupUIGObj = FieldPositionsCtrl.getUILineup(team.choosedLineup.typeLineup);
        UIColorAnimation[] colorAnimations = lineupUIGObj.transform.GetComponentsInChildren<UIColorAnimation>();
        foreach (UIColorAnimation item in colorAnimations)
        {
            item.hueColorVar = team.colorVar;
        }
        ToggleCtrl[] toggleCtrls = lineupUIGObj.transform.GetComponentsInChildren<ToggleCtrl>();
        //DebugsList.testing.print("ShowMenu");
        foreach (ToggleCtrl item in toggleCtrls)
        {
            TypeFieldPosition typeFieldPosition = item.GetComponent<TypeFieldPosition>();
            string playerID = team.fieldPositionOfPlayers[typeFieldPosition.Value];
            //MyFunctions.GetKeyByValue(team.fieldPositionOfPlayers, typeFieldPosition.Value,out playerID);
            
            if (team.fieldPositionOfPlayers.ContainsKey(typeFieldPosition.Value) && team.players.Contains(playerID))
            {
                //DebugsList.testing.print("Disable "+ playerID+ " "+ typeFieldPosition.Value.ToString());
                if (item.isSelected)
                {
                    //Si hay un fieldPosition seleccionado que ahora está ocupado lo deselecciona
                    item.Deselect();
                    okButton.Disable();
                }
                item.Disable();
            }
            else
            {
                if (item.isSelected)
                {
                    //Si hay un fieldPosition seleccionado que ahora no está ocupado
                    okButton.Enable();
                }
                //DebugsList.testing.print("Enable " + playerID + " " + typeFieldPosition.Value.ToString());
                item.Enable();
            }
        }
        dataFieldPositionCtrl.clearFieldPositionDataUIText(team);
        dataFieldPositionCtrl.showDataFieldPositionsOfTeam(team);
        chooseFieldPositionMenu.ShowMenu();
        CursorCtrl.notifyShowMenu();
    }
    public void updateData()
    {
        if (chooseFieldPositionMenu.isShow())
        {

            DebugsList.RPCRequestFieldPosition.print("ChooseFieldPositionCtrl.updateData()");
            Team team = ChooseTeamCtrl.teamSelected;
            GameObject lineupUIGObj = FieldPositionsCtrl.getUILineup( team.choosedLineup.typeLineup);
            ToggleCtrl[] toggleCtrls = lineupUIGObj.transform.GetComponentsInChildren<ToggleCtrl>();
            bool allFieldPositionsOccupied = true;
            foreach (ToggleCtrl item in toggleCtrls)
            {
                TypeFieldPosition typeFieldPosition = item.GetComponent<TypeFieldPosition>();
                string playerID = team.fieldPositionOfPlayers[typeFieldPosition.Value];
                //MyFunctions.GetKeyByValue(team.fieldPositionOfPlayers, typeFieldPosition.Value, out playerID);
                if (team.fieldPositionOfPlayers.ContainsKey(typeFieldPosition.Value) && team.players.Contains(playerID))
                {
                    if (item.isSelected)
                    {
                        item.Deselect();
                    }
                    item.Disable();
                }
                else
                {
                    item.Enable();
                    allFieldPositionsOccupied = false;
                }
            }

            if (allFieldPositionsOccupied)
            {
                okButton.Disable();
            }
            //dataFieldPositionCtrl.clearFieldPositionDataUIText(team);
            //dataFieldPositionCtrl.showDataFieldPositionsOfTeam(team);
        }
    }
    public void HideMenu()
    {
        CursorCtrl.notifyHideMenu();
        chooseFieldPositionMenu.HideMenu();
    }
}
