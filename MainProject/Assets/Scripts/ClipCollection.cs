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
public class ClipCollection<T> where T : Sound
{
    public SoundType Type;
    public RandomChoice<T>[] Clips;

    public static T ChooseClipFromType(SoundType type, ClipCollection<T>[] sounds)
    {
        RandomChoice<T>[] clips = sounds.FirstOrDefault(x => x.Type == type).Clips;
        if (clips == null || clips.Length <= 0)
        {
            Debug.LogWarning("No suitableAudioclipsFound");
        }
        return RandomChoice<T>.Choose(clips);
    }

}
