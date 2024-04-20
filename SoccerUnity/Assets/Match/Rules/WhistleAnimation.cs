using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhistleAnimation : MonoBehaviour
{
    public AudioSource whistle;
    private void Start()
    {
        MatchComponents.rulesComponents.whistleAnimation = this;
    }
    public void Play()
    {
        whistle.Play();
    }
}
