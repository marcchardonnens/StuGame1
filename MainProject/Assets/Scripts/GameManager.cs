using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//this class ist responsible for the whole flow of the game.
//switching scenes, managing game state, etc...



public class GameManager : MonoBehaviour
{
    public bool SkipIntro = false;
    private static GameManager instance;

    public static GameManager Instance { get { return instance; } }

    public static ProfileData ProfileData = new ProfileData(false);

    public UIController UIController;
    public bool UnlockedProfile = false;

    public bool instructionsOKPressed = false;
    public bool PlayerHasControl = false;

    public bool InGamePlayScene = false;
    public bool SceneLoaded = false;
    public bool SceneCompletelyRady = false;

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

    public IEnumerator FadeSceneToBlack(float fadeOutDuration)
    {
        UIController.FadePannel.alpha = 0;

        for (float i = 0; i <= 1; i += Time.unscaledDeltaTime / fadeOutDuration)
        {
            UIController.FadePannel.alpha = i;
            yield return null;
        }

        UIController.FadePannel.alpha = 1;
    }

    public IEnumerator FadeSceneToTransparent(float fadeInDuration)
    {
        UIController.FadePannel.alpha = 1;
        for (float i = 1; i >= 0; i -= Time.unscaledDeltaTime / fadeInDuration)
        {
            UIController.FadePannel.alpha = i;
            yield return null;
        }
        UIController.FadePannel.alpha = 0;
    }

    public IEnumerator MainMenuToHub(float fadeOutDuration, float fadeInDuration)
    {
        UIController.InstructionsScreen.interactable = false;
        UIController.SetLoadingScreenText(".....Cozyfying House.....");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(GameConstants.HUBSCENE, LoadSceneMode.Additive);
        yield return StartCoroutine(FadeSceneToBlack(fadeOutDuration));
        UIController.LoadingScreen.SetActive(true);
        UIController.MainMenu.SetActive(false);
        yield return StartCoroutine(FadeSceneToTransparent(fadeInDuration));
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(GameConstants.HUBSCENE));




        //! Wait for a bit longer to make person stare at instructions at least a little bit
        //sounds really stupid, but heres why its useful
        //loading probably doesnt take 5 seconds, but in those 2 seconds the person looks at the screen,
        //and has the time to realize that the instructions are there, after which they might take more time to full read them
        //this is the first loading screen they see, so it makes sense here
        //on a side effect - loading times feel like something important is happening, if they are not too long
        //its also a good minibreak for water/something else
        //tbd if the others should have it but as of now, they do not
        //TODO Loading screen animation (gif)
        if (!GameManager.instance.SkipIntro)
            yield return new WaitForSecondsRealtime(5f);


        GameManager.instance.UnlockCursor();
        UIController.InstructionsScreen.interactable = true;
        UIController.SetLoadingScreenText("Start Journey");
        SceneLoaded = true;
        while (!instructionsOKPressed)
        {
            yield return null;
        }
        UIController.InstructionsScreen.interactable = false;
        instructionsOKPressed = false;
        yield return StartCoroutine(FadeSceneToBlack(fadeOutDuration));
        UIController.LoadingScreen.SetActive(false);
        yield return StartCoroutine(FadeSceneToTransparent(fadeInDuration));
        UIController.InstructionsScreen.interactable = false;
        SceneCompletelyRady = true;

    }

    public IEnumerator HubToGameplay(float fadeOutDuration, float fadeInDuration)
    {
        UIController.InstructionsScreen.interactable = false;
        UIController.SetLoadingScreenText(".....Creating Magical World.....");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(GameConstants.GAMEPLAYSCENE);
        yield return StartCoroutine(FadeSceneToBlack(fadeOutDuration));
        UIController.LoadingScreen.SetActive(true);
        yield return StartCoroutine(FadeSceneToTransparent(fadeInDuration));
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        while (!StageManager.NavMeshBaked && !StageManager.TerrainReady)
        {
            yield return null;
        }
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(GameConstants.GAMEPLAYSCENE));
        GameManager.instance.UnlockCursor();
        UIController.InstructionsScreen.interactable = true;
        UIController.SetLoadingScreenText("I must save the Survivor!");
        while (!instructionsOKPressed)
        {
            yield return null;
        }
        UIController.InstructionsScreen.interactable = false;
        instructionsOKPressed = false;
        yield return StartCoroutine(FadeSceneToBlack(fadeOutDuration));
        UIController.LoadingScreen.SetActive(false);
        yield return StartCoroutine(FadeSceneToTransparent(fadeInDuration));
    }

    //TODO show credits
    public IEnumerator GameplayToHub(float fadeOutDuration, float fadeInDuration, bool ShowCredits)
    {
        UIController.InstructionsScreen.interactable = false;
        UIController.SetLoadingScreenText(".....Lighting Firepit.....");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(GameConstants.HUBSCENE);
        yield return StartCoroutine(FadeSceneToBlack(fadeOutDuration));
        UIController.LoadingScreen.SetActive(true);
        yield return StartCoroutine(FadeSceneToTransparent(fadeInDuration));
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(GameConstants.HUBSCENE));
        GameManager.instance.UnlockCursor();
        UIController.InstructionsScreen.interactable = true;
        UIController.SetLoadingScreenText("Home sweet Home");
        while (!instructionsOKPressed)
        {
            yield return null;
        }
        UIController.InstructionsScreen.interactable = false;
        instructionsOKPressed = false;
        yield return StartCoroutine(FadeSceneToBlack(fadeOutDuration));
        UIController.LoadingScreen.SetActive(false);
        yield return StartCoroutine(FadeSceneToTransparent(fadeInDuration));
    }


    public void UnlockEverything()
    {
        ProfileData = new ProfileData(true);
    }

    public void NewProfile()
    {
        ProfileData = new ProfileData(false);
    }

    public void DoorInteract()
    {
        if (InGamePlayScene)
        {
            StageManager.DoorPressed();
        }
        else
        {
            StartCoroutine(HubToGameplay(2f, 2f));
        }
        InGamePlayScene = !InGamePlayScene;
    }


    public void PauseGame()
    {
        // Time.timeScale = 0;
    }

    public void UnPauseGame()
    {
        // Time.timeScale = 1;
    }
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
