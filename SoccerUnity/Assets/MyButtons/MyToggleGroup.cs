using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyToggleGroup : MonoBehaviour
{
    public GameObject parent;
    List<ToggleCtrl> toogleList;
    private void Awake()
    {
        toogleList = MyFunctions.GetComponentsInChilds<ToggleCtrl>(parent,false, true);
        foreach(ToggleCtrl item in toogleList)
        {
            item.selectUnityEvent.AddListener(delegate { ToggleWasSelected(item); });
        }
    }
    public void ToggleWasSelected(ToggleCtrl selelectedToggle)
    {
        foreach(ToggleCtrl toggle in toogleList)
        {
            if (toggle != selelectedToggle)
            {
                toggle.Deselect();
            }
        }
    }
}
