using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetParent : MonoBehaviour
{
    public Transform parent;
    public List<Transform> childs;
    public void Set()
    {
        foreach (var item in childs)
        {
            if (item != null)
            {
                item.parent = parent;
            }
        }
    }
}
