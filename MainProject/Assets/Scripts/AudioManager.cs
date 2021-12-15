using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public const int EFFECTCHANNEL = 0;
    public const int MUSICCHANNEL = 1;
    public const int UISOUNDCHANNEL = 2;
    public const int STEPSOUNDCHANNEL = 3;
    public const int PLAYERACTIONCHANNEL = 4;

    public ClipCollection<Sound>[] Music;
    public static AudioManager Instance { get; private set; }
    private AudioSource[] AudioSources;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Destroy(this);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        AudioSources = GetComponents<AudioSource>();

        StageManager.OnSceneReady += PlayGameplayMusic;
        HubManager.OnSceneReady += PlayHubMusic;
        // SceneTransition.OnMenuTransitionComplete += PlayMenuMusic;
        // SceneTransition.OnAnyTransitionBegin += FadeOutAllSound;
        // SceneTransition.OnAnyTransitionComplete += FadeInAllSound;


        AudioSources[MUSICCHANNEL].loop = true;
    }


    public void PlayGameplayMusic()
    {
        Sound s = ClipCollection<Sound>.ChooseClipFromType(SoundType.MusicGameplay, Music);
        if (s == null)
            return;

        AudioSources[MUSICCHANNEL].volume = s.Volume;
        AudioSources[MUSICCHANNEL].clip = s.Clip;

        // StartCoroutine(FadeIn(AudioSources[MUSICCHANNEL]));
    }

    public void PlayHubMusic()
    {
        Sound s = ClipCollection<Sound>.ChooseClipFromType(SoundType.MusicHub, Music);
        if (s == null)
            return;

        AudioSources[MUSICCHANNEL].volume = s.Volume;
        AudioSources[MUSICCHANNEL].clip = s.Clip;
        // StartCoroutine(FadeIn(AudioSources[MUSICCHANNEL]));
    }

    public void PlayMenuMusic()
    {
        AudioSources[MUSICCHANNEL].Stop();
        StopCoroutine(FadeOut(AudioSources[MUSICCHANNEL]));
        Sound s = ClipCollection<Sound>.ChooseClipFromType(SoundType.MusicMenu, Music);
        if (s == null)
            return;

        AudioSources[MUSICCHANNEL].volume = s.Volume;
        AudioSources[MUSICCHANNEL].clip = s.Clip;
        // StartCoroutine(FadeIn(AudioSources[MUSICCHANNEL]));
    }

    public void PlayClip(Sound sound, int channel, bool canRandomize = true)
    {
        if (channel < 0 || channel >= AudioSources.Length)
        {
            Debug.LogWarning("Invalid Channel given");
            return;
        }
        if (sound == null)
        {
            Debug.LogWarning("Sound is null");
            return;
        }
        if (sound.Clip == null)
        {
            Debug.LogWarning("Sound Clip is null");
            return;
        }

        if (canRandomize && sound.Randomize)
        {
            AudioSources[channel].volume = sound.Volume + Util.HalfRandomRange(sound.VolumeRandomRange);
            AudioSources[channel].pitch = sound.Pitch + Util.HalfRandomRange(sound.PitchRandomRange);
        }
        AudioSources[channel].Stop();
        AudioSources[channel].PlayOneShot(sound.Clip);
    }

    private IEnumerator FadeOut(AudioSource source, float fadeTime = SceneTransition.FADETIME)
    {
        if (source == null)
            yield break;
        float volume = source.volume;
        while (source != null && source.volume > 0)
        {
            if (source == null)
                yield break;
            source.volume -= volume * Time.unscaledDeltaTime / fadeTime;
            yield return null;
        }
        if (source == null)
            yield break;
        source.Stop();
        source.volume = volume;
    }

    private IEnumerator FadeIn(AudioSource source, float fadeTime = SceneTransition.FADETIME)
    {
        if (source == null)
            yield break;
        source.Stop();
        float volume = source.volume;
        source.volume = 0;
        source.Play();
        while (source.volume < volume)
        {
            if (source == null)
                yield break;
            // if (source == AudioSources[MUSICCHANNEL])
            // {
            //     yield return null;
            // }

            source.volume += volume * Time.unscaledDeltaTime / fadeTime;
            // Debug.Log("audio source " + gameObject.name + " volume " + source.volume);
            yield return null;
        }
        source.volume = volume;
    }

    public void FadeInAllSound()
    {
        StartCoroutine(FadeInAllSoundDelayed());
    }

    public void FadeOutAllSound()
    {
        FadeOutAllSoundDelayed();
    }
    private IEnumerator FadeInAllSoundDelayed()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (var source in audioSources)
        {
            StartCoroutine(FadeIn(source));
            // Debug.Log("fading in " + source.gameObject.name);
        }
    }

    private void FadeOutAllSoundDelayed()
    {
        // yield return new WaitForSecondsRealtime(0.1f);
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (var source in audioSources)
        {
            StartCoroutine(FadeOut(source));
            // Debug.Log("fading out " + source.gameObject.name);
        }
    }
}
