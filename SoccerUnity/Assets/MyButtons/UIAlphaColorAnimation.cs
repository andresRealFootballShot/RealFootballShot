using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAlphaColorAnimation : MyUIAnimation
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
        float initAlpha = maskableGraphic.color.a;
        maskableGraphics.Add(maskableGraphic);
        while (time <= duration)
        {
            time += Time.deltaTime;
            float t = curve.Evaluate(time / duration);
            float currentAlpha = Mathf.Lerp(initAlpha, targetValue, t);
            Color color = maskableGraphic.color;
            color.a = currentAlpha;
            maskableGraphic.color = color;
            yield return null;
        }
        Color color2 = maskableGraphic.color;
        color2.a = targetValue;
        maskableGraphic.color = color2;
        maskableGraphics.Remove(maskableGraphic);
    }
}
