using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Collider))]
public class Door : MonoBehaviour, IInteractable
{
    public string Name => "Door";
    public static event Action OnDoorInteract = delegate{};
    private void Awake() {
        //TODO play door close animation
    }
    public void Interact()
    {
        //TODO play door open animation
        OnDoorInteract?.Invoke();
    }

    public string UiText()
    {
        if(GameManager.Instance.CurrentSceneIndex == GameConstants.GAMEPLAYSCENE)
        {
            return PlayerUIController.InteractPrefix + "Return Home Safely";
        }
        else if(GameManager.Instance.CurrentSceneIndex == GameConstants.HUBSCENE)
        {
            return PlayerUIController.InteractPrefix + "Start Run";
        }

        return PlayerUIController.InteractPrefix + "Door";
    }
}
