using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public ClipCollection[] Music;
    public static AudioManager Instance {get; private set;}

    private AudioSource effects, music;
    private void Awake() 
    {
        if(Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        AudioSource[] sources = GetComponents<AudioSource>();
        effects = sources[0];
        music = sources[1];

        StageManager.OnSceneReady += PlayGameplayMusic;
    }

    private void PlayGameplayMusic()
    {
        music.PlayOneShot(ClipCollection.ChooseClipFromType(SoundType.MusicGameplay, Music));
    }

    public void PlayClip(AudioClip clip)
    {
        effects.PlayOneShot(clip);
    }


}
