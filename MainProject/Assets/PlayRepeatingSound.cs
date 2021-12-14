using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRepeatingSound : MonoBehaviour
{
    public ClipCollection<SpacialSound>[] Sounds;
    public SpacialAudioSource SpacialAudio;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PlaySound());
    }

    private IEnumerator PlaySound()
    {
        while(true)
        {
            SpacialAudio.Play(ClipCollection<SpacialSound>.ChooseClipFromType(SoundType.Default, Sounds));
            // GetComponent<AudioSource>().Play();
            yield return new WaitForSeconds(4f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
