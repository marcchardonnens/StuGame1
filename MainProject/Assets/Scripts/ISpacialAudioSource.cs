using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ISpacialAudioSource
{
    public List<ClipCollection<SpacialSound>> Sounds {get;}
    public SpacialAudioSource SpacialAudioSource { get; }
    public void Play(SpacialSound sound);
    public void Play(SoundType type);
}
