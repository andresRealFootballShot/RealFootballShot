using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyUIAnimation : MonoBehaviour
{
    private MaskableGraphic maskableGraphic;
    public bool enable = true;
    public MaskableGraphic MaskableGraphic { get => maskableGraphic; set => maskableGraphic = value; }
    protected List<MaskableGraphic> maskableGraphics = new List<MaskableGraphic>();
    public virtual void Play()
    {

    }
    public virtual void setMaskableGraphic(MaskableGraphic maskableGraphic)
    {
        MaskableGraphic = maskableGraphic;
    }
}
