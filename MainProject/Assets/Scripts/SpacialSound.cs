using UnityEngine;

[System.Serializable]
public class SpacialSound : Sound
{

    [Range(-1f, 1f)]
    public float StereoPan = 0f;
    public new float SpacialBlend { get; } = 1f;
    public AudioRolloffMode RolloffMode { get; } = AudioRolloffMode.Linear;
    [Min(0f)]
    public float MinDistance = 1;
    [Min(0f)]
    public float MaxDistance = 100;
}
