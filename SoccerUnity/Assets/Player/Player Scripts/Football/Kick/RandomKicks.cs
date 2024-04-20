using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class RandomKicks : MonoBehaviour
{
    public BallComponents componentsBall;
    public float minTimeRandom, maxTimeRandom;
    public float maxForce;
    public bool end;
    bool compute;
    private void Start()
    {

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            compute = !compute;
            if (!compute)
            {
                end = true;
                StopCoroutine(AddRandomForce());
                
            }
            else
            {
                end = false;
                StartCoroutine(AddRandomForce());
            }
            
        }

    }
    IEnumerator AddRandomForce()
    {
        while (!end) {
            yield return new WaitForSeconds(Random.Range(minTimeRandom, maxTimeRandom));
            
            if (componentsBall.rigBall != null)
            {
                if (PhotonNetwork.IsConnected)
                {
                    Vector3 dir = new Vector3(Random.Range(-1, 1) * maxForce, Random.Range(-1, 1) * maxForce, Random.Range(-1, 1) * maxForce);
                    componentsBall.photonViewBall.RPC("AddForceRPC", RpcTarget.All, componentsBall.transBall.position, componentsBall.transBall.eulerAngles,dir, ForceMode.Impulse,dir.magnitude/maxForce);
                }
                else
                {
                    componentsBall.rigBall.AddForce(new Vector3(Random.Range(-1, 1) * maxForce, Random.Range(-1, 1) * maxForce, Random.Range(-1, 1) * maxForce), ForceMode.Impulse);
                }
            }
        }
    }
}
