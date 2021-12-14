using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpacialAudioSource 
{
    private readonly AudioSource AudioSource;

    public SpacialAudioSource(AudioSource audioSource)
    {
        AudioSource = audioSource;
    }

    public void Play(SpacialSound sound, bool canRandomize = true)
    {
        if(sound == null)
        {
            Debug.LogWarning("Spacial Sound null");
            return;
        }

        AudioSource.Stop();

        if(canRandomize && sound.Randomize)
        {
            AudioSource.volume = sound.Volume + Util.HalfRandomRange(sound.VolumeRandomRange);
            AudioSource.pitch = sound.Pitch + Util.HalfRandomRange(sound.PitchRandomRange);
        }
        else
        {
            AudioSource.volume = sound.Volume;
            AudioSource.pitch = sound.Pitch;
        }
        AudioSource.spatialBlend = sound.SpacialBlend;
        AudioSource.panStereo = sound.PanStereo;
        AudioSource.minDistance = sound.MinDistance;
        AudioSource.maxDistance = sound.MaxDistance;
        AudioSource.spread = sound.Spread;
        
        // Debug.Log("Playing Spacial Sound " + sound.Clip.name);
        AudioSource.loop = false;
        AudioSource.playOnAwake = false;
        AudioSource.clip = sound.Clip;
        AudioSource.Play();
        
        // AudioSource.PlayOneShot(sound.Clip);

    }
}
