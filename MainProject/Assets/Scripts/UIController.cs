using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    //main menu
    public Button StartButton, ProfileButton, SettingsButtonMainMenu, ExitButtonMainMenu;

    public TMPro.TextMeshProUGUI profileText;

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

        StartCoroutine(Intro());
    }

    private IEnumerator Intro()
    {
        const int loops = 100;
        const float fadeDur = 2f;
        const float lingerDur = 3f;
        
        MainMenu.SetActive(false);
        LoadingScreen.SetActive(false);
        CreditsScreen.SetActive(false);
        SplashScreen.SetActive(true);
        FadePannel.gameObject.SetActive(true);
        FadePannel.alpha = 1;
        for (int i = loops - 1; i >= 0; i--)
        {
            FadePannel.alpha = fadeDur/loops * i;
            yield return new WaitForSecondsRealtime(fadeDur/loops);
        }
        yield return new WaitForSecondsRealtime(lingerDur);
        
        for (int i = 0; i < loops; i++)
        {
            FadePannel.alpha = fadeDur/loops * i;
            yield return new WaitForSecondsRealtime(fadeDur/loops);
        }
        SplashScreen.SetActive(false);
        MainMenu.SetActive(true);
        
        for (int i = loops - 1; i >= 0; i--)
        {
            FadePannel.alpha = fadeDur/loops * i;
            yield return new WaitForSecondsRealtime(fadeDur/loops);
        }
    }

    private void OnStartButtonClicked()
    {
        Debug.Log("start button");
        StartCoroutine(GameManager.Instance.MainMenuToHub(1f,1f));
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
