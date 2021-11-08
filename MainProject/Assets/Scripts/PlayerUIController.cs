using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    public const string InteractPrefix = "E - ";
    public Button BackToGameButton, SettingsButton, WakeupButton, ExitGameButton;
    public GameObject PauseScreen, GameplayHUD, HubHUD;
    public TMPro.TextMeshProUGUI interactText;

    void Awake()
    {
        BackToGameButton.onClick.AddListener(OnBackToGameButtonClicked);
        SettingsButton.onClick.AddListener(OnSettingsButtonClicked);
        WakeupButton.onClick.AddListener(OnWakeupButtonClicked);
        ExitGameButton.onClick.AddListener(OnExitGameButtonClicked);
    }


    public void ShowGameplayHud()
    {

    }

    //TODO Hub hud
    public void ShowHubHud()
    {

    }

    public void HideHud()
    {

    }

    public void ShowMenu()
    {

    }


    public void HideMenu()
    {

    }

    private void OnBackToGameButtonClicked()
    {
        Debug.Log("back to game");
    }

    private void OnSettingsButtonClicked()
    {
        Debug.Log("settings");
    }

    private void OnWakeupButtonClicked()
    {
        Debug.Log("wake up");
    }

    private void OnExitGameButtonClicked()
    {
        Debug.Log("exit game from game");
    }

    public void SetInteractText(string text)
    {
        interactText.text = text;
    }

}
