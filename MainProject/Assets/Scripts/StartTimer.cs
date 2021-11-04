using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartTimer : MonoBehaviour
{
    private float timer;
    
    void Start ()
    {
        timer = 10;
    }
 
    void Update ()
    {
        if( timer > 0 )
        {
            timer -= Time.deltaTime;

            if( timer <= 0 )
            {
                SceneManager.LoadScene ("MenuScene");
            }
        }
    }
}
