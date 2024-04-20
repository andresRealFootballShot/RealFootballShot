using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HudCtrl : MonoBehaviour
{
    public Canvas canvas;
    public ImageCtrl gunSight,pointKick;
    public void HideHUD()
    {
        canvas.enabled = false;
    }
    public void ShowHUD()
    {
        canvas.enabled = true;
    }
    public void HideGunSight()
    {
        gunSight.hideImage();
    }
    public void ShowGunSight()
    {
        gunSight.showImage();
    }
    public void HidePointKick()
    {
        pointKick.hideImage();
    }
    public void ShowPointKick()
    {
        pointKick.showImage();
    }
}
