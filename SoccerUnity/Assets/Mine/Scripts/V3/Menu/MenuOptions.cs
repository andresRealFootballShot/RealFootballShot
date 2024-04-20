using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuOptions : MonoBehaviour
{
    
    public GameObject volumeMenu,mouseMenu,controlsMenu;
    public Slider sliderVolume, sliderMousePlayerX, sliderMousePlayerY, sliderMouseAimX, sliderMouseAimY;
    public Text mousePlayerX, mousePlayerY, mouseAimX, mouseAimY;
    public event emptyDelegate backEvent;
    float scaleVolume = 10;
    float minMousePlayerX=25, maxMousePlayerX=200;
    float minMousePlayerY = 25, maxMousePlayerY=200;
    float minMouseAimX = 20, maxMouseAimX = 400;
    float minMouseAimY = 20, maxMouseAimY = 400;
    private void Awake()
    {
        sliderVolume.value = PlayerPrefs.GetFloat("generalVolume");
        initialValues();
    }
    void initialValues()
    {
        if (PlayerPrefs.HasKey("mousePlayerSensX"))
        {
            
            sliderMousePlayerX.value = PlayerPrefs.GetFloat("mousePlayerSensX");
            mousePlayerX.text = (sliderMousePlayerX.value*100).ToString("f1") + "%";
            ComponentsPlayer.currentComponentsPlayer.scriptsPlayer.cameraRotation.controllerSpeed.speedMouseX = Mathf.Lerp(minMousePlayerX, maxMousePlayerX, sliderMousePlayerX.value);
        }
        else
        {
            float value = (ComponentsPlayer.currentComponentsPlayer.scriptsPlayer.cameraRotation.controllerSpeed.speedMouseX - minMousePlayerX) /maxMousePlayerX;
            PlayerPrefs.SetFloat("mousePlayerSensX", value);
            mousePlayerX.text = value*100 + "%";
        }
        if (PlayerPrefs.HasKey("mousePlayerSensY"))
        {
            sliderMousePlayerY.value = PlayerPrefs.GetFloat("mousePlayerSensY");
            mousePlayerY.text = (sliderMousePlayerY.value * 100).ToString("f1") + "%";
            ComponentsPlayer.currentComponentsPlayer.scriptsPlayer.cameraRotation.controllerSpeed.speedMouseY = Mathf.Lerp(minMousePlayerY, maxMousePlayerY, sliderMousePlayerY.value);
        }
        else
        {
            float value = (ComponentsPlayer.currentComponentsPlayer.scriptsPlayer.cameraRotation.controllerSpeed.speedMouseY - minMousePlayerY) / maxMousePlayerY;
            PlayerPrefs.SetFloat("mousePlayerSensY", value);
            mousePlayerX.text = value * 100 + "%";
        }

        if (PlayerPrefs.HasKey("mouseAimSensX"))
        {
            sliderMouseAimX.value = PlayerPrefs.GetFloat("mouseAimSensX");
            mouseAimX.text = (sliderMouseAimX.value * 100).ToString("f1") + "%";
            ComponentsPlayer.currentComponentsPlayer.scriptsPlayer.controllerZoneAim.controllerSpeedMouse.speedMouseX = Mathf.Lerp(minMouseAimX, maxMouseAimX, sliderMouseAimX.value);
        }
        else
        {
            float value = (ComponentsPlayer.currentComponentsPlayer.scriptsPlayer.controllerZoneAim.controllerSpeedMouse.speedMouseX - minMouseAimX) / maxMouseAimX;
            PlayerPrefs.SetFloat("mouseAimSensX", value);
            mousePlayerX.text = value * 100 + "%";
        }
        if (PlayerPrefs.HasKey("mouseAimSensY"))
        {
            sliderMouseAimY.value = PlayerPrefs.GetFloat("mouseAimSensY");
            mouseAimY.text = (sliderMouseAimY.value * 100).ToString("f1") + "%";
            ComponentsPlayer.currentComponentsPlayer.scriptsPlayer.controllerZoneAim.controllerSpeedMouse.speedMouseY = Mathf.Lerp(minMouseAimY, maxMouseAimY, sliderMouseAimX.value);
        }
        else
        {
            float value = (ComponentsPlayer.currentComponentsPlayer.scriptsPlayer.controllerZoneAim.controllerSpeedMouse.speedMouseY - minMouseAimY) / maxMouseAimY;
            PlayerPrefs.SetFloat("mouseAimSensY", value);
            mousePlayerY.text = value * 100 + "%";
        }
    }
    public void VolumeSiler_Change(float value)
    {
        PlayerPrefs.SetFloat("generalVolume", value);
        //float scaleVolume = PlayerPrefs.GetFloat("scaleVolume");
        AudioListener.volume = value * scaleVolume;
    }
    public void MousePlayerXSlider_Change(float value)
    {
        PlayerPrefs.SetFloat("mousePlayerSensX", value);
        mousePlayerX.text = (value * 100).ToString("f1");
        ComponentsPlayer.currentComponentsPlayer.scriptsPlayer.cameraRotation.controllerSpeed.speedMouseX = Mathf.Lerp(25, 200, value);
    }
    public void MousePlayerYSlider_Change(float value)
    {
        PlayerPrefs.SetFloat("mousePlayerSensY", value);
        mousePlayerY.text = (value * 100).ToString("f1");
        ComponentsPlayer.currentComponentsPlayer.scriptsPlayer.cameraRotation.controllerSpeed.speedMouseY = Mathf.Lerp(25, 200, value);
    }
    public void MouseAimXSlider_Change(float value)
    {
        PlayerPrefs.SetFloat("mouseAimSensX", value);
        mouseAimX.text = (value * 100).ToString("f1");
        ComponentsPlayer.currentComponentsPlayer.scriptsPlayer.controllerZoneAim.controllerSpeedMouse.speedMouseX = Mathf.Lerp(20, 400, value);
    }
    public void MouseAimYSlider_Change(float value)
    {
        PlayerPrefs.SetFloat("mouseAimSensY", value);
        mouseAimY.text = (value * 100).ToString("f1");
        ComponentsPlayer.currentComponentsPlayer.scriptsPlayer.controllerZoneAim.controllerSpeedMouse.speedMouseX = Mathf.Lerp(20, 400, value);
    }
    public void OptionsButton_Click(string buttonClicked)
    {
        volumeMenu.SetActive(buttonClicked.Equals("volume"));
        mouseMenu.SetActive(buttonClicked.Equals("mouse"));
        controlsMenu.SetActive(buttonClicked.Equals("controls"));
        if (buttonClicked.Equals("back"))
        {
            //menu.showMenu();
            gameObject.SetActive(false);
            backEvent?.Invoke();
        }
    }
    private void OnEnable()
    {
        volumeMenu.SetActive(false);
        mouseMenu.SetActive(false);
        controlsMenu.SetActive(false);
    }
    public void Show()
    {
        
    }
}
