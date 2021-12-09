using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Sound
{
    public AudioClip Clip;
    [Range(0f, 1f)]
    public float Volume = 1f;

    [Range(-3f, 3f)]
    public float pitch = 1f;
    public float SpacialBlend { get; } = 0f;

    // public static Sound GetRandomSound(SoundType type, Sound[] sounds)
    // {
    //     return Sound.Choose(sounds);
    // }
}
