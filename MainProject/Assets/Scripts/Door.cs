using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{

    public void Interact()
    {

        Debug.Log("loading level");
        if(SceneManager.GetActiveScene().name == "GameplayFinal")
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
            FindObjectOfType<StageManager>().ShowLoadingScreen();

            //SceneManager.LoadScene("GameplayFinal", LoadSceneMode.Single);
        }
    }
}
