using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Collider))]
public class Door : MonoBehaviour, IInteractable
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

    public string UiText()
    {
        return "";
    }
}
