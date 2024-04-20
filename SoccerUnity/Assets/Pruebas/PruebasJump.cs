using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PruebasJump : MonoBehaviour
{
    public Rigidbody player;
    public Vector3 maxHeight;
    public float force;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            float v0 = 20;
            //float mh = (v0 * v0) / (2 * 9.81f);
            float mh = getMaximumY();
            float t = Mathf.Sqrt((mh * 8) / 9.81f);
            maxHeight = MyFunctions.setYToVector3(player.position, mh);
            player.AddForce(Vector3.up* force, ForceMode.VelocityChange);
            print("t=" + t);
            Invoke(nameof(printHeight), t);
        }
       
    }
    public float getMaximumY()
    {
        float k = player.drag;
        float g = 9.81f;
        float vy0 = force;
        float y = vy0 / k - (g / (k * k)) * Mathf.Log(1 + (k * vy0 / g));
        return y;
    }
    void printHeight()
    {
        float v0 = force;
        float mh = getMaximumY();
        float t = Mathf.Sqrt((mh * 8) / 9.81f);
        print("height=" + player.position.y + " | maxHeight=" + maxHeight);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(maxHeight, 0.05f);
    }
}
