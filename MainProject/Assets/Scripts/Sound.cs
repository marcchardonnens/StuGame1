using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Sound
{
    public bool Randomize = false;
    public AudioClip Clip;
    [Range(0f, 1f)]
    public float Volume = 1f;
    public float VolumeRandomRange = 0f;

    [Range(-3f, 3f)]
    public float Pitch = 1f;
    public float PitchRandomRange = 0f;

    public Sound()
    {
    }
    public Sound(Sound sound)
    {
        Randomize = sound.Randomize;
        Clip = sound.Clip;
        Volume = sound.Volume;
        VolumeRandomRange = sound.VolumeRandomRange;
        Pitch = sound.Pitch;
        PitchRandomRange = sound.PitchRandomRange;
    }
}
