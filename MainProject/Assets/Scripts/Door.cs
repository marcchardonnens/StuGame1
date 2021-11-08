using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Collider))]
public class Door : MonoBehaviour, IInteractable
{

    public void Interact()
    {
        GameManager.Instance.DoorInteract();
    }

    public string UiText()
    {
        if(GameManager.Instance.InGamePlayScene)
        {
            return PlayerUIController.InteractPrefix + "Return Home Safely";
        }
        else
        {
            return PlayerUIController.InteractPrefix + "Start Run";
        }
    }

}
