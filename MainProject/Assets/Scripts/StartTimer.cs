using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartTimer : MonoBehaviour
{
    public float timer;
    public Slider slider;
    
    void Start ()
    {
        timer = 5;
        slider.maxValue = timer;
    }
 
    void Update ()
    {
        if( timer > 0 )
        {
            timer -= Time.deltaTime;
            slider.value = timer;


            if( timer <= 0 )
            {
                SceneManager.LoadScene ("MenuScene");
            }
        }
    }
}
