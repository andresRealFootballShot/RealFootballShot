using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseFieldPositionMenu : MonoBehaviour
{
    public Canvas lineupUIListCanvas;
    void Start()
    {
        
    }
     public void DisableMenu()
    {
        
    }
    public bool isShow()
    {
        return lineupUIListCanvas.enabled;
    }
    public void ShowMenu()
    {
        lineupUIListCanvas.enabled = true;
    }
    public void HideMenu()
    {
        if (lineupUIListCanvas!=null)
        {
            lineupUIListCanvas.enabled=false;
        }
    }
    public void EnableMenu()
    {

    }
}
