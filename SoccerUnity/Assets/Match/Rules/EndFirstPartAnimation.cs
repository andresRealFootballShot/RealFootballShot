using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndFirstPartAnimation : MonoBehaviour
{
    public AudioSource audioSource;
    public event emptyDelegate endEvent;
    void Start()
    {
        
    }

    public void play()
    {
        audioSource.Play();
        Invoke(nameof(invokeEndEvent), MatchComponents.rulesSettings.timeWaitToStartNextPart);
        
    }
    void invokeEndEvent()
    {
        endEvent?.Invoke();
    }
}
