using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PruebasTimeScale : MonoBehaviour
{
    static float currentScale;
    static bool computer;
    static bool nextFrame;
    void Start()
    {
        currentScale = 1;
        Time.timeScale = 1f;
#if UNITY_EDITOR
        SceneView.duringSceneGui += view =>
        {
            var e =Event.current;
            if (e != null && e.keyCode != KeyCode.None && e.type == EventType.KeyDown)
            {
                setScale(e.keyCode);
            }
        };
#endif
    }
    // Update is called once per frame
    void Update()
    {
        check();
        
    }
    void check()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            currentScale = 1;
            computer = false;
            Time.timeScale = 1f;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            currentScale = 0.5f;
            computer = false;
            Time.timeScale = 0.5f;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            currentScale = 0.25f;
            computer = false;
            Time.timeScale = 0.25f;
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            currentScale = 0.1f;
            computer = false;
            Time.timeScale = 0.1f;
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            currentScale = 0.05f;
            computer = false;
            Time.timeScale = 0.05f;
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            computer = !computer;
            if (computer)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = currentScale;
            }
        }
        if (nextFrame)
        {
            Time.timeScale = 0;
            nextFrame = false;
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Time.timeScale = 1;
            nextFrame = true;

        }
    }
    public static void setScale(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.P:
                currentScale = 1;
                computer = false;
                Time.timeScale = 1f;
                break;
            case KeyCode.T:
                currentScale = 0.5f;
                computer = false;
                Time.timeScale = 0.5f;
                break;
            case KeyCode.Y:
                currentScale = 0.25f;
                computer = false;
                Time.timeScale = 0.25f;
                break;
            case KeyCode.U:
                currentScale = 0.1f;
                computer = false;
                Time.timeScale = 0.1f;
                break;
            case KeyCode.O:
                currentScale = 0.05f;
                computer = false;
                Time.timeScale = 0.05f;
                break;
            case KeyCode.B:
                computer = !computer;
                if (computer)
                {
                    Time.timeScale = 0f;
                }
                else
                {
                    Time.timeScale = currentScale;
                }
                break;
            case KeyCode.L:
                Time.timeScale = 1;
                nextFrame = true;
                break;
        }
    }
}
