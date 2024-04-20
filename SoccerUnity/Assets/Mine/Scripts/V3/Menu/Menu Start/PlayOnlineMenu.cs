using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
public class PlayOnlineMenu : MonoBehaviour
{
    public Button playOnlineButton;
    public GameObject nickNameMenu,searchingMenu,searchFailedMenu,chooseTypeMatchMenu;
    public InputField inputField;
    public FindRandomMatch findRandomMatch;
    public Button okButton;
    public PlayTimeMenu menu;
    bool enableNick;
    Coroutine checkEnterCoroutine;
    void Awake()
    {
        playOnlineButton.onClick.AddListener(playOnlineClick);
        OnlineEvents.connected.AddListenerConsiderInvoked(() => playOnlineButton.interactable = true);
        OnlineEvents.createRoomFailed.AddListenerConsiderInvoked(createRoomFailed);

        //okButtonCtrl.Disable();
        checkOkButton();
    }

    public void playOnlineClick()
    {
        string nickName = PlayerPrefs.GetString("nickName");
        if (nickName.Equals("") || true)
        {
            showInitStateOfPlayOnlineMenu();
        }
        else
        {
            PhotonNetwork.NickName = nickName;
            //startFindRandomMatch();
            showChooseTypeMatch();
        }
    }

    public void NickName(string value)
    {
        if (value != " " && value != "")
        {
            PhotonNetwork.NickName = value;
            enableNick = true;
            //okButtonCtrl.Enable();
            okButton.interactable = true;
            if (checkEnterCoroutine == null)
            {
                //print("StartCoroutine");
                checkEnterCoroutine = StartCoroutine(checkEnter());
            }
        }
        else if (value == " " || value == "")
        {
            inputField.text = "";
            inputField.caretPosition = 0;
            enableNick = false;
            //okButtonCtrl.Disable();
            okButton.interactable = false;
            if (checkEnterCoroutine != null)
            {
                StopCoroutine(checkEnterCoroutine);
            }
            checkEnterCoroutine = null;
        }
    }
    void createRoomFailed()
    {
        searchingMenu.SetActive(false);
        searchFailedMenu.SetActive(true);
        Invoke(nameof(showMenu),3);
    }
    void showMenu()
    {
        menu.Back();
    }
    IEnumerator checkEnter()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                //print("Enter");
                endEnterNickName();
                break;
            }
            yield return null;
        }
    }
    public void NickName_End(string value)
    {
        if (enableNick)
        {
            endEnterNickName();
            
            //playOnline.SetActive(true);
        }
    }
    public void Ok_Click()
    {
        if (enableNick)
        {
            //playOnline.SetActive(true);
            endEnterNickName();
        }
    }
    void endEnterNickName()
    {
        PlayerPrefs.SetString("nickName", PhotonNetwork.NickName);
        showChooseTypeMatch();
        //startFindRandomMatch();
    }
    void showChooseTypeMatch()
    {
        chooseTypeMatchMenu.SetActive(true);
        nickNameMenu.SetActive(false);
        searchingMenu.SetActive(false);
        searchFailedMenu.SetActive(false);
    }
    void startFindRandomMatch()
    {
        searchingMenu.SetActive(true);
        nickNameMenu.SetActive(false);
        //findRandomMatch.find();
    }
    public void back_Click()
    {

    }
    private void OnDisable()
    {
        if (checkEnterCoroutine != null)
        {
            //print("StopCoroutine");
            StopCoroutine(checkEnterCoroutine);
            checkEnterCoroutine = null;
        }
        hidePlayOnlineMenu();
    }
    private void OnEnable()
    {
        if (inputField.text !="" || checkString(inputField.text))
        {
            //print("OnEnable StartCoroutine");
            checkEnterCoroutine = StartCoroutine(checkEnter());
        }
        showInitStateOfPlayOnlineMenu();
    }
    void showInitStateOfPlayOnlineMenu()
    {
        checkOkButton();
        gameObject.SetActive(true);
        nickNameMenu.SetActive(true);
        searchingMenu.SetActive(false);
        searchFailedMenu.SetActive(false);
        chooseTypeMatchMenu.SetActive(false);
    }
    void hidePlayOnlineMenu()
    {
        checkOkButton();
        gameObject.SetActive(false);
        nickNameMenu.SetActive(false);
        searchingMenu.SetActive(false);
        searchFailedMenu.SetActive(false);
        chooseTypeMatchMenu.SetActive(false);
    }
    void checkOkButton()
    {
        okButton.interactable = inputField.text != "" || checkString(inputField.text);
    }
    bool checkString(string text)
    {
        foreach (var character in text)
        {
            if(character!=' ')
            {
                return true;
            }
        }
        return false;
    }
}
