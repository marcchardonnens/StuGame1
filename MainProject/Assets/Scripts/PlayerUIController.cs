using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    public const string InteractPrefix = "E - ";
    public Button BackToGameButton, SettingsButton, WakeupButton, ExitGameButton;
    public GameObject PauseMenu, GameplayHUD, HubHUD;
    public Slider HealthSlider, RageSlider;
    //GameplayHUD
    public TMPro.TextMeshProUGUI interactText, HealthText, RageText, MushroomAmountText, SeedAmountText, SeedFunctionText, TimeText;
    //HubHUD
    public TMPro.TextMeshProUGUI interactTextHub, HealthTextHub, RageTextHub, SeedAmountTextHub, SeedFunctionTextHub, WoodText, MonsterXPText, OxygenText, LuciferiumText, CalciumText;
    public bool PauseMenuOpen = false;

    void Awake()
    {
        BackToGameButton.onClick.AddListener(OnBackToGameButtonClicked);
        SettingsButton.onClick.AddListener(OnSettingsButtonClicked);
        WakeupButton.onClick.AddListener(OnWakeupButtonClicked);
        ExitGameButton.onClick.AddListener(OnExitGameButtonClicked);
        PauseMenu.SetActive(false);
        GameplayHUD.SetActive(false);
        HubHUD.SetActive(false);
    }

    void Update()
    {
        ManageOpenMenus();
    }

    private void ManageOpenMenus()
    {
        if (GameManager.Instance.PlayerHasControl)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(PauseMenuOpen)
                {
                    HidePauseMenu();
                }
                else
                {
                    ShowPauseMenu();
                }
            }

            if(!PauseMenuOpen)
            {
                if(GameManager.Instance.CurrentSceneIndex == 1)
                {
                    ShowHubHud();
                }
                else if(GameManager.Instance.CurrentSceneIndex == 2)
                {
                    ShowGameplayHud();
                }
            }

            // if (GameManager.Instance.CurrentSceneIndex == 1)
            // {
            //     if (PauseMenuOpen)
            //     {
            //         ShowMenu();
            //     }
            //     else
            //     {
            //         ShowHubHud();
            //     }
            // }
            // else if (GameManager.Instance.CurrentSceneIndex == 2)
            // {
            //     if (PauseMenuOpen)
            //     {
            //         ShowMenu();
            //     }
            //     else
            //     {
            //         ShowGameplayHud();
            //     }
            // }
            // else
            // {
            //     HideHud();
            // }
        }
        else
        {
            HideHud();
        }
    }

    private void ShowGameplayHud()
    {
        GameplayHUD.SetActive(true);
    }

    private void ShowHubHud()
    {
        HubHUD.SetActive(true);
    }

    private void HideAllPannels()
    {
        GameplayHUD.SetActive(false);
        HubHUD.SetActive(false);
        PauseMenu.SetActive(false);
        PauseMenuOpen = false;
    }

    private void HideHud()
    {
        GameplayHUD.SetActive(false);
        HubHUD.SetActive(false);
    }

    private void ShowPauseMenu()
    {
        HideHud();
        PauseMenu.SetActive(true);
        PauseMenuOpen = true;
        GameManager.Instance.PauseGame();
    }

    private void HidePauseMenu()
    {
        PauseMenu.SetActive(false);
        PauseMenuOpen = false;
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
        HideHud();
        StageManager.GiveUp();
    }

    private void OnExitGameButtonClicked()
    {
        Debug.Log("exit game from game");
        HideHud();
        StageManager.ExitGame();
    }

    public void SetInteractText(string text)
    {
        interactText.text = text;
        interactTextHub.text = text;
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

    public void UpdateResources()
    {
        WoodText.text = GameManager.ProfileData.WoodCurrent.ToString();
        MonsterXPText.text = GameManager.ProfileData.MonsterXPCurrent.ToString();
        OxygenText.text = GameManager.ProfileData.MonsterXPCurrent.ToString();
        LuciferiumText.text = GameManager.ProfileData.LuciferinCurrent.ToString();
        CalciumText.text = GameManager.ProfileData.CalciumCurrent.ToString();
    }
}
