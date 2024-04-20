using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalAnimation : MonoBehaviour
{
    public AudioSource grada;
    public Canvas canvas;
    public delegate void EmptyDelegate();
    public MyEvent EndEvent = new MyEvent();

    public void Play(GoalData args)
    {
        grada.Play();
        Invoke(nameof(End),MatchComponents.rulesSettings.goalAnimationDuration);
    }
    // Update is called once per frame
    void End()
    {
        canvas.enabled = false;
        EndEvent.Invoke();
    }
}
