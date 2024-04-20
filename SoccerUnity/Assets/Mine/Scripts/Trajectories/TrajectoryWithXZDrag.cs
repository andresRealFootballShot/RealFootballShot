using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryWithXZDrag
{
    //Apuntes: la resistencia de arrastre solo se aplica en los ejes XZ (mediante el script GravityAirDragForce) debido a que la ecuacion del tiempo es muy compleja (W de lambert) y no la he encontrado y tenemos que saber el tiempo que tarda en llegar a cierta altura
    //Utilizamos fuerza de rozamiento proporcional a la velocidad (Unity utiliza esta en el rigidbody)
    //También hay fuerza de rozamiento proporcional al cuadrado de la velocidad pero es mucho mas compleja y no he encontrado ecuaciones del espacio ni del tiempo
    private Vector3 initialVelocity, initialPosition;
    Vector2 vxz0;
    float g;
    public float impactTime;
    public Vector3 impactPoint;
    float drag;
    public TrajectoryWithXZDrag(float _g, Vector3 _initialVelocity, Vector3 _initialPosition, float _drag)
    {
        g = _g;
        initialVelocity = _initialVelocity;
        vxz0 = new Vector2(initialVelocity.x, initialVelocity.z);
        initialPosition = _initialPosition;
        drag = _drag;
    }
    public float getImpactTime()
    {
        float impactTime = 2 * initialVelocity.y / g;
        return impactTime;
    }
    public Vector3 getVelocity(float t)
    {
        Vector2 vx0 = new Vector2(initialVelocity.x, initialVelocity.z);
        float vy0 = initialVelocity.y;
        Vector2 vx = vx0 * Mathf.Exp(-drag * t);
        float vy = vy0 - g * t;
        Vector3 endVelocity = new Vector3(vx.x, vy, vx.y);
        return endVelocity;
    }
    public Vector3 getPositionAtTime(float t)
    {
        Vector2 vx0 = new Vector2(initialVelocity.x, initialVelocity.z);
        float vy0 = initialVelocity.y;
        float ekt = Mathf.Exp(-drag * t);
        Vector2 x = (vx0 / drag) * (1 - ekt);
        float y = (-g / 2) * (t * t) + vy0 * t;
        Vector3 endPosition = new Vector3(x.x, y, x.y) + initialPosition;
        return endPosition;
    }
    public bool timeToReachHeight(float height, out float solution, bool getSolution1)
    {
        float a, b, c;
        a = -g / 2;
        b = initialVelocity.y;
        c = initialPosition.y - height;
        float solution1, solution2;
        bool result = MyFunctions.SolveQuadratic(a, b, c, out solution1, out solution2);
        if (getSolution1)
        {
            solution = solution1;
        }
        else
        {
            solution = solution2;
        }
        //Debug.Log("timeToReachHeight | solution1=" + solution1 + " | " + solution2);
        return result;
    }
    public bool timeToReachPositionXZ(Vector3 position, out float solution)
    {
        float k = drag;
        Vector2 positionXZ = new Vector2(position.x, position.z);
        float x = positionXZ.magnitude;
        float vx0 = vxz0.magnitude;
        solution = Mathf.Log(1-((x*k)/ vx0))/-k;
        
        //Debug.Log("timeToReachHeight | solution1=" + solution1 + " | " + solution2);
        return true;
    }
    public float getMaximumHeight()
    {
        return initialPosition.y + ((initialVelocity.y* initialVelocity.y)/(2*g));
    }
}
