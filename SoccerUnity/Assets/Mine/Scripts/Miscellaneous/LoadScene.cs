using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class LoadScene
{
    public static void loadPlaytimeScene()
    {
        SceneSetup.setStaticSetup(TypeMatchID.Playtime, SceneModeID.Offline, false, false);
        //PlaytimeCtrl.setupTypeMatch();
        notifyClearBeforeLoadScene();
        SceneManager.LoadScene("Scenes/" + TypeMatch.getLevel());
    }
    public static void notifyClearBeforeLoadScene()
    {
        var list = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().OfType<IClearBeforeLoadScene>();
        foreach (IClearBeforeLoadScene item in list)
        {
            item.Clear();
        }
    }
}
