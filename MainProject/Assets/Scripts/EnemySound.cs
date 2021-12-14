using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySound : ISpacialAudioSource
{
    [field: SerializeField] public List<ClipCollection<SpacialSound>> Sounds { get; } = new List<ClipCollection<SpacialSound>>();
    [HideInInspector] public SpacialAudioSource SpacialAudioSource { get; }
    public EnemySound(SpacialAudioSource spacialaudiosource)
    {
        SpacialAudioSource = spacialaudiosource;
    }

    public void Play(SpacialSound sound)
    {
        SpacialAudioSource.Play(sound);
    }

    public void Play(SoundType type)
    {
        SpacialAudioSource.Play(ClipCollection<SpacialSound>.ChooseClipFromType(type, Sounds));
    }
}
