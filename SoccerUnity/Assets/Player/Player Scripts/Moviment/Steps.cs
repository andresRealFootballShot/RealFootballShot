using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steps : MonoBehaviour
{
    public SoundSet soundSet;
    public AudioSource[] audioSource;
    public AudioClip[] audioClip { get => soundSet.sounds.ToArray(); }
    public float minVolume,maxVolume;
    public AnimationCurve animationCurve;
    public PublicPlayerData publicPlayerData;
    int currentIndex;
    bool compute;

    void Start()
    {
        
    }
    void Step1()
    {
        if (!compute)
        {
            int random = Random.Range(0, audioClip.Length-1);
            audioSource[currentIndex].clip = audioClip[random];
            float normalizedVelocity = publicPlayerData.speed/ publicPlayerData.maxSpeed;
            if (!MyFunctions.floatIsNanOrInfinity(normalizedVelocity))
            {
                float curve = animationCurve.Evaluate(normalizedVelocity);
                audioSource[currentIndex].volume = Mathf.Clamp((curve + minVolume) * maxVolume, minVolume, maxVolume);
                //audioSource[currentIndex].volume = 0.15f;
                //print(publicPlayerData.speed + " | "+ publicPlayerData.maxSpeed + " | " + audioSource[currentIndex].volume);
                audioSource[currentIndex].Play();
                if (currentIndex < audioSource.Length - 1)
                    currentIndex++;
                else
                    currentIndex = 0;
            }
            compute = true;
        }
    }
    void Step2()
    {
        if (compute)
        {
            int random = Random.Range(0, audioClip.Length - 1);
            audioSource[currentIndex].clip = audioClip[random];
            float normalizedVelocity = publicPlayerData.speed / publicPlayerData.maxSpeed;
            audioSource[currentIndex].volume = Mathf.Clamp((normalizedVelocity + minVolume)*maxVolume,minVolume,maxVolume);
            //audioSource[currentIndex].volume = 0.15f;
            audioSource[currentIndex].Play();
            if (currentIndex < audioSource.Length-1)
                currentIndex++;
            else
                currentIndex = 0;
            compute = false;
        }
    }
}
