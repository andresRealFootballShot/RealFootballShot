using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundSet", menuName = "ScriptableObjects/SoundSet", order = 1)]
public class SoundSet : ScriptableObject
{
    public List<AudioClip> sounds;
    public AudioClip getRandomSound()
    {
        int random = Random.Range(0, sounds.Count - 1);
        return sounds[random];
    }
}
