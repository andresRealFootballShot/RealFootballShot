using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public virtual void rotation()
    { 
    }
    public void EnableScript()
    {
        enabled = true;
    }
    public void DisableScript()
    {
        enabled = false;
    }
}
