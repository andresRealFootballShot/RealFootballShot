using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sprint : MonoBehaviour
{
    public bool state;
    public float sprint;
    public ComponentsKeys keys;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(ComponentsKeys.keySprint) || Input.GetKeyDown(ComponentsKeys.joySprint))
        {
            state = !state;
        }
        if (state)
        {
            sprint = Mathf.Lerp(sprint, 1, Time.deltaTime * 5);
        }
        else
        {
            sprint = Mathf.Lerp(sprint, 0, Time.deltaTime * 5);
        }
    }
}
