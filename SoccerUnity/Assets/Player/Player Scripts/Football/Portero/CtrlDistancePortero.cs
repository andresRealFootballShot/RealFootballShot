using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlDistancePortero : MonoBehaviour
{
    public ComponentsPorteria componentsPorteria;
    public float maxDistance;
    public bool isClose()
    {
        Vector3 centerPorteria = componentsPorteria.transCenterPorteria.position;
        Vector3 ballPos = componentsPorteria.componentsBall.transCenterBall.position;
        float angle = Vector3.Angle(componentsPorteria.transPorteria.forward, ballPos - new Vector3(centerPorteria.x, ballPos.y, centerPorteria.z));
        if (Vector3.Distance(ballPos, centerPorteria) <= maxDistance && angle<=90)
            return true;
        else
            return false;
    }
    public float GetDistance()
    {
        Vector3 centerPorteria = componentsPorteria.transCenterPorteria.position;
        Vector3 ballPos = componentsPorteria.componentsBall.transCenterBall.position;
        float angle = Vector3.Angle(componentsPorteria.transPorteria.forward, ballPos - new Vector3(centerPorteria.x, ballPos.y, centerPorteria.z));
        if (angle <= 90)
            return 0;
        else
            return Vector3.Distance(ballPos, centerPorteria);
    }
    public float GetDistanceNormalized()
    {
        Vector3 centerPorteria = componentsPorteria.transCenterPorteria.position;
        Vector3 ballPos = componentsPorteria.componentsBall.transCenterBall.position;
        float angle = Vector3.Angle(componentsPorteria.transPorteria.forward, ballPos - new Vector3(centerPorteria.x, ballPos.y, centerPorteria.z));
        if (angle <= 90)
            return 0;
        else
            return Vector3.Distance(ballPos, centerPorteria) / maxDistance;
    }
}
