using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionYCtrl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < 0.5f)
        {
            transform.position = MyFunctions.setYToVector3(transform.position, 0.1f);
        }
    }
}
