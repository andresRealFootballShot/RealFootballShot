using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHandColliders : MonoBehaviour
{
    public Transform parentColliders;
    public Transform rootArmature;
    void Start()
    {
        if(parentColliders!=null && rootArmature!=null && parentColliders.gameObject.activeInHierarchy)
            TraverseHierarchy(parentColliders);
    }
    public void StartProcess(Transform rootArmature)
    {
        return;
        this.rootArmature = rootArmature;
        if (parentColliders != null && rootArmature != null && parentColliders.gameObject.activeInHierarchy)
            TraverseHierarchy(parentColliders);
    }
    void TraverseHierarchy(Transform root)
    {
         List<Transform> list = getChilds(root);
        for(int i = 0; i < list.Count; i++)
        {
            Transform parent = SearchWithName(rootArmature, list[i].name);
            
            list[i].parent = parent;

        }
    }
    List<Transform> getChilds(Transform root)
    {
        if (root != null)
        {
            List<Transform> list = new List<Transform>();
            
            foreach (Transform child in root)
            {
                
                list.Add(child);
                list.AddRange(getChilds(child));
            }
            return list;
        }
        else
        {
            return new List<Transform>();
        }
       
    }
    Transform SearchWithName(Transform root,string name)
    {
        foreach (Transform child in root)
        {
            if (child.name.Equals(name))
            {
                return child;
            }
            Transform t = SearchWithName(child, name);
            if (t != null)
            {
                return t;
            }
        }
        return null;
    }
}
