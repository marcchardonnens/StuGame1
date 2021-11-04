using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//this class ist responsible for the whole flow of the game.
//switching scenes, managing game state, etc...



//TODO should prob be static class
public class GameManager : MonoBehaviour
{

    public static ProfileData ProfileData = new ProfileData();

    public static Interactable currentInteractable = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
