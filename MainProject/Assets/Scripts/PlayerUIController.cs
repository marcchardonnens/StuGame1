using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    public Button BackToGameButton, SettingsButton, WakeupButton, ExitGameButton;

    void Awake()
    {
        BackToGameButton.onClick.AddListener(OnBackToGameButtonClicked);
        SettingsButton.onClick.AddListener(OnSettingsButtonClicked);
        WakeupButton.onClick.AddListener(OnWakeupButtonClicked);
        ExitGameButton.onClick.AddListener(OnExitGameButtonClicked);
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

}
