using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parabola
{
    private Vector3 m_gravity, m_initialVelocity, m_initialPosition;

    public float impactTime;
    public Vector3 impactPoint;

    public Parabola(Vector3 gravity, Vector3 initialVelocity, Vector3 initialPosition)
    {
        m_gravity = gravity;
        m_initialVelocity = initialVelocity;
        m_initialPosition = initialPosition;

        // To find impact point:
        // gravity.y/2 * t^2 + velocity.y*t + position.y = 0
        // (-b +/- sqrt(b^2 - 4ac))/2a
        /*float a = gravity.y / 2;
        
        float b = initialVelocity.magnitude * Mathf.Sin(angle*Mathf.Deg2Rad);
        float c = initialPosition.y;
        float solution1 = (-b + Mathf.Sqrt(b * b - 4 * a * c))/ 2 * a;
        float solution2 = (-b - Mathf.Sqrt(b * b - 4 * a * c))/ 2 * a;
        if (solution1 > solution2) impactTime = solution1;
        else impactTime = solution2;*/
        float angle = Vector3.Angle(initialVelocity - Vector3.up * initialVelocity.y, initialVelocity);
        impactTime = 2 * initialVelocity.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad) / 9.8f;
        //Debug.Log("t=" + impactTime);
        impactPoint = GetPositionAtTime(impactTime);
    }

    public Vector3 GetPositionAtTime(float time)
    {
        Vector3 position = new Vector3();
        position.x = m_initialVelocity.x * time + m_initialPosition.x;
        position.y = (m_gravity.y / 2) * (time * time) + m_initialVelocity.y * time + m_initialPosition.y;
        position.z = m_initialVelocity.z * time + m_initialPosition.z;
        return position;
    }
    public bool timeToReachHeight(float height)
    {
        float a, b, c;
        a = m_gravity.y / 2;
        b = m_initialVelocity.y;
        c = m_initialPosition.y - height;
        float solution1, solution2;
        bool result = MyFunctions.SolveQuadratic(a, b, c, out solution1, out solution2);
        //Debug.Log("timeToReachHeight | solution1=" + solution1 + " | " + solution2);
        return result;
    }
}

