using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPointsList : MonoBehaviour
{
    public int count;
    public GameObject soundPointPrefab;
    public Transform parent;
    List<AudioSource> audioSources = new List<AudioSource>();
    int index;
    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = Instantiate(soundPointPrefab,parent);
            AudioSource audioSource = prefab.GetComponent<AudioSource>();
            audioSources.Add(audioSource);
        }
    }
    public void playSound(Vector3 position,AudioClip audioClip,float volume,float maxDistance,float minDistance)
    {
        audioSources[index].transform.position = position;
        audioSources[index].clip = audioClip;
        audioSources[index].volume = volume;
        audioSources[index].maxDistance = maxDistance;
        audioSources[index].minDistance = minDistance;
        audioSources[index].Play();
        index = index < audioSources.Count - 1 ? ++index : 0;
    }
}
