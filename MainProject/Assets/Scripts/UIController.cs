using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    //main menu
    public Button StartButton, ProfileButton, SettingsButtonMainMenu, ExitButtonMainMenu;

    public TMPro.TextMeshProUGUI profileText, loadingScreenText;

    //Loading screen
    public Button ReadyButton;

    public CanvasGroup FadePannel;
    public CanvasGroup InstructionsScreen;

    public GameObject MainMenu, SplashScreen, LoadingScreen, CreditsScreen;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        StartButton.onClick.AddListener(OnStartButtonClicked);
        ProfileButton.onClick.AddListener(OnProfileButtonClicked);
        SettingsButtonMainMenu.onClick.AddListener(OnSettingsButtonClicked);
        ExitButtonMainMenu.onClick.AddListener(OnExitButtonClicked);
        ReadyButton.onClick.AddListener(OnReadyButtonClicked);
        InstructionsScreen.interactable = false;
        SceneTransition.UIController = this;
        DisableUIInteraction();
        GameplayManagerBase.OnSceneReady += EnableUIInteraction;

        if (!GameManager.Instance.SkipIntro)
        {
            Intro();
        }
        else
        {
            MainMenu.SetActive(true);
            LoadingScreen.SetActive(false);
            CreditsScreen.SetActive(false);
            SplashScreen.SetActive(false);
            FadePannel.gameObject.SetActive(true);
            FadePannel.alpha = 0;
        }


    }

    private async void Intro()
    {
        const float fadeDur = 2f;
        const int lingerDur = 3000;
        const int startingDelay = 1000;

        MainMenu.SetActive(false);
        LoadingScreen.SetActive(false);
        CreditsScreen.SetActive(false);
        SplashScreen.SetActive(true);
        FadePannel.gameObject.SetActive(true);
        FadePannel.alpha = 1;
        await Task.Delay(startingDelay);
        await SceneTransition.FadeSceneToTransparent(fadeDur);
        await Task.Delay(lingerDur);
        await SceneTransition.FadeSceneToBlack(fadeDur);
        SplashScreen.SetActive(false);
        MainMenu.SetActive(true);
        await SceneTransition.FadeSceneToTransparent(fadeDur);
    }

    public void EnableUIInteraction()
    {
        InstructionsScreen.interactable = true;
        GameManager.Instance.UnlockCursor();
    }

    public void DisableUIInteraction()
    {
        InstructionsScreen.interactable = false;
        GameManager.Instance.LockCursor();
    }


    public void ShowLoadingScreen(string text = "")
    {
        HideAll();
        LoadingScreen.SetActive(true);
        if (text != "")
        {
            SetLoadingScreenText(text);
        }
    }
    public void SetLoadingScreenText(string text)
    {
        loadingScreenText.text = text;
    }

    public void ShowMenu()
    {

    }

    public void ShowCredits()
    {

    }

    public void HideAll()
    {
        MainMenu.SetActive(true);
        LoadingScreen.SetActive(false);
        CreditsScreen.SetActive(false);
        SplashScreen.SetActive(false);
    }


    private void OnStartButtonClicked()
    {
        Debug.Log("start button");
        StartCoroutine(GameManager.Instance.MainMenuToHub(1.5f, 1.5f));
    }

    private void OnProfileButtonClicked()
    {
        GameManager.Instance.UnlockedProfile = !GameManager.Instance.UnlockedProfile;
        if (GameManager.Instance.UnlockedProfile)
        {
            profileText.text = "Unlocked Profile";
        }
        else
        {
            profileText.text = "Local Profile";
        }
    }

    private void OnSettingsButtonClicked()
    {
        Debug.Log("setting");
    }

    private void OnExitButtonClicked()
    {
        Debug.Log("exit game");
        Application.Quit();
    }

    private void OnReadyButtonClicked()
    {
        // GameManager.Instance.InstructionsOKPressed = true;
        SceneTransition.FadeAwayLoadingScreen();
        Debug.Log("ready");
    }
}
