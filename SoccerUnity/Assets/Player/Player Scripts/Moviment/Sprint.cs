using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Sprint : MonoBehaviour
{
    public bool state;
    public float sprint;
    public ComponentsKeys keys;
    public PlayerInput playerInput;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        state = !playerInput.actions["Sprint"].IsPressed();
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
