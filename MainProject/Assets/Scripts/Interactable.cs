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
        Collider collider = GetComponent<Collider>();
        if(collider)
        {
            if(!collider.isTrigger)
            {
                Debug.Log("Interactable Collider was not set to Trigger. It was set to trigger by script." + "    " + gameObject.name);
                collider.isTrigger = true;
            }
        }
    }

    public void Interact()
    {
        OnInteract.Invoke();
    }


}
