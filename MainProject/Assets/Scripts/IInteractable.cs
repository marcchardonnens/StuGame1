using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IInteractable
{
    bool Enabled {get; set;}
    string Name{get;}
    void Interact();
    string UiText();
}
