using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public abstract class SpacialAudioSource : MonoBehaviour, ISpacialAudioSource
{
    private AudioSource AudioSource;

    private void Awake() {
        AudioSource = GetComponent<AudioSource>();
        AudioSource.spatialize = true;
        AudioSource.spatialBlend = 1f; //full spacial always
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play(SpacialSound sound)
    {
        if(!sound.Enaled)
        {
            Debug.LogWarning("Disabled Sound was attempted to be played");
            return;
        }
        AudioSource.volume = sound.Volume;
        AudioSource.pitch = sound.pitch;
        AudioSource.panStereo = sound.StereoPan;
        AudioSource.rolloffMode = sound.RolloffMode;
        AudioSource.minDistance = sound.MinDistance;
        AudioSource.maxDistance = sound.MaxDistance;
        AudioSource.PlayOneShot(sound.Object);

    }
}
