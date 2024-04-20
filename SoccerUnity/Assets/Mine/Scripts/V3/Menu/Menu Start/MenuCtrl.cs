using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCtrl : MonoBehaviour,IClearBeforeLoadScene
{
    public static MenuCtrl menu;
    public PlayTimeMenu playTimeMenu;
    public MenuNet netMenu;
    public Canvas canvas;
    public static bool isEnable;
    public static MyEvent showMenuEvent = new MyEvent(nameof(showMenuEvent));
    public static MyEvent hideMenuEvent = new MyEvent(nameof(hideMenuEvent));
    void Start()
    {
        MatchEvents.enableMenu.AddListenerConsiderInvoked(enableMenu);
        menu = this;
        canvas.enabled = true;
        menu.playTimeMenu.InitHide();
        menu.netMenu.InitHide();
        enabled = false;
    }
    void enableMenu()
    {
        enabled = true;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneSetup.staticTypeMatch == TypeMatchID.Playtime)
            {
                if (!playTimeMenu.enable)
                {
                    menu.playTimeMenu.Show();
                    
                }
                else
                {
                    menu.playTimeMenu.Hide();
                }
                //netMenu.Hide();
            }else if(SceneSetup.staticTypeMatch == TypeMatchID.NormalMatch)
            {
                //playTimeMenu.Hide();
                
                if (!netMenu.enable)
                {
                    menu.netMenu.Show();
                }
                else
                {
                    menu.netMenu.Hide();
                }
            }
        }
        //print(isEnable);
    }

    public void Clear()
    {
        showMenuEvent = new MyEvent(nameof(showMenuEvent));
        hideMenuEvent = new MyEvent(nameof(hideMenuEvent));
    }
}
