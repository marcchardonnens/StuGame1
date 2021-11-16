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
public class ClipCollection<T> where T : RandomChoice<AudioClip>
{
    public SoundType Type;
    public T[] Clips;


    public static AudioClip ChooseClipFromType(SoundType type, ClipCollection<T>[] sounds)
    {
        T[] clips = sounds.FirstOrDefault(x => x.Type == type).Clips;
        if (clips == null || clips.Length <= 0)
        {
            Debug.LogWarning("No suitableAudioclipsFound");
        }
        return RandomChoice<AudioClip>.Choose(clips);
    }

}
