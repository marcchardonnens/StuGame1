using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//this class ist responsible for the whole flow of the game.
//switching scenes, managing game state, etc...



//TODO should prob be static class
public class GameManager : MonoBehaviour
{

    public static ProfileData ProfileData = new ProfileData(false);

    public static Interactable currentInteractable = null;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public static void UnlockEverything()
    {
        ProfileData = new ProfileData(true);
    }

    public static void NewProfile()
    {
        ProfileData = new ProfileData(false);
    }

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
