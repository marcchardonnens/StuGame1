using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Collider))]
public class Door : MonoBehaviour, IInteractable
{
    public const string OPENANIM = "door|doorOpen";
    public const string CLOSEANIM = "door|doorClose";
    public bool Enabled { get; set; } = true;
    public string Name => "Door";
    public static event Action OnDoorInteract = delegate { };
    private void Awake()
    {
        Animator anim = GetComponentInParent<Animator>();
        anim.SetBool("Open", false);
    }
    public void Interact()
    {
        if(!Enabled) return;

        OnDoorInteract?.Invoke();
        Enabled = false;

        Animator anim = GetComponentInParent<Animator>();
        anim.SetBool("Open", true);
    }

    public string UiText()
    {
        if(!Enabled) return "";
        if (GameManager.Instance.CurrentSceneIndex == GameConstants.GAMEPLAYSCENE)
        {
            return PlayerUIController.InteractPrefix + "Return Home Safely";
        }
        else if (GameManager.Instance.CurrentSceneIndex == GameConstants.HUBSCENE)
        {
            return PlayerUIController.InteractPrefix + "Start Run";
        }

        return PlayerUIController.InteractPrefix + "Door";
    }
}
