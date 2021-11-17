using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    public static PlayerUIController Instance {get; private set;}
    public const string InteractPrefix = "E - ";
    public Button BackToGameButton, SettingsButton, WakeupButton, ExitGameButton;
    public GameObject PauseMenu, SharedHUD, GameplayHUD, HubHUD;
    public Slider HealthSlider, RageSlider;
    //shared HUD
    public TMPro.TextMeshProUGUI interactText, HealthText, RageText, SeedAmountText, SeedFunctionText;
    //GameplayHUD
    public TMPro.TextMeshProUGUI MushroomAmountText, TimeText;
    //HubHUD
    public TMPro.TextMeshProUGUI WoodText, MonsterXPText, OxygenText, LuciferiumText, CalciumText;
    public bool PauseMenuOpen = false;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        BackToGameButton.onClick.AddListener(OnBackToGameButtonClicked);
        SettingsButton.onClick.AddListener(OnSettingsButtonClicked);
        WakeupButton.onClick.AddListener(OnWakeupButtonClicked);
        ExitGameButton.onClick.AddListener(OnExitGameButtonClicked);
        PauseMenu.SetActive(false);
        GameplayHUD.SetActive(false);
        HubHUD.SetActive(false);
        SharedHUD.SetActive(false);

        SceneTransition.OnAnyTransitionBegin += HideAllPannels;
        HubManager.OnSceneCompletelyReady += ShowHubHud;
        StageManager.OnSceneCompletelyReady += ShowGameplayHud;

        PlayerController.OnHealthChanged += UpdateHealth;
        PlayerController.OnRageAmountChanged += UpdateRage;
        PlayerController.OnSeedCountChanged += UpdateSeedCount;
        PlayerController.OnPowerupComsume += UpdateMushroomCount;
        PlayerController.OnResourcesChanged += UpdateResources;
    }
    private void ShowGameplayHud()
    {
        SharedHUD.SetActive(true);
        GameplayHUD.SetActive(true);
    }

    private void ShowHubHud()
    {
        SharedHUD.SetActive(true);
        HubHUD.SetActive(true);
    }

    private void HideAllPannels()
    {
        SharedHUD.SetActive(false);
        GameplayHUD.SetActive(false);
        HubHUD.SetActive(false);
        PauseMenu.SetActive(false);
        PauseMenuOpen = false;
    }

    private void HideHud()
    {
        SharedHUD.SetActive(false);
        GameplayHUD.SetActive(false);
        HubHUD.SetActive(false);
    }

    private void ShowPauseMenu()
    {
        HideHud();
        PauseMenu.SetActive(true);
        PauseMenuOpen = true;
        GameManager.Instance.UnlockCursor();
        GameManager.Instance.PauseGame();
    }

    private void HidePauseMenu()
    {
        PauseMenu.SetActive(false);
        PauseMenuOpen = false;
        GameManager.Instance.LockCursor();
        GameManager.Instance.UnPauseGame();
    }

    private void OnBackToGameButtonClicked()
    {
        Debug.Log("back to game");
        HidePauseMenu();
    }

    private void OnSettingsButtonClicked()
    {
        Debug.Log("settings");
    }

    private void OnWakeupButtonClicked()
    {
        Debug.Log("wake up");
        HideAllPannels();
        // StageManager.GiveUp();
    }

    private void OnExitGameButtonClicked()
    {
        Debug.Log("exit game from game");
        HideAllPannels();
        // StageManager.ExitGame();
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
        HealthText.text = Mathf.CeilToInt(current) + "/" + Mathf.CeilToInt(max);
    }

    public void UpdateRage(float current, float max)
    {
        Debug.Log("update rage");
        RageSlider.minValue = 0;
        RageSlider.maxValue = max;
        RageSlider.value = current;
        RageText.text = Mathf.CeilToInt(current) + "/" + Mathf.CeilToInt(max);
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

    public void UpdateResources()
    {
        WoodText.text = GameManager.ProfileData.WoodCurrent.ToString();
        MonsterXPText.text = GameManager.ProfileData.MonsterXPCurrent.ToString();
        OxygenText.text = GameManager.ProfileData.MonsterXPCurrent.ToString();
        LuciferiumText.text = GameManager.ProfileData.LuciferinCurrent.ToString();
        CalciumText.text = GameManager.ProfileData.CalciumCurrent.ToString();
    }
}
