using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPartAnimation : MonoBehaviour
{
    public AudioSource audioSource;
    public delegate void EmptyEvent();
    public event EmptyEvent endAnimation;
    public Animation animation;
    void Start()
    {
        
    }
    public void play()
    {
        if (animation != null)
        {
            animation.Play();
        }
        else
        {
            //Debug.LogError("StartPartAnimation.play(): animation is null");
            //PlayPitido();
            //End();
        }
    }
    // Update is called once per frame
    public void End()
    {
        ComponentsPlayer componentsPlayer = GameObject.FindGameObjectWithTag("ComponentsPlayer").GetComponent<ComponentsPlayer>();
        //componentsPlayer.EnableAll();
        //Timer timer = GameObject.FindWithTag("TimerMatch").GetComponent<Timer>();
        endAnimation?.Invoke();
    }
    public void PlayPitido()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
        //SaqueBall();
    }
    public void SaqueBall()
    {
        
    }
}
