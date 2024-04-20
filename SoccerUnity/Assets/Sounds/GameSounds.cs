using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSounds", menuName = "ScriptableObjects/GameSounds", order = 1)]
public class GameSounds : ScriptableObject
{
    public SoundSet grassSet;
    public AudioClip others, body, kick, post;
}
