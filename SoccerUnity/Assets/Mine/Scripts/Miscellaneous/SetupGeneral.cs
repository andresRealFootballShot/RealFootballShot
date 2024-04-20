using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupGeneral : MonoBehaviour
{
    ComponentsPlayer componentsPlayer;
    public void StartProcess()
    {
        componentsPlayer = FindObjectOfType<ComponentsPlayer>();
        volume();

    }
    void volume()
    {
        if (!PlayerPrefs.HasKey("scaleVolume"))
        {
            PlayerPrefs.SetFloat("scaleVolume", 10);
        }
        if (!PlayerPrefs.HasKey("generalVolume"))
        {
            PlayerPrefs.SetFloat("generalVolume", 0.5f);
        }
        if (!PlayerPrefs.HasKey("nickName"))
        {
            PlayerPrefs.SetString("nickName", "");
        }
        AudioListener.volume = PlayerPrefs.GetFloat("generalVolume") * PlayerPrefs.GetFloat("scaleVolume");
        if (componentsPlayer != null)
        {
            //componentsPlayer.scriptsPlayer.menuOptions.sliderVolume.value = PlayerPrefs.GetFloat("generalVolume");
        }
    }
}
