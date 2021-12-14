using UnityEngine;

[System.Serializable]
public class SpacialSound : Sound
{

    [Range(-1f, 1f)]
    public float PanStereo = 0f;
    [Range(0f, 1f)]
    public float SpacialBlend = 1f;
    [Range(0f, 360f)]
    public float Spread = 0f;
    [Min(0f)]
    public float MinDistance = 1f;
    [Min(0f)]
    public float MaxDistance = 100f;

}
