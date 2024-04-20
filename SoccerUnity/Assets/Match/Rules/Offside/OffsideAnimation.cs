using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsideAnimation : MonoBehaviour
{
    Canvas canvas;
    void Awake()
    {
        gameObject.SetActive(true);
        if ((canvas = GetComponent<Canvas>()) != null)
        {
            canvas.enabled = false;
        }
        MatchEvents.endPart.AddListener(disableCanvas);
    }

    public void Play()
    {
        if (canvas != null)
        {
            canvas.enabled = true;
            Invoke(nameof(disableCanvas), 2);
        }
    }
    void disableCanvas()
    {
        canvas.enabled = false;
    }
}
