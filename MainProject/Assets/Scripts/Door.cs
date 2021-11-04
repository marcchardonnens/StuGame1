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
            StageManager stage = FindObjectOfType<StageManager>();
            if (stage.SurvivorFreed)
            {
                stage.EndStage(StageResult.SurvivorRescued);
            }
            else
            {
                stage.EndStage(StageResult.EnterHomeEarly);
            }
        }
        else
        {
            SceneManager.LoadScene("LoadingScene2", LoadSceneMode.Single);
        }
    }
}
