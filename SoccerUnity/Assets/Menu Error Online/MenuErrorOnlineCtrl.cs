using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuErrorOnlineCtrl : MonoBehaviour
{
    public Canvas canvas;
    public TMP_Text Text;
    bool showMenu;
    float period;
    string text;
    string scene;
    void start(Scene scene, LoadSceneMode mode)
    {
        if (showMenu && SceneManager.GetActiveScene().name.Equals(this.scene))
        {
            ShowMenu();
            Invoke(nameof(HideMenu), period);
        }
        else
        {
            canvas.enabled = false;
        }
    }
    public void setup(string scene, float period, string text,bool showMenu)
    {
        this.scene = scene;
        this.period = period;
        this.text = text;
        this.showMenu = showMenu;
        canvas.enabled = false;
        SceneManager.sceneLoaded += start;
    }
    public void ShowMenu()
    {
        canvas.enabled = true;
        Text.text = text;
    }
    public void HideMenu()
    {
        canvas.enabled = false;
        Destroy(gameObject);
    }
}
