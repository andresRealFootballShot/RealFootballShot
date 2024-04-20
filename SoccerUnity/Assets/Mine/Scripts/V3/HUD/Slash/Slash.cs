using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Slash : MonoBehaviour
{
    public Image image;
    float sizeMax;
    void Awake()
    {
        sizeMax = image.rectTransform.sizeDelta.x;
        image.rectTransform.sizeDelta = new Vector2(0, image.rectTransform.sizeDelta.y);
    }
    public void ValueChange(float value)
    {
        Vector2 vector2;
        if(value>=sizeMax)
            vector2= new Vector2( sizeMax, image.rectTransform.sizeDelta.y);
        else
            vector2 = new Vector2((value)*sizeMax, image.rectTransform.sizeDelta.y);

        image.rectTransform.sizeDelta = vector2;
    }
}
