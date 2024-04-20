using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UITextFieldPositionCtrl : MonoBehaviour
{
    public TypeFieldPosition typeFieldPosition;
    public Variable<string> namePlayer;
    public TextMeshProUGUI typeFieldPositionText,namePlayerText;
    void Awake()
    {
        if (namePlayer != null)
        {
            namePlayer.VariableChanged += namePlayerChanged;
        }
    }
    public void setNameStringVar(Variable<string> namePlayer)
    {
        if (this.namePlayer != null)
        {
            this.namePlayer.VariableChanged -= namePlayerChanged;
        }
        this.namePlayer = namePlayer;
        this.namePlayer.VariableChanged += namePlayerChanged;
        
    }
    public void removeNameStringVar()
    {
        if (this.namePlayer != null)
        {
            this.namePlayer.VariableChanged -= namePlayerChanged;
        }
        namePlayer = null;
    }
    void namePlayerChanged(string value)
    {
        updateText();
    }
    public void updateText()
    {
        typeFieldPositionText.text = typeFieldPosition.getUIString();
        namePlayerText.text = namePlayer.Value;
    }
    public void clearText()
    {
        typeFieldPositionText.text = typeFieldPosition.getUIString();
        namePlayerText.text = "";
    }
}
