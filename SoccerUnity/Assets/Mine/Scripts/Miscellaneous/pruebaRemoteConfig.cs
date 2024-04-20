using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.RemoteConfig;
using UnityEngine.UI;
public class pruebaRemoteConfig : MonoBehaviour
{
    public Text text;
    public struct userAttributes
    {
        // Optionally declare variables for any custom user attributes; if none keep an empty struct:
        public bool expansionFlag;
    }

    public struct appAttributes
    {
        // Optionally declare variables for any custom app attributes; if none keep an empty struct:
        public int level;
        public int score;
        public string appVersion;
    }
    void Awake()
    {
        ConfigManager.FetchCompleted += ApplyRemoteSettings;
    }
    private void OnEnable()
    {
        // Fetch configuration setting from the remote service: 
        ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
    }
    void ApplyRemoteSettings(ConfigResponse configResponse)
    {
        switch (configResponse.requestOrigin)
        {
            case ConfigOrigin.Default:
                Debug.Log("No settings loaded this session; using default values.");
                break;
            case ConfigOrigin.Cached:
                Debug.Log("No settings loaded this session; using cached values from a previous session.");
                break;
            case ConfigOrigin.Remote:
                Debug.Log("New settings loaded this session; update values accordingly.");
                text.text = ConfigManager.appConfig.GetString("prueba");
                break;
        }
    }

}
