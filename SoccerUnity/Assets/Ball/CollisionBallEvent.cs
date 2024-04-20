using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionBallEvent : MonoBehaviour
{
    public new Rigidbody rigidbody;
    public float forceEffect = 0.7f;
    public float adjustVelocity = 0.2f;
    public float adjustDistance = 15;
    public float adjustTime = 10, adjustMaxTimeEffect = 20, adjustImpact = 1;
    public float minEffect = 0, maxEffect = 70;
    public float maxImpactEffect = 200;
    public float frictionTerrain = 1;
    public AnimationCurve animationCurveEffect;
    float collisionImpact;
    float effect;
    bool inEffect;
    int offsetTime;
    

    // Update is called once per frame
    void OnCollisionEnter(UnityEngine.Collision collision)
    {
        collisionImpact = collision.impulse.magnitude;

    }
    void OnCollisionStay(UnityEngine.Collision collision)
    {
        
    }

    public IEnumerator ApplyEffect(EffectArgs effectArgs)
    {
        float time = 0;
        float maxTime = effectArgs.timeMovimentMouse * adjustMaxTimeEffect;
        inEffect = true;
        while (time < maxTime && collisionImpact < 100)
        {
            time += Time.deltaTime;
            float distance = effectArgs.distance * adjustDistance;
            float velocity = effectArgs.initForce * adjustVelocity;
            float numerador = distance * velocity * forceEffect;
            float timeMoviment = effectArgs.timeMovimentMouse * adjustTime;
            effect = numerador / timeMoviment;
            float impact = 1 + collisionImpact * adjustImpact;
            effect *= animationCurveEffect.Evaluate(time / maxTime);
            effect = Mathf.Clamp(effect, minEffect, maxEffect);
            effect /= impact;
            //print("distance="+distance + " velocity=" + velocity + " timeMoviment=" + timeMoviment+ " force=" + force+" maxTime="+maxTime+" collisionImpact="+collisionImpact+" impact="+impact);

            rigidbody.AddForce(effectArgs.gloabalDir * effect);
            yield return null;
        }
        inEffect = false;
    }
}
public class EffectArgs
{
    public List<Vector2> listMoviment;
    public Vector2 start, end, middle;
    public Vector3 localDir;
    public Vector3 gloabalDir;
    public RaycastHit hit;
    public Rigidbody rigidbody;
    public float distance;
    public float timeMovimentMouse;
    public float initForce;
    public float normal;
    public EffectArgs(List<Vector2> listMouse, RaycastHit hit, Rigidbody rigidbody, float time, float initForce)
    {
        this.listMoviment = listMouse;
        this.hit = hit;
        this.rigidbody = rigidbody;
        timeMovimentMouse = time;
        this.initForce = initForce;
    }
    public void calculatePoints()
    {
        if (listMoviment.Count > 0)
        {
            start = listMoviment[0];
            end = listMoviment[listMoviment.Count - 1];
            middle = listMoviment[(listMoviment.Count - 1) / 2];

        }
    }
    public void printPoints()
    {
        foreach (Vector2 v in listMoviment)
        {
            Debug.Log("printPoints " + v);
        }
    }
    public void calculateLocalDir()
    {
        localDir = (end - start).normalized;

    }
    public void calculateNormal(Vector3 vector1, Vector3 vector2)
    {
        normal = Vector3.Angle(vector1, vector2);
    }
    public void calculateGlobalDir(Transform transform)
    {
        gloabalDir = transform.TransformDirection(new Vector3(-localDir.x, -localDir.y, 0));
    }
    public float getDistance()
    {
        return distance = Vector2.Distance(start, end);
    }
}
