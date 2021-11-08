using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//this class ist responsible for the whole flow of the game.
//switching scenes, managing game state, etc...



public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance { get { return instance; } }

    public static ProfileData ProfileData = new ProfileData(false);

    public UIController UIController;
    public bool UnlockedProfile = false;

    public bool instructionsOKPressed = false;
    private bool sceneLoaded = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

    }

    public IEnumerator FadeToSceneBlack(float fadeOutDuration, float fadeInDuration, int nextScene, LoadSceneMode mode)
    {
        const int loops = 100;
        UIController.FadePannel.alpha = 0;
        for (int i = 0; i < loops; i++)
        {
            UIController.FadePannel.alpha = fadeOutDuration / loops * i;
            yield return new WaitForSecondsRealtime(fadeOutDuration / loops);
        }
        // SceneManager.LoadScene(SceneManager.)
        SceneManager.LoadScene(nextScene, mode);

        for (int i = loops - 1; i >= 0; i--)
        {
            UIController.FadePannel.alpha = fadeInDuration / loops * i;
            yield return new WaitForSecondsRealtime(fadeInDuration / loops);
        }
        UIController.FadePannel.alpha = 0;
    }

    public IEnumerator MainMenuToHub(float fadeOutDuration, float fadeInDuration)
    {
        UIController.LoadingScreen.SetActive(true);
        const int loops = 100;
        UIController.InstructionsScreen.alpha = 0;
        UIController.InstructionsScreen.interactable = false;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(GameConstants.HUBSCENE, LoadSceneMode.Additive);
        for (int i = 0; i < loops; i++)
        {
            UIController.InstructionsScreen.alpha = fadeOutDuration / loops * i;
            yield return new WaitForSecondsRealtime(fadeOutDuration / loops);
        }
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        UIController.MainMenu.SetActive(false);
        PlayerController.UnlockCursor();
        UIController.InstructionsScreen.interactable = true;
        while (!instructionsOKPressed)
        {
            yield return null;
        }
        PlayerController.LockCursor();
        GameManager.UnPauseGame();
        instructionsOKPressed = false;
        UIController.InstructionsScreen.interactable = false;
        for (int i = loops - 1; i >= 0; i--)
        {
            UIController.InstructionsScreen.alpha = fadeInDuration / loops * i;
            yield return new WaitForSecondsRealtime(fadeInDuration / loops);
        }
        UIController.InstructionsScreen.alpha = 0;

    }

    public IEnumerator HubToGameplay(float fadeOutDuration, float fadeInDuration)
    {
        const int loops = 100;
        UIController.InstructionsScreen.alpha = 0;
        UIController.InstructionsScreen.interactable = false;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(GameConstants.GAMEPLAYSCENE);
        for (int i = 0; i < loops; i++)
        {
            UIController.FadePannel.alpha = fadeOutDuration / loops * i;
            yield return new WaitForSecondsRealtime(fadeOutDuration / loops);
        }
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        StageManager stageManager = FindObjectOfType<StageManager>();

        while (!stageManager.NavMeshBaked && !stageManager.TerrainReady)
        {
            yield return null;
        }

        UIController.InstructionsScreen.interactable = true;
        while (!instructionsOKPressed)
        {
            yield return null;
        }
        instructionsOKPressed = false;

        for (int i = loops - 1; i >= 0; i--)
        {
            UIController.InstructionsScreen.alpha = fadeInDuration / loops * i;
            yield return new WaitForSecondsRealtime(fadeInDuration / loops);
        }
        UIController.InstructionsScreen.alpha = 0;
    }

    public IEnumerator GameplayToHub(float fadeOutDuration, float fadeInDuration)
    {
        const int loops = 100;
        UIController.InstructionsScreen.alpha = 0;
        UIController.InstructionsScreen.interactable = false;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(GameConstants.HUBSCENE);
        for (int i = 0; i < loops; i++)
        {
            UIController.FadePannel.alpha = fadeOutDuration / loops * i;
            yield return new WaitForSecondsRealtime(fadeOutDuration / loops);
        }
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        UIController.InstructionsScreen.interactable = true;
        while (!instructionsOKPressed)
        {
            yield return null;
        }
        instructionsOKPressed = false;

        for (int i = loops - 1; i >= 0; i--)
        {
            UIController.InstructionsScreen.alpha = fadeInDuration / loops * i;
            yield return new WaitForSecondsRealtime(fadeInDuration / loops);
        }
        UIController.InstructionsScreen.alpha = 0;
    }


    public static void UnlockEverything()
    {
        ProfileData = new ProfileData(true);
    }

    public static void NewProfile()
    {
        ProfileData = new ProfileData(false);
    }

    public static void ChangeScene()
    {

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
