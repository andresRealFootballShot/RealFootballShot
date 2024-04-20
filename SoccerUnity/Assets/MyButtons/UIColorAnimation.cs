using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIColorAnimation : MyUIAnimation
{
    public AnimationCurve curve;
    public ColorVar hueColorVar;
    public float saturationTarget,valueTarget,alphaTarget;
    public bool setHue, setSaturation, setValue,setAlpha;
    public float duration;
    public override void Play()
    {
        if (this!=null && gameObject.activeInHierarchy && !maskableGraphics.Contains(MaskableGraphic) && enable)
        {
            StartCoroutine(changeColor(duration, MaskableGraphic));
        }
        
    }

    IEnumerator changeColor(float duration, MaskableGraphic maskableGraphic)
    {
        float time = 0;
        Color initColor = maskableGraphic.color;
        float h, s, v, h2;
        Color.RGBToHSV(hueColorVar.Value, out h2, out s, out v);
        Color.RGBToHSV(maskableGraphic.color, out h, out s, out v);
        maskableGraphics.Add(maskableGraphic);
        if (setHue)
        {
            h = h2;
        }
        if (setSaturation)
        {
            s = saturationTarget;
        }
        if (setValue)
        {
            v = valueTarget;
        }
        Color newColor = Color.HSVToRGB(h, s, v);
        if (setAlpha)
        {
            newColor.a = alphaTarget;
        }
        else
        {
            newColor.a = maskableGraphic.color.a;
        }
        while (time <= duration)
        {
            
            float t = curve.Evaluate(time / duration);
            maskableGraphic.color = Color.Lerp(initColor,newColor,t);
            time += Time.deltaTime;
            yield return null;
        }
        maskableGraphic.color = newColor;
        maskableGraphics.Remove(maskableGraphic);
    }
}
