using DOTS_ChaserDataCalculation;
using System.Collections.Generic;
using UnityEngine;

public class BallTrajectorySimulator2 : MonoBehaviour
{
    public float simulationTime = 5f;
    public float timeStep = 0.05f;
    public float gravity = 9.81f;

    float airFriction => MatchComponents.ballComponents.rigBall.drag;
    float bounciness => MatchComponents.ballComponents.bounciness;
    float friction => MatchComponents.ballComponents.friction;
    float dynamicFriction => MatchComponents.ballComponents.dynamicFriction;
    float ballRadio => MatchComponents.ballComponents.radio;

    float groundLevel => MatchComponents.ballComponents.sphereCollider.radius *
                         MatchComponents.ballComponents.sphereCollider.transform.localScale.x;

    public Transform fieldMinTransform, fieldMaxTransform;
    Vector2 fieldMin => new Vector2(fieldMinTransform.position.x, fieldMinTransform.position.z);
    Vector2 fieldMax => new Vector2(fieldMaxTransform.position.x, fieldMaxTransform.position.z);

    public List<Vector3> positions { get; private set; } = new List<Vector3>();
    public List<float> times { get; private set; } = new List<float>();

    float vfMagnitude => gravity / Mathf.Max(airFriction, 0.0001f);

    public void Simulate()
    {
        positions.Clear();
        times.Clear();

        Vector3 pos = MatchComponents.ballComponents.rigBall.position;
        Vector3 v0 = MatchComponents.ballComponents.rigBall.velocity;

        Vector3 pos0 = pos;
        float t0 = 0f;
        float time = 0f;

        while (time <= simulationTime)
        {
            if (InsideField(pos))
            {
                positions.Add(pos);
                times.Add(time);
            }

            if (v0.magnitude < 0.01f)
            {
                times[times.Count - 1] = Mathf.Infinity;
                break;
            }


            // === Movimiento analítico ===
            pos = GetAnalyticPosition(t0 + timeStep, pos0, v0);
            Vector3 vel = GetAnalyticVelocity(t0 + timeStep, v0);

            // === Rebote ===
            if (pos.y <= groundLevel && vel.y < 0f)
            {
                pos.y = groundLevel;
                v0 = CalculateBounce(vel, Vector3.up, bounciness, dynamicFriction);

                pos0 = pos;
                t0 = 0f;
            }
            else
            {
                t0 += timeStep;
            }

            time += timeStep;
        }
    }

    // =========================================================
    // ================== ANALYTIC SOLUTION ====================
    // =========================================================

    Vector3 GetAnalyticPosition(float t, Vector3 pos0, Vector3 v0)
    {
        float k = Mathf.Max(airFriction, 0.0001f);
        float ekt = Mathf.Exp(-k * t);

        Vector2 vx0 = new Vector2(v0.x, v0.z);
        Vector2 x = (vx0 / k) * (1f - ekt);

        float y = -vfMagnitude * t +
                  ((v0.y + vfMagnitude) / k) * (1f - ekt);

        return pos0 + new Vector3(x.x, y, x.y);
    }

    Vector3 GetAnalyticVelocity(float t, Vector3 v0)
    {
        float k = Mathf.Max(airFriction, 0.0001f);
        float ekt = Mathf.Exp(-k * t);

        Vector2 vx = new Vector2(v0.x, v0.z) * ekt;
        float vy = -vfMagnitude + (v0.y + vfMagnitude) * ekt;

        return new Vector3(vx.x, vy, vx.y);
    }

    // =========================================================
    // ======================= BOUNCE ==========================
    // =========================================================

    public Vector3 CalculateBounce(Vector3 velocity, Vector3 normal, float bounciness, float friction)
    {
        Vector3 vNormal = Vector3.Dot(velocity, normal) * normal;
        Vector3 vTangent = velocity - vNormal;

        Vector3 vBounce = -bounciness * vNormal;
        Vector3 vFriction = vTangent * (1f - friction);

        return vBounce + vFriction;
    }

    // =========================================================

    bool InsideField(Vector3 pos)
    {
        return pos.x >= fieldMin.x && pos.x <= fieldMax.x &&
               pos.z >= fieldMin.y && pos.z <= fieldMax.y;
    }
}