using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingScreen : MonoBehaviour
{
    static bool isShown;
    public new Animation animation;
    public Canvas canvas;
    public void StartProcess()
    {
        if (!isShown && SceneSetup.showStartingScreen)
        {
            canvas.enabled = true;
            animation.Play();
            isShown = true;
        }
        else{
            canvas.enabled = false;
            MatchEvents.endStartingScreen.Invoke();
        }
    }
    public void endAnimation()
    {
        MatchEvents.endStartingScreen.Invoke();
        canvas.enabled = false;
    }
}
