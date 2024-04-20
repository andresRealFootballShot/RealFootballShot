using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIValueColorAnimation : MyUIAnimation
{
    public AnimationCurve curve;
    public float targetValue;
    public float duration;
    public override void Play()
    {
        if (!maskableGraphics.Contains(MaskableGraphic))
        {
            StartCoroutine(changeColor(MaskableGraphic));
        }
    }

    IEnumerator changeColor(MaskableGraphic maskableGraphic)
    {
        float time = 0;
        float h, s, v;
        Color.RGBToHSV(maskableGraphic.color, out h, out s, out v);
        float initValue = v;
        maskableGraphics.Add(maskableGraphic);
        while (time <= duration)
        {
            time += Time.deltaTime;
            float t = curve.Evaluate(time / duration);
            float currentValue = Mathf.Lerp(initValue, targetValue, t);
            Color.RGBToHSV(maskableGraphic.color, out h, out s, out v);
            maskableGraphic.color = Color.HSVToRGB(h, s, currentValue);
            yield return null;
        }
        Color.RGBToHSV(maskableGraphic.color, out h, out s, out v);
        maskableGraphic.color = Color.HSVToRGB(h, s, targetValue);
        maskableGraphics.Remove(maskableGraphic);
    }
}
