using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingMenu : MonoBehaviour
{
    public Canvas canvas;
    void Start()
    {
        
    }
    public void ShowMenu()
    {
        canvas.enabled = true;
    }
    public void HideMenu()
    {
        canvas.enabled = false;
    }
}
