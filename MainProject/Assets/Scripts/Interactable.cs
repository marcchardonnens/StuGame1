using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [SerializeField] private UnityEvent OnInteract;
    private void Awake()
    {
        gameObject.layer = GameConstants.INTERACTABLELAYER;
    }

    public void Interact()
    {
        OnInteract.Invoke();
    }


}
