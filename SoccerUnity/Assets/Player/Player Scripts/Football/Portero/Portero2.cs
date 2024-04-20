using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portero2 : MonoBehaviour
{
    public Transform transPorteria;
    public Transform transPortero;
    public Transform up;
    public Transform right;
    public Transform left;
    public Transform centerGoal;
    public ComponentsPorteria componentsPorteria;
    float limUp, lim, Down, limRight, limLeft, limDown;
    public float speed = 1f, minSpeed, maxSpeed = 5;
    public float maxAngle;
    public float offsetLimitX = 1, offsetLimitY = 1;
    public float adjustVelocity = 1, adjustDistance = 1;
    public CtrlDistancePortero ctrlDistancePortero;
    public float maxVelocityFollowPortero = 100, maxDistancePosPortero;
    public AnimationCurve curveDistance;
    void Start()
    {
        Vector3 porteroSize = transPortero.localScale;

        limUp = porteroSize.y / 2;
        limRight = porteroSize.x / 2;
        limLeft = porteroSize.x / 2;
        limDown = porteroSize.y / 2;
    }

    void Update()
    {
        Transform transCenterPorteria = componentsPorteria.transCenterPorteria;
        Transform transBall = componentsPorteria.componentsBall.transCenterBall;
        Rigidbody rigBall = componentsPorteria.componentsBall.rigBall;
        Recta recta = new Recta(transBall.position, componentsPorteria.componentsBall.rigBall.velocity.normalized);

        //Vector3 pointPlano = Vector3.Lerp(transBall.position,transCenterPorteria.position,curveDistance.Evaluate(ctrlDistancePortero.GetDistanceNormalized()));
        Vector3 pointPlano = transCenterPorteria.position;
        Plano plano = new Plano(pointPlano, transPorteria.right, transPorteria.up);
        Vector3 position = plano.findInterseccion(recta);






        Vector3 crossLeft = Vector3.Cross(left.forward, position - left.TransformPoint(Vector3.left * offsetLimitX));
        Vector3 crossRight = Vector3.Cross(-right.forward, position - right.TransformPoint(Vector3.right * offsetLimitX));
        Vector3 crossUp = Vector3.Cross(up.forward, position - up.TransformPoint(Vector3.up * offsetLimitY));
        if (crossLeft.y > 0 && crossRight.y > 0 && crossUp.x > 0 && !containsInfinity(position) && ballIsForward())
        {
            if (transPorteria.eulerAngles.y > 90 && transPorteria.eulerAngles.y < 270)
            {
                position.x = Mathf.Clamp(position.x, left.position.x + limLeft, right.position.x - limRight);
            }
            else
            {
                position.x = Mathf.Clamp(position.x, right.position.x + limRight, left.position.x - limLeft);
            }
            position.y = Mathf.Clamp(position.y, limUp, up.position.y - limUp);
        }
        else
        {
            position = centerGoal.position;
        }
        Vector3 posPortero;
        float distance = ctrlDistancePortero.GetDistance() / maxDistancePosPortero;

        Vector3 position1 = Vector3.zero;
        if (transPorteria.eulerAngles.y > 90 && transPorteria.eulerAngles.y < 270)
        {
            position1.x = Mathf.Clamp(transBall.position.x, left.position.x + limLeft, right.position.x - limRight);
        }
        else
        {
            position1.x = Mathf.Clamp(transBall.position.x, right.position.x + limRight, left.position.x - limLeft);
        }
        position1.y = Mathf.Clamp(transBall.position.y, limUp, up.position.y - limUp);
        position1.z = transCenterPorteria.position.z;
        posPortero = Vector3.Lerp(position1, position, rigBall.velocity.magnitude / maxVelocityFollowPortero + (1 - distance));
        
        transPortero.position = Vector3.Lerp(transPortero.position, posPortero, Time.deltaTime * speed);


    }
    bool ballIsForward()
    {
        Vector3 centerPorteria = componentsPorteria.transCenterPorteria.position;
        Vector3 ballPos = componentsPorteria.componentsBall.transCenterBall.position;
        float angle = Vector3.Angle(-componentsPorteria.transPorteria.forward, ballPos - new Vector3(centerPorteria.x, ballPos.y, centerPorteria.z));
        return angle <= 90;
    }
    bool containsInfinity(Vector3 pos)
    {
        return pos.x == Mathf.Infinity || pos.y == Mathf.Infinity || pos.z == Mathf.Infinity;

    }
}
