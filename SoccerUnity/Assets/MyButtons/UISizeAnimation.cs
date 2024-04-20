using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISizeAnimation : MyUIAnimation
{
    public AnimationCurve curve;
    public float duration=0.1f,scale=1;
    public Vector3 offsetSize;
    Vector3 targetSize;
    public override void Play()
    {
        if (this != null && gameObject.activeInHierarchy && enable && !maskableGraphics.Contains(MaskableGraphic))
        {
            StartCoroutine(changeSize(duration, MaskableGraphic));
        }
    }
    public override void setMaskableGraphic(MaskableGraphic maskableGraphic)
    {
        MaskableGraphic = maskableGraphic;
        targetSize = Vector3.Scale(maskableGraphic.rectTransform.localScale,offsetSize);
    }
    IEnumerator changeSize(float duration, MaskableGraphic maskableGraphic)
    {
        float time = 0;
        float animationValue;
        Vector3 initScale =  maskableGraphic.rectTransform.localScale;
        maskableGraphics.Add(maskableGraphic);
        while (time <= duration)
        {
            time += Time.deltaTime;
            animationValue = curve.Evaluate(time / duration);
            //maskableGraphic.rectTransform.localScale = initScale + initScale.normalized* animationValue* scale;
            maskableGraphic.rectTransform.localScale = Vector3.Lerp(initScale,targetSize,animationValue);
            yield return null;
        }
        animationValue = curve.Evaluate(1);
        //maskableGraphic.rectTransform.localScale = initScale + initScale * animationValue * scale;
        maskableGraphic.rectTransform.localScale = Vector3.Lerp(initScale, targetSize, animationValue);
        maskableGraphics.Remove(maskableGraphic);
    }
}
