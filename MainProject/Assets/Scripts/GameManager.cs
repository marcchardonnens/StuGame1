using System;
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
    Intro,
    Menu,
    TransitionBegin,
    Transition,
    TransitionEnding,
    StageInitializing,
    StagePlaying,
    StagePaused,
    StageEnding,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [field: SerializeField] public GameObject PlayerPrefab { get; set; }
    [field: SerializeField] public bool SkipIntro { get; set; } = false;
    [field: SerializeField] public bool SkipToGameplayScene { get; set; }
    public static ProfileData ProfileData { get; set; } = new ProfileData(false);
    public PlayerController Player { get; set; }
    public GameState State { get; private set; } = GameState.Intro;
    public GameplayManagerBase CurrentManager { get; set; }
    [field: SerializeField]
    public UIController UIController { get; set; }
    [field: SerializeField] public Camera MainCamera { get; set; }
    public event Action<GameState, GameState> OnGameStateChanged = delegate { };
    [field: SerializeField] public bool UnlockedProfile { get; set; } = false;
    public bool InstructionsOKPressed { get; set; } = false;
    [field: SerializeField] public bool GameUnpaused { get; private set; } = false;
    public bool InGamePlayScene { get; set; } = false;
    public bool SceneLoaded { get; set; } = false;
    public bool SceneCompletelyReady { get; set; } = false;
    public int CurrentSceneIndex { get; set; } = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Destroy(this);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        PlayerController.OnPlayerCreated += OnPlayerCreation;
        PlayerController.OnPlayerDestroyed += OnRemovePlayerSwapToMainCamera;
    }

    public void DestroyCurrentManager()
    {
        if (CurrentManager != null)
        {
            Destroy(CurrentManager.gameObject);
            Destroy(CurrentManager);
            CurrentManager = null;
            GameplayManagerBase.Instance = null;
        }
    }

    public void UpdateState(GameState newState)
    {
        OnGameStateChanged?.Invoke(State, newState);
        State = newState;

        switch (newState)
        {
            case GameState.Intro:
                break;
            case GameState.Menu:
                break;
            case GameState.TransitionBegin:
                PauseGame();
                if(!GameManager.Instance.SkipToGameplayScene)
                    AudioManager.Instance.FadeOutAllSound();
                break;
            case GameState.Transition:
                break;
            case GameState.TransitionEnding:
                if(!GameManager.Instance.SkipToGameplayScene)
                    AudioManager.Instance.FadeInAllSound();
                break;
            case GameState.StageInitializing:
                break;
            case GameState.StagePlaying:
                UnPauseGame();
                break;
            case GameState.StagePaused:
                PauseGame();
                break;
            case GameState.StageEnding:
                break;
            default:
                break;
        }
    }

    public void OnPlayerCreation(PlayerController player)
    {
        if (Player != null)
        {
            Destroy(Player.gameObject);
        }
        Player = player;
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

    public void OnRemovePlayerSwapToMainCamera(PlayerController player)
    {
        if (Player != null)
        {
            Destroy(Player.gameObject);
            Player = null;
        }
        if (MainCamera == null)
        {
            return;
        }
        MainCamera.enabled = true;
        MainCamera.GetComponent<AudioListener>().enabled = true;
    }

    public void UnlockEverything()
    {
        ProfileData = new ProfileData(true);
    }

    public void NewProfile()
    {
        ProfileData = new ProfileData(false);
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        GameUnpaused = false;
    }

    public void UnPauseGame()
    {
        Time.timeScale = 1;
        GameUnpaused = true;
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
