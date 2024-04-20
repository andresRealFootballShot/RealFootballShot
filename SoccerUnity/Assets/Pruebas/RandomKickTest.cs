using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomKickTest : MonoBehaviour
{
    public PublicPlayerData publicPlayerData;
    public Transform kickTransform;
    public float min, max;
    public bool useKickTransform;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Rigidbody ballRigidbody = MatchComponents.ballRigidbody;
            if (useKickTransform)
            {
                ballRigidbody.position = MyFunctions.setYToVector3(kickTransform.position, MatchComponents.ballRadio);
                ballRigidbody.velocity = kickTransform.forward * Random.Range(min, max);
                StartCoroutine(print());
            }
            else
            {
                float x = Random.Range(min, max);
                float z = Random.Range(min, max);
                ballRigidbody.velocity += new Vector3(x,0,z);
            }
        }
    }
    IEnumerator print()
    {
        ChaserData chaserData;
        publicPlayerData.getFirstChaserData(out chaserData);
        float duration = chaserData.OptimalTime;
        float t = 0;
        while (t <= duration)
        {
           
            yield return null;
            t += Time.deltaTime;
            //print("t=" + t + " | optimalTime=" + chaserData.OptimalTime + " | optimalPoint=" + chaserData.OptimalPoint);
            
        }
    }
}
