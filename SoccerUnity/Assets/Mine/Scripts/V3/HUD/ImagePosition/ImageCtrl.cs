using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ImageCtrl : MonoBehaviour
{
    public Image image;
    public RectTransform rectTransform;
    public Canvas canvas;
    void Start()
    {
        
    }

    // Update is called once per frame
    public void ValueChanged(Vector2 value)
    {
        rectTransform.anchoredPosition = new Vector2(value.x / canvas.scaleFactor, value.y / canvas.scaleFactor);
        //rectTransform.anchoredPosition = new Vector2(value.x, value.y);
    }
    public void hideImage()
    {
        image.enabled = false;
    }
    public void showImage()
    {
        image.enabled = true;
    }
}
