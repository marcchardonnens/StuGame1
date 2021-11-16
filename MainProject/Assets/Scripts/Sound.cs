using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Sound : RandomChoice<AudioClip>
{
    [Range(0f,1f)]
    public float Volume = 1f;

    [Range(-3f,3f)]
    public float pitch = 1f;
    public float SpacialBlend {get;} = 0f;
}
