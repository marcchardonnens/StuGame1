using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
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
        StartButton.onClick.AddListener(OnStartButtonClicked);
        ProfileButton.onClick.AddListener(OnProfileButtonClicked);
        SettingsButtonMainMenu.onClick.AddListener(OnSettingsButtonClicked);
        ExitButtonMainMenu.onClick.AddListener(OnExitButtonClicked);
        ReadyButton.onClick.AddListener(OnReadyButtonClicked);

        if(!GameManager.Instance.SkipIntro)
        {
            StartCoroutine(Intro());
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

    private IEnumerator Intro()
    {
        const float fadeDur = 2f;
        const float lingerDur = 3f;
        const float startingDelay = 1f;
        
        MainMenu.SetActive(false);
        LoadingScreen.SetActive(false);
        CreditsScreen.SetActive(false);
        SplashScreen.SetActive(true);
        FadePannel.gameObject.SetActive(true);
        FadePannel.alpha = 1;
        yield return new WaitForSecondsRealtime(startingDelay);
        yield return StartCoroutine(GameManager.Instance.FadeSceneToTransparent(fadeDur));
        yield return new WaitForSecondsRealtime(lingerDur);
        yield return StartCoroutine(GameManager.Instance.FadeSceneToBlack(fadeDur));
        SplashScreen.SetActive(false);
        MainMenu.SetActive(true);
        yield return StartCoroutine(GameManager.Instance.FadeSceneToTransparent(fadeDur));
    }

    public void SetLoadingScreenText(string text)
    {
        loadingScreenText.text = text;
    }

    private void OnStartButtonClicked()
    {
        Debug.Log("start button");
        StartCoroutine(GameManager.Instance.MainMenuToHub(1.5f,1.5f));
    }

    private void OnProfileButtonClicked()
    {
        GameManager.Instance.UnlockedProfile = !GameManager.Instance.UnlockedProfile;
        if(GameManager.Instance.UnlockedProfile)
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
        GameManager.Instance.instructionsOKPressed = true;
        Debug.Log("ready");
    }
}
