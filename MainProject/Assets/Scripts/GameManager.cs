using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//this class ist responsible for the whole flow of the game.
//switching scenes, managing game state, etc...

//TODO propper playerstate

//TODO use this
public enum GameState
{
    Broken = 0,
    Menu,
    TransitionMenuHub,
    Hub,
    TransitionHubGameplay,
    Gameplay,
    TransitionGameplayHub,
    TransitionGameplayMenu
}

public class GameManager : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public bool SkipIntro = false;
    private static GameManager instance;

    public static GameManager Instance { get { return instance; } }

    /* [HideInInspector] */ public static ProfileData ProfileData = new ProfileData(false);
    public PlayerController Player;
    /* [HideInInspector] */ public GameplayManagerBase CurrentManager;

    public UIController UIController;
    public Camera MainCamera;
    public bool UnlockedProfile = false;
    public bool instructionsOKPressed = false;
    public bool PlayerHasControl = false;
    public bool InGamePlayScene = false;
    public bool SceneLoaded = false;
    public bool SceneCompletelyReady = false;
    public int CurrentSceneIndex = 0; //0 = menu

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
        // SceneManager.sceneLoaded += OnSceneLoaded;
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
        GameManager.Instance.PauseGame();
        GameManager.Instance.PlayerHasControl = false;
        GameManager.instance.SceneLoaded = false;
        GameManager.instance.SceneCompletelyReady = false;
        UIController.InstructionsScreen.interactable = false;
        UIController.SetLoadingScreenText(".....Cozyfying House.....");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(GameConstants.HUBSCENE, LoadSceneMode.Single);
        yield return StartCoroutine(FadeSceneToBlack(fadeOutDuration));
        UIController.LoadingScreen.SetActive(true);
        UIController.MainMenu.SetActive(false);
        yield return StartCoroutine(FadeSceneToTransparent(fadeInDuration));
        asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        CurrentSceneIndex = 1;
        CurrentManager = FindObjectOfType<HubManager>();
        CurrentManager.SetupStage();
        
        PlayerCreation(CurrentManager);

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
        GameManager.instance.LockCursor();
        yield return StartCoroutine(FadeSceneToBlack(fadeOutDuration));
        UIController.LoadingScreen.SetActive(false);
        GameManager.instance.PlayerHasControl = true;
        yield return StartCoroutine(FadeSceneToTransparent(fadeInDuration));
        UIController.InstructionsScreen.interactable = false;
        SceneCompletelyReady = true;
        GameManager.Instance.UnPauseGame();

    }

    public IEnumerator HubToGameplay(float fadeOutDuration, float fadeInDuration)
    {
        GameManager.Instance.PauseGame();
        GameManager.Instance.PlayerHasControl = false;
        GameManager.instance.SceneLoaded = false;
        GameManager.instance.SceneCompletelyReady = false;
        UIController.InstructionsScreen.interactable = false;
        GameManager.instance.LockCursor();
        UIController.SetLoadingScreenText(".....Creating Magical World.....");
        yield return StartCoroutine(FadeSceneToBlack(fadeOutDuration));
        UIController.LoadingScreen.SetActive(true);
        yield return StartCoroutine(FadeSceneToTransparent(fadeInDuration));
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(GameConstants.GAMEPLAYSCENE, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        CurrentSceneIndex = 2;
        GameManager.instance.SceneLoaded = true;
        CurrentManager = FindObjectOfType<StageManager>();
        CurrentManager.SetupStage();
        while (!CurrentManager.StageReady)
        {
            yield return null;
        }
        PlayerCreation(CurrentManager);
        GameManager.instance.UnlockCursor();
        UIController.InstructionsScreen.interactable = true;
        UIController.SetLoadingScreenText("I must save the Survivor!");
        instructionsOKPressed = false;
        while (!instructionsOKPressed)
        {
            yield return null;
        }
        instructionsOKPressed = false;
        UIController.InstructionsScreen.interactable = false;
        yield return StartCoroutine(FadeSceneToBlack(fadeOutDuration));
        GameManager.instance.PlayerHasControl = true;
        GameManager.instance.LockCursor();
        UIController.LoadingScreen.SetActive(false);
        yield return StartCoroutine(FadeSceneToTransparent(fadeInDuration));
        GameManager.Instance.UnPauseGame();
        SceneCompletelyReady = true;
    }

    //TODO show credits & bugged
    public IEnumerator GameplayToHub(float fadeOutDuration, float fadeInDuration, bool ShowCredits)
    {
        GameManager.Instance.PauseGame();
        GameManager.Instance.PlayerHasControl = false;
        GameManager.instance.SceneLoaded = false;
        GameManager.instance.SceneCompletelyReady = false;
        UIController.InstructionsScreen.interactable = false;
        UIController.SetLoadingScreenText(".....Lighting Firepit.....");
        yield return StartCoroutine(FadeSceneToBlack(fadeOutDuration));
        UIController.LoadingScreen.SetActive(true);
        Debug.Log("just before load");
        // AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(GameConstants.HUBSCENE, LoadSceneMode.Additive);
        yield return StartCoroutine(FadeSceneToTransparent(fadeInDuration));
        SceneManager.LoadScene(GameConstants.HUBSCENE);
        Debug.Log("just after load");
        // Debug.Log("just after fade to loadscreen");

        //BUG gets stuck between here


        CurrentSceneIndex = 1;
        SceneLoaded = true;
        Debug.Log("searching hubmanager");
        CurrentManager = null;
        CurrentManager = FindObjectOfType<HubManager>();
        if(CurrentManager == null)
        {
            CurrentManager = Instantiate(new GameObject("HubManager"), null).AddComponent<HubManager>();
            CurrentManager.PlayerPrefab = PlayerPrefab;
        }


        CurrentManager.SetupStage();
        Debug.Log("current manager not null");

        while(!CurrentManager.StageReady)
        {
            yield return null;
        }
        Debug.Log("stage ready");
        PlayerCreation(CurrentManager);
        instructionsOKPressed = false;
        GameManager.instance.UnlockCursor();
        ///--------------------------


        UIController.SetLoadingScreenText("Home Sweet Home");
        UIController.InstructionsScreen.interactable = true;
        instructionsOKPressed = false;
        Debug.Log("before ok pressed");
        while (!instructionsOKPressed)
        {
            yield return null;
        }
        Debug.Log("ok pressed");
        UIController.InstructionsScreen.interactable = false;
        GameManager.instance.LockCursor();
        yield return StartCoroutine(FadeSceneToBlack(fadeOutDuration));
        UIController.LoadingScreen.SetActive(false);
        GameManager.instance.PlayerHasControl = true;
        yield return StartCoroutine(FadeSceneToTransparent(fadeInDuration));
        SceneCompletelyReady = true;
        GameManager.Instance.UnPauseGame();

    }

        public IEnumerator TransitionToMenu(float fadeOutDuration, float fadeInDuration)
    {
        GameManager.Instance.PauseGame();
        GameManager.Instance.PlayerHasControl = false;
        yield return StartCoroutine(FadeSceneToBlack(fadeOutDuration));
        CurrentSceneIndex = 0;
        RemovePlayerAndSwapToMainCamera();
        SceneManager.LoadScene(GameConstants.MAINMENUSCENE, LoadSceneMode.Single);
        yield return StartCoroutine(FadeSceneToTransparent(fadeInDuration));
        GameManager.instance.UnlockCursor();
    }

    public void PlayerCreation(IGameplayManager manager)
    {
        Debug.Log("player creation");
        if(Player != null)
        {
            Destroy(Player.gameObject);
        }
        Player = manager.CreatePlayer();
        SwapActiveCameraAndSoundlistenerToPlayer(Player);
    }

    public void SwapActiveCameraAndSoundlistenerToPlayer(PlayerController player)
    {
        MainCamera.enabled = false;
        MainCamera.GetComponent<AudioListener>().enabled = false;
        player.playerCamera.enabled = true;
        player.playerCamera.GetComponent<AudioListener>().enabled = true;
        Player.transform.SetSiblingIndex(0);
    }

    public void RemovePlayerAndSwapToMainCamera()
    {
        if(Player != null)
        {
            Destroy(Player.gameObject);
        }
        MainCamera.enabled = false;
        MainCamera.GetComponent<AudioListener>().enabled = false;
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
        if (GameManager.instance.CurrentSceneIndex == 2)
        {
            StageManager.DoorPressed();
        }
        else
        {

            StartCoroutine(HubToGameplay(2f, 2f));
        }
    }


    public void PauseGame()
    {
        Time.timeScale = 0;
        PlayerHasControl = false;
    }

    public void UnPauseGame()
    {
        Time.timeScale = 1;
        PlayerHasControl = true;
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
