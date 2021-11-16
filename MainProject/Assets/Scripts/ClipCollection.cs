using System.Security.Claims;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum SoundType
{
    MusicMenu,
    MusicGameplay,
    PlayerAttack,
    PlayerHurt,
    PlayerJump,
    PlayerBlock,
    PlayerAction,
    PlayerDialogue,
    Enemy,
    EnemyBoss,
}


[System.Serializable]
public class ClipCollection<T> where T : RandomChoice<T>
{
    public SoundType Type;
    public T[] Clips;



    public static T[] GetAllClipsOfType(SoundType type, ClipCollection<T>[] soundcollections)
    {
        ClipCollection<T>[] collections = soundcollections.Where(x => x.Type == type).ToArray();
        
        List<T> allrelevantClips = new List<T>();
        foreach (var c in collections)
        {
            allrelevantClips.AddRange(c.Clips);
        }

        return allrelevantClips.ToArray();
    }

    public static T ChooseClipFromArray(T[] sounds)
    {
        // RandomChoice<T>.Choose(sounds);




        return null;
    }

    public static T ChooseClipFromType(SoundType type, ClipCollection<T>[] sounds)
    {
        T[] clips = sounds.FirstOrDefault(x => x.Type == type).Clips;
        if (clips == null || clips.Length <= 0)
        {
            Debug.LogWarning("No suitableAudioclipsFound");
        }
        return RandomChoice<T>.Choose(clips);
    }

}
