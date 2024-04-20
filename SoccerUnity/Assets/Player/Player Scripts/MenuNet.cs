using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class MenuNet : MonoBehaviourPunCallbacks
{
    
    public GameObject initMenu, options;
    public bool enable;
    void Start()
    {
        if(SceneSetup.staticTypeMatch == TypeMatchID.NormalMatch)
        {
            MenuOptions menuOptions = options.GetComponent<MenuOptions>();
            menuOptions.backEvent += back;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menu.enabled)
            {
                hideMenu();
            }
            else
            {
                showMenu();
            }
        }*/
    }
    private void OnApplicationFocus(bool focus)
    {
    }
    public void CloseGameClick()
    { 
        Application.Quit();
    }
    public void DisconnectClick()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            LoadScene.loadPlaytimeScene();
        }
    }
    public void Back_Click()
    {
        Hide();
    }
    public void Options_Click()
    {
        initMenu.SetActive(false);
        options.SetActive(true);
    }
    void back()
    {
        initMenu.SetActive(true);
        options.SetActive(false);
        enable = true;
    }
    public void Show()
    {
        initMenu.SetActive(true);
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
    public void Hide()
    {
        initMenu.SetActive(false);
        options.SetActive(false);
        enable = false;
        ComponentsPlayer.currentComponentsPlayer.EnableAll();
        CursorCtrl.notifyHideMenu();
        MenuCtrl.isEnable = false;
        MenuCtrl.hideMenuEvent.Invoke();
    }
    public void InitHide()
    {
        initMenu.SetActive(false);
        options.SetActive(false);
        enable = false;
        //ComponentsPlayer.currentComponentsPlayer.EnableAll();
        CursorCtrl.notifyHideMenu();

    }
}
