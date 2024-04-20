using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISaturationColorAnimation : MyUIAnimation
{
    public float saturationTarget, duration;
    public override void Play()
    {
        StartCoroutine(changeColor(saturationTarget,duration,MaskableGraphic));
    }

    protected IEnumerator changeColor(float saturationTarget, float duration,MaskableGraphic maskableGraphic)
    {
        float time = 0;
        float h, s, v;
        Color.RGBToHSV(maskableGraphic.color, out h, out s, out v);
        float initSaturation = s;
        float currentSaturation;
        maskableGraphics.Add(maskableGraphic);
        while (time <= duration)
        {
            time += Time.deltaTime;
            currentSaturation = Mathf.Lerp(initSaturation, saturationTarget, time / duration);
            Color.RGBToHSV(maskableGraphic.color, out h, out s, out v);
            maskableGraphic.color = Color.HSVToRGB(h, currentSaturation, v);
            yield return null;
        }
        Color.RGBToHSV(maskableGraphic.color, out h, out s, out v);
        maskableGraphic.color = Color.HSVToRGB(h, saturationTarget, v);
        maskableGraphics.Remove(maskableGraphic);
    }
}
