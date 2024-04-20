using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalFavorAnimation : MonoBehaviour
{
    public AudioSource grada,boom;
    public Canvas canvas;
    public delegate void EmptyDelegate();
    public event EmptyDelegate EndEvent;
    public void PlaySoundGrada()
    {
        canvas.enabled = true;
        grada.Play();
    }
    public void PlaySoundBoom()
    {
        boom.Play();
    }
    // Update is called once per frame
    public void End()
    {
        canvas.enabled = false;
        EndEvent?.Invoke();
    }
}
