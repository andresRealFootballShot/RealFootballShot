using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostHit : MonoBehaviour
{
    public AudioSource audioSource;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        float magnitude = collision.impulse.magnitude/400;
        audioSource.volume = magnitude;
        audioSource.Play();
    }
}
