using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Collider))]
public class Door : MonoBehaviour, IInteractable
{

    public void Interact()
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

    public string UiText()
    {
        return "";
    }
}
