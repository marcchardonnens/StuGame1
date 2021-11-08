using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    public const string InteractPrefix = "E - ";
    public Button BackToGameButton, SettingsButton, WakeupButton, ExitGameButton;
    public GameObject PauseScreen, GameplayHUD, HubHUD;
    public Slider HealthSlider, RageSlider;
    public TMPro.TextMeshProUGUI interactText, HealthText, RageText, MushroomAmountText, SeedAmountText, SeedFunctionText, TimeText;

    void Awake()
    {
        BackToGameButton.onClick.AddListener(OnBackToGameButtonClicked);
        SettingsButton.onClick.AddListener(OnSettingsButtonClicked);
        WakeupButton.onClick.AddListener(OnWakeupButtonClicked);
        ExitGameButton.onClick.AddListener(OnExitGameButtonClicked);
        PauseScreen.SetActive(false);
        GameplayHUD.SetActive(false);
        HubHUD.SetActive(false);
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

    public void UpdateHealth(float current, float max)
    {
        HealthSlider.minValue = 0;
        HealthSlider.maxValue = max;
        HealthSlider.value = current;
        HealthText.text = current + "/" + max;
    }

    public void UpdateRage(float current, float max)
    {
        RageSlider.minValue = 0;
        RageSlider.maxValue = max;
        RageSlider.value = current;
        RageText.text = current + "/" + max;
    }

    public void UpdateSeedCount(int current, int max)
    {
        SeedAmountText.text = current + "/" + max;
    }

    public void UpdateMushroomCount(int amount)
    {
        MushroomAmountText.text = amount.ToString();
    }

    public void UpdateTime(float time)
    {
        if (time < 0)
        {
            time = 0;
        }

        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);

        TimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateSeedSelectionText(string text)
    {
        SeedFunctionText.text = text;
    }

}
