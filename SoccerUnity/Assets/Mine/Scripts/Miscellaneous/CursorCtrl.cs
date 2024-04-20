using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CursorCtrl : MonoBehaviour
{
    public static int openedMenus;
    public static void notifyShowMenu()
    {
        openedMenus++;
        Cursor.lockState = CursorLockMode.None;
        
        Cursor.visible = true;
    }
    public static void notifyHideMenu()
    {
        openedMenus--;
        openedMenus = Mathf.Clamp(openedMenus, 0, 1000);
        if (openedMenus == 0)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public static void clear()
    {
        openedMenus = 0;
    }
 
}
