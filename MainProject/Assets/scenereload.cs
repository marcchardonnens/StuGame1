using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class scenereload : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.UnloadSceneAsync(3);
            SceneManager.LoadSceneAsync(3, LoadSceneMode.Additive);
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(3, LoadSceneMode.Single);
        }
    }
}
