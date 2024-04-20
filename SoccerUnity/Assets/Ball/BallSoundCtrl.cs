using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSoundCtrl : MonoBehaviour
{
    SoundSet grassSet { get => gameSounds.grassSet; }
    AudioClip others { get => gameSounds.others; }
    AudioClip body { get => gameSounds.body; }
    AudioClip kick { get => gameSounds.kick; }
    AudioClip post { get => gameSounds.post; }
    public SoundPointsList soundPoints;
    public float maxVolume = 0.2f;
    GameSounds gameSounds { get => MatchComponents.gameSounds; }
    void Start()
    {
        MatchEvents.kick.AddListener(ApplySoundKick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ApplySoundKick(KickEventArgs args)
    {
        float volume = GetVolumeKick(args.kickDirection.magnitude);
        playSound(kick, volume, 10, 1);
    }
    public float GetVolumeKick(float currentForce)
    {
        //float result = Mathf.Clamp((currentValue * (volumeMax - volumeMin)) / (volumeMax - volumeMin), volumeMin, volumeMax);

        float result = KickValues.volumenAdjust.Evaluate((currentForce / KickValues.maxForceVolume)* maxVolume);
        return result;
    }
    private void OnCollisionEnter(Collision collision)
    {
        MaterialSoundTypeTag type = collision.transform.GetComponent<MaterialSoundTypeTag>();
        if (type != null)
        {
            float force = collision.relativeVelocity.magnitude;
            switch (type.tag)
            {
                case MaterialSoundType.Grass:
                    dispatchGrass(collision);
                    break;
                case MaterialSoundType.Body:
                    float volume = force / 100;
                    playSound(body, volume, 10, 1);
                    break;
                case MaterialSoundType.Post:
                    volume = force / 100;
                    playSound(body, volume, 10, 1);
                    break;
            }

        }
        else
        {
            float force = collision.relativeVelocity.magnitude;
            float volume = force / 100;
            playSound(others, volume, 10, 1);
        }
    }
    void dispatchGrass(Collision collision)
    {
        Transform ballTransform = transform;
        float force = collision.relativeVelocity.magnitude;
        float volume = force / 100;
        if (MatchComponents.footballField.fullFieldArea.PointIsInside(ballTransform.position))
        {
            if(collision.transform == MatchComponents.footballField.transform)
            {
                
                playSound(grassSet.getRandomSound(),volume,10,1);
            }
            else
            {
                //audioSource.clip = null;
            }
        }
        else
        {
            if (collision.transform != MatchComponents.footballField.transform)
            {
                playSound(grassSet.getRandomSound(), volume, 10, 1);
            }
            else
            {
                //audioSource.clip = null;
            }
        }
    }
    void playSound(AudioClip audioClip,float volume,float maxDistance,float minDistance)
    {
        soundPoints.playSound(MatchComponents.ballComponents.position, audioClip,volume,maxDistance,minDistance);
    }
}
