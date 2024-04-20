using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchBall : MonoBehaviour
{
    public ComponentsPorteria componentsPorteria;
    public Vector3 dirMin,dirMax;
    public Transform posSaque;
    bool saque;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Rigidbody rigBall = componentsPorteria.componentsBall.rigBall;
        Transform transPortero = componentsPorteria.transPortero;
        Transform transBall = componentsPorteria.componentsBall.transBall;
        if (!saque&&rigBall.velocity.magnitude < 20 && Vector3.Distance(transPortero.position, transBall.position) < 1)
        {
            rigBall.isKinematic = true;
            rigBall.velocity = Vector3.zero;
            rigBall.angularVelocity = Vector3.zero;
            StartCoroutine(Saque());
            saque = true;
        }
    }
    IEnumerator Saque()
    {
        Rigidbody rigBall = componentsPorteria.componentsBall.rigBall;
        Transform transPortero = componentsPorteria.transPortero;
        Transform transBall = componentsPorteria.componentsBall.transBall;
        float time = 0;
        do
        {
            yield return null;
            transBall.position = transPortero.position + transPortero.TransformDirection(Vector3.forward*0.5f);
            time += Time.deltaTime;
        } while (time < 3);
        transBall.position = posSaque.position + transPortero.TransformDirection(Vector3.forward);
        rigBall.velocity = Vector3.zero;
        rigBall.angularVelocity = Vector3.zero;
        rigBall.isKinematic = false;
        Vector3 dir = new Vector3(Random.Range(dirMin.x, dirMax.x), Random.Range(dirMin.y, dirMax.y), Random.Range(dirMin.z, dirMax.z));
        rigBall.AddForce(transPortero.TransformDirection(dir), ForceMode.VelocityChange);
        saque = false;
    }
}
