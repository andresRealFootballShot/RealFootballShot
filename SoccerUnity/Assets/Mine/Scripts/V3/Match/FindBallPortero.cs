using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindBallPortero : MonoBehaviour
{
    public string tagName;
    public Portero3 portero;
    public ComponentsPorteria componentsPorteria;
    void Start()
    {
        StartCoroutine(FindBallRoutine());
    }

    IEnumerator FindBallRoutine()
    {
        GameObject ball;
        while ((ball = GameObject.FindGameObjectWithTag(tagName)) == null)
            yield return null;
        componentsPorteria.componentsBall = ball.GetComponent<BallComponents>();
        portero.enabled=true;
    }
}
