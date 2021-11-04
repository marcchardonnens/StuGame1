using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{

    public void Interact()
    {
        if(SceneManager.GetActiveScene().name == "Gameplay")
        {
            SceneManager.LoadScene("LoadingScene1", LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene("LoadingScene2", LoadSceneMode.Single);
        }
    }
}
