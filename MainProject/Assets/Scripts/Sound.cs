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
public class Sound
{
    public SoundType Type;
    public RandomChoice<AudioClip>[] Clips;


    public static AudioClip ChooseClipFromType(SoundType type, Sound[] sounds)
    {
        RandomChoice<AudioClip>[] clips = sounds.FirstOrDefault(x => x.Type == type).Clips;
        if (clips == null || clips.Length <= 0)
        {
            Debug.LogWarning("No suitableAudioclipsFound");
        }
        return RandomChoice<AudioClip>.Choose(clips);
    }

}
