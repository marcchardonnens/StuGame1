using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//this class ist responsible for the whole flow of the game.
//switching scenes, managing game state, etc...



//TODO should prob be static class
public class GameManager : MonoBehaviour
{

    public static ProfileData ProfileData = new ProfileData();

    public static Interactable currentInteractable = null;

    private static Scene queuedScene;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void ChangeScene()
    {
        //SceneManager.LoadScene(sceneIndex);
    }


    public static void PauseGame()
    {
        Time.timeScale = 0;
    }

    public static void UnPauseGame()
    {
        Time.timeScale = 1;
    }
}
