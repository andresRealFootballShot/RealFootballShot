using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickValues : MonoBehaviour
{
    public AnimationCurve _volumenAdjust;
    public static AnimationCurve volumenAdjust;
    public static float maxForceVolume = 40;
    private void Start()
    {
        volumenAdjust = _volumenAdjust;
    }
}
