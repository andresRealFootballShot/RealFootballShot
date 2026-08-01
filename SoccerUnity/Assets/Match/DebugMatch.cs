using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMatch : MonoBehaviour
{
    public Transform ballPosition;
    public float timeScale = 0.5f;
    float previouTimeScale;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Time.timeScale = Time.timeScale== timeScale?1: timeScale;

        }
        if (Input.GetKeyDown(KeyCode.P))
        {
           float t = Time.timeScale;
           Time.timeScale = Time.timeScale != 0 ? 0 : previouTimeScale;
            previouTimeScale = t;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            MatchComponents.Brains.dontDefense = !MatchComponents.Brains.dontDefense;
            MatchComponents.ballComponents.position = ballPosition.position;
            MatchComponents.ballVelocity = Vector3.zero;
            MatchComponents.ballAngularVelocity = Vector3.zero;

        }
    }
}
