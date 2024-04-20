using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHueColorAnimation : MyUIAnimation
{
    public ColorVar colorVar;
    public float duration;
    public override void Play()
    {
        StartCoroutine(changeColor(duration, MaskableGraphic));
    }

    IEnumerator changeColor(float duration, MaskableGraphic maskableGraphic)
    {
        float time = 0;
        float h, s, v;
        Color.RGBToHSV(colorVar.Value, out h, out s, out v);
        float hueTarget = h;
        Color.RGBToHSV(maskableGraphic.color, out h, out s, out v);
        float initHue = h;
        float currentHue;
        maskableGraphics.Add(maskableGraphic);
        while (time <= duration)
        {
            time += Time.deltaTime;
            currentHue = Mathf.Lerp(initHue, hueTarget, time / duration);
            Color.RGBToHSV(maskableGraphic.color, out h, out s, out v);
            maskableGraphic.color = Color.HSVToRGB(currentHue,s , v);
            yield return null;
        }
        Color.RGBToHSV(maskableGraphic.color, out h, out s, out v);
        maskableGraphic.color = Color.HSVToRGB(hueTarget, s, v);
        maskableGraphics.Remove(maskableGraphic);
    }
}
