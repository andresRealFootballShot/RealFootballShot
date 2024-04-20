using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveConstraint : MonoBehaviour
{
    public List<GameObject> childs;
    private void OnEnable()
    {
        foreach (var item in childs)
        {
            if(item!=null)
            {
                item.SetActive(true);
            }
        }
    }
    private void OnDisable()
    {
        foreach (var item in childs)
        {
            if (item != null)
            {
                
                item.SetActive(false);
            }
        }
    }
}
