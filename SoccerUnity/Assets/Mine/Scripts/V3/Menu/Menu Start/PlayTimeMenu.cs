using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
public class PlayTimeMenu : MonoBehaviour
{
    public GameObject playOnline,nickName,initMenu,options;
    public bool enable;
    void Start()
    {
        if (SceneSetup.staticTypeMatch == TypeMatchID.Playtime)
        {
            MenuOptions menuOptions = options.GetComponent<MenuOptions>();
            menuOptions.backEvent += Back;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            /*
            menu.enabled = !menu.enabled;
            if (menu.enabled)
            {
                showMenu();
            }
            else
            {
                hideMenu();
            }*/
        }
    }
    public void Back()
    {
        initMenu.SetActive(true);
        enable = true;
    }
    public void Show()
    {
        gameObject.SetActive(true);
        initMenu.SetActive(true);
        playOnline.SetActive(false);
        options.SetActive(false);
        enable = true;
        if (ComponentsPlayer.currentComponentsPlayer != null)
        {
            ComponentsPlayer.currentComponentsPlayer.EnableOnlyCamera();
        }
        CursorCtrl.notifyShowMenu();
        MenuCtrl.isEnable = true;
        MenuCtrl.showMenuEvent.Invoke();
    }
    public void InitHide()
    {
        /*initMenu.SetActive(false);
        playOnline.SetActive(false);
        options.SetActive(false);*/
        gameObject.SetActive(false);
        enable = false;
        MenuCtrl.isEnable = false;
        //ComponentsPlayer.currentComponentsPlayer.EnableAll();
        CursorCtrl.notifyHideMenu();
    }
    public void Hide()
    {
        /*initMenu.SetActive(false);
        playOnline.SetActive(false);
        options.SetActive(false);*/
        gameObject.SetActive(false);
        options.SetActive(false);
        enable = false;
        if (ComponentsPlayer.currentComponentsPlayer != null)
        {
            ComponentsPlayer.currentComponentsPlayer.EnableAll();
        }
        CursorCtrl.notifyHideMenu();
        MenuCtrl.isEnable = false;
        MenuCtrl.hideMenuEvent.Invoke();
    }
    public void playOnline_Click()
    {
        //nickName.enabled = true;
        initMenu.SetActive(false);
        playOnline.SetActive(true);
        options.SetActive(false);
    }
    public void playOptions_Click()
    {
        initMenu.SetActive(false);
        playOnline.SetActive(false);
        options.SetActive(true);
    }
    public void back_Click()
    {
        Hide();
    }
    public void CloseGame_Click()
    {
        Application.Quit();
    }
}
