using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{

    public GameObject holding;
    public AnimationClip[] clips;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void attack()
    {
        
        Animation a = GetComponent<Animation>();

        a.clip = clips[0];
        a.Play();
    }


}
