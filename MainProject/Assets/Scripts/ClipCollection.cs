using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ClipCollection<T> where T : Sound
{
    [SerializeField] private bool enabled = true;
    public SoundType Type;
    public RandomChoice<T>[] Clips;

    public static T ChooseClipFromType(SoundType type, ClipCollection<T>[] sounds)
    {
        RandomChoice<T>[] clips = sounds.FirstOrDefault(x => x.enabled && x.Type == type)?.Clips;
        if (clips == null || clips.Length <= 0)
        {
            Debug.LogWarning("No suitableAudioclipsFound");
            return null;
        }
        return RandomChoice<T>.Choose(clips);
    }

        public static T ChooseClipFromType(SoundType type, List<ClipCollection<T>> sounds)
    {
        RandomChoice<T>[] clips = sounds.FirstOrDefault(x => x.enabled && x.Type == type)?.Clips;
        if (clips == null || clips.Length <= 0)
        {
            Debug.LogWarning("No suitableAudioclipsFound");
            return null;
        }
        return RandomChoice<T>.Choose(clips);
    }

}
