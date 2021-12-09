using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpacialAudioSource : MonoBehaviour, ISpacialAudioSource
{
    private AudioSource AudioSource;

    private void Awake() {
        AudioSource = GetComponent<AudioSource>();
        AudioSource.spatialize = true;
        AudioSource.spatialBlend = 1f; //full spacial always
    }

    public void Play(SpacialSound sound)
    {
        AudioSource.volume = sound.Volume;
        AudioSource.pitch = sound.pitch;
        AudioSource.panStereo = sound.StereoPan;
        AudioSource.rolloffMode = sound.RolloffMode;
        AudioSource.minDistance = sound.MinDistance;
        AudioSource.maxDistance = sound.MaxDistance;
        AudioSource.clip = sound.Clip;
        AudioSource.PlayOneShot(sound.Clip);

    }
}
