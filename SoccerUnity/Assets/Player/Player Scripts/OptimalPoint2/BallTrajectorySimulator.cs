using DOTS_ChaserDataCalculation;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

public class BallTrajectorySimulator : MonoBehaviour
{
    public float simulationTime = 5f;
    public float timeStep = 0.05f;
    public float gravity = 9.81f;
    float airFriction{ get => MatchComponents.ballComponents.rigBall.drag; }

    float groundLevel{ get => getGroundLevel(); }
    public Transform fieldMinTransform, fieldMaxTransform;
    Vector2 fieldMin{ get => new Vector2(fieldMinTransform.position.x, fieldMinTransform.position.z); }
    Vector2 fieldMax { get => new Vector2(fieldMaxTransform.position.x, fieldMaxTransform.position.z); }

    float bounciness { get => MatchComponents.ballComponents.bounciness; }
    float friction{ get=>MatchComponents.ballComponents.friction; }
    float dynamicFriction{ get=>MatchComponents.ballComponents.dynamicFriction; }
    float ballRadio{ get=>MatchComponents.ballComponents.radio; }

    public List<Vector3> positions { get; private set; } = new List<Vector3>();
    public List<float> times { get; private set; } = new List<float>();

    public void Simulate()
    {
        positions.Clear();
        times.Clear();

        Vector3 pos = MatchComponents.ballComponents.rigBall.position;
        Vector3 vel = MatchComponents.ballComponents.rigBall.velocity;
        Vector3 v0 = vel;
        Vector3 pos0 = pos;
       
        float time = 0;
        float t0 = 0;
        int outOfFieldCounter = 0;
        int maxOutCount = 20;
        float vfMagnitude = gravity / airFriction;
        float v0Magnitude = v0.magnitude;
        while (time <= simulationTime)
        {
            if (pos.x >= fieldMin.x && pos.x <= fieldMax.x &&
                pos.z >= fieldMin.y && pos.z <= fieldMax.y)
            {
                positions.Add(pos);
                times.Add(time);
                outOfFieldCounter = 0;
            }
            else
            {
                outOfFieldCounter++;
                if (outOfFieldCounter > maxOutCount)
                    break;
            }
           
            if (vel.magnitude < 0.01f)
            {
                break;

            }
            if (Mathf.Abs(vel.y) < 0.1f && pos.y <= groundLevel)
            {
                vel.y = 0;
                float t1 = getTofWMax_WithRollDrag(v0Magnitude);
                if (t0 < t1)
                {
                    t1 = t0;
                }
                Vector3 velocity = Vector3.zero;
                Vector3 normalizedV0 = vel.normalized;
                velocity = getVelocityAtTime(t1,v0Magnitude,normalizedV0);
                float v2 = velocity.magnitude;
                if (airFriction == 0)
                {
                    float roll = friction * 9.81f;
                    float t2 = (t0 - t1);
                    float a = -0.5f * roll * t1 * t1;
                    float d = v0Magnitude * t1 + a + v2 * t2;
                    pos = normalizedV0 * d + pos0;
                }
                else
                {
                    float e = (1 - Mathf.Exp(-airFriction * t1));
                    float e2 = (1 - Mathf.Exp(-airFriction * Mathf.Clamp((t0 - t1), 0, Mathf.Infinity)));
                    float vf = (friction * gravity) / airFriction;
                    float b = (v0Magnitude + vf) / airFriction;
                    float roll = -vf * t1 + b * e;
                    float drag = (v2 / airFriction) * e2;
                    //Debug.Log("getPositionAtTime " + roll+ " "+drag + " | v2=" +v2 + " |t1=" +t1+" | vf="+vf);
                    pos = normalizedV0 * roll + normalizedV0 * drag + pos0;
                }
            }
            else
            {
                // Aplicar física
                //vel += Vector3.down * gravity * timeStep;
                //vel *= 1f - airFriction * timeStep;


                float ekt = Mathf.Exp(-airFriction * (t0 + timeStep));



                Vector2 vx0 = new Vector2(v0.x, v0.z);
                Vector2 x = (vx0 / airFriction) * (1 - ekt);
                float vy0 = v0.y;
                float y = -vfMagnitude * (t0 + timeStep) + ((vy0 + vfMagnitude) / airFriction) * (1 - ekt);
                Vector3 previousPos = pos;
                pos = new Vector3(x.x, y, x.y) + pos0;

                vel = (pos - previousPos) / timeStep;


                //pos += vel * timeStep;

                // Rebote realista
                if (pos.y <= groundLevel)
                {

                    pos.y = groundLevel;

                    v0 = CalculateBounce(vel, Vector3.up, bounciness, dynamicFriction);
                    v0Magnitude = v0.magnitude;
                    t0 = 0;
                    pos0 = pos;

                }
            }


            time += timeStep;
            t0 += timeStep;
        }
    }
    public Vector3 getVelocityAtTime(float t,float v0Magnitude,Vector3 normalizedV0)
    {
        if (v0Magnitude == 0)
        {
            return Vector3.zero;
        }
        float maxWt = getTofWMax_WithRollDrag(v0Magnitude);
        if (airFriction == 0)
        {
            return normalizedV0 * (v0Magnitude - airFriction * 9.81f * maxWt);
        }
        else
        {
            float vf = (friction * gravity) / airFriction;
            float rollSlipV = -vf + (v0Magnitude + vf) * Mathf.Exp(-airFriction * Mathf.Clamp(maxWt, 0, t));
            if (t <= maxWt)
            {
                return normalizedV0 * rollSlipV;
            }
            else
            {
                float pureRollV = rollSlipV * Mathf.Exp(-airFriction * (t - maxWt));
                return normalizedV0 * pureRollV;
            }
        }
    }
    float getTofWMax_WithRollDrag(float v0Magnitude)
    {
        float r = ballRadio;
        float w = Mathf.Clamp(v0Magnitude * 5.86923f, 0, 50);
        if (airFriction == 0)
        {
            return (v0Magnitude - r * w) / (friction * gravity);
        }
        else
        {
            float vf = (friction * gravity) / airFriction;
            float ln = Mathf.Log((r * w + vf) / (v0Magnitude + vf));
            return ln / -airFriction;
        }
    }
    public Vector3 CalculateBounce(Vector3 velocity, Vector3 normal, float bounciness, float friction)
    {
        Vector3 vNormal = Vector3.Dot(velocity, normal) * normal;
        Vector3 vTangent = velocity - vNormal;

        Vector3 vRebote = -bounciness * vNormal;
        Vector3 vFriccion = vTangent * (1f - friction);

        return vRebote + vFriccion;
    }
    float getGroundLevel()
    {
        return MatchComponents.ballComponents.sphereCollider.radius * MatchComponents.ballComponents.sphereCollider.transform.localScale.x;
    }
}